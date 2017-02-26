using System;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core;
using kino.Core.Diagnostics;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public partial class InstanceLeaseProvider : IInstanceLeaseProvider
    {
        private readonly Instance instance;
        private readonly IRoundBasedRegister register;
        private readonly IBallotGenerator ballotGenerator;
        private readonly LeaseConfiguration leaseConfig;
        private readonly Node localNode;
        private readonly ILogger logger;
        private volatile Lease lastKnownLease;

        public InstanceLeaseProvider(Instance instance,
                                     IRoundBasedRegister register,
                                     IBallotGenerator ballotGenerator,
                                     LeaseConfiguration leaseConfig,
                                     ISynodConfiguration synodConfig,
                                     ILogger logger)
        {
            localNode = synodConfig.LocalNode;
            this.instance = instance;
            this.register = register;
            this.ballotGenerator = ballotGenerator;
            this.leaseConfig = leaseConfig;
            this.logger = logger;

            logger.Info($"{instance.Identity.GetAnyString()}-InstanceLeaseProvider created");
        }

        public Lease GetLease(byte[] requestorIdentity, TimeSpan leaseTimeSpan)
        {
            if (LeaseNullOrExpired(lastKnownLease))
            {
                ReadOrRenewLease(requestorIdentity, leaseTimeSpan);
            }

            return lastKnownLease;
        }

        private void ReadOrRenewLease(byte[] requestorIdentity, TimeSpan leaseTimeSpan)
        {
            var lease = AсquireOrLearnLease(ballotGenerator.New(instance.Identity), requestorIdentity, leaseTimeSpan);

            lastKnownLease = lease;
        }

        private Lease AсquireOrLearnLease(Ballot ballot, byte[] requestorIdentity, TimeSpan leaseTimeSpan)
        {
            var read = register.Read(ballot);
            if (read.TxOutcome == TxOutcome.Commit)
            {
                var lease = read.Lease;
                if (LeaseIsNotSafelyExpired(lease))
                {
                    LogStartSleep();
                    leaseConfig.ClockDrift.Sleep();
                    LogAwake();

                    // TODO: Add recursion exit condition
                    return AсquireOrLearnLease(ballotGenerator.New(instance.Identity), requestorIdentity, leaseTimeSpan);
                }

                if (LeaseNullOrExpired(lease) || IsLeaseOwner(lease))
                {
                    var now = DateTime.UtcNow;
                    LogLeaseProlonged(lease);
                    lease = new Lease(localNode.SocketIdentity, now + leaseTimeSpan, requestorIdentity);
                }

                var write = register.Write(ballot, lease);
                if (write.TxOutcome == TxOutcome.Commit)
                {
                    return lease;
                }
            }

            return null;
        }

        private bool IsLeaseOwner(Lease lease)
            => lease != null && Unsafe.ArraysEqual(lease.OwnerIdentity, localNode.SocketIdentity);

        private bool LeaseIsNotSafelyExpired(Lease lease)
        {
            var now = DateTime.UtcNow;

            return lease != null
                   && lease.ExpiresAt < now
                   && lease.ExpiresAt + leaseConfig.ClockDrift > now;
        }

        private static bool LeaseNullOrExpired(Lease lease)
            => lease == null || lease.ExpiresAt < DateTime.UtcNow;
    }
}
using System;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.LeaseProvider.Configuration;

namespace kino.LeaseProvider
{
    public partial class InstanceLeaseProvider : IInstanceLeaseProvider
    {
        private readonly Instance instance;
        private readonly IInstanceRoundBasedRegister register;
        private readonly IBallotGenerator ballotGenerator;
        private readonly InstanceLeaseProviderConfiguration leaseConfig;
        private readonly Node localNode;
        private readonly ILogger logger;
        private volatile Lease lastKnownLease;
        private readonly TimeSpan minLeaseValidityPeriod;

        public InstanceLeaseProvider(Instance instance,
                                     IInstanceRoundBasedRegister register,
                                     IBallotGenerator ballotGenerator,
                                     InstanceLeaseProviderConfiguration leaseConfig,
                                     ISynodConfigurationProvider synodConfigProvider,
                                     ILogger logger)
        {
            localNode = synodConfigProvider.LocalNode;
            this.instance = instance;
            this.register = register;
            this.ballotGenerator = ballotGenerator;
            this.leaseConfig = leaseConfig;
            this.logger = logger;
            minLeaseValidityPeriod = leaseConfig.MessageRoundtrip.MultiplyBy(2) + leaseConfig.ClockDrift;
            logger.Info($"{instance.Identity.GetAnyString()}-InstanceLeaseProvider created");
        }

        public Lease GetLease(GetLeaseRequest request)
        {
            if (LeaseNullOrExpired(lastKnownLease)
                || IsLeaseOwner(request.RequestorIdentity, lastKnownLease) && LeaseShouldBeProlonged(request, lastKnownLease))
            {
                ReadOrRenewLease(request);
            }

            return lastKnownLease;
        }

        public bool IsConsensusReached()
            => register.IsConsensusReached();

        public bool IsInstanceStale()
            => register.IsInstanceStale();

        public void Dispose()
            => register.Dispose();

        private bool LeaseShouldBeProlonged(GetLeaseRequest request, Lease lastKnownLease)
            => lastKnownLease.ExpiresAt - DateTime.UtcNow
               <=
               Max(minLeaseValidityPeriod, request.LeaseTimeSpan.DivideBy(Math.Max(request.MinValidityTimeFraction, 1)));

        private void ReadOrRenewLease(GetLeaseRequest request)
        {
            var lease = AсquireOrLearnLease(ballotGenerator.New(instance.Identity),
                                            request.RequestorIdentity,
                                            request.LeaseTimeSpan);

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

                if (LeaseNullOrExpired(lease) || IsLeaseOwner(requestorIdentity, lease))
                {
                    var now = DateTime.UtcNow;
                    LogLeaseProlonged(requestorIdentity, lease);
                    lease = new Lease(requestorIdentity, now + leaseTimeSpan, requestorIdentity);
                }

                var write = register.Write(ballot, lease);
                if (write.TxOutcome == TxOutcome.Commit)
                {
                    return lease;
                }
            }

            return null;
        }

        private static bool IsLeaseOwner(byte[] requestorIdentity, Lease lease)
            => lease != null && Unsafe.ArraysEqual(lease.OwnerIdentity, requestorIdentity);

        private bool LeaseIsNotSafelyExpired(Lease lease)
        {
            var now = DateTime.UtcNow;

            return lease != null
                   && lease.ExpiresAt < now
                   && lease.ExpiresAt + leaseConfig.ClockDrift > now;
        }

        private static bool LeaseNullOrExpired(Lease lease)
            => lease == null || lease.ExpiresAt < DateTime.UtcNow;

        private static TimeSpan Max(TimeSpan left, TimeSpan right)
            => left > right
                   ? left
                   : right;
    }
}
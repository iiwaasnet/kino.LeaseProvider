using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Consensus.Messages;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.Core.Messaging;
using Ballot = kino.Consensus.Ballot;
using Lease = kino.Consensus.Lease;

namespace kino.LeaseProvider
{
    public partial class InstanceRoundBasedRegister : IRoundBasedRegister
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private Ballot readBallot;
        private Ballot writeBallot;
        private Lease lease;
        private readonly Listener listener;
        private readonly ISynodConfiguration synodConfig;
        private readonly LeaseConfiguration leaseConfig;
        private readonly Instance instance;
        private readonly ILogger logger;

        private readonly IObservable<IMessage> ackReadStream;
        private readonly IObservable<IMessage> nackReadStream;
        private readonly IObservable<IMessage> ackWriteStream;
        private readonly IObservable<IMessage> nackWriteStream;

        public InstanceRoundBasedRegister(Instance instance,
                                          IIntercomMessageHub intercomMessageHub,
                                          IBallotGenerator ballotGenerator,
                                          ISynodConfiguration synodConfig,
                                          LeaseConfiguration leaseConfig,
                                          ILogger logger)
        {
            this.instance = instance;
            this.logger = logger;
            this.synodConfig = synodConfig;
            this.leaseConfig = leaseConfig;
            this.intercomMessageHub = intercomMessageHub;
            readBallot = ballotGenerator.Null();
            writeBallot = ballotGenerator.Null();

            listener = intercomMessageHub.Subscribe();

            listener.Where(IsLeaseRead)
                    .Subscribe(OnReadReceivedMessage);
            listener.Where(IsWriteLeaseMessage)
                    .Subscribe(OnWriteReceived);

            ackReadStream = listener.Where(IsLeaseAckReadMessage);
            nackReadStream = listener.Where(IsLeaseNackReadMessage);
            ackWriteStream = listener.Where(IsLeaseAckWriteMessage);
            nackWriteStream = listener.Where(IsLeaseNackWriteMessage);

            logger.Info($"{instance.Identity.GetString()}-InstanceRoundBasedRegister created");
        }

        private bool IsLeaseNackWriteMessage(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseNackWrite))
            {
                var payload = message.GetPayload<LeaseNackWriteMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private bool IsLeaseAckWriteMessage(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseAckWrite))
            {
                var payload = message.GetPayload<LeaseAckWriteMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private bool IsLeaseNackReadMessage(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseNackRead))
            {
                var payload = message.GetPayload<LeaseNackReadMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private bool IsLeaseAckReadMessage(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseAckRead))
            {
                var payload = message.GetPayload<LeaseAckReadMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private bool IsWriteLeaseMessage(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseWrite))
            {
                var payload = message.GetPayload<LeaseWriteMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private bool IsLeaseRead(IMessage message)
        {
            if (message.Equals(ConsensusMessages.LeaseRead))
            {
                var payload = message.GetPayload<LeaseReadMessage>();

                return Unsafe.Equals(payload.Ballot.Identity, instance.Identity);
            }

            return false;
        }

        private void OnWriteReceived(IMessage message)
        {
            var payload = message.GetPayload<LeaseWriteMessage>();

            var ballot = new Ballot(new DateTime(payload.Ballot.Timestamp, DateTimeKind.Utc),
                                    payload.Ballot.MessageNumber,
                                    payload.Ballot.Identity);
            IMessage response;
            if (Interlocked.Exchange(ref writeBallot, writeBallot) > ballot || Interlocked.Exchange(ref readBallot, readBallot) > ballot)
            {
                LogNackWrite(ballot);

                response = Message.Create(new LeaseNackWriteMessage
                                          {
                                              Ballot = payload.Ballot,
                                              SenderUri = synodConfig.LocalNode.Uri.ToSocketAddress()
                                          });
            }
            else
            {
                LogAckWrite(ballot);

                Interlocked.Exchange(ref writeBallot, ballot);
                Interlocked.Exchange(ref lease, new Lease(payload.Lease.Identity, new DateTime(payload.Lease.ExpiresAt, DateTimeKind.Utc), payload.Lease.OwnerPayload));

                response = Message.Create(new LeaseAckWriteMessage
                                          {
                                              Ballot = payload.Ballot,
                                              SenderUri = synodConfig.LocalNode.Uri.ToSocketAddress()
                                          });
            }
            intercomMessageHub.Send(response, payload.SenderIdentity);
        }

        private void OnReadReceivedMessage(IMessage message)
        {
            var payload = message.GetPayload<LeaseReadMessage>();

            var ballot = new Ballot(new DateTime(payload.Ballot.Timestamp, DateTimeKind.Utc),
                                    payload.Ballot.MessageNumber,
                                    payload.Ballot.Identity);

            IMessage response;
            if (Interlocked.Exchange(ref writeBallot, writeBallot) >= ballot || Interlocked.Exchange(ref readBallot, readBallot) >= ballot)
            {
                LogNackRead(ballot);

                response = Message.Create(new LeaseNackReadMessage
                                          {
                                              Ballot = payload.Ballot,
                                              SenderUri = synodConfig.LocalNode.Uri.ToSocketAddress()
                                          });
            }
            else
            {
                LogAckRead(ballot);

                Interlocked.Exchange(ref readBallot, ballot);

                response = CreateLeaseAckReadMessage(payload);
            }

            intercomMessageHub.Send(response, payload.SenderIdentity);
        }

        public LeaseTxResult Read(Ballot ballot)
        {
            var ackFilter = new LeaderElectionMessageFilter(ballot, m => m.GetPayload<LeaseAckReadMessage>(), synodConfig);
            var nackFilter = new LeaderElectionMessageFilter(ballot, m => m.GetPayload<LeaseNackReadMessage>(), synodConfig);

            var awaitableAckFilter = new AwaitableMessageStreamFilter(ackFilter.Match, m => m.GetPayload<LeaseAckReadMessage>(), GetQuorum());
            var awaitableNackFilter = new AwaitableMessageStreamFilter(nackFilter.Match, m => m.GetPayload<LeaseNackReadMessage>(), GetQuorum());

            using (ackReadStream.Subscribe(awaitableAckFilter))
            {
                using (nackReadStream.Subscribe(awaitableNackFilter))
                {
                    var message = CreateReadMessage(ballot);
                    intercomMessageHub.Broadcast(message);

                    var index = WaitHandle.WaitAny(new[] {awaitableAckFilter.Filtered, awaitableNackFilter.Filtered},
                                                   leaseConfig.NodeResponseTimeout);

                    if (ReadNotAcknowledged(index))
                    {
                        return new LeaseTxResult {TxOutcome = TxOutcome.Abort};
                    }

                    var lease = awaitableAckFilter
                        .MessageStream
                        .Select(m => m.GetPayload<LeaseAckReadMessage>())
                        .Max(p => CreateLastWrittenLease(p))
                        .Lease;

                    return new LeaseTxResult
                           {
                               TxOutcome = TxOutcome.Commit,
                               Lease = lease
                           };
                }
            }
        }

        private static LastWrittenLease CreateLastWrittenLease(LeaseAckReadMessage p)
        {
            return new LastWrittenLease(new Ballot(p.KnownWriteBallot.Timestamp, p.KnownWriteBallot.MessageNumber, p.KnownWriteBallot.Identity),
                                        (p.Lease != null)
                                            ? new Lease(p.Lease.Identity,
                                                        p.Lease.ExpiresAt,
                                                        p.Lease.OwnerPayload)
                                            : null);
        }

        public LeaseTxResult Write(Ballot ballot, Lease lease)
        {
            var ackFilter = new LeaderElectionMessageFilter(ballot, m => m.GetPayload<LeaseAckWriteMessage>(), synodConfig);
            var nackFilter = new LeaderElectionMessageFilter(ballot, m => m.GetPayload<LeaseNackWriteMessage>(), synodConfig);

            var awaitableAckFilter = new AwaitableMessageStreamFilter(ackFilter.Match, m => m.GetPayload<LeaseAckWriteMessage>(), GetQuorum());
            var awaitableNackFilter = new AwaitableMessageStreamFilter(nackFilter.Match, m => m.GetPayload<LeaseNackWriteMessage>(), GetQuorum());

            using (ackWriteStream.Subscribe(awaitableAckFilter))
            {
                using (nackWriteStream.Subscribe(awaitableNackFilter))
                {
                    intercomMessageHub.Broadcast(CreateWriteMessage(ballot, lease));

                    var index = WaitHandle.WaitAny(new[] {awaitableAckFilter.Filtered, awaitableNackFilter.Filtered},
                                                   leaseConfig.NodeResponseTimeout);

                    if (ReadNotAcknowledged(index))
                    {
                        return new LeaseTxResult {TxOutcome = TxOutcome.Abort};
                    }

                    return new LeaseTxResult
                           {
                               TxOutcome = TxOutcome.Commit,
                               // NOTE: needed???
                               Lease = lease
                           };
                }
            }
        }

        private static bool ReadNotAcknowledged(int index)
        {
            return index == 1 || index == WaitHandle.WaitTimeout;
        }

        private int GetQuorum()
        {
            return synodConfig.Synod.Count() / 2 + 1;
        }

        public void Dispose()
        {
            intercomMessageHub.Stop();
            listener.Dispose();
        }

        private IMessage CreateWriteMessage(Ballot ballot, Lease lease)
        {
            return Message.Create(new LeaseWriteMessage
                                  {
                                      Ballot = new Consensus.Messages.Ballot
                                               {
                                                   Identity = ballot.Identity,
                                                   Timestamp = ballot.Timestamp.Ticks,
                                                   MessageNumber = ballot.MessageNumber
                                               },
                                      Lease = new Consensus.Messages.Lease
                                              {
                                                  Identity = lease.OwnerIdentity,
                                                  ExpiresAt = lease.ExpiresAt.Ticks,
                                                  OwnerPayload = lease.OwnerPayload
                                              }
                                  });
        }

        private IMessage CreateReadMessage(Ballot ballot)
        {
            return Message.Create(new LeaseReadMessage
                                  {
                                      Ballot = new Consensus.Messages.Ballot
                                               {
                                                   Identity = ballot.Identity,
                                                   Timestamp = ballot.Timestamp.Ticks,
                                                   MessageNumber = ballot.MessageNumber
                                               }
                                  });
        }

        private IMessage CreateLeaseAckReadMessage(LeaseReadMessage payload)
        {
            Lease lastKnownLease = null;
            Interlocked.Exchange(ref lastKnownLease, lease);
            Ballot lastKnownWriteBallot = null;
            Interlocked.Exchange(ref lastKnownWriteBallot, writeBallot);

            return Message.Create(new LeaseAckReadMessage
                                  {
                                      Ballot = payload.Ballot,
                                      KnownWriteBallot = new Consensus.Messages.Ballot
                                                         {
                                                             Identity = lastKnownWriteBallot.Identity,
                                                             Timestamp = lastKnownWriteBallot.Timestamp.Ticks,
                                                             MessageNumber = lastKnownWriteBallot.MessageNumber
                                                         },
                                      Lease = (lastKnownLease != null)
                                                  ? new Consensus.Messages.Lease
                                                    {
                                                        Identity = lastKnownLease.OwnerIdentity,
                                                        ExpiresAt = lastKnownLease.ExpiresAt.Ticks,
                                                        OwnerPayload = lastKnownLease.OwnerPayload
                                                    }
                                                  : null,
                                      SenderUri = synodConfig.LocalNode.Uri.ToSocketAddress()
                                  });
        }
    }
}
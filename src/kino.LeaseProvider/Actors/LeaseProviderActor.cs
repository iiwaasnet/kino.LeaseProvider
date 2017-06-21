using System.Collections.Generic;
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using kino.Messaging;

namespace kino.LeaseProvider.Actors
{
    public class LeaseProviderActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;
        private readonly ILogger logger;
        private readonly byte[] clusterName;

        public LeaseProviderActor(ILeaseProvider leaseProvider,
                                  IMessageSerializer messageSerializer,
                                  LeaseProviderConfiguration leaseProviderConfiguration,
                                  ILogger logger)
        {
            this.leaseProvider = leaseProvider;
            this.messageSerializer = messageSerializer;
            this.logger = logger;
            clusterName = leaseProviderConfiguration.ClusterName.GetBytes();
        }

        public override IEnumerable<MessageHandlerDefinition> GetInterfaceDefinition()
            => new[]
               {
                   new MessageHandlerDefinition
                   {
                       Message = MessageDefinition.Create<LeaseRequestMessage>(clusterName),
                       Handler = GetLease
                   }
               };

        private async Task<IActorResult> GetLease(IMessage message)
        {
            var payload = message.GetPayload<LeaseRequestMessage>();

            try
            {
                var lease = leaseProvider.GetLease(new Instance(payload.Instance),
                                                   new GetLeaseRequest
                                                   {
                                                       LeaseTimeSpan = payload.LeaseTimeSpan,
                                                       RequestorIdentity = messageSerializer.Serialize(payload.Requestor),
                                                       MinValidityTimeFraction = payload.MinValidityTimeFraction
                                                   });

                var leaseOwner = (lease != null)
                                     ? messageSerializer.Deserialize<Node>(lease.OwnerPayload)
                                     : null;

                var requestorWonTheLease = RequestorWonTheLease(payload.Requestor, leaseOwner);

                var result = Message.Create(new LeaseResponseMessage
                                            {
                                                LeaseAcquired = requestorWonTheLease,
                                                Lease = requestorWonTheLease
                                                            ? new Lease
                                                              {
                                                                  Instance = payload.Instance,
                                                                  ExpiresAt = lease.ExpiresAt,
                                                                  Owner = leaseOwner
                                                              }
                                                            : null,
                                                Partition = clusterName
                                            });

                return new ActorResult(result);
            }
            catch (InstanceNotRegisteredException err)
            {
                logger.Error(err);

                return new ActorResult(Message.Create(new LeaseInstanceNotRegisteredMessage
                                                      {
                                                          Instance = payload.Instance,
                                                          Partition = clusterName
                                                      }));
            }
        }

        private bool RequestorWonTheLease(Node requestor, Node leaseOwner)
            => requestor.Uri == leaseOwner?.Uri && Unsafe.ArraysEqual(requestor.Identity, leaseOwner?.Identity);
    }
}
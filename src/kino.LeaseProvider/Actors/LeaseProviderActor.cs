using System.Collections.Generic;
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Framework;
using kino.Core.Messaging;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using Node = kino.LeaseProvider.Messages.Node;

namespace kino.LeaseProvider.Actors
{
    public class LeaseProviderActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;
        private readonly byte[] clusterName;

        public LeaseProviderActor(ILeaseProvider leaseProvider,
                                  IMessageSerializer messageSerializer,
                                  LeaseProviderConfiguration leaseProviderConfiguration)
        {
            this.leaseProvider = leaseProvider;
            this.messageSerializer = messageSerializer;
            clusterName = leaseProviderConfiguration.ClusterName.GetBytes();
        }

        public override IEnumerable<MessageHandlerDefinition> GetInterfaceDefinition()
        {
            return new[]
                   {
                       new MessageHandlerDefinition
                       {
                           Message = MessageDefinition.Create<LeaseRequestMessage>(clusterName),
                           Handler = GetLease
                       }
                   };
        }

        private async Task<IActorResult> GetLease(IMessage message)
        {
            var payload = message.GetPayload<LeaseRequestMessage>();

            var lease = leaseProvider.GetLease(new Instance(payload.Instance),
                                               payload.LeaseTimeSpan,
                                               messageSerializer.Serialize(payload.Requestor));

            var leaseOwner = (lease != null)
                                 ? messageSerializer.Deserialize<Node>(lease.OwnerPayload)
                                 : null;

            var requestorWonTheLease = RequestorWonTheLease(payload.Requestor, leaseOwner);

            var result = Message.Create(new LeaseResponseMessage
                                        {
                                            LeaseAquired = requestorWonTheLease,
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

        private bool RequestorWonTheLease(Node requestor, Node leaseOwner)
            => requestor.Uri == leaseOwner?.Uri && Unsafe.Equals(requestor.Identity, leaseOwner?.Identity);
    }
}
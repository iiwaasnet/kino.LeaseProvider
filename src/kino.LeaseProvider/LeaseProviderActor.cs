using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Framework;
using kino.Core.Messaging;
using kino.LeaseProvider.Messages;
using Node = kino.LeaseProvider.Messages.Node;

namespace kino.LeaseProvider
{
    public class LeaseProviderActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;

        public LeaseProviderActor(ILeaseProvider leaseProvider,
                                  IMessageSerializer messageSerializer)
        {
            this.leaseProvider = leaseProvider;
            this.messageSerializer = messageSerializer;
        }

        [MessageHandlerDefinition(typeof (LeaseRequestMessage))]
        public async Task<IActorResult> GetLease(IMessage message)
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
                                                        : null
                                        });
            return new ActorResult(result);
        }

        private bool RequestorWonTheLease(Node requestor, Node leaseOwner)
            => requestor.Uri == leaseOwner?.Uri && Unsafe.Equals(requestor.Identity, leaseOwner?.Identity);

        [MessageHandlerDefinition(typeof (CreateLeaseProviderInstanceRequestMessage))]
        public async Task<IActorResult> CreateLeaseProviderInstance(IMessage message)
        {
            var payload = message.GetPayload<CreateLeaseProviderInstanceRequestMessage>();

            var res = leaseProvider.RegisterInstanceLeaseProvider(new Instance(payload.Instance));

            var response = Message.Create(new CreateLeaseProviderInstanceResponseMessage
                                          {
                                              Instance = payload.Instance,
                                              ActivationWaitTime = res.ActivationWaitTime
                                          });
            var broadcastRequest = Message.Create(new InternalCreateLeaseProviderInstanceRequestMessage {Instance = payload.Instance},
                                                  DistributionPattern.Broadcast);

            return new ActorResult(broadcastRequest, response);
        }

        [MessageHandlerDefinition(typeof (InternalCreateLeaseProviderInstanceRequestMessage))]
        public Task<IActorResult> InternalCreateLeaseProviderInstance(IMessage message)
        {
            var payload = message.GetPayload<InternalCreateLeaseProviderInstanceRequestMessage>();

            leaseProvider.RegisterInstanceLeaseProvider(new Instance(payload.Instance));

            return null;
        }
    }
}
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Messaging;
using kino.LeaseProvider.Messages;

namespace kino.LeaseProvider
{
    public class LeaseProviderActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;

        public LeaseProviderActor(ILeaseProvider leaseProvider, IMessageSerializer messageSerializer)
        {
            this.leaseProvider = leaseProvider;
            this.messageSerializer = messageSerializer;
        }

        [MessageHandlerDefinition(typeof (LeaseRequestMessage))]
        public async Task<ActorResult> GetLease(IMessage message)
        {
            var payload = message.GetPayload<LeaseRequestMessage>();

            var lease = leaseProvider.GetLease(new Instance(payload.Instance),
                                               payload.LeaseTimeSpan,
                                               messageSerializer.Serialize(payload.Requestor),
                                               payload.RequestTimeout);

            return new ActorResult(Message.Create(new LeaseResponseMessage
                                                  {
                                                      LeaseAquired = false, // set to TRUE only if lease is obtained and owner is the requestor
                                                      Lease = null
                                                  }));
        }
    }
}
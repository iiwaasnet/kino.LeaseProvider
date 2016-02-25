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

        public LeaseProviderActor(ILeaseProvider leaseProvider)
        {
            this.leaseProvider = leaseProvider;
        }

        [MessageHandlerDefinition(typeof (GetLeaseMessage))]
        public Task<ActorResult> GetLease(IMessage message)
        {
            return null;
        }
    }
}
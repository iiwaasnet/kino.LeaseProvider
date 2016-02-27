using System.Collections.Generic;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public class LeaseProviderService : ILeaseProviderService
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageRouter messageRouter;
        private readonly IEnumerable<IActor> actors;
        private readonly IActorHostManager actorHostManager;

        public LeaseProviderService(ILeaseProvider leaseProvider,
                                    IMessageRouter messageRouter,
                                    IEnumerable<IActor> actors,
                                    IActorHostManager actorHostManager)
        {
            this.leaseProvider = leaseProvider;
            this.messageRouter = messageRouter;
            this.actors = actors;
            this.actorHostManager = actorHostManager;
        }

        public void Start()
        {
            messageRouter.Start();
            //Thread.Sleep(TimeSpan.FromMilliseconds(300));
            actors.ForEach(a => actorHostManager.AssignActor(a));
            leaseProvider.Start();
        }

        public void Stop()
        {
            leaseProvider.Stop();
            messageRouter.Stop();
            actorHostManager.Dispose();
        }
    }
}
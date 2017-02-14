using System;
using System.Collections.Generic;
using kino.Actors;
using kino.Client;
using kino.Core.Framework;
using kino.Routing;

namespace kino.LeaseProvider
{
    public partial class LeaseProviderService : ILeaseProviderService
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageRouter messageRouter;
        private readonly IMessageHub messageHub;
        private readonly IEnumerable<IActor> actors;
        private readonly IActorHostManager actorHostManager;

        public LeaseProviderService(ILeaseProvider leaseProvider,
                                    IMessageHub messageHub,
                                    IEnumerable<IActor> actors,
                                    IActorHostManager actorHostManager)
        {
            this.leaseProvider = leaseProvider;
            this.messageRouter = messageRouter;
            this.messageHub = messageHub;
            this.actors = actors;
            this.actorHostManager = actorHostManager;
        }

        public bool Start(TimeSpan startTimeout)
        {
            messageRouter.Start();
            TimeSpan.FromMilliseconds(300).Sleep();
            messageHub.Start();
            actors.ForEach(a => actorHostManager.AssignActor(a));
            return leaseProvider.Start(startTimeout);
        }

        public void Stop()
        {
            leaseProvider.Stop();
            messageHub.Stop();
            messageRouter.Stop();
            actorHostManager.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using kino.Actors;
using kino.Client;
using kino.Core.Connectivity;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public class LeaseProviderService : ILeaseProviderService
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageRouter messageRouter;
        private readonly IMessageHub messageHub;
        private readonly IEnumerable<IActor> actors;
        private readonly IActorHostManager actorHostManager;

        public LeaseProviderService(ILeaseProvider leaseProvider,
                                    IMessageRouter messageRouter,
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
            if (messageRouter.Start(startTimeout))
            {
                //Thread.Sleep(TimeSpan.FromMilliseconds(300));
                messageHub.Start();
                actors.ForEach(a => actorHostManager.AssignActor(a));
                return leaseProvider.Start(startTimeout);
            }

            return false;
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
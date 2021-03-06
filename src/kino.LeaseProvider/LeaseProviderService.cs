﻿using System;
using kino.Client;

namespace kino.LeaseProvider
{
    public partial class LeaseProviderService : IDisposable
    {
        private readonly IDependencyResolver resolver;
        private ILeaseProvider leaseProvider;
        private kino kino;
        private bool isStarted;
        private IMessageHub messageHub;

        public LeaseProviderService(IDependencyResolver resolver)
            => this.resolver = resolver;

        public LeaseProviderService()
            : this(null)
        {
        }

        public bool Start(TimeSpan startTimeout)
        {
            AssertDependencyResolverSet();

            Build();

            kino.Start();
            messageHub.Start();
            TimeSpan.FromMilliseconds(300).Sleep();
            isStarted = leaseProvider.Start(startTimeout);

            return isStarted;
        }

        public void Stop()
        {
            leaseProvider.Stop();
            messageHub.Start();
            kino.Stop();

            isStarted = false;
        }

        public void Dispose()
        {
            kino.Dispose();
        }

        private void AssertDependencyResolverSet()
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver), "Dependency resolver is not assigned!");
            }
        }
    }
}
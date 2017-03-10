using System;
using System.Collections.Generic;
using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface ILeaseProvider
    {
        Lease GetLease(Instance instance, GetLeaseRequest leaseRequest);

        RegistrationResult RegisterInstance(Instance instance);

        IEnumerable<Instance> GetRegisteredInstances();

        bool Start(TimeSpan startTimeout);

        void Stop();
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Framework;
using kino.Core.Messaging;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;

namespace kino.LeaseProvider.Actors
{
    public class InstanceDiscoveryActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;
        private readonly byte[] clusterName;

        public InstanceDiscoveryActor(ILeaseProvider leaseProvider,
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
                           Message = MessageDefinition.Create<InternalDiscoverLeaseProviderInstancesRequestMessage>(clusterName),
                           Handler = InternalDiscoverLeaseProviderInstancesRequest
                       },
                       new MessageHandlerDefinition
                       {
                           Message = MessageDefinition.Create<InternalDiscoverLeaseProviderInstancesResponseMessage>(clusterName),
                           Handler = InternalDiscoverLeaseProviderInstancesResponse
                       }
                   };
        }

        private async Task<IActorResult> InternalDiscoverLeaseProviderInstancesRequest(IMessage message)
        {
            var instances = leaseProvider.GetRegisteredInstances();
            if (instances.Any())
            {
                var response = Message.Create(new InternalDiscoverLeaseProviderInstancesResponseMessage
                                              {
                                                  Instances = instances.Select(i => i.ToString()).ToList(),
                                                  Partition = clusterName
                                              },
                                              DistributionPattern.Broadcast);

                return new ActorResult(response);
            }

            return null;
        }

        private async Task<IActorResult> InternalDiscoverLeaseProviderInstancesResponse(IMessage message)
        {
            var payload = message.GetPayload<InternalDiscoverLeaseProviderInstancesResponseMessage>();

            foreach (var instance in payload.Instances)
            {
                leaseProvider.RegisterInstance(new Instance(instance));
            }

            return null;
        }
    }
}
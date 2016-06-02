using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Connectivity;
using kino.Core.Framework;
using kino.Core.Messaging;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using Node = kino.LeaseProvider.Messages.Node;

namespace kino.LeaseProvider
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
                       },
                       new MessageHandlerDefinition
                       {
                           Message = MessageDefinition.Create<CreateLeaseProviderInstanceRequestMessage>(clusterName),
                           Handler = CreateLeaseProviderInstance
                       },
                       new MessageHandlerDefinition
                       {
                           Message = MessageDefinition.Create<InternalCreateLeaseProviderInstanceRequestMessage>(clusterName),
                           Handler = InternalCreateLeaseProviderInstance
                       },
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
                                                        : null,
                                            Partition = clusterName
                                        });

            return new ActorResult(result);
        }

        private bool RequestorWonTheLease(Node requestor, Node leaseOwner)
            => requestor.Uri == leaseOwner?.Uri && Unsafe.Equals(requestor.Identity, leaseOwner?.Identity);

        public async Task<IActorResult> CreateLeaseProviderInstance(IMessage message)
        {
            var payload = message.GetPayload<CreateLeaseProviderInstanceRequestMessage>();

            var res = leaseProvider.RegisterInstance(new Instance(payload.Instance));

            var response = Message.Create(new CreateLeaseProviderInstanceResponseMessage
                                          {
                                              Instance = payload.Instance,
                                              ActivationWaitTime = res.ActivationWaitTime,
                                              Partition = clusterName
                                          });
            var broadcastRequest = Message.Create(new InternalCreateLeaseProviderInstanceRequestMessage
                                                  {
                                                      Instance = payload.Instance,
                                                      Partition = clusterName
                                                  },
                                                  DistributionPattern.Broadcast);

            return new ActorResult(broadcastRequest, response);
        }

        public Task<IActorResult> InternalCreateLeaseProviderInstance(IMessage message)
        {
            var payload = message.GetPayload<InternalCreateLeaseProviderInstanceRequestMessage>();

            leaseProvider.RegisterInstance(new Instance(payload.Instance));

            return null;
        }

        public async Task<IActorResult> InternalDiscoverLeaseProviderInstancesRequest(IMessage message)
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

        public async Task<IActorResult> InternalDiscoverLeaseProviderInstancesResponse(IMessage message)
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
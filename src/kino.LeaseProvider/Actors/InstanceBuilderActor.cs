using System.Collections.Generic;
using System.Threading.Tasks;
using kino.Actors;
using kino.Core.Framework;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using kino.Messaging;

namespace kino.LeaseProvider.Actors
{
    public class InstanceBuilderActor : Actor
    {
        private readonly ILeaseProvider leaseProvider;
        private readonly IMessageSerializer messageSerializer;
        private readonly byte[] clusterName;

        public InstanceBuilderActor(ILeaseProvider leaseProvider,
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
                           Message = MessageDefinition.Create<CreateLeaseProviderInstanceRequestMessage>(clusterName),
                           Handler = CreateLeaseProviderInstance
                       },
                       new MessageHandlerDefinition
                       {
                           Message = MessageDefinition.Create<InternalCreateLeaseProviderInstanceRequestMessage>(clusterName),
                           Handler = InternalCreateLeaseProviderInstance
                       }
                   };
        }

        private async Task<IActorResult> CreateLeaseProviderInstance(IMessage message)
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

        private Task<IActorResult> InternalCreateLeaseProviderInstance(IMessage message)
        {
            var payload = message.GetPayload<InternalCreateLeaseProviderInstanceRequestMessage>();

            leaseProvider.RegisterInstance(new Instance(payload.Instance));

            return null;
        }
    }
}
using System.Threading.Tasks;
using kino.Actors;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Connectivity;
using kino.Core.Diagnostics;
using kino.Core.Messaging;
using kino.LeaseProvider.Messages;

namespace kino.LeaseProvider
{
    public class LeaseProviderActor : Actor
    {
        private readonly IRoundBasedRegister register;
        private readonly IBallotGenerator ballotGenerator;
        private readonly LeaseConfiguration config;
        private readonly ISynodConfiguration synodConfig;
        private readonly ILogger logger;

        public LeaseProviderActor(IRoundBasedRegister register,
                                  IBallotGenerator ballotGenerator,
                                  LeaseConfiguration config,
                                  ISynodConfiguration synodConfig,
                                  ILogger logger)
        {
            this.register = register;
            this.ballotGenerator = ballotGenerator;
            this.config = config;
            this.synodConfig = synodConfig;
            this.logger = logger;
        }

        [MessageHandlerDefinition(typeof(GetLeaseMessage))]
        public Task<ActorResult> GetLease(IMessage message)
        {
        }
    }
}
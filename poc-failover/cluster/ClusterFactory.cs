using System.Linq;

namespace poc_failover
{
    public class NodeFactory 
    {
        private MessageBus _bus;
        private HeartbeatPolicy _heartbeatPolicy;
        private Randomizer _randomizer;

        public NodeFactory() 
        {
            _bus =  new MessageBus();
            _heartbeatPolicy = new HeartbeatPolicy();
            _randomizer = new Randomizer();
        }

        public Node CreateActiveNode(string serverId)
        {
            return new ActiveNode(serverId,
                CreateHeartbeatWatcher(),
                new HeartbeatGenerator(_heartbeatPolicy, _bus)
            );
        }

        public Cluster CreateCluster() {
            return new Cluster(CreateHeartbeatWatcher());
        }


        private HeartbeatWatcher CreateHeartbeatWatcher()
        {
            return new HeartbeatWatcher(_bus.GetMessageStream<HeartbeatMessage>(), _heartbeatPolicy);
        }

        public Node CreatePassiveNode(string serverId) {
             return new PassiveNode(serverId, 
                CreateHeartbeatWatcher(), 
                new HeartbeatGenerator(_heartbeatPolicy, _bus)
             );
        }
    }
}

using System;
using System.Linq;
using System.Reactive.Linq;

namespace poc_failover
{
    public class ClusterFactory 
    {
        private MessageBus _bus;
        private HeartbeatPolicy _heartbeatPolicy;
        private NetworkRandomizer _randomizer;

        public ClusterFactory() 
        {
              _randomizer = new NetworkRandomizer();
            _bus =  new MessageBus(_randomizer);
            _heartbeatPolicy = new HeartbeatPolicy();
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
            return new HeartbeatWatcher(_bus.GetMessageStream<HeartbeatMessage>().Delay(_randomizer.GetRandomDelay()), _heartbeatPolicy);
        }

        public Node CreatePassiveNode(string serverId) {
             return new PassiveNode(serverId, 
                CreateHeartbeatWatcher(), 
                new HeartbeatGenerator(_heartbeatPolicy, _bus)
             );
        }
    }
}

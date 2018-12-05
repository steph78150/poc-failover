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

        public INode CreateActiveNode(string serverId) {
            return new ActiveNode(serverId, 
                new HeartbeatWatcher(_bus.GetMessageStream<HeartbeatMessage>(), _heartbeatPolicy), 
                new HeartbeatGenerator(_heartbeatPolicy, _bus)
            );
        }

        public INode CreatePassiveNode(string serverId) {
             return new PassiveNode(serverId, 
                new HeartbeatWatcher(_bus.GetMessageStream<HeartbeatMessage>(), _heartbeatPolicy), 
                new HeartbeatGenerator(_heartbeatPolicy, _bus)
             );
        }
    }
}

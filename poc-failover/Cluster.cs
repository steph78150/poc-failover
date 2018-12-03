using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace poc_failover
{
    public class Cluster 
    {
        private readonly string[] _serverIds;

        private readonly Node[] _nodes; 

        public Cluster(int numberOfServers) {
            var bus = new MessageBus();
            var heartbeatPolicy = new HeartbeatPolicy();
            var randomizer = new Randomizer();
            _serverIds = Enumerable.Range(1, numberOfServers)
                .Select(index => "srv" + index).ToArray();
            _nodes = _serverIds
            .Select(id => new Node(new HeartbeatGenerator(heartbeatPolicy, randomizer, bus), id))
            .ToArray();

            var watcher = new HeartbeatWatcher(bus.GetMessageStream<HeartbeatMessage>(), heartbeatPolicy);;
        }

        public Node FindNode(string id) 
        {
            var process = _nodes.SingleOrDefault(p => p.Id == id);
            return process ?? throw new ArgumentException($"Node '{id}' does not exist");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Cluster nodes :");
            foreach (var node in _nodes)
            {
                sb.AppendLine($"\t{node}");
            }
            return sb.ToString();
        }
    }
}

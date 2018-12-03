using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace poc_failover
{

    public class Cluster 
    {
        private readonly string[] _serverIds;

        private readonly Node[] _processes; 

        public Cluster(int numberOfServers) {
            var bus = new MessageBus();
            var heartbeatPolicy = new HeartbeatPolicy();
            var randomizer = new Randomizer();
            _serverIds = Enumerable.Range(1, numberOfServers)
                .Select(index => "srv" + index).ToArray();
            _processes = _serverIds
            .Select(id => new Node(bus, new HeartbeatGenerator(heartbeatPolicy, randomizer), id))
            .ToArray();

            var watcher = new HeartbeatWatcher(bus.GetMessageStream().OfType<HeartbeatMessage>(), heartbeatPolicy);;
        }

        public Node FindProcess(string id) 
        {
            var process = _processes.SingleOrDefault(p => p.Id == id);
            return process ?? throw new ArgumentException($"Process '{id}' does not exist");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current cluster state :");
            foreach (var proc in _processes)
            {
                sb.AppendLine($"\t{proc}");
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace poc_failover
{


    public class Cluster : IDisposable
    {
        private readonly IList<Node> _nodes = new List<Node>();
        private readonly MessageBus _bus;
        private readonly HeartbeatPolicy _heartbeatPolicy;
        private readonly Randomizer _randomizer;

        public Cluster(MessageBus bus, HeartbeatPolicy policy, Randomizer randomizer) 
        {
            this._bus = bus;
            this._heartbeatPolicy = policy;
            this._randomizer = randomizer;
        }

        private Node CreateNode(string serverId) 
        {
            var generator = new HeartbeatGenerator(_heartbeatPolicy, _randomizer, _bus);
            var heartbeatMessages = _bus.GetMessageStream<HeartbeatMessage>().Delay(_randomizer.GetRandomDelay(_heartbeatPolicy.NetworkDelay));
            var watcher = new HeartbeatWatcher(heartbeatMessages, _heartbeatPolicy);
            return new Node(generator, new ClusterWatcher(serverId, watcher), serverId);
        }

        public void AddNode(string serverId)
        {
            var node = CreateNode(serverId);
            _nodes.Add(node); 
            node.Start();
        }

        public void StartNode(string serverId) 
        {
            var found = FindNode(serverId);
            found.Start();
        }

        public void StopNode(string serverId) 
        {
            var found = FindNode(serverId);
            found.Stop();
        }

        public void RemoveNode(string serverId) 
        {
            var found = FindNode(serverId);
            _nodes.Remove(found);
            found.Dispose();
        }

        public void Dispose() 
        {
            foreach (var node in this._nodes) {
                node.Dispose();
            }
        }

        private Node FindNode(string id) 
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

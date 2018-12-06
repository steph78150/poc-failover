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
        private readonly IHeartbeatWatcher _watcher;

        public Cluster(IHeartbeatWatcher watcher) 
        {
            this._watcher = watcher;
        }

        public void AddNode(Node node)
        {
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
            _watcher.Dispose();
        }

        private Node FindNode(string id) 
        {
            var node = _nodes.SingleOrDefault(p => p.RealServerName == id);
            return node ?? throw new ArgumentException($"Node '{id}' does not exist");
        }

        private int ExpectedActiveNodes => _nodes.OfType<ActiveNode>().Count();

        private bool IsHealthy => _watcher.ActiveNodeCount == ExpectedActiveNodes;

        private string HealthCheck => IsHealthy ? "HEALTHY" : $"KO ({_watcher.ActiveNodeCount}/{ExpectedActiveNodes})";

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Cluster nodes : " + HealthCheck );
            foreach (var node in _nodes)
            {
                sb.AppendLine($"\t{node} => {node.CurrentIdentity ?? "<WAITING>"}");
                
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace poc_failover
{
    public class Cluster : IDisposable
    {
        private readonly IList<INode> _nodes = new List<INode>();

        public Cluster() 
        {
        }

        public void AddNode(INode node)
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
        }

        private INode FindNode(string id) 
        {
            var process = _nodes.SingleOrDefault(p => p.ServerName == id);
            return process ?? throw new ArgumentException($"Node '{id}' does not exist");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Cluster nodes :");
            foreach (var node in _nodes)
            {
                sb.AppendLine($"\t{node} => {node.CurrentIdentity ?? "IDLE"}");
            }
            return sb.ToString();
        }
    }
}

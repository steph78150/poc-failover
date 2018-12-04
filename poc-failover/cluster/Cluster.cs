using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace poc_failover
{

    public interface ICluster {
        int NumberOfNodes {get;}

        int Quorum {get;}
    }

    public class Cluster : IDisposable, ICluster
    {
        private readonly IList<Node> _nodes;
        private readonly MessageBus _bus;
        private readonly TimingPolicy _timingPolicy;
        private readonly Randomizer _randomizer;

        public int NumberOfNodes => _nodes.Count;

        public int Quorum => (NumberOfNodes / 2) + 1;

        public Cluster(MessageBus bus, TimingPolicy policy, Randomizer randomizer, IList<string> serverIds) 
        {
            this._bus = bus;
            this._timingPolicy = policy;
            this._randomizer = randomizer;
            _nodes = serverIds.Select(id => CreateNode(id, serverIds.Count)).ToList();
        }

        private Node CreateNode(string serverId, int totalNumberOfNodes) 
        {
            var state = ElectionState.Empty();
            var generator = new HeartbeatGenerator(_timingPolicy, _randomizer, _bus);
            var heartbeatMessages = _bus.GetMessageStream<HeartbeatMessage>().Delay(_randomizer.GetRandomDelay(_timingPolicy.NetworkDelay));
            var watcher = new HeartbeatWatcher(heartbeatMessages, _timingPolicy);
            var voter = new ElectionVoter(_bus, _bus.GetMessageStream<CandidateMessage>(), state, serverId);
            var candidate = new ElectionCandidate(_bus, _bus.GetMessageStream<VoteMessage>(), state, this, _timingPolicy);
            return new Node(serverId, generator, watcher, candidate);
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

using System;

namespace poc_failover
{
    public class ClusterWatcher : IDisposable
    {
        private readonly string _name;
        private readonly IHeartbeatWatcher _heartbeatWatcher;

        public ClusterWatcher(string name, IHeartbeatWatcher heartbeatWatcher) 
        {
            this._name = name;
            this._heartbeatWatcher = heartbeatWatcher;
            heartbeatWatcher.NodeJoined += OnNodeJoined;
            heartbeatWatcher.NodeLeft += OnNodeLeft;
        }

        public void Dispose() 
        {
            this._heartbeatWatcher.NodeJoined -= OnNodeJoined;
            this._heartbeatWatcher.NodeLeft -= OnNodeLeft;
        }

        private void OnNodeLeft(object sender, string serverId)
        {
            Console.Out.WriteLine($"{_name} noticed that node {serverId} has left the cluster");
        }

        private void OnNodeJoined(object sender, string serverId)
        {
            Console.Out.WriteLine($"{_name} noticed that node {serverId} has joined the cluster");
        }
    }
}

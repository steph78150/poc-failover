using System;

namespace poc_failover
{

    public class ActiveNode : INode
    {
        private readonly string _serverName;

        private readonly IHeartbeatWatcher _heartbeatWatcher;
        private readonly IHeartbeatGenerator _heartbeatGenerator;

        private IDisposable _heartbeats = null;

        private string _backupServerName;

        public ActiveNode(string serverName, IHeartbeatWatcher heartbeatWatcher, 
            IHeartbeatGenerator heartbeatGenerator) 
        {
            this._serverName = serverName;
            this._heartbeatWatcher = heartbeatWatcher;
            this._heartbeatGenerator = heartbeatGenerator;

            heartbeatWatcher.NodeJoined += OnNodeJoined;
            heartbeatWatcher.NodeLeft += OnNodeLeft;
        }

        public void Start() 
        {
            if (CurrentState == State.Running) 
            {
                throw new InvalidOperationException("Node is already started");
            }

            var msg = new HeartbeatMessage { Sender = _serverName, CurrentIdentity = _serverName };
            _heartbeats = _heartbeatGenerator.StartSendingHeartbearts(msg);
        }

        public string ServerName => _serverName;

        public string CurrentIdentity => CurrentState ==  State.Stopped ? null : _serverName;

        private State CurrentState 
        {
            get 
            {
                if (_heartbeats != null) return State.Running;
                return _backupServerName != null ? State.Stopped : State.Stopping;
            }
        }

        public void Dispose() 
        {
            this._heartbeatWatcher.NodeJoined -= OnNodeJoined;
            this._heartbeatWatcher.NodeLeft -= OnNodeLeft;
            StopHeartbeats();
        }

        public void Stop()
        {
            if (CurrentState != State.Running)
            {
                throw new InvalidOperationException("Node is already stopped");
            }
            StopHeartbeats();
        }

        private void StopHeartbeats()
        {
            _heartbeats.Dispose();
            _heartbeats = null;
        }

        private void OnNodeLeft(object sender, string serverId)
        {
            if (serverId == _serverName) return;
            Console.Out.WriteLine($"{_serverName} noticed that node {serverId} has left the cluster");
            if (_backupServerName == serverId) {
                _backupServerName = null;
                Console.Out.WriteLine($"Node {_serverName} had no longer any backup");
            }
        }

        private void OnNodeJoined(object sender, HeartbeatMessage message)
        {
            if (message.Sender == _serverName) return;
            Console.Out.WriteLine($"{_serverName} noticed that node {message.Sender} has joined the cluster");
            if (message.CurrentIdentity == _serverName) {
                 _backupServerName = message.Sender;
                 Console.Out.WriteLine($"Node {_serverName} knows that it is backuped by {_backupServerName} and can safely stop now");
            }
        }

        public override string ToString() => $"Node {_serverName} is {this.CurrentState}";
    }
}

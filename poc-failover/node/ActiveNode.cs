using System;

namespace poc_failover
{

    public class ActiveNode : Node
    {
    
        private readonly IHeartbeatGenerator _heartbeatGenerator;

        private IDisposable _heartbeats = null;

        private string _backupServerName;

        public ActiveNode(string serverName, IHeartbeatWatcher heartbeatWatcher, IHeartbeatGenerator heartbeatGenerator) 
        : base(serverName, heartbeatWatcher)
        {
            this._heartbeatGenerator = heartbeatGenerator;
        }

        public override void Start() 
        {
            if (CurrentState == State.Running) 
            {
                throw new InvalidOperationException("Node is already started");
            }

            var msg = new HeartbeatMessage { Sender = RealServerName, CurrentIdentity = RealServerName};
            _heartbeats = _heartbeatGenerator.StartSendingHeartbearts(msg);
        }

      
        public override string CurrentIdentity => CurrentState ==  State.Stopped ? null : RealServerName;

        private State CurrentState 
        {
            get 
            {
                if (_heartbeats != null) return State.Running;
                return _backupServerName != null ? State.Stopped : State.Stopping;
            }
        }

        public override void Dispose() 
        {
            base.Dispose();
            StopHeartbeats();
        }

        public override void Stop()
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

        protected override void OnNodeLeft(object sender, string serverId)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {serverId} has left the cluster");
            if (_backupServerName == serverId) {
                _backupServerName = null;
                Console.Out.WriteLine($"Node {RealServerName} had no longer any backup");
            }
        }

        protected override void OnNodeJoined(object sender, HeartbeatMessage message)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {message.Sender} has joined the cluster");
            if (message.CurrentIdentity == RealServerName) {
                 _backupServerName = message.Sender;
                 Console.Out.WriteLine($"Node {RealServerName} knows that it is backuped by {_backupServerName} and can safely stop now");
            }
        }

        public override string ToString() => $"Node {RealServerName} is {this.CurrentState}";
    }
}

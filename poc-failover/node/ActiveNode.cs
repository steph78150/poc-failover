using System;
using System.Diagnostics;

namespace poc_failover
{

    public class ActiveNode : Node
    {
    
        private readonly IHeartbeatGenerator _heartbeatGenerator;

        private IDisposable _heartbeats = null;

        private string _backupServerName;

        private Stopwatch _stopping = null;

        public ActiveNode(string serverName, IHeartbeatWatcher heartbeatWatcher, 
            IHeartbeatGenerator heartbeatGenerator) 
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
            _stopping = null;
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
            _stopping = null;
        }

        public override void Stop()
        {
            _stopping = Stopwatch.StartNew();
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

        protected override void OnNodeLeft(object sender, HeartbeatMessage msg)
        {
            if (_backupServerName == msg.Sender && msg.CurrentIdentity == RealServerName) {
                _backupServerName = null;
                Console.Out.WriteLine($"Node {RealServerName} had no longer any backup");
            }
        }

        protected override void OnNodeJoined(object sender, HeartbeatMessage message)
        {
             if (message.CurrentIdentity == RealServerName) {
                 _backupServerName = message.Sender;
                 Console.Out.WriteLine($"Node {RealServerName} knows that it is backuped by {_backupServerName} and can safely stop now after having to wait for {_stopping.Elapsed.TotalMilliseconds} ms");
            }
        }

        public override string ToString() => $"Node {RealServerName} is {this.CurrentState}";
    }
}

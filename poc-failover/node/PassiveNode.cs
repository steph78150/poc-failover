using System;
using System.Collections.Concurrent;
using System.Linq;

namespace poc_failover
{
    public class PassiveNode : Node, INode
    {
        private readonly PassiveNodeState _state;

        private readonly ConcurrentDictionary<string, string> _deadServersQueue = new ConcurrentDictionary<string, string>();

        public PassiveNode(string serverName, IHeartbeatWatcher heartbeatWatcher, IHeartbeatGenerator heartbeatGenerator) 
        : base(serverName, heartbeatWatcher)
        {
            _state = new PassiveNodeState(heartbeatGenerator);
        }

        public  override string CurrentIdentity => _state.CurrentIdentity;

        public override void Stop() 
        {
            _state.StopBackuping();
        }

        protected override void OnNodeLeft(object sender, HeartbeatMessage msg)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {msg} has left the cluster");
            if (_state.TryStartBackuping(msg.Sender, RealServerName))
            {
                Console.Out.WriteLine($"{RealServerName} will now act as backup for {msg.Sender}");
            }
            else
            {
                _deadServersQueue.TryAdd(msg.Sender, msg.Sender);
                Console.Out.WriteLine($"S{RealServerName} can not backup server {msg.Sender} because it is already busy backuping for {CurrentIdentity}");
            }
        }
    
        protected override void OnNodeJoined(object sender, HeartbeatMessage message)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {message} has joined the cluster");
            if (_state.TryStopBackuping(message.Sender))
            {
                Console.Out.WriteLine($"Spare node {RealServerName} no longer need to backup {CurrentIdentity} because it is back");
                var nextDeadServer = _deadServersQueue.Keys.FirstOrDefault();
                if (nextDeadServer != null && _deadServersQueue.TryRemove(nextDeadServer, out var _)) {
                    if (_state.TryStartBackuping(nextDeadServer, RealServerName)) {
                        Console.Out.WriteLine($"{RealServerName} will now act as backup for {nextDeadServer}");
                    }
                }
            } else {
                _deadServersQueue.TryRemove(message.Sender, out var _);
            }
        }

        private string QueueText => _deadServersQueue.Count > 0 ? string.Join(",", _deadServersQueue.Keys.ToArray()) : "empty";

    
        public override string ToString() => $"Spare node {RealServerName} is {_state.StateText} (Queue is {QueueText})";


        public class PassiveNodeState 
        {
       
            public class BackupingNode {
                private readonly IDisposable _heartbeats = null;

                private readonly string _masqueradingAsServerName = null;

                public BackupingNode(string masqueradingAsServerName, IDisposable heartbeats)
                {
                    _masqueradingAsServerName = masqueradingAsServerName;
                    _heartbeats = heartbeats ?? throw new ArgumentNullException(nameof(heartbeats));
                }

                public void Dispose() {
                    _heartbeats.Dispose();
                }


                public string MasqueradingAsServerName => _masqueradingAsServerName;
            }

            private readonly IHeartbeatGenerator _heartbeatGenerator;

            private BackupingNode _backup = null;

            private object _lock = new object();

            public string CurrentIdentity
            {
                get
                {
                      // TODO : use very fast locking mechanism here
                      
                    lock (_lock) {
                        return _backup?.MasqueradingAsServerName;
                    }
                }
            }

            public PassiveNodeState(IHeartbeatGenerator heartbeatGenerator)
            {
                this._heartbeatGenerator = heartbeatGenerator;
            }

            private bool IsFree()
            {
                return _backup == null;;
            }

            public string StateText
            {
                get
                {
                    lock (_lock) {
                        return IsFree() ? "Idle" : $"Backuping node {CurrentIdentity}";
                    }
                }
            }

            public bool TryStartBackuping(string masqueradingAsServerName, string realServerName)
            {
                lock (_lock) 
                {
                    if (IsFree()) {
                        
                        var msg = new HeartbeatMessage { Sender = realServerName, CurrentIdentity = masqueradingAsServerName };
                        var heartbeats = _heartbeatGenerator.StartSendingHeartbearts(msg);
                        _backup = new BackupingNode(masqueradingAsServerName, heartbeats);
                        return true;
                    }
                    return false;
                }
            }

            public bool TryStopBackuping(string serverName)
            {
                lock (_lock) 
                {
                    if (_backup?.MasqueradingAsServerName == serverName)
                    {
                        StopBackuping();
                        return true;
                    }
                    return false;
                }
            }

            public void StopBackuping()
            {
                _backup?.Dispose();
                _backup = null;
            }
        }

    }
}

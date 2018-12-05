using System;

namespace poc_failover
{
    public class PassiveNode : Node, INode
    {
        private readonly PassiveNodeState _state;

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

        protected override void OnNodeLeft(object sender, string serverId)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {serverId} has left the cluster");
            if (_state.TryStartBackuping(serverId, RealServerName))
            {
                Console.Out.WriteLine($"{RealServerName} will now act as backup for {serverId}");
            }
            else
            {
                Console.Out.WriteLine($"S{RealServerName} can not backup server {serverId} because it is already busy backuping for {CurrentIdentity}");
            }
        }
    
        protected override void OnNodeJoined(object sender, HeartbeatMessage message)
        {
            Console.Out.WriteLine($"{RealServerName} noticed that node {message.Sender} has joined the cluster");
            if (_state.TryStopBackuping(message.Sender))
            {
                Console.Out.WriteLine($"Spare node {RealServerName} no longer need to backup {CurrentIdentity} because it is back");
            }
        }

    
        public override string ToString() => $"Spare node {RealServerName} is {_state.StateText}";


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

            private bool IsBackupingNode(string serverId)
            {
                return _backup != null && _backup.MasqueradingAsServerName == serverId;
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
                    if (IsBackupingNode(serverName))
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

using System;

namespace poc_failover
{

    public class PassiveNode : INode
    {
        private readonly string _realserverName;

        private readonly IHeartbeatWatcher _heartbeatWatcher;
        private readonly IHeartbeatGenerator _heartbeatGenerator;

        private IDisposable _heartbeats = null;

        private string _masqueradingAsServerName = null;

        public PassiveNode(string serverName, IHeartbeatWatcher heartbeatWatcher, 
            IHeartbeatGenerator heartbeatGenerator) 
        {
            this._realserverName = serverName;
            this._heartbeatWatcher = heartbeatWatcher;
            this._heartbeatGenerator = heartbeatGenerator;

            heartbeatWatcher.NodeJoined += OnNodeJoined;
            heartbeatWatcher.NodeLeft += OnNodeLeft;
        }

        public void Start() 
        {
        }

        public string CurrentIdentity => _masqueradingAsServerName;

        public string ServerName => _realserverName;

        public void Dispose() 
        {
            this._heartbeatWatcher.NodeJoined -= OnNodeJoined;
            this._heartbeatWatcher.NodeLeft -= OnNodeLeft;
        }

        public void Stop() 
        {
           if (!IsFree()) StopBackuping();
        }

        private void OnNodeLeft(object sender, string serverId)
        {
            if (serverId == _realserverName) return;
           
            Console.Out.WriteLine($"{_realserverName} noticed that node {serverId} has left the cluster");
            if (IsFree())
            {
                Console.Out.WriteLine($"Spare node {_realserverName} will now act as backup for {serverId}");
                StartBackuping(serverId);
            }
            else
            {
                Console.Out.WriteLine($"Spare node {_realserverName} can not backup server {serverId} because it is already busy backuping for {_masqueradingAsServerName}");
            }
        }
    
        private void OnNodeJoined(object sender, HeartbeatMessage message)
        {
             if (message.Sender == _realserverName) return;

            Console.Out.WriteLine($"{_realserverName} noticed that node {message.Sender} has joined the cluster");
            if (IsBackupingNode(message.Sender))
            {
                Console.Out.WriteLine($"Spare node {_realserverName} no longer need to backup {_masqueradingAsServerName} because it is back");
                StopBackuping();
            }
        }

        private bool IsBackupingNode(string serverId)
        {
            return _heartbeats != null && _masqueradingAsServerName == serverId;
        }

        private bool IsFree()
        {
            return _heartbeats == null && _masqueradingAsServerName == null;
        }

        private void StartBackuping(string serverId)
        {
            _masqueradingAsServerName = serverId;
            var msg = new HeartbeatMessage { Sender = _realserverName, CurrentIdentity = _masqueradingAsServerName };
            _heartbeats = _heartbeatGenerator.StartSendingHeartbearts(msg);
        }

        private void StopBackuping()
        {
            _heartbeats.Dispose();
            _heartbeats = null;
            _masqueradingAsServerName = null;
        }

        public override string ToString() => $"Spare node {_realserverName} is {(IsFree() ? "FREE" : "BACKUPING " + _masqueradingAsServerName)}";
    }
}

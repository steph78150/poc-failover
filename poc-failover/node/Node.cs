﻿namespace poc_failover
{
    public abstract class Node 
    {

        private readonly string _realserverName;

        private readonly IHeartbeatWatcher _heartbeatWatcher;
    

        public string RealServerName => _realserverName;

        public abstract string CurrentIdentity {get;}

        protected  Node(string serverName, IHeartbeatWatcher heartbeatWatcher) 
        {
            this._realserverName = serverName;
            this._heartbeatWatcher = heartbeatWatcher;

            heartbeatWatcher.NodeJoined += NodeJoined;
            heartbeatWatcher.NodeLeft += NodeLeft;
        }

        public virtual void Dispose() 
        {
            this._heartbeatWatcher.NodeJoined -= NodeJoined;
            this._heartbeatWatcher.NodeLeft -= NodeLeft;
            this._heartbeatWatcher.Dispose();
        }

        private void NodeLeft(object sender, string serverId) 
        {
            if (serverId == RealServerName) return;
            OnNodeLeft(sender, serverId);
        }
        private void NodeJoined(object sender, HeartbeatMessage msg) 
        {
            if (msg.Sender == RealServerName) return;
            OnNodeJoined(sender,msg );
        }

        protected abstract void OnNodeLeft(object sender, string e);
        protected abstract void OnNodeJoined(object sender, HeartbeatMessage e);

        public virtual void Start()
        {  
        }

        public virtual void Stop()
        {
        }
    }
}

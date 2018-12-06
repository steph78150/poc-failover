using System;

namespace poc_failover
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

        private void NodeLeft(object sender, HeartbeatMessage msg) 
        {
            if (msg.Sender == RealServerName) return;
            Console.Out.WriteLine($"{RealServerName} noticed that node {msg} had left the cluster at {DateTime.Now.TimeOfDay}");
            OnNodeLeft(sender, msg);
        }
        private void NodeJoined(object sender, HeartbeatMessage msg) 
        {
            if (msg.Sender == RealServerName) return;
            Console.Out.WriteLine($"{RealServerName} noticed that node {msg} had joined the cluster at {DateTime.Now.TimeOfDay}");
            OnNodeJoined(sender,msg );
        }

        protected abstract void OnNodeLeft(object sender, HeartbeatMessage heartbeat);
        protected abstract void OnNodeJoined(object sender, HeartbeatMessage heartbeat);

        public virtual void Start()
        {  
        }

        public virtual void Stop()
        {
        }
    }
}

using System.Linq;

namespace poc_failover
{
    public static class ClusterFactory 
    {
        public static Cluster Create(int numberOfServers) 
        {
            var bus = new MessageBus();
            var heartbeatPolicy = new HeartbeatPolicy();
            var randomizer = new Randomizer();
            var cluster = new Cluster(bus, heartbeatPolicy, randomizer);

            foreach (var serverId in Enumerable.Range(1, numberOfServers).Select(index => "server_" + index)) 
            {
                cluster.AddNode(serverId);
            }

            return cluster;
        }
    }
}

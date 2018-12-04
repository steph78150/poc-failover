using System.Linq;

namespace poc_failover
{
    public static class ClusterFactory 
    {
        public static Cluster Create(int numberOfServers) 
        {
            var bus = new MessageBus();
            var randomizer = new Randomizer();
            var heartbeatPolicy = new TimingPolicy(randomizer);

            var serverIds = Enumerable.Range(1, numberOfServers)
                .Select(index => "server_" + index)
                .ToArray(); 
           
            var cluster = new Cluster(bus, heartbeatPolicy, randomizer, serverIds);
            return cluster;
        }
    }
}

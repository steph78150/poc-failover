using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public class HeartbeatGenerator 
    {
        private readonly HeartbeatPolicy _policy;
        private readonly Randomizer _randomizer;

        public HeartbeatGenerator(HeartbeatPolicy policy, Randomizer randomizer) 
        {
            _policy = policy;
            _randomizer = randomizer;
        }

        public IObservable<long> GetHeartbeatStream()
         {
            return Observable
                .Interval(_policy.Interval)
                .Delay(_randomizer.GetRandomDelay(_policy.Interval));
        }
    }
}


using System;

namespace poc_failover
{
    public class HeartbeatPolicy 
    {
        private readonly Randomizer _randomizer;

        public HeartbeatPolicy(Randomizer randomizer) 
        {
            _randomizer = randomizer;
        }

        public TimeSpan NetworkDelay => TimeSpan.FromMilliseconds(15);

        public TimeSpan Interval => 3 * NetworkDelay;

        public TimeSpan ElectionTimeout => 10 * NetworkDelay;
    }
}


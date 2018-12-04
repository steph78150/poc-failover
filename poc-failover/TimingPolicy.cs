using System;

namespace poc_failover
{
    public class TimingPolicy 
    {
        private readonly Randomizer _randomizer;

        public TimingPolicy(Randomizer randomizer) 
        {
            _randomizer = randomizer;
        }

        public TimeSpan NetworkDelay => TimeSpan.FromMilliseconds(15);

        public TimeSpan HeartbeatInterval => 1 * NetworkDelay;

        public TimeSpan GetElectionTimeout()
        {
            var timeout = 10 * NetworkDelay;
            return timeout + _randomizer.GetRandomDelay(timeout);
        }

        public TimeSpan VoteTimeout => 3 * NetworkDelay;
    }
}


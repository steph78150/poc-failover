using System;

namespace poc_failover
{
    public class HeartbeatPolicy 
    {
        public TimeSpan NetworkDelay => Interval / 2;

        public TimeSpan Interval => TimeSpan.FromMilliseconds(1000);

        public TimeSpan Timeout => 3 * Interval;
    }
}


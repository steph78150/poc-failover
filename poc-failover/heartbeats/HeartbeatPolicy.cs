using System;

namespace poc_failover
{
    public class HeartbeatPolicy 
    {
        public TimeSpan NetworkDelay => TimeSpan.FromMilliseconds(15);

        public TimeSpan HeartbeatInterval => TimeSpan.FromMilliseconds(100);

        public TimeSpan Timeout => HeartbeatInterval + 5 * NetworkDelay;
    }
}


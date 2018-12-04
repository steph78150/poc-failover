using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public interface IHeartbeatGenerator
    {
        IDisposable StartSendingHeartbearts(HeartbeatMessage heartbeatMessage);
    }

    public class HeartbeatGenerator : IHeartbeatGenerator
    {
        private readonly TimingPolicy _policy;
        private readonly Randomizer _randomizer;
        private readonly IMessageBusPublisher _publisher;

        public HeartbeatGenerator(TimingPolicy policy, Randomizer randomizer, IMessageBusPublisher publisher) 
        {
            _policy = policy;
            _randomizer = randomizer;
            _publisher = publisher;
        }

        public IDisposable StartSendingHeartbearts(HeartbeatMessage heartbeat)
         {
            return Observable
                .Interval(_policy.HeartbeatInterval)
                .Delay(_randomizer.GetRandomDelay(_policy.HeartbeatInterval))
                .Select((counter) => heartbeat)
                .Subscribe(msg =>
                {
                     _publisher.Publish(msg);
                });
         }
    }
}


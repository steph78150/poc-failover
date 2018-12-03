using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public interface IHeartbeatGenerator
    {
        IDisposable StartSendingHeartbearts(string sender);
    }

    public class HeartbeatGenerator : IHeartbeatGenerator
    {
        private readonly HeartbeatPolicy _policy;
        private readonly Randomizer _randomizer;
        private readonly IMessageBusPublisher _publisher;

        public HeartbeatGenerator(HeartbeatPolicy policy, Randomizer randomizer, IMessageBusPublisher publisher) 
        {
            _policy = policy;
            _randomizer = randomizer;
            _publisher = publisher;
        }

        public IDisposable StartSendingHeartbearts(string sender)
         {
            return Observable
                .Interval(_policy.Interval)
                .Delay(_randomizer.GetRandomDelay(_policy.Interval))
                .Select((counter) =>  new HeartbeatMessage { Counter = counter, Sender = sender })
                .Subscribe(msg =>
                {
                     _publisher.Publish(msg);
                });
         }
    }
}


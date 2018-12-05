using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public interface IHeartbeatGenerator
    {
        IDisposable StartSendingHeartbearts(HeartbeatMessage heartbeatMsg);
    }

    public class HeartbeatGenerator : IHeartbeatGenerator
    {
        private readonly HeartbeatPolicy _policy;
        private readonly IMessageBusPublisher _publisher;

        public HeartbeatGenerator(HeartbeatPolicy policy, IMessageBusPublisher publisher) 
        {
            _policy = policy;
            _publisher = publisher;
        }

        public IDisposable StartSendingHeartbearts(HeartbeatMessage heartbeatMsg)
         {
            return Observable
                .Interval(_policy.HeartbeatInterval)
                .Subscribe( (_) =>
                {
                     _publisher.Publish(heartbeatMsg);
                });
         }
    }
}


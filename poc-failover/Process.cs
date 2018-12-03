using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public class Node 
    {
        private readonly IMessageBus _bus;

        public string Id {get;}

        private State _state;

        bool IsSendingHearbeats => _state == State.Running;

        public Node(IMessageBus bus, HeartbeatGenerator heartbeats, string id)
        {
            _bus = bus;
            this.Id = id;
            this._state = State.Stopped;
            var heatbeats = heartbeats.GetHeartbeatStream()
            .Select((counter) => MakeHearbeatMessage(counter))
            .Subscribe(msg =>
             {
                 if (IsSendingHearbeats)
                 {
                     _bus.Send(msg);
                 }
             });
        }

        private HeartbeatMessage MakeHearbeatMessage(long counter)
        {
            return new HeartbeatMessage { Counter = counter, Sender = this.Id };
        }

        public enum State {
            Stopped, Running,
        }

        public void Start() {
            Console.WriteLine($"Node {this.Id} is starting");
            this._state = State.Running;
        }

        public override string ToString() => $"Node {Id} is {_state}";

        public void Stop()
        {
             Console.WriteLine($"Node {this.Id} is stopping");
            this._state = State.Stopped;
        }
    }
}


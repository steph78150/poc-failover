using System;
using System.Reactive.Linq;

namespace poc_failover
{
    public class Node 
    {
        public string Id {get;}
        private IHeartbeatGenerator _generator;
        private IDisposable _heartbeats = null;
        private State _state;

        public Node(IHeartbeatGenerator generator, string id)
        {
            this.Id = id;
            this._state = State.Stopped;
            this._generator = generator;
        }

        public enum State {
            Stopped, 
            Running,
        }

        public void Start() {
            if (this._state != State.Stopped) {
                throw new InvalidOperationException("Server is already started.");
            }
            Console.WriteLine($"Node {this.Id} is starting");
            this._state = State.Running;
            _heartbeats = _generator.StartSendingHeartbearts(this.Id);
        }

        public override string ToString() => $"Node {Id} is {_state}";

        public void Stop()
        {
            if (this._state != State.Running) {
                throw new InvalidOperationException("Server is already stopped.");
            }
            Console.WriteLine($"Node {this.Id} is stopping");
            this._state = State.Stopped;
            _heartbeats?.Dispose();
        }
    }
}


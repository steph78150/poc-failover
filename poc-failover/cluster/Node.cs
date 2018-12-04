using System;
using System.Reactive.Linq;

namespace poc_failover
{

    public class Node : IDisposable
    {
        public string Id {get;}
        private readonly IHeartbeatGenerator _generator;
        private readonly ClusterWatcher _watcher;
        private IDisposable _heartbeats = null;
        private State _state;

        public Node(IHeartbeatGenerator generator, ClusterWatcher watcher, string id)
        {
            this.Id = id;
            this._state = State.Stopped;
            this._generator = generator;
            this._watcher = watcher;
        }

        public enum State {
            Stopped, 
            Passive,
            Active,
        }

        public void Start() {
            if (this._state != State.Stopped) {
                throw new InvalidOperationException("Server is already started.");
            }
            Console.WriteLine($"Node {this.Id} is starting");
            this._state = State.Passive;
            _heartbeats = _generator.StartSendingHeartbearts(this.Id);
        }

        public override string ToString() => $"Node {Id} is {_state}";

        public void Stop()
        {
            if (this._state == State.Stopped) {
                throw new InvalidOperationException("Server is already stopped.");
            }
            Console.WriteLine($"Node {this.Id} is stopping");
            this._state = State.Stopped;
            _heartbeats?.Dispose();
        }

        public void Dispose()
        {
            _heartbeats?.Dispose();
            _watcher.Dispose();
        }
    }
}


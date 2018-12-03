using System;

namespace poc_failover
{
    public interface IMessageBus
    {
        void Publish<T>();
    }

    public class Process 
    {
        public string Id {get;}

        private State _state;

        public Process(string id) {
            this.Id = id;
            this._state = State.Stopped;
        }

        public enum State {
            Stopped, Running,
        }

        public void Start() {
            Console.WriteLine($"Process {this.Id} starting");
            this._state = State.Running;
        }

        public override string ToString() => $"Process {Id} is {_state}";

        public void Stop()
        {
             Console.WriteLine($"Process {this.Id} stopping");
            this._state = State.Stopped;
        }
    }
}


using System;
using System.Reactive.Linq;

namespace poc_failover
{

    public class Node : IDisposable
    {
        public string Id {get;}
        private readonly IHeartbeatGenerator _generator;
        private readonly HeartbeatWatcher _watcher;
        private readonly ElectionCandidate _candidate;
        private IDisposable _heartbeats = null;
        private State _state;

        private IDisposable _pendingElection;

        public Node(string id, IHeartbeatGenerator generator, HeartbeatWatcher watcher, 
            ElectionCandidate candidate)
        {
            this.Id = id;
            this._state = State.Follower;
            this._generator = generator;
            this._watcher = watcher;
            this._candidate = candidate;
            _watcher.MasterDead += OnMasterDead;
            _watcher.MasterElected += OnMasterElected;
            _candidate.Win += OnElectionWin;
            _candidate.Defeat += OnElectionDefeat;

            _pendingElection = _candidate.ScheduleNextElection(this.Id);
        }   

        private void OnElectionDefeat(object sender, EventArgs e)
        {
            Console.WriteLine($"node {Id} had lost an election, triggering another one in a moment");
            _pendingElection = _candidate.ScheduleNextElection(this.Id);
        }

        private void OnElectionWin(object sender, int termId)
        {
            Console.WriteLine($"node {Id} just won an election on term {termId}");
            this.BecomeMaster(termId);
        }
        private void OnMasterElected(object sender, MasterElectedEvent e)
        {
            Console.WriteLine($"server {Id} noticed that {e.NewMaster} had been elected during term {e.Term}");
            if (_state == State.Master && e.NewMaster != this.Id)
            {
                throw new InvalidOperationException($"two master on the same cluster : {e.NewMaster} and {this.Id}");
            }
            CancelPendingElection();
        }

        private void CancelPendingElection()
        {
            if (_pendingElection != null) {
                Console.WriteLine($"canceling pending election on {Id}");
                _pendingElection?.Dispose();
                _pendingElection = null;
            } else {
                Console.WriteLine($"no pending election to cancel on node {this.Id}");
            }
        }

        private void OnMasterDead(object sender, string e)
        {
            Console.WriteLine($"server {Id} noticed that master is dead, triggering another election");
            _candidate.RunForElection(this.Id);
        }

        public enum State {
            Stopped, 
            Follower,
            Master,
        }

        public void BecomeMaster(int term) 
        {
            if (this._state == State.Stopped) 
            {
                return;
            }
            Console.WriteLine($"Node {this.Id} is now master of cluster");
            _heartbeats = _generator.StartSendingHeartbearts(new HeartbeatMessage
            {
                CurrentMaster = this.Id,
                Term = term
            });
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

        private void BecomeFollower() 
        {
            this._state = State.Follower;
             _heartbeats?.Dispose();
        }

        public void Dispose()
        {
            _heartbeats?.Dispose();
            _watcher.Dispose();

            _watcher.MasterDead -= OnMasterDead;
            _watcher.MasterElected -= OnMasterElected;
            _candidate.Win -= OnElectionWin;
            _candidate.Defeat -= OnElectionDefeat;
        }
    }
}


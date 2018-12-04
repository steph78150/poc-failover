using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace poc_failover
{
    public class MasterElectedEvent {
        public string NewMaster {get;set;}
        public int Term {get; set;}
    }
    
     public interface IHeartbeatWatcher : IDisposable
    {
        event EventHandler<MasterElectedEvent> MasterElected;

        event EventHandler<string> MasterDead;
    }

    public class HeartbeatWatcher : IHeartbeatWatcher
    {
        private IObservable<HeartbeatMessage> _heartbeatStream;
        private ISet<string> _knownServerIds = new HashSet<string>();

        private IList<IDisposable> _watching = new List<IDisposable>();

        public event EventHandler<MasterElectedEvent> MasterElected;
        public event EventHandler<string> MasterDead;
        
        public HeartbeatWatcher(IObservable<HeartbeatMessage> heartbeatStream, TimingPolicy policy) {
            this._heartbeatStream = heartbeatStream;
            this._heartbeatStream.Subscribe( (h) => {
                if (!_knownServerIds.Contains(h.CurrentMaster)) {
                    _knownServerIds.Add(h.CurrentMaster);
                    StartWatching(h.CurrentMaster, policy);
                    OnMasterElected(h.CurrentMaster, h.Term);
                }
            });
        }

        private void StartWatching(string serverId, TimingPolicy policy) {
           var disposable =  _heartbeatStream
                .Where(h => h.CurrentMaster == serverId)
                .Select((_h) => Observable.Return(true).Delay(policy.GetElectionTimeout()))
                .Switch()
                .Take(1)
                .Subscribe( (msg) => {
                    OnMasterDead(serverId);
                    _knownServerIds.Remove(serverId);
                });

            this._watching.Add(disposable);
        }

        private void OnMasterDead(string serverId) {
            if (MasterDead != null) {
                MasterDead(this, serverId);
            }
        }

        private void OnMasterElected(string serverId, int term) {
            if (MasterElected != null) {
                MasterElected(this, new MasterElectedEvent {NewMaster = serverId, Term = term});
            }
        }

        public void Dispose()
        {
            foreach (var w in this._watching) {
                w.Dispose();
            }
        }
    }
}


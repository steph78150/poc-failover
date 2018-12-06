using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace poc_failover
{

    public interface IHeartbeatWatcher : IDisposable
    {
        event EventHandler<HeartbeatMessage> NodeJoined;

        event EventHandler<HeartbeatMessage> NodeLeft;

        int ActiveNodeCount {get;}
    }

    public class HeartbeatWatcher : IHeartbeatWatcher
    {
        private ConcurrentDictionary<HeartbeatMessage, HeartbeatMessage> _knownHeartbeats;

        private ConcurrentBag<IDisposable> _watching;

        public event EventHandler<HeartbeatMessage> NodeJoined;
        public event EventHandler<HeartbeatMessage> NodeLeft;


        public HeartbeatWatcher(IObservable<HeartbeatMessage> heartbeatStream, HeartbeatPolicy policy) {
            _knownHeartbeats = new ConcurrentDictionary<HeartbeatMessage, HeartbeatMessage>();
            _watching = new ConcurrentBag<IDisposable>();
            heartbeatStream.Subscribe( (h) => {
                if (_knownHeartbeats.TryAdd(h, h)) {
                    StartWatching(heartbeatStream, h, policy);
                    OnServerJoined(h);
                }
            });
        }

        public int ActiveNodeCount => _knownHeartbeats.Keys.Select(h => h.CurrentIdentity).Distinct().Count();

        private void StartWatching(IObservable<HeartbeatMessage> heartbeatStream, HeartbeatMessage first, HeartbeatPolicy policy) {
           var disposable =  heartbeatStream
                .Where(h => h.Equals(first))
                .Select((_h) => Observable.Return(true).Delay(policy.Timeout))
                .Switch()
                .Take(1)
                .Subscribe( (msg) => {
                    OnServerLeft(first);
                    _knownHeartbeats.TryRemove(first, out var _);
                });

            this._watching.Add(disposable);
        }

        private void OnServerLeft(HeartbeatMessage msg) {
            if (NodeLeft != null) {
                NodeLeft(this, msg);
            }
        }

        private void OnServerJoined(HeartbeatMessage msg) {
            if (NodeJoined != null) {
                NodeJoined(this, msg);
            }
        }

        public void Dispose()
        {
            while (_watching.TryTake(out var taken)) {
                taken.Dispose();
            }
        }
    }
}


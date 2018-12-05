using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace poc_failover
{
    public interface IHeartbeatWatcher : IDisposable
    {
        event EventHandler<HeartbeatMessage> NodeJoined;

        event EventHandler<string> NodeLeft;
    }

    public class HeartbeatWatcher : IHeartbeatWatcher
    {
        private ConcurrentDictionary<string, string> _knownServerIds;

        private ConcurrentBag<IDisposable> _watching;

        public event EventHandler<HeartbeatMessage> NodeJoined;
        public event EventHandler<string> NodeLeft;

        public HeartbeatWatcher(IObservable<HeartbeatMessage> heartbeatStream, HeartbeatPolicy policy) {
            _knownServerIds = new ConcurrentDictionary<string, string>();
            _watching = new ConcurrentBag<IDisposable>();
            heartbeatStream.Subscribe( (h) => {
                if (_knownServerIds.TryAdd(h.Sender, h.Sender)) {
                    StartWatching(heartbeatStream, h.Sender, policy);
                    OnServerJoined(h);
                }
            });
        }

        private void StartWatching(IObservable<HeartbeatMessage> heartbeatStream, string serverId, HeartbeatPolicy policy) {
           var disposable =  heartbeatStream
                .Where(h => h.Sender == serverId)
                .Select((_h) => Observable.Return(true).Delay(policy.Timeout))
                .Switch()
                .Take(1)
                .Subscribe( (msg) => {
                    OnServerLeft(serverId);
                    _knownServerIds.TryRemove(serverId, out var _);
                });

            this._watching.Add(disposable);
        }

        private void OnServerLeft(string serverId) {
            if (NodeLeft != null) {
                NodeLeft(this, serverId);
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


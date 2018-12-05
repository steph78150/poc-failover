using System;
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
        private ISet<string> _knownServerIds;

        private IList<IDisposable> _watching;

        public event EventHandler<HeartbeatMessage> NodeJoined;
        public event EventHandler<string> NodeLeft;

        public HeartbeatWatcher(IObservable<HeartbeatMessage> heartbeatStream, HeartbeatPolicy policy) {
            _knownServerIds = new HashSet<string>();
            _watching = new List<IDisposable>();
            heartbeatStream.Subscribe( (h) => {
                lock (_knownServerIds) {
                    if (!_knownServerIds.Contains(h.Sender)) {
                        _knownServerIds.Add(h.Sender);
                        StartWatching(heartbeatStream, h.Sender, policy);
                        OnServerJoined(h);
                    }
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
                    _knownServerIds.Remove(serverId);
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
            throw new NotImplementedException();
        }
    }
}


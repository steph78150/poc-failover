using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace poc_failover
{
    public interface IHeartbeatWatcher : IDisposable
    {
        event EventHandler<string> NodeJoined;

        event EventHandler<string> NodeLeft;
    }

    public class HeartbeatWatcher : IHeartbeatWatcher
    {
        private IObservable<HeartbeatMessage> _heartbeatStream;
        private ISet<string> _knownServerIds = new HashSet<string>();

        private IList<IDisposable> _watching = new List<IDisposable>();

        public event EventHandler<string> NodeJoined;
        public event EventHandler<string> NodeLeft;

        public HeartbeatWatcher(IObservable<HeartbeatMessage> heartbeatStream, HeartbeatPolicy policy) {
            this._heartbeatStream = heartbeatStream;
            this._heartbeatStream.Subscribe( (h) => {
                if (!_knownServerIds.Contains(h.Sender)) {
                    _knownServerIds.Add(h.Sender);
                    StartWatching(h.Sender, policy);
                    OnServerJoined(h.Sender);
                }
            });
        }

        private void StartWatching(string serverId, HeartbeatPolicy policy) {
           var disposable =  _heartbeatStream
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

        private void OnServerJoined(string serverId) {
            if (NodeJoined != null) {
                NodeJoined(this, serverId);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}


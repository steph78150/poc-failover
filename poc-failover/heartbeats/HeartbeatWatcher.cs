using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace poc_failover
{
    public class HeartbeatWatcher {

        private IObservable<HeartbeatMessage> _heartbeatStream;

        private ISet<string> _knownServerIds = new HashSet<string>();

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
             _heartbeatStream
                .Where(h => h.Sender == serverId)
                .Select((_h) => Observable.Return(true).Delay(policy.Timeout))
                .Switch()
                .Take(1)
                .Subscribe( (msg) => {
                    OnServerLeft(serverId);
                    _knownServerIds.Remove(serverId);
                });
        }

        private void OnServerLeft(string serverId) {
            Console.Out.WriteLine($"Server {serverId} has left the cluster");
        }

        private void OnServerJoined(string serverId) {
            Console.Out.WriteLine($"Server {serverId} has joined the cluster");
        }
    }
}


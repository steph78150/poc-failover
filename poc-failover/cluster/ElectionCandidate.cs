using System;
using System.Reactive.Linq;

namespace poc_failover
{

    public class ElectionCandidate
    {
        private readonly IMessageBusPublisher _publisher;
        private readonly ElectionState _state;
        private readonly ICluster _cluster;
        private readonly TimingPolicy _policy;
        private readonly IObservable<VoteMessage> _messageStream;

        public event EventHandler<int> Win;

        public event EventHandler Defeat;

        public ElectionCandidate(
            IMessageBusPublisher publisher, 
            IObservable<VoteMessage> messageStream, 
            ElectionState state, ICluster cluster, TimingPolicy policy) 
        {
            _publisher = publisher;
            _messageStream = messageStream;
            _state = state;
            _cluster = cluster;
            _policy = policy;
        }

        public IDisposable ScheduleNextElection(string myself) {
            return Observable.Return(0).Delay(_policy.GetElectionTimeout())
                .Subscribe(x => this.RunForElection(myself));
        } 

        public void RunForElection(string myself) 
        {
           var candidateMsg = _state.NewElection(myself);
            Console.WriteLine(candidateMsg);
            
            _messageStream
                .Where( m => m.Term == candidateMsg.Term && m.VotedFor == myself)
                .Buffer( 
                    Observable.Return(false).Delay(TimeSpan.FromMilliseconds(1000))
                ).Subscribe(votes => {
                    foreach (var vote in votes) {
                        Console.Out.WriteLine($"Candidate {myself} received vote : {vote}");
                    }
                    if (votes.Count >= _cluster.Quorum) {
                        OnVictory(candidateMsg.Term);
                    } else {
                        OnDefeat();
                    }
                    
                });

             _publisher.Publish(candidateMsg);
        }

        private void OnDefeat()
        {
            if (Defeat != null)
                Defeat(this, new EventArgs());
        }

        private void OnVictory(int term)
        {
            if (Win != null) 
                Win(this, term);
        }
    }
}


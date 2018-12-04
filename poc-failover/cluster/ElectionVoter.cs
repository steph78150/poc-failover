using System;
using System.Reactive.Linq;

namespace poc_failover
{

    public class ElectionVoter 
    {
        private readonly IMessageBusPublisher _publisher;
        private readonly ElectionState _state;
        private readonly string id;

        public ElectionVoter(
            IMessageBusPublisher publisher, 
            IObservable<CandidateMessage> messageStream, 
            ElectionState state, string id) 
        {
            this._publisher = publisher;
            this._state = state;
            this.id = id;
            messageStream.Subscribe((c) => OnVote(c));
        }

        private void OnVote(CandidateMessage candidateMessage)
        {
            if (_state.TryToVoteFor(candidateMessage)) 
            {
                _publisher.Publish(VoteMessage.VoteForHim(this.id,                  candidateMessage));
            }
        }
    }
}


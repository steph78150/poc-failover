using System;

namespace poc_failover
{

    public class ElectionOrganizer {
        
    }

    public class ElectionVoter 
    {
        private readonly IMessageBusPublisher _publisher;
        private readonly ElectionState _state;

        public ElectionVoter(
            IMessageBusPublisher publisher, 
            IObservable<CandidateMessage> messageStream, 
            ElectionState state) 
        {
            this._publisher = publisher;
            this._state = state;

            messageStream.Subscribe((c) => OnVote(c));
        }

        private void OnVote(CandidateMessage candidateMessage)
        {
            if (_state.TryToVoteFor(candidateMessage)) 
            {
                _publisher.Publish(VoteMessage.VoteForHim(candidateMessage));
            }
        }
    }
}


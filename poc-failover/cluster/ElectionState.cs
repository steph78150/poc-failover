namespace poc_failover
{
    public class ElectionState 
    {
        public static ElectionState Empty() 
        {
            return new ElectionState 
            {
                Term = 0,
                VotedFor = null,
            };
        } 

        public int Term { get; private set; }

        public string VotedFor { get; private set;}

        public CandidateMessage NewElection(string myself) 
        {
            Term ++;
            VotedFor = myself;
            return new CandidateMessage 
            {
                Candidate = this.VotedFor,
                Term = this.Term
            };
        }

        private bool IsOlderTerm(CandidateMessage msg) => (msg.Term < this.Term);

        private bool IsNewerTerm(CandidateMessage msg) => (msg.Term > this.Term);

        private bool CanVoteFor(string candidate) => VotedFor == null || VotedFor == candidate;

        private void ResetTerm(int term)
        {
            Term = term;
            VotedFor = null;
        }

        public bool TryToVoteFor(CandidateMessage msg)
        {
            if (IsOlderTerm(msg))
            {
                return false;
            }
            if (IsNewerTerm(msg))
            {
                ResetTerm(msg.Term);
            }
            if (CanVoteFor(msg.Candidate))
            {
                VotedFor = msg.Candidate;
                return true;
            }
            return false;
        }

        public override string ToString() => $"Current term is {Term}, last vote cast was {VotedFor ?? "NONE"}";
    }
}


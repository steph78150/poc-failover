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

        public int Term { get; set;}

        public string VotedFor { get; set ;}

        public void NewElection(string candidateId) 
        {
            Term ++;
            VotedFor = candidateId;
        }

        public void SeenTerm(int term) 
        {
            if (Term < term) 
            {
                Term = term;
                VotedFor = null;
            }
        }

        public bool TryToVoteFor(string candidateId) 
        {
            if (VotedFor == null || VotedFor == candidateId) 
            {
                VotedFor = candidateId;
                return true;
            }
            return false;
        }
    }
}


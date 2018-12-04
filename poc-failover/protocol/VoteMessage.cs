using System;

namespace poc_failover
{
    public class VoteMessage : IMessage
    {
        public string VotedFor { get; set;}

        public int Term { get; set; }

        public static VoteMessage VoteForHim(CandidateMessage candidateMessage)
        {
            return new VoteMessage 
            {
                Term = candidateMessage.Term,
                VotedFor = candidateMessage.Candidate
            };
        }
    } 
}
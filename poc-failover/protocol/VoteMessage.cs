using System;

namespace poc_failover
{
    public class VoteMessage : IMessage
    {
        public string Voter {get;set;}

        public string VotedFor { get; set;}

        public int Term { get; set; }

        public static VoteMessage VoteForHim(string voter, CandidateMessage candidateMessage)
        {
            return new VoteMessage 
            {
                Voter = voter,
                Term = candidateMessage.Term,
                VotedFor = candidateMessage.Candidate
            };
        }

        public override string ToString() => $"Vote from {Voter} for candidate {VotedFor} during term {Term}";
    } 
}
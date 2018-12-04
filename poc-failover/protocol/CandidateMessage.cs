namespace poc_failover
{
    public class CandidateMessage : IMessage 
    {
        public string Candidate { get; set;}
        
        public int Term { get; set; }

        public override string ToString() => $"Candidate {Candidate} for term {Term}";
    }
}
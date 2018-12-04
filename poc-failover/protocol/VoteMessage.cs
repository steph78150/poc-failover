namespace poc_failover
{
    public class VoteMessage : IMessage
    {
        public string VotedFor { get; set;}

        public int Term { get; set; }
    } 
}
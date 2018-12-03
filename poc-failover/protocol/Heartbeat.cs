namespace poc_failover
{
    public class HeartbeatMessage: IMessage {
        public string Sender {get; set;}

        public long Counter {get; set;}

        public override string ToString() => $"Heatbeat #{Counter}";
    }
    
}
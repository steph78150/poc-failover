namespace poc_failover
{
    public class HeartbeatMessage: IMessage {
        public string Sender {get; set;}

        public string CurrentIdentity {get; set;}

        public override string ToString() => $"Heatbeat from {Sender} masquerading as {(CurrentIdentity == Sender ? "ITSELF" : CurrentIdentity)}";
    }

    
    
}
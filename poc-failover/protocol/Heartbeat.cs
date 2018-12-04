namespace poc_failover
{
    public class HeartbeatMessage: IMessage {
        public string CurrentMaster {get; set;}

        public int Term {get; set;}

        public override string ToString() => $"Heatbeat from {CurrentMaster} which had won term #{Term} ";
    }

    
    
}
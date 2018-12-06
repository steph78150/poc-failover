namespace poc_failover
{
    public class HeartbeatMessage: IMessage {
        public string Sender {get; set;}

        public string CurrentIdentity {get; set;}

        public override bool Equals(object obj) {
            if (obj is HeartbeatMessage) {
                var other = (HeartbeatMessage) obj;
                return this.Sender == other.Sender && this.CurrentIdentity == other.CurrentIdentity;
            }
            return false;
        }

        public override int GetHashCode() {
            return 7 * this.Sender.GetHashCode() + 13 * this.CurrentIdentity.GetHashCode();
        }

        private string IdentityText => CurrentIdentity == Sender ? "" : $"[{CurrentIdentity}]";

        public override string ToString() => Sender + IdentityText;
    }

    
    
}
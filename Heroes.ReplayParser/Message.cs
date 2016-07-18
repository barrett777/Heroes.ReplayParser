using static Heroes.ReplayParser.ReplayMessageEvents;

namespace Heroes.ReplayParser
{
    public class Message
    {
        public MessageEventType MessageEventType { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public PingMessage PingMessage { get; set; }
        public PlayerAnnounceMessage PlayerAnnounceMessage { get; set; }

        public override string ToString()
        {
            if (ChatMessage != null)
                return ChatMessage.ToString();
            else if (PingMessage != null)
                return PingMessage.ToString();
            else if (PlayerAnnounceMessage != null)
                return PlayerAnnounceMessage.ToString();
            else
                return base.ToString();
        }
    }
}
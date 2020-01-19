using System;
using static Heroes.ReplayParser.ReplayMessageEvents;

namespace Heroes.ReplayParser
{
    public class Message
    {
        public Player MessageSender { get; set; }
        public int PlayerIndex { get; set; }
        public TimeSpan Timestamp { get; set; }
        public MessageEventType MessageEventType { get; set; }
        public ChatMessage ChatMessage { get; set; }
        public PingMessage PingMessage { get; set; }
        public PlayerAnnounceMessage PlayerAnnounceMessage { get; set; }

        public override string ToString()
        {
            if (ChatMessage != null)
            {
                if (MessageSender == null)
                    // I've seen this in at least one replay, and I think it happens right before or after a player is disconnecting or reconnecting
                    return $"({Timestamp}) [{ChatMessage.MessageTarget}] ((Unknown)): {ChatMessage.Message}";
                else if (!string.IsNullOrEmpty(MessageSender.Character))
                    return $"({Timestamp}) [{ChatMessage.MessageTarget}] {MessageSender.Name} ({MessageSender.Character}): {ChatMessage.Message}";
                else
                    return $"({Timestamp}) [{ChatMessage.MessageTarget}] {MessageSender.Name}: {ChatMessage.Message}";
            }
            else if (PingMessage != null)
            {
                if (MessageSender == null)
                    // I've seen this in at least one replay, and I think it happens right before or after a player is disconnecting or reconnecting
                    return $"({Timestamp}) [{PingMessage.MessageTarget}] ((Unknown)) used a ping";
                else if (!string.IsNullOrEmpty(MessageSender.Character))
                    return $"({Timestamp}) [{PingMessage.MessageTarget}] {MessageSender.Name} ({MessageSender.Character}) used a ping";
                else
                    return $"({Timestamp}) [{PingMessage.MessageTarget}] {MessageSender.Name} used a ping";
            }
            else if (PlayerAnnounceMessage != null)
            {
                if (MessageSender == null)
                    // I've seen this in at least one replay, and I think it happens right before or after a player is disconnecting or reconnecting
                    return $"({Timestamp}) ((Unknown)) announced {PlayerAnnounceMessage.AnnouncementType}";
                else if (!string.IsNullOrEmpty(MessageSender.Character))
                    return $"({Timestamp}) {MessageSender.Name} ({MessageSender.Character}) announced {PlayerAnnounceMessage.AnnouncementType}";
                else
                    return $"({Timestamp}) {MessageSender.Name} announced {PlayerAnnounceMessage.AnnouncementType}";
            }
            else
                return base.ToString();
        }
    }
}
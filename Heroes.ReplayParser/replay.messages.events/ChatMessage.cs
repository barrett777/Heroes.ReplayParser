namespace Heroes.ReplayParser
{
    using System;

    /// <summary>
    /// Defines a single line of text in a replay's conversation log.
    /// </summary>
    public class ChatMessage
    {
        /// <summary> Gets or sets the chat message. </summary>
        public string Message { get; set; }

        /// <summary> Gets or sets the target of the message. </summary>
        public ChatMessageTarget MessageTarget { get; set; }

        /// <summary> Gets or sets the Player Id who spoke the message. </summary>
        public int PlayerId { get; set; }

        /// <summary> Gets or sets the timestamp at which point the message was displayed. </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary> Overrides the ToString method, to display the chat message similar to what's expected in-game. </summary>
        /// <returns> Returns a chat message formatted similar to the in-game chat log. </returns>
        public override string ToString()
        {
            return string.Format("({0}) [{1}] Player {2}: {3}", this.Timestamp, this.MessageTarget, this.PlayerId, this.Message);
        }
    }
}
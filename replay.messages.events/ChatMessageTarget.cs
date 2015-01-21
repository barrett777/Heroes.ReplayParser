namespace Heroes.ReplayParser
{
    /// <summary>
    /// Defines who a message in-game was sent to. This also depends
    /// on whether the player was a spectator or player, as spectator chat
    /// is also considered "All"
    /// </summary>
    public enum ChatMessageTarget
    {
        /// <summary> Chat message delivered to all players. </summary>
        All = 0, 

        /// <summary> Chat message only delivered to allied players. </summary>
        Allies = 2, 

        /// <summary> Chat message delivered to observers. </summary>
        Observers = 4,
    }
}
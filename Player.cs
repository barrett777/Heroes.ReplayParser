namespace Heroes.ReplayParser
{
    public class Player
    {
        /// <summary>
        /// Gets or sets the Battle.NET region of a player.
        /// </summary>
        public int BattleNetRegionId { get; set; }

        /// <summary>
        /// Gets or sets the Battle.NET Sub-ID of a player, describing the URI to find the player profile. 
        /// </summary>
        public int BattleNetSubId { get; set; }

        /// <summary>
        /// Gets or sets the Battle.NET ID of a player, describing the URI to find the player profile.
        /// </summary>
        public int BattleNetId { get; set; }

        /// <summary>
        /// Gets or sets the player's color.
        /// </summary>
        public int[] Color { get; set; }

        /// <summary>
        /// Gets or sets the difficulty of a computer player. 
        /// Human players will default to either Unknown or Medium.
        /// </summary>
        public Difficulty Difficulty { get; set; }

        /// <summary>
        /// Gets or sets the player's handicap.
        /// </summary>
        public int Handicap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// Gets or sets the player's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the BattleTag (Numbers only)
        /// </summary>
        public int BattleTag { get; set; }

        /// <summary>
        /// Gets or sets the type of player, whether he is human or computer.
        /// </summary>
        public PlayerType PlayerType { get; set; }

        /// <summary>
        /// Gets or sets the player's team number.
        /// </summary>
        public int Team { get; set; }

        /// <summary>
        /// Gets or sets whether the player was auto select or not
        /// </summary>
        public bool IsAutoSelect { get; set; }

        /// <summary>
        /// Gets or sets the player's character.
        /// </summary>
        public string Character { get; set; }

        /// <summary>
        /// Gets or sets the player's character level.
        /// </summary>
        public int CharacterLevel { get; set; }

        /// <summary>
        /// Gets or sets the player's in game selected Hero talents.
        /// </summary>
        public int[] Talents { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
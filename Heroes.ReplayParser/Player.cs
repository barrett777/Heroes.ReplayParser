using System;

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
        /// Gets or sets the player's in game selected Hero talents, and the TimeSpan when they were selected in game.
        /// </summary>
        public Tuple<int, TimeSpan>[] Talents { get; set; }

        /// <summary>
        /// Gets or sets the player's in game Hero units.
        /// </summary>
        public Unit[] HeroUnits { get; set; }

        /// <summary>
        /// Gets or sets the player's in game deaths.  This probably shouldn't be stored separately like this; the array of 'HeroUnits' should probably each have their own death.  Currently 'HeroUnits' is only one unit that 'lives' the entire match
        /// </summary>
        public TimeSpan[] Deaths { get; set; }

        public Player()
        {
            Color = new int[0];
            Talents = new Tuple<int, TimeSpan>[0];
            HeroUnits = new Unit[0];
            Deaths = new TimeSpan[0];
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
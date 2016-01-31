using System;
using System.Collections.Generic;

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
        public int[] Color { get; set; } = new int[0];

        /// <summary>
        /// Gets or sets the difficulty of a computer player.
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
        /// Gets or sets the player's skin / skin tint.
        /// </summary>
        public string SkinAndSkinTint { get; set; } = null;

        /// <summary>
        /// Gets or sets the player's mount / mount tint.
        /// </summary>
        public string MountAndMountTint { get; set; } = null;

        /// <summary>
        /// Gets or sets the player's character level.
        /// </summary>
        public int CharacterLevel { get; set; }

        /// <summary>
        /// Gets or sets if the player has been given the silenced penalty
        /// </summary>
        public bool IsSilenced { get; set; } = false;

        /// <summary>
        /// Gets or sets the player's in game selected Hero talents, and the TimeSpan when they were selected in game.
        /// </summary>
        public Tuple<int, TimeSpan>[] Talents { get; set; } = new Tuple<int, TimeSpan>[0];

        /// <summary>
        /// Gets or sets the player's in game Hero units.
        /// </summary>
        public List<Unit> HeroUnits { get; set; } = new List<Unit>();
    }

    public enum PlayerType
    {
        Human,
        Computer,
        Spectator
    }

    public enum Difficulty
    {
        Unknown,
        Beginner,
        Recruit,
        Adept,
        Veteran,
        Elite
    }
}
namespace Heroes.ReplayParser
{
    using System;
    using System.Collections.Generic;

    public class Replay
    {
        /// <summary> Gets a list of all messages which took place during the game. </summary>
        public List<Message> Messages { get; set; } = new List<Message>();

        /// <summary> Gets the speed the game was played at. </summary>
        public GameSpeed GameSpeed { get; set; }

        /// <summary> Gets the type of game this replay covers, whether it was a private or open match. </summary>
        public GameMode GameMode { get; set; }

        /// <summary> Gets the map the game was played on. </summary>
        public string Map { get; set; }

        /// <summary> Gets the size of the map the game was played on. </summary>
        public Point MapSize { get; set; }

        /// <summary> Gets the details of all players in the replay. </summary>
        public Player[] Players { get; set; }

        /// <summary> Gets the build number of the Heroes version used in creating the replay. </summary>
        public int ReplayBuild { get; set; }

        /// <summary> Gets the version number of the replay. </summary>
        public string ReplayVersion { get; set; }

        /// <summary> Gets the team size of the selected gametype. </summary>
        public string TeamSize { get; set; }

        /// <summary> Gets the Time at which the game took place. </summary>
        public DateTime Timestamp { get; set; }

        /// <summary> Gets the list of all clients connected to the game, using 'm_userId' as index </summary>
        public Player[] ClientListByUserID { get; set; } = new Player[16];

        /// <summary> Gets the list of all clients connected to the game, using 'm_workingSetSlotId' as index </summary>
        public Player[] ClientListByWorkingSetSlotID { get; set; } = new Player[16];

        /// <summary> Gets the game events. </summary>
        public List<GameEvent> GameEvents { get; set; } = new List<GameEvent>();

        /// <summary> Gets the tracker events. </summary>
        public List<TrackerEvent> TrackerEvents { get; set; }

        /// <summary> Gets a list of units. </summary>
        public List<Unit> Units { get; set; } = new List<Unit>();

        /// <summary> Gets the number of frames in this replay. </summary>
        public int Frames { get; set; }

        /// <summary> Gets the length of this replay as a timespan. </summary>
        public TimeSpan ReplayLength { get { return new TimeSpan(0, 0, (int)(Frames / 16.0)); } }

        /// <summary> Gets a single random value from replay.init.data; currently using as part of replay hash for deduplication. </summary>
        public uint RandomValue { get; set; }

        /// <summary> Team Levels ([Team][Level] = TimeSpan) </summary>
        public Dictionary<int, TimeSpan>[] TeamLevels { get; set; } = new Dictionary<int, TimeSpan>[2];

        /// <summary> Periodic XP Breakdown ([Team][PeriodicXPBreakdown]) </summary>
        public List<PeriodicXPBreakdown>[] TeamPeriodicXPBreakdown { get; set; } = new List<PeriodicXPBreakdown>[2];

        /// <summary> Team Objectives ([Team][TeamObjective]) </summary>
        public List<TeamObjective>[] TeamObjectives { get; set; } = new List<TeamObjective>[2];

        /// <summary> Team Hero Bans ([Team][HeroBanned]) </summary>
        public string[][] TeamHeroBans { get; set; } = new string[2][] { new string[2] { null, null }, new string[2] { null, null } };

        public bool IsGameEventsParsedSuccessfully { get; set; } = false;
        public bool? IsStatisticsParsedSuccessfully { get; set; } = null;
    }

    public class PeriodicXPBreakdown
    {
        public int TeamLevel { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public int MinionXP { get; set; }
        public int CreepXP { get; set; }
        public int StructureXP { get; set; }
        public int HeroXP { get; set; }
        public int TrickleXP { get; set; }
    }

    public class TeamObjective
    {

    }

    public enum TeamObjectiveType
    {

    }

    public enum GameMode
    {
        Unknown = -9,
        Event = -2,
        Custom = -1,
        TryMe = 0,
        Practice = 1,
        Cooperative = 2,
        QuickMatch = 3,
        HeroLeague = 4,
        TeamLeague = 5,
        UnrankedDraft = 6
    }

    public enum GameSpeed
    {
        Unknown = 0,
        Slower = 1,
        Slow,
        Normal,
        Fast,
        Faster
    }
}

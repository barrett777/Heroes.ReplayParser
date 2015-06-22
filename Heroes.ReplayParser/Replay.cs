namespace Heroes.ReplayParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Replay
    {
        /// <summary> Gets a list of all chat messages which took place during the game. </summary>
        public IList<ChatMessage> ChatMessages { get; set; }

        /// <summary> Gets the speed the game was played at. </summary>
        public GameSpeed GameSpeed { get; set; }

        /// <summary> Gets the type of game this replay covers, whether it was a private or open match. </summary>
        public GameMode GameMode { get; set; }

        /// <summary> Gets the Gateway (KR, NA, etc.) that the game was played in. </summary>
        public string Gateway { get; set; }

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

        /// <summary> Gets the list of all clients connected to the game. </summary>
        public Player[] ClientList { get; set; }

        /// <summary> Gets the game events. </summary>
        public List<GameEvent> GameEvents { get; set; }

        /// <summary> Gets the tracker events. </summary>
        public List<TrackerEvent> TrackerEvents { get; set; }

        /// <summary> Gets a list of units. </summary>
        public List<Unit> Units { get; set; }

        /// <summary> Gets the number of frames in this replay. </summary>
        public int Frames { get; set; }

        /// <summary> Gets the length of this replay as a timespan. </summary>
        public TimeSpan ReplayLength { get; set; }

        /// <summary> Gets a single random value replay.init.data; currently using as part of replay hash for deduplication. </summary>
        public UInt32 RandomValue { get; set; }

        /// <summary> Gets a hash of the replay; components used are players in game, and a random value (potentially a seed). </summary>
        public Guid ReplayHash { get; set; }

        /// <summary> Team Level Milestones (From talent choices: 1 / 4 / 7 / 10 / 13 / 16 / 20) </summary>
        public TimeSpan[][] TeamLevelMilestones { get; set; }

        public Replay()
        {
            ClientList = new Player[0x10];
            TeamLevelMilestones = new TimeSpan[2][];
            GameEvents = new List<GameEvent>();
            Units = new List<Unit>();
        }
    }
}

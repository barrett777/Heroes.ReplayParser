using System;
using System.Linq;
using System.Text;

namespace Heroes.ReplayParser
{
    using System.Collections.Generic;

    public class ReplayAttributeEvents
    {
        public const string FileName = "replay.attributes.events";

        public ReplayAttribute[] Attributes { get; set; }

        public static void Parse(Replay replay, byte[] buffer)
        {
            var headerSize = 5;

            var numAttributes = BitConverter.ToInt32(buffer, headerSize);

            var attributes = new ReplayAttribute[numAttributes];

            var initialOffset = 4 + headerSize;

            for (int i = 0; i < numAttributes; i++)
                attributes[i] = ReplayAttribute.Parse(buffer, initialOffset + (i*13));

            new ReplayAttributeEvents { Attributes = attributes.OrderBy(i => i.AttributeType).ToArray() }.ApplyAttributes(replay);

            /* var stringList = attributes.OrderBy(i => i.AttributeType);
            Console.WriteLine(stringList.Count()); */
        }

        /// <summary>
        /// Applies the set of attributes to a replay.
        /// </summary>
        /// <param name="replay">Replay to apply the attributes to.</param>
        public void ApplyAttributes(Replay replay)
        {
            // I'm not entirely sure this is the right encoding here. Might be unicode...
            var encoding = Encoding.UTF8;

            var attributes1 = new List<ReplayAttribute>();
            var attributes2 = new List<ReplayAttribute>();
            var attributes3 = new List<ReplayAttribute>();
            var attributes4 = new List<ReplayAttribute>();
            var attributesffa = new List<ReplayAttribute>();

            foreach (var attribute in Attributes)
                switch (attribute.AttributeType)
                {
                    case ReplayAttributeEventType.PlayerTypeAttribute:
                        {
                            var type = encoding.GetString(attribute.Value.Reverse().ToArray()).ToLower();

                            if (type == "comp")
                                replay.Players[attribute.PlayerId - 1].PlayerType = PlayerType.Computer;
                            else if (type == "humn")
                                replay.Players[attribute.PlayerId - 1].PlayerType = PlayerType.Human;
                            else
                                throw new Exception("Unexpected value for PlayerType");

                            break;
                        }

                    case ReplayAttributeEventType.TeamSizeAttribute:
                        {
                            // This fixes issues with reversing the string before encoding. Without this, you get "\01v1"
                            replay.TeamSize = new string(encoding.GetString(attribute.Value, 0, 3).Reverse().ToArray());
                            break;
                        }

                    case ReplayAttributeEventType.DifficultyLevelAttribute:
                        {
                            var diffLevel = encoding.GetString(attribute.Value.Reverse().ToArray());
                            var player = replay.Players[attribute.PlayerId - 1];

                            switch (diffLevel)
                            {
                                case "VyEy":
                                    player.Difficulty = Difficulty.Beginner;
                                    break;
                                case "Easy":
                                    player.Difficulty = Difficulty.Recruit;
                                    break;
                                case "Medi":
                                    player.Difficulty = Difficulty.Adept;
                                    break;
                                case "HdVH":
                                    player.Difficulty = Difficulty.Veteran;
                                    break;
                                case "VyHd":
                                    player.Difficulty = Difficulty.Elite;
                                    break;
                            }

                            break;
                        }

                    case ReplayAttributeEventType.GameSpeedAttribute:
                        {
                            var speed = encoding.GetString(attribute.Value.Reverse().ToArray()).ToLower();

                            switch (speed)
                            {
                                case "slor":
                                    replay.GameSpeed = GameSpeed.Slower;
                                    break;
                                case "slow":
                                    replay.GameSpeed = GameSpeed.Slow;
                                    break;
                                case "norm":
                                    replay.GameSpeed = GameSpeed.Normal;
                                    break;
                                case "fast":
                                    replay.GameSpeed = GameSpeed.Fast;
                                    break;
                                case "fasr":
                                    replay.GameSpeed = GameSpeed.Faster;
                                    break;

                                // Otherwise, Game Speed will remain "Unknown"
                            }

                            break;
                        }

                    case ReplayAttributeEventType.PlayerTeam1v1Attribute:
                        {
                            attributes1.Add(attribute);
                            break;
                        }

                    case ReplayAttributeEventType.PlayerTeam2v2Attribute:
                        {
                            attributes2.Add(attribute);
                            break;
                        }

                    case ReplayAttributeEventType.PlayerTeam3v3Attribute:
                        {
                            attributes3.Add(attribute);
                            break;
                        }

                    case ReplayAttributeEventType.PlayerTeam4v4Attribute:
                        {
                            attributes4.Add(attribute);
                            break;
                        }

                    case ReplayAttributeEventType.PlayerTeamFFAAttribute:
                        {
                            attributesffa.Add(attribute);
                            break;
                        }


                    case ReplayAttributeEventType.GameTypeAttribute:
                        {
                            switch (encoding.GetString(attribute.Value.Reverse().ToArray()).ToLower().Trim('\0'))
                            {
                                case "priv":
                                    replay.GameMode = GameMode.Custom;
                                    break;
                                case "amm":
                                    if (replay.ReplayBuild < 33684)
                                        replay.GameMode = GameMode.QuickMatch;
                                    break;
                                default:
                                    throw new Exception("Unexpected Game Type");
                            }

                            break;
                        }

                    case ReplayAttributeEventType.Hero:
                        {
                            replay.Players[attribute.PlayerId - 1].IsAutoSelect = encoding.GetString(attribute.Value.Reverse().ToArray()) == "Rand";
                            break;
                        }

                    case ReplayAttributeEventType.CharacterLevel:
                        {
                            var characterLevel = int.Parse(encoding.GetString(attribute.Value.Reverse().ToArray()));
                            var player = replay.Players[attribute.PlayerId - 1];
                            player.CharacterLevel = characterLevel;

                            break;
                        }

                    case ReplayAttributeEventType.LobbyMode:
                        {
                            if (replay.GameMode != GameMode.Custom)
                                switch (encoding.GetString(attribute.Value.Reverse().ToArray()).ToLower().Trim('\0'))
                                {
                                    case "stan":
                                        replay.GameMode = GameMode.QuickMatch;
                                        break;
                                    case "drft":
                                        replay.GameMode = GameMode.HeroLeague;
                                        break;
                                }
                        }
                        break;

                    case ReplayAttributeEventType.ReadyMode:
                        if (replay.GameMode == GameMode.HeroLeague && encoding.GetString(attribute.Value.Reverse().ToArray()).ToLower().Trim('\0') == "fcfs")
                            replay.GameMode = GameMode.TeamLeague;
                        break;

                    case (ReplayAttributeEventType)4011: // What is this? Draft order?
                        break;
                    case (ReplayAttributeEventType)4016: // What is this? Always '1' in Hero League
                        // if (replay.GameMode == GameMode.HeroLeague && int.Parse(encoding.GetString(attribute.Value.Reverse().ToArray())) != 1)
                            // Console.WriteLine("WAAT!?");
                        break;
                    case (ReplayAttributeEventType)4017: // What is this? Always '5' in Hero League
                        // if (replay.GameMode == GameMode.HeroLeague && int.Parse(encoding.GetString(attribute.Value.Reverse().ToArray())) != 5)
                            // Console.WriteLine("WAAT!?");
                        break;
                }

            List<ReplayAttribute> currentList = null;

            if (replay.TeamSize.Equals("1v1"))
                currentList = attributes1;
            else if (replay.TeamSize.Equals("2v2"))
                currentList = attributes2;
            else if (replay.TeamSize.Equals("3v3"))
                currentList = attributes3;
            else if (replay.TeamSize.Equals("4v4"))
                currentList = attributes4;
            else if (replay.TeamSize.Equals("FFA"))
                currentList = attributesffa;

            if (currentList != null)
                foreach (var att in currentList)
                    // Reverse the values then parse, you don't notice the effects of this until theres 10+ teams o.o
                    replay.Players[att.PlayerId - 1].Team = int.Parse(encoding.GetString(att.Value.Reverse().ToArray()).Trim('\0', 'T'));
        }

        public enum ReplayAttributeEventType
        {
            PlayerTypeAttribute = 500,
            Rules = 1000,
            IsPremadeGame = 1001,

            /* 2000 - 2024 are related to team sizes */
            TeamSizeAttribute = 2001,
            PlayerTeam1v1Attribute = 2002,
            PlayerTeam2v2Attribute = 2003,
            PlayerTeam3v3Attribute = 2004,
            PlayerTeam4v4Attribute = 2005,
            PlayerTeamFFAAttribute = 2006,

            GameSpeedAttribute = 3000,
            PlayerRaceAttribute = 3001,
            TeamColorIndexAttribute = 3002,
            PlayerHandicapAttribute = 3003,
            DifficultyLevelAttribute = 3004,
            ComputerRace = 3005,
            LobbyDelay = 3006,
            ParticipantRole = 3007,
            WatcherType = 3008,
            GameTypeAttribute = 3009,
            LockedAlliances = 3010,
            PlayerLogo = 3011,
            TandemLeader = 3012,
            Commander = 3013,
            CommanderLevel = 3014,
            GameDuration = 3015,
            /* 3100 - 3300 are related to AI builds (for Starcraft 2) */

            PrivacyOption = 4000,
            UsingCustomObserverUI = 4001,
            Hero = 4002,
            SkinAndSkinTint = 4003,
            MountAndMountTint = 4004,
            Ready = 4005,
            HeroRange = 4006,
            HeroRole = 4007,
            CharacterLevel = 4008,
            CanReady = 4009,
            LobbyMode = 4010,
            ReadyOrder = 4011,
            ReadyingTeam = 4012,
            HeroDuplicates = 4013,
            HeroVisibility = 4014,
            LobbyPhase = 4015,
            ReadyingCount = 4016,
            ReadyingRound = 4017,
            ReadyMode = 4018,
            ReadyRequirements = 4019,
            FirstReadyingTeam = 4020

            /* 4100 - 4200 are related to Artifacts, no longer in the game */
        }
    }
}

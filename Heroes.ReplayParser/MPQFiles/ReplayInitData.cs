namespace Heroes.ReplayParser
{
    using System.IO;
    using System.Text;
    using Heroes.ReplayParser.Streams;
    using System;
    using System.Linq;
    /// <summary> Parses the replay.Initdata file in the replay file. </summary>
    public class ReplayInitData
    {
        public const string FileName = "replay.initData";

        /// <summary> Parses the replay.initdata file in a replay file. </summary>
        /// <param name="replay"> The replay file to apply the parsed data to. </param>
        /// <param name="buffer"> The buffer containing the replay.initdata file. </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var reader = new BitReader(stream);
                
                var playerListLength = reader.Read(5);
                for (var i = 0; i < playerListLength; i++)
                {
                    var playerName = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(8)); // Player name

                    // Populate the name for each client in the client list by UserID
                    if (playerName != "")
                        replay.ClientListByUserID[i] = new Player { Name = playerName };

                    if (reader.ReadBoolean())
                        reader.ReadBlobPrecededWithLength(8); // clanTag

                    if (reader.ReadBoolean())
                        reader.ReadBlobPrecededWithLength(40); // Clan Logo

                    if (reader.ReadBoolean())
                        reader.Read(8); // highestLeague

                    if (reader.ReadBoolean())
                        reader.ReadInt32(); // combinedRaceLevels

                    reader.ReadInt32(); // Random seed (So far, always 0 in Heroes)

                    if (reader.ReadBoolean())
                        reader.Read(8); // Race Preference

                    if (reader.ReadBoolean())
                        reader.Read(8); // Team Preference

                    reader.ReadBoolean(); //test map
                    reader.ReadBoolean(); //test auto
                    reader.ReadBoolean(); //examine
                    reader.ReadBoolean(); //custom interface

                    reader.ReadInt32(); // m_testType

                    reader.Read(2); //observer

                    Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_hero - Currently Empty String
                    Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_skin - Currently Empty String
                    Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_mount - Currently Empty String
					if (replay.ReplayVersionMajor >= 2)
					{
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_banner - Currently Empty String
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_spray - Currently Empty String
					}
					Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7)); // m_toonHandle - Currently Empty String
                }

                // Marked as 'Random Value', so I will use as seed
                replay.RandomValue = (uint)reader.ReadInt32();

                reader.ReadBlobPrecededWithLength(10); // m_gameCacheName - "Dflt"

                reader.ReadBoolean(); // Lock Teams
                reader.ReadBoolean(); // Teams Together
                reader.ReadBoolean(); // Advanced Shared Control
                reader.ReadBoolean(); // Random Races
                reader.ReadBoolean(); // BattleNet
                reader.ReadBoolean(); // AMM
                reader.ReadBoolean(); // Competitive
                reader.ReadBoolean(); // m_practice
                reader.ReadBoolean(); // m_cooperative
                reader.ReadBoolean(); // m_noVictoryOrDefeat
                reader.ReadBoolean(); // m_heroDuplicatesAllowed
                reader.Read(2); // Fog
                reader.Read(2); // Observers
                reader.Read(2); // User Difficulty
                reader.ReadInt32(); reader.ReadInt32(); // 64 bit int: Client Debug Flags

                // m_ammId
                if (replay.ReplayBuild >= 43905 && reader.ReadBoolean())
                    switch (reader.ReadInt32())
                    {
                        case 50021: // Versus AI (Cooperative)
                        case 50041: // Practice
                            break;

                        case 50001:
                            replay.GameMode = GameMode.QuickMatch;
                            break;

                        case 50031:
                            replay.GameMode = GameMode.Brawl;
                            break;

                        case 50051:
                            replay.GameMode = GameMode.UnrankedDraft;
                            break;

                        case 50061:
                            replay.GameMode = GameMode.HeroLeague;
                            break;

                        case 50071:
                            replay.GameMode = GameMode.TeamLeague;
                            break;

                        default:
                            replay.GameMode = GameMode.Unknown;
                            break;
                    }

                reader.Read(3); // Game Speed

                // Not sure what this 'Game Type' is
                reader.Read(3);

                var maxUsers = reader.Read(5);
                if (maxUsers != 10 && replay.GameMode != GameMode.Brawl) // Max Players
                    replay.GameMode = GameMode.TryMe;

                reader.Read(5); // Max Observers
                reader.Read(5); // Max Players
                reader.Read(4); // + 1 = Max Teams
                reader.Read(6); // Max Colors
                reader.Read(8); // + 1 = Max Races

				// Max Controls
				if(replay.ReplayBuild < 59279)
					reader.Read(8);
				else
					reader.Read(4);

                replay.MapSize = new Point { X = (int)reader.Read(8), Y = (int)reader.Read(8) };
                if (replay.MapSize.Y == 1)
                    replay.MapSize.Y = replay.MapSize.X;
                else if (replay.MapSize.X == 0)
                    replay.MapSize.X = replay.MapSize.Y;

                // I haven't tested the following code on replays before build 39595 (End of 2015)
                if (replay.ReplayBuild < 39595)
                    return;

                reader.Read(32); // m_mapFileSyncChecksum
                reader.ReadBlobPrecededWithLength(11); // m_mapFileName
                reader.ReadBlobPrecededWithLength(8); // m_mapAuthorName
                reader.Read(32); // m_modFileSyncChecksum

                // m_slotDescriptions
                var slotDescriptionLength = reader.Read(5);
                for (var i = 0; i < slotDescriptionLength; i++)
                {
                    reader.ReadBitArray(reader.Read(6)); // m_allowedColors
                    reader.ReadBitArray(reader.Read(8)); // m_allowedRaces
                    reader.ReadBitArray(reader.Read(6)); // m_allowedDifficulty

					// m_allowedControls
					if(replay.ReplayBuild < 59279)
						reader.ReadBitArray(reader.Read(8));
					else
						reader.ReadBitArray(reader.Read(4));

					reader.ReadBitArray(reader.Read(2)); // m_allowedObserveTypes
                    reader.ReadBitArray(reader.Read(7)); // m_allowedAIBuilds
                }

                reader.Read(6); // m_defaultDifficulty
                reader.Read(7); // m_defaultAIBuild

                // m_cacheHandles
                var cacheHandlesLength = reader.Read(6);
                for (var i = 0; i < cacheHandlesLength; i++)
                    reader.ReadBytes(40);

                reader.ReadBoolean(); // m_hasExtensionMod
                reader.ReadBoolean(); // m_isBlizzardMap
                reader.ReadBoolean(); // m_isPremadeFFA
                reader.ReadBoolean(); // m_isCoopMode

                #region m_lobbyState

                reader.Read(3); // m_phase
                reader.Read(5); // m_maxUsers
                reader.Read(5); // m_maxObservers

                // m_slots
                var slotsLength = reader.Read(5);
                for (var i = 0; i < slotsLength; i++)
                {
                    int? userID = null;

                    reader.Read(8); // m_control
                    if (reader.ReadBoolean())
                        userID = (int) reader.Read(4); // m_userId
                    reader.Read(4); // m_teamId
                    if (reader.ReadBoolean())
                        reader.Read(5); // m_colorPref
                    if (reader.ReadBoolean())
                        reader.Read(8); // m_racePref
                    reader.Read(6); // m_difficulty
                    reader.Read(7); // m_aiBuild
                    reader.Read(7); // m_handicap

                    // m_observe
                    var observerStatus = reader.Read(2);

                    reader.Read(32); // m_logoIndex

                    reader.ReadBlobPrecededWithLength(9); // m_hero

                    var skinAndSkinTint = Encoding.ASCII.GetString(reader.ReadBlobPrecededWithLength(9)); // m_skin
                    if (skinAndSkinTint == "")
                        skinAndSkinTint = null;

                    var mountAndMountTint = Encoding.ASCII.GetString(reader.ReadBlobPrecededWithLength(9)); // m_mount
                    if (mountAndMountTint == "")
                        mountAndMountTint = null;

                    // m_artifacts
                    var artifactsLength = reader.Read(4);
                    for (var j = 0; j < artifactsLength; j++)
                        reader.ReadBlobPrecededWithLength(9);

                    int? workingSetSlotID = null;
                    if (reader.ReadBoolean())
                        workingSetSlotID = (int) reader.Read(8); // m_workingSetSlotId

                    if (userID.HasValue && workingSetSlotID.HasValue)
                    {
                        if (replay.ClientListByWorkingSetSlotID[workingSetSlotID.Value] != null)
                            replay.ClientListByUserID[userID.Value] = replay.ClientListByWorkingSetSlotID[workingSetSlotID.Value];

                        if (observerStatus == 2)
                            replay.ClientListByUserID[userID.Value].PlayerType = PlayerType.Spectator;

                        replay.ClientListByUserID[userID.Value].SkinAndSkinTint = skinAndSkinTint;
                        replay.ClientListByUserID[userID.Value].MountAndMountTint = mountAndMountTint;
                    }

                    // m_rewards
                    var rewardsLength = reader.Read(17);
                    for (var j = 0; j < rewardsLength; j++)
                        reader.Read(32);

                    reader.ReadBlobPrecededWithLength(7); // m_toonHandle

                    // m_licenses
                    if (replay.ReplayBuild < 49582 || replay.ReplayBuild == 49838)
                    {
                        var licensesLength = reader.Read(9);
                        for (var j = 0; j < licensesLength; j++)
                            reader.Read(32);
                    }

                    if (reader.ReadBoolean())
                        reader.Read(4); // m_tandemLeaderUserId

                    if (replay.ReplayBuild <= 41504)
                    {
                        reader.ReadBlobPrecededWithLength(9); // m_commander - Empty string

                        reader.Read(32); // m_commanderLevel - So far, always 0
                    }

					if (reader.ReadBoolean() && userID.HasValue) // m_hasSilencePenalty
                        replay.ClientListByUserID[userID.Value].IsSilenced = true;

					if(replay.ReplayVersionMajor >= 2)
					{
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_banner
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_spray
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_announcerPack
						Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(9)); // m_voiceLine

						// m_heroMasteryTiers
						if(replay.ReplayBuild >= 52561)
						{
							var heroMasteryTiersLength = reader.Read(10);
							for (var j = 0; j < heroMasteryTiersLength; j++)
							{
								reader.Read(32); // m_hero
								reader.Read(8); // m_tier
							}
						}
					}
				}

                if (reader.Read(32) != replay.RandomValue) // m_randomSeed
                    throw new Exception("Replay Random Seed Values in Replay Init Data did not match");

                if (reader.ReadBoolean())
                    reader.Read(4); // m_hostUserId

                reader.ReadBoolean(); // m_isSinglePlayer

                reader.Read(8); // m_pickedMapTag - So far, always 0

                reader.Read(32); // m_gameDuration - So far, always 0

                reader.Read(6); // m_defaultDifficulty

                reader.Read(7); // m_defaultAIBuild

                #endregion
            }
        }
    }
}
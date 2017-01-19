using System.Linq;

namespace Heroes.ReplayParser
{
    using System;
    using System.IO;

    public static class ReplayDetails
    {
        public const string FileName = "replay.details";

        /// <summary> Parses the replay.details file, applying it to a Replay object. </summary>
        /// <param name="replay"> The replay object to apply the parsed information to. </param>
        /// <param name="buffer"> The buffer containing the replay.details file. </param>
        public static void Parse(Replay replay, byte[] buffer, bool ignoreErrors = false)
        {
            using (var stream = new MemoryStream(buffer))
                using (var reader = new BinaryReader(stream))
                {
                    var replayDetailsStructure = new TrackerEventStructure(reader);
                    replay.Players = replayDetailsStructure.dictionary[0].optionalData.array.Select(i => new Player {
                        Name = i.dictionary[0].blobText,
                        BattleNetRegionId = (int)i.dictionary[1].dictionary[0].vInt.Value,
                        BattleNetSubId = (int)i.dictionary[1].dictionary[2].vInt.Value,
                        BattleNetId = (int)i.dictionary[1].dictionary[4].vInt.Value,
                        // [2] = Race (SC2 Remnant, Always Empty String in Heroes of the Storm)
                        Color = i.dictionary[3].dictionary.Keys.OrderBy(j => j).Select(j => (int)i.dictionary[3].dictionary[j].vInt.Value).ToArray(),
                        // [4] = Player Type (2 = Human, 3 = Computer (Practice, Try Me, or Cooperative)) - This is more accurately gathered in replay.attributes.events
                        Team = (int)i.dictionary[5].vInt.Value,
                        Handicap = (int)i.dictionary[6].vInt.Value,
                        // [7] = VInt, Default 0 - 'm_observe'
                        IsWinner = i.dictionary[8].vInt.Value == 1,
                        // [9] = 'm_workingSetSlotId'
                        Character = i.dictionary[10].blobText }).ToArray();

                    if (!ignoreErrors && (replay.Players.Length != 10 || replay.Players.Count(i => i.IsWinner) != 5))
                        // Try Me Mode, or something strange
                        return;

                    for (var i = 0; i < replay.Players.Length; i++)
                        if (replayDetailsStructure.dictionary[0].optionalData.array[i].dictionary[9].optionalData != null)
                            replay.ClientListByWorkingSetSlotID[replayDetailsStructure.dictionary[0].optionalData.array[i].dictionary[9].optionalData.vInt.Value] = replay.Players[i];
                        else
                            // Less than 0.1% of replays seem to be missing this.  I'm not sure why
                            return;

                    replay.Map = replayDetailsStructure.dictionary[1].blobText;
                    // [2] - m_difficulty
                    // [3] - m_thumbnail - "Minimap.tga", "CustomMiniMap.tga", etc
                    // [4] - m_isBlizzardMap
                    
                    replay.Timestamp = DateTime.FromFileTimeUtc(replayDetailsStructure.dictionary[5].vInt.Value); // m_timeUTC

                    // There was a bug during the below builds where timestamps were buggy for the Mac build of Heroes of the Storm
                    // The replay, as well as viewing these replays in the game client, showed years such as 1970, 1999, etc
                    // I couldn't find a way to get the correct timestamp, so I am just estimating based on when these builds were live
                    if (replay.ReplayBuild == 34053 && replay.Timestamp < new DateTime(2015, 2, 8))
                        replay.Timestamp = new DateTime(2015, 2, 13);
                    else if (replay.ReplayBuild == 34190 && replay.Timestamp < new DateTime(2015, 2, 15))
                        replay.Timestamp = new DateTime(2015, 2, 20);

                    // [6] - m_timeLocalOffset - For Windows replays, this is Utc offset.  For Mac replays, this is actually the entire Local Timestamp
                    // [7] - m_description - Empty String
                    // [8] - m_imageFilePath - Empty String
                    // [9] - m_mapFileName - Empty String
                    // [10] - m_cacheHandles - "s2ma"
                    // [11] - m_miniSave - 0
                    // [12] - m_gameSpeed - 4
                    // [13] - m_defaultDifficulty - Usually 1 or 7
                    // [14] - m_modPaths - Null
                    // [15] - m_campaignIndex - 0
                    // [16] - m_restartAsTransitionMap - 0
                }
        }
    }
}
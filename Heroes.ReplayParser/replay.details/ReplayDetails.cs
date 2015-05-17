using System.Linq;

namespace Heroes.ReplayParser
{
    using System;
    using System.IO;

    public static class ReplayDetails
    {
        /// <summary> Parses the replay.details file, applying it to a Replay object. </summary>
        /// <param name="replay"> The replay object to apply the parsed information to. </param>
        /// <param name="buffer"> The buffer containing the replay.details file. </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
                using (var reader = new BinaryReader(stream))
                {
                    var replayDetailsStructure = new TrackerEventStructure(reader);

                    replay.Players = replayDetailsStructure.dictionary[0].optionalData.array.Select(i => new Player
                    {
                        Name = i.dictionary[0].blobText,
                        BattleNetRegionId = (int)i.dictionary[1].dictionary[0].vInt.Value,
                        BattleNetSubId = (int)i.dictionary[1].dictionary[2].vInt.Value,
                        BattleNetId = (int)i.dictionary[1].dictionary[4].vInt.Value,
                        // [2] = Race (SC2 Remnant, Always Empty String in Heroes of the Storm)
                        Color = i.dictionary[3].dictionary.Keys.OrderBy(j => j).Select(j => (int)i.dictionary[3].dictionary[j].vInt.Value).ToArray(),
                        // [4] = Player Type (2 = Human, 3 = Computer (Practice, Try Me, or Coop)) - This is more accurately gathered in replay.attributes.events
                        Team = (int)i.dictionary[5].vInt.Value,
                        Handicap = (int)i.dictionary[6].vInt.Value,
                        // [7] = VInt, Default 0
                        IsWinner = i.dictionary[8].vInt.Value == 1,
                        // [9] = Sometimes player index in ClientList array; usually 0-9, but can be higher if there are observers. I don't fully understand this, as this was incorrect in at least one Custom game, where this said ClientList[8] was null
                        Character = i.dictionary[10].blobText
                    }).ToArray();

                    if (replay.Players.Length != 10 || replay.Players.Count(i => i.IsWinner) != 5)
                        // Try Me Mode, or something strange
                        return;

                    replay.Map = replayDetailsStructure.dictionary[1].blobText;
                    // [2] - This is typically an empty string, no need to decode.
                    // [3] - Blob: "Minimap.tga" or "CustomMiniMap.tga"
                    // [4] - Uint, Default 1

                    // [5] - Utc Timestamp
                    replay.Timestamp = DateTime.FromFileTimeUtc(replayDetailsStructure.dictionary[5].vInt.Value);

                    // There was a bug during the below builds where timestamps were buggy for the Mac build of Heroes of the Storm
                    // The replay, as well as viewing these replays in the game client, showed years such as 1970, 1999, etc
                    // I couldn't find a way to get the correct timestamp, so I am just estimating based on when these builds were live
                    if (replay.ReplayBuild == 34053 && replay.Timestamp < new DateTime(2015, 2, 8))
                        replay.Timestamp = new DateTime(2015, 2, 13);
                    else if (replay.ReplayBuild == 34190 && replay.Timestamp < new DateTime(2015, 2, 15))
                        replay.Timestamp = new DateTime(2015, 2, 20);

                    // [6] - Windows replays, this is Utc offset.  Mac replays, this is actually the entire Local Timestamp
                    // var potentialUtcOffset = new TimeSpan(replayDetailsStructure.dictionary[6].vInt.Value);

                    // [7] - Blob, Empty String
                    // [8] - Blob, Empty String
                    // [9] - Blob, Empty String
                    // [10] - Optional, Array: 0 - Blob, "s2ma"
                    // [11] - UInt, Default 0
                    // [12] - VInt, Default 4
                    // [13] - VInt, Default 1 or 7
                    // [14] - Optional, Null
                    // [15] - VInt, Default 0
                    // [16] - Optional, UInt, Default 0
                }
        }
    }
}
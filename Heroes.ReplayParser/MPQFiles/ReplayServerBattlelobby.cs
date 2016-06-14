namespace Heroes.ReplayParser
{
    using System.IO;
    using Heroes.ReplayParser.Streams;
    using System;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary> Parses the replay.server.battlelobby file in the replay file. </summary>
    public class ReplayServerBattlelobby
    {
        public const string FileName = "replay.server.battlelobby";

        /// <summary> Parses the replay.server.battlelobby file in a replay file. </summary>
        /// <param name="replay"> The replay file to apply the parsed data to. </param>
        /// <param name="buffer"> The buffer containing the replay.initdata file. </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var reader = new BitReader(stream);
                
                int arrayLength = reader.ReadByte();
                var stringLength = reader.ReadByte();
                for (var i = 0; i < arrayLength; i++)
                {
                    reader.ReadString(stringLength);
                    reader.ReadBytes(2); // Unknown
                }

                // Search for the BattleTag for each player
                var battleTagDigits = new List<char>();
                for (var playerNum = 0; playerNum < replay.Players.Length; playerNum++)
                {
                    var player = replay.Players[playerNum];
                    if (player == null)
                        continue;

                    // Find each player's name, and then their associated BattleTag
                    battleTagDigits.Clear();
                    var playerNameBytes = Encoding.UTF8.GetBytes(player.Name);
                    while (!reader.EndOfStream)
                    {
                        var isFound = true;
                        for (var i = 0; i < playerNameBytes.Length + 1; i++)
                            if ((i == playerNameBytes.Length && reader.ReadByte() != 35 /* '#' Character */) || (i < playerNameBytes.Length && reader.ReadByte() != playerNameBytes[i]))
                            {
                                isFound = false;
                                break;
                            }

                        if (isFound)
                            break;
                    }

                    // Get the digits from the BattleTag
                    while (!reader.EndOfStream)
                    {
                        var currentCharacter = (char)reader.ReadByte();

                        if (playerNum == 9 && (currentCharacter == 'z' || currentCharacter == 'Ø'))
                        {
                            // If player is in slot 9, there's a chance that an extra digit could be appended to the BattleTag
                            battleTagDigits.RemoveAt(battleTagDigits.Count - 1);
                            break;
                        }
                        else if (char.IsDigit(currentCharacter))
                            battleTagDigits.Add(currentCharacter);
                        else
                            break;
                    }

                    if (reader.EndOfStream)
                        break;

                    player.BattleTag = int.Parse(string.Join("", battleTagDigits));
                }
            }
        }
    }
}

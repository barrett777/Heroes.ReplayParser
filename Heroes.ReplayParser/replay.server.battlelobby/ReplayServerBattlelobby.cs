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

                // This is not always here; we can't blindly wait for 's2mh'
                /* while (!reader.EndOfStream)
                    if (reader.ReadString(1) == "s" && reader.ReadString(1) == "2" && reader.ReadString(1) == "m" && reader.ReadString(1) == "h")
                    {
                        reader.stream.Position -= 4;
                        break;
                    }

                if (reader.EndOfStream)
                    return;

                for (var j = 0; j < arrayLength; j++)
                {
                    reader.ReadString(4); // s2mh
                    reader.ReadBytes(2); // 0x00 0x00
                    reader.ReadBytes(2); // 'Realm'
                    reader.ReadBytes(32); // 'DepHash'
                }
                reader.ReadBytes(2); // 0x00 0x00

                // Different Skins / Artifacts / Characters - I think this is what users mouse over in the UI before the game
                arrayLength = reader.ReadInt16();
                for (var j = 0; j < arrayLength; j++)
                    reader.ReadString(reader.ReadByte());

                reader.ReadBytes(2); // 0x00 0x00
                reader.ReadInt16();

                do
                    arrayLength = reader.ReadByte();
                while (!reader.EndOfStream && (arrayLength == 0 || arrayLength == 1));

                if (reader.EndOfStream)
                    return; */

                // Now get the BattleTag for each player
                var battleTagDigits = new List<char>();
                foreach (var player in replay.Players.Where(i => i != null))
                {
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

                    // Get the numbers from the BattleTag
                    while (!reader.EndOfStream)
                    {
                        var currentCharacter = (char)reader.ReadByte();
                        if (char.IsDigit(currentCharacter))
                            battleTagDigits.Add(currentCharacter);
                        else
                            break;
                    }

                    if (reader.EndOfStream)
                        break;

                    player.BattleTag = int.Parse(string.Join("", battleTagDigits));
                }

                if (replay.Players.Any(i => i != null && i.BattleTag == 0))
                    throw new Exception("Couldn't retrieve BattleTag");
            }
        }
    }
}
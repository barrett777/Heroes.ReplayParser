namespace Heroes.ReplayParser
{
    using System.IO;
    using System.Text;
    using Heroes.ReplayParser.Streams;
    using System;

    /// <summary> Parses the replay.Initdata file in the replay file. </summary>
    public class ReplayInitData
    {
        /// <summary> Parses the replay.initdata file in a replay file. </summary>
        /// <param name="replay"> The replay file to apply the parsed data to. </param>
        /// <param name="buffer"> The buffer containing the replay.initdata file. </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var reader = new BitReader(stream);
                
                var i = reader.ReadByte();

                var playerList = new string[i];
                for (int j = 0; j < i; j++)
                {
                    playerList[j] = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(8));

                    if (reader.ReadBoolean())
                    {
                        var clanTag = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(8));
                        // Console.WriteLine(clanTag);
                    }

                    if (reader.ReadBoolean())
                        // Clan Logo
                        reader.ReadBlobPrecededWithLength(40);

                    if (reader.ReadBoolean())
                    {
                        var highestLeague = reader.Read(8);
                        // Console.WriteLine(highestLeague);
                    }

                    if (reader.ReadBoolean())
                    {
                        var combinedRaceLevels = reader.ReadInt32();
                        // Console.WriteLine(combinedRaceLevels);
                    }

                    reader.ReadInt32(); // Random seed (So far, always 0 in Heroes)

                    if (reader.ReadBoolean())
                        reader.Read(8); // Race Preference

                    if (reader.ReadBoolean())
                        reader.Read(8); // Team Preference

                    reader.ReadBoolean(); //test map
                    reader.ReadBoolean(); //test auto
                    reader.ReadBoolean(); //examine
                    reader.ReadBoolean(); //custom interface

                    var unknown1 = reader.ReadInt32();

                    reader.Read(2);       //observer

                    var unknown2 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));
                    var unknown3 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));
                    var unknown4 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));
                    var unknown5 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));
                    var unknown6 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));
                    var unknown7 = Encoding.UTF8.GetString(reader.ReadBlobPrecededWithLength(7));

                    // Console.WriteLine(unknown1 + unknown2 + unknown3 + unknown4 + unknown5 + unknown6 + unknown7);
                }

                // Marked as 'Random Value', so I will use as seed
                replay.RandomValue = (uint)reader.ReadInt32();

                reader.ReadBlobPrecededWithLength(10); // Dflt

                reader.ReadBoolean(); // Lock Teams
                reader.ReadBoolean(); // Teams Together
                reader.ReadBoolean(); // Advanced Shared Control
                reader.ReadBoolean(); // Random Races
                reader.ReadBoolean(); // BattleNet
                reader.ReadBoolean(); // AMM
                reader.ReadBoolean(); // Competitive
                reader.ReadBoolean(); // No Victory Or Defeat
                reader.ReadBoolean(); // Unknown 0
                reader.ReadBoolean(); // Unknown 1
                reader.ReadBoolean(); // Unknown 2
                reader.Read(2); // Fog
                reader.Read(2); // Observers
                reader.Read(2); // User Difficulty
                reader.ReadInt32(); reader.ReadInt32(); // 64 bit int: Client Debug Flags

                reader.Read(3); // Game Speed

                // Not sure what this 'Game Type' is
                reader.Read(3);

                var maxPlayers = reader.Read(5);
                if (maxPlayers != 10) // Max Players
                    replay.GameMode = GameMode.TryMe;

                // About 1000 bytes from here is a list of characters, character skins, character mounts, artifact selections, and other data
            }
        }
    }
}
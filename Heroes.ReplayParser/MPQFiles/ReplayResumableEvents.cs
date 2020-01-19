using System;
using System.IO;
using System.Text;

namespace Heroes.ReplayParser.MPQFiles
{
    public static class ReplayResumableEvents
    {
        public const string FileName = "replay.resumable.events";

        public static void Parse(Replay replay, byte[] buffer)
        {
            using (var memoryStream = new MemoryStream(buffer))
            using (var binaryReader = new BinaryReader(memoryStream))
                while (memoryStream.Position < memoryStream.Length)
                {
                    var timeSpan = TimeSpan.FromSeconds((int)(binaryReader.ReadInt32() / 16.0));

                    // Always 1 or 2 or 3, but doesn't seem to be useful to us
                    // Most of the time is 1
                    // 3 seems to usually be at the end of the game
                    // This seems to be the 'data' for this event, but doesn't seem to be anything useful
                    binaryReader.ReadByte();

                    Player player = null;
                    var clientListIndex = binaryReader.ReadByte();
                    if (clientListIndex != 16 /* Global Event, or event with Observer */)
                        player = replay.ClientListByUserID[clientListIndex];

                    // Team color?
                    // Team 0 is always 255 / 255 / 255
                    // Team 1 is always 35 / 35 / 35
                    binaryReader.ReadBytes(3);

                    // Always 255.  May be part of the above three bytes
                    binaryReader.ReadByte();

                    // Player name without BattleTag.  For global events, this is an empty string.  This can also contain an Observer's player name in a Custom game
                    Encoding.UTF8.GetString(binaryReader.ReadBytes(binaryReader.ReadInt16()));
                }
        }
    }
}
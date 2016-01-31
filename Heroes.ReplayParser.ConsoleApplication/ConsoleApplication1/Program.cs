using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Heroes.ReplayParser;
using Foole.Mpq;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
            var randomReplayFileName = Directory.GetFiles(heroesAccountsFolder, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid()).First();

            // Use temp directory for MpqLib directory permissions requirements
            var tmpPath = Path.GetTempFileName();
            File.Copy(randomReplayFileName, tmpPath, overwrite: true);

            try
            {
                // Attempt to parse the replay
                // Ignore errors can be set to true if you want to attempt to parse currently unsupported replays, such as 'VS AI' or 'PTR Region' replays
                var replayParseResult = DataParser.ParseReplay(tmpPath, ignoreErrors: false, deleteFile: false);

                // If successful, the Replay object now has all currently available information
                if (replayParseResult.Item1 == DataParser.ReplayParseResult.Success)
                {
                    var replay = replayParseResult.Item2;

                    Console.WriteLine("Replay Build: " + replay.ReplayBuild);
                    Console.WriteLine("Map: " + replay.Map);
                    foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                        Console.WriteLine("Player: " + player.Name + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + ", Talents: " + string.Join(",", player.Talents.OrderBy(i => i.Item1)));

                    Console.WriteLine("Press Any Key to Close");
                }
                else
                    Console.WriteLine("Failed to Parse Replay: " + replayParseResult.Item1);

                Console.Read();
            }
            finally
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private static byte[] GetMpqArchiveFileBytes(MpqArchive archive, string fileName)
        {
            using (var mpqStream = archive.OpenFile(archive.Single(i => i.Filename == fileName)))
            {
                var buffer = new byte[mpqStream.Length];
                mpqStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}

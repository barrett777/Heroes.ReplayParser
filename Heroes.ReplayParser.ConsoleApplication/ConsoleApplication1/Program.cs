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
            File.Copy(randomReplayFileName, tmpPath, true);

            try
            {
                // Create our Replay object: this object will be filled as you parse the different files in the .StormReplay archive
                var replay = new Replay();
                Heroes.ReplayParser.MpqHeader.ParseHeader(replay, tmpPath);
                using (var archive = new MpqArchive(tmpPath))
                {
                    archive.AddListfileFilenames();
                    
                    ReplayDetails.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayDetails.FileName));
                    ReplayTrackerEvents.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayTrackerEvents.FileName));
                    ReplayInitData.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayInitData.FileName), partialParse: false);
                    ReplayAttributeEvents.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayAttributeEvents.FileName));
                    if (replay.ReplayBuild >= 32455)
                        ReplayGameEvents.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayGameEvents.FileName));
                    ReplayServerBattlelobby.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayServerBattlelobby.FileName));
                    ReplayMessageEvents.Parse(replay, GetMpqArchiveFileBytes(archive, ReplayMessageEvents.FileName));
                    Unit.ParseUnitData(replay);
                }

                // Our Replay object now has all currently available information
                Console.WriteLine("Replay Build: " + replay.ReplayBuild);
                Console.WriteLine("Map: " + replay.Map);
                foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                    Console.WriteLine("Player: " + player.Name + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + (replay.ReplayBuild >= 32524 ? ", Talents: " + string.Join(",", player.Talents.OrderBy(i => i)) : ""));
                    
                Console.WriteLine("Press Any Key to Close");
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

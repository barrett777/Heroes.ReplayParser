using System.IO;
using MpqLib.Mpq;
using System;
using System.Linq;
using Heroes.ReplayParser;

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
                MpqHeader.ParseHeader(replay, tmpPath);
                using (var archive = new CArchive(tmpPath))
                {
                    ReplayInitData.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.initData"));
                    ReplayTrackerEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.tracker.events"));
                    ReplayDetails.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.details"));
                    ReplayAttributeEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.attributes.events"));
                    if (replay.ReplayBuild >= 32455)
                        ReplayGameEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.game.events"));
                    ReplayServerBattlelobby.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.server.battlelobby"));
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

        private static byte[] GetMpqArchiveFileBytes(CArchive archive, string archivedFileName)
        {
            var buffer = new byte[archive.FindFiles(archivedFileName).Single().Size];
            archive.ExportFile(archivedFileName, buffer);
            return buffer;
        }
    }
}

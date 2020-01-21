using System;
using System.IO;
using System.Linq;
using Heroes.ReplayParser;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
            var randomReplayFileName = Directory.GetFiles(heroesAccountsFolder, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid()).First();

            // Attempt to parse the replay
            // Ignore errors can be set to true if you want to attempt to parse currently unsupported replays, such as 'VS AI' or 'PTR Region' replays
            var (replayParseResult, replay) = DataParser.ParseReplay(randomReplayFileName, deleteFile: false, ParseOptions.DefaultParsing);

            // If successful, the Replay object now has all currently available information
            if (replayParseResult == DataParser.ReplayParseResult.Success)
            {
                Console.WriteLine("Replay Build: " + replay.ReplayBuild);
                Console.WriteLine("Map: " + replay.Map);
                foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                    Console.WriteLine("Player: " + player.Name + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + ", Talents: " + string.Join(",", player.Talents.Select(i => i.TalentID + ":" + i.TalentName)));

                Console.WriteLine("Press Any Key to Close");
            }
            else
                Console.WriteLine("Failed to Parse Replay: " + replayParseResult);

            Console.Read();
        }
    }
}

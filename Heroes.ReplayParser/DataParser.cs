using Foole.Mpq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heroes.ReplayParser
{
    public class DataParser
    {
        public enum ReplayParseResult
        {
            Success = 0,
            ComputerPlayerFound = 1,
            Incomplete = 2,
            Duplicate = 3,
            // ChatGlitch = 4, - Past issue that is no longer applicable
            TryMeMode = 5,
            UnexpectedResult = 9,
            Exception = 10,
            FileNotFound = 11,
            // AutoSelectBug = 12, - Past issue that is no longer applicable
            PreAlphaWipe = 13,
            FileSizeTooLarge = 14,
            PTRRegion = 15
        }

        public static Tuple<ReplayParseResult, Replay> ParseReplay(byte[] bytes, bool ignoreErrors = false)
        {
            try
            {
                var replay = new Replay();

                // File in the version numbers for later use.
                MpqHeader.ParseHeader(replay, bytes);

                if (!ignoreErrors && replay.ReplayBuild < 32455)
                    return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.PreAlphaWipe, null);

                using (var memoryStream = new MemoryStream(bytes))
                using (var archive = new MpqArchive(memoryStream))
                    ParseReplayArchive(replay, archive, ignoreErrors);

                return ParseReplayResults(replay, ignoreErrors);
            }
            catch
            {
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.Exception, null);
            }
        }

        public static Tuple<ReplayParseResult, Replay> ParseReplay(string fileName, bool ignoreErrors, bool deleteFile)
        {
            try
            {
                var replay = new Replay();

                // File in the version numbers for later use.
                MpqHeader.ParseHeader(replay, fileName);

                if (!ignoreErrors && replay.ReplayBuild < 32455)
                    return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.PreAlphaWipe, null);

                using (var archive = new MpqArchive(fileName))
                    ParseReplayArchive(replay, archive, ignoreErrors);

                if (deleteFile)
                    File.Delete(fileName);

                return ParseReplayResults(replay, ignoreErrors);
            }
            catch
            {
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.Exception, null);
            }
        }

        private static Tuple<ReplayParseResult, Replay> ParseReplayResults(Replay replay, bool ignoreErrors)
        {
            if (ignoreErrors)
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.UnexpectedResult, replay);
            else if (replay.Players.Length == 1)
                // Filter out 'Try Me' games, as they have unusual format that throws exceptions in other areas
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.TryMeMode, null);
            else if (replay.Players.Length == 5)
                // Custom game with all computer players on the opposing team won't register them as players at all (Noticed at build 34053)
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.ComputerPlayerFound, null);
            else if (replay.Players.All(i => !i.IsWinner) || replay.ReplayLength.TotalMinutes < 2)
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.Incomplete, null);
            else if (replay.Timestamp < new DateTime(2014, 10, 6, 0, 0, 0, DateTimeKind.Utc))
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.PreAlphaWipe, null);
            else if (replay.Players.Any(i => i.PlayerType == PlayerType.Computer || i.Character == "Random Hero" || i.Name.Contains(' ')))
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.ComputerPlayerFound, null);
            else if (replay.Players.Any(i => i.BattleNetRegionId >= 90 /* PTR/Test Region */))
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.PTRRegion, null);
            else if (replay.Players.Count(i => i.IsWinner) != 5 || replay.Players.Length != 10 || (replay.GameMode != GameMode.TeamLeague && replay.GameMode != GameMode.HeroLeague && replay.GameMode != GameMode.QuickMatch && replay.GameMode != GameMode.Custom))
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.UnexpectedResult, null);
            else
                return new Tuple<ReplayParseResult, Replay>(ReplayParseResult.Success, replay);
        }

        private static void ParseReplayArchive(Replay replay, MpqArchive archive, bool ignoreErrors)
        {
            archive.AddListfileFilenames();

            // Replay Details
            ReplayDetails.Parse(replay, GetMpqFile(archive, ReplayDetails.FileName));

            if (!ignoreErrors && (replay.Players.Length != 10 || replay.Players.Count(i => i.IsWinner) != 5))
                // Filter out 'Try Me' games, any games without 10 players, and incomplete games
                return;
            else if (!ignoreErrors && replay.Timestamp < new DateTime(2014, 10, 6, 0, 0, 0, DateTimeKind.Utc))
                // Technical Alpha replays
                return;

            // Replay Init Data
            ReplayInitData.Parse(replay, GetMpqFile(archive, ReplayInitData.FileName));
            
            ReplayAttributeEvents.Parse(replay, GetMpqFile(archive, ReplayAttributeEvents.FileName));

            replay.TrackerEvents = ReplayTrackerEvents.Parse(GetMpqFile(archive, ReplayTrackerEvents.FileName));

            try
            {
                replay.GameEvents = ReplayGameEvents.Parse(GetMpqFile(archive, ReplayGameEvents.FileName), replay.ClientList, replay.ReplayBuild);
            }
            catch
            {
                replay.GameEvents = new List<GameEvent>();
            }
            
            // Gather talent selections
            var talentGameEventsDictionary = replay.GameEvents
                .Where(i => i.eventType == GameEventType.CHeroTalentSelectedEvent)
                .GroupBy(i => i.player)
                .ToDictionary(
                    i => i.Key,
                    i => i.Select(j => new Tuple<int, TimeSpan>((int)j.data.unsignedInt.Value, j.TimeSpan)).OrderBy(j => j.Item2).ToArray());

            foreach (var player in talentGameEventsDictionary.Keys)
                player.Talents = talentGameEventsDictionary[player];

            // Gather Team Level Milestones (From talent choices: 1 / 4 / 7 / 10 / 13 / 16 / 20)
            for (var currentTeam = 0; currentTeam < replay.TeamLevelMilestones.Length; currentTeam++)
            {
                var maxTalentChoices = replay.Players.Where(i => i.Team == currentTeam).Select(i => i.Talents.Length).Max();
                replay.TeamLevelMilestones[currentTeam] = new TimeSpan[maxTalentChoices];
                var appropriatePlayers = replay.Players.Where(j => j.Team == currentTeam && j.Talents.Length == maxTalentChoices);
                for (var i = 0; i < replay.TeamLevelMilestones[currentTeam].Length; i++)
                    replay.TeamLevelMilestones[currentTeam][i] = appropriatePlayers.Select(j => j.Talents[i].Item2).Min();
            }

            // Replay Server Battlelobby
            if (!ignoreErrors)
                ReplayServerBattlelobby.Parse(replay, GetMpqFile(archive, ReplayServerBattlelobby.FileName));

            // Parse Unit Data using Tracker events
            Unit.ParseUnitData(replay);

            // Replay Message Events
            // ReplayMessageEvents.Parse(replay, GetMpqFile(archive, ReplayMessageEvents.FileName));

            // Replay Resumable Events
            // So far it doesn't look like this file has anything we would be interested in
            // ReplayResumableEvents.Parse(replay, GetMpqFile(archive, "replay.resumable.events"));
        }

        private static byte[] GetMpqFile(MpqArchive archive, string fileName)
        {
            using (var mpqStream = archive.OpenFile(archive.Single(i => i.Filename == fileName)))
            {
                var buffer = new byte[mpqStream.Length];
                mpqStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static bool VerifyReplayMessageEventCleared(string fileName)
        {
            using (var archive = new MpqArchive(fileName))
            {
                archive.AddListfileFilenames();
                return GetMpqFile(archive, "replay.message.events").Length == 1;
            }
        }
    }
}

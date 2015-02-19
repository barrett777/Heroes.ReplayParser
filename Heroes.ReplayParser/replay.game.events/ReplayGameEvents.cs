using System.Linq;

namespace Heroes.ReplayParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ReplayGameEvents
    {
        public static void Parse(Replay replay, byte[] buffer)
        {
            var gameEvents = new List<GameEvent>();
            var ticksElapsed = 0;
            using (var stream = new MemoryStream(buffer))
            {
                var bitReader = new Heroes.ReplayParser.Streams.BitReader(stream);
                while (!bitReader.EndOfStream)
                {
                    var gameEvent = new GameEvent();
                    ticksElapsed += (int)bitReader.Read(6 + (bitReader.Read(2) << 3));
                    gameEvent.ticksElapsed = ticksElapsed;
                    gameEvent.playerIndex = (int)bitReader.Read(5);
                    if (gameEvent.playerIndex == 16)
                        gameEvent.isGlobal = true;

                    gameEvent.eventType = (GameEventType)bitReader.Read(7);
                    switch (gameEvent.eventType)
                    {
                        case GameEventType.CStartGameEvent:
                            break;
                        case GameEventType.CUserFinishedLoadingSyncEvent:
                            break;
                        case GameEventType.CUserOptionsEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // Base Build Number
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) },
                                new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) } } };
                            break;
                        case GameEventType.CBankFileEvent:
                            gameEvent.data = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) };
                            break;
                        case GameEventType.CBankSectionEvent:
                            gameEvent.data = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(6) };
                            break;
                        case GameEventType.CBankKeyEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(6) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) },
                                new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) } } };
                            break;
                        case GameEventType.CBankSignatureEvent:
                            gameEvent.data = new TrackerEventStructure { DataType = 2, array = new TrackerEventStructure[bitReader.Read(5)] };
                            for (var i = 0; i < gameEvent.data.array.Length; i++)
                                gameEvent.data.array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(8) };
                            gameEvent.data.blob = bitReader.ReadBlobPrecededWithLength(7);
                            break;
                        case GameEventType.CCameraSaveEvent:
                            bitReader.Read(3);
                            bitReader.Read(16);
                            bitReader.Read(16);
                            break;
                        case GameEventType.CCommandManagerResetEvent:
                            bitReader.Read(32);
                            break;
                        case GameEventType.CCmdEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[5] };
                            if (replay.ReplayBuild < 33684)
                                gameEvent.data.array[0] = new TrackerEventStructure { unsignedInt = bitReader.Read(22) };
                            else
                                gameEvent.data.array[0] = new TrackerEventStructure { unsignedInt = bitReader.Read(23) };
                            if (bitReader.ReadBoolean())
                            {
                                gameEvent.data.array[1] = new TrackerEventStructure {
                                    array = new[] {
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(16) },
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(5) },
                                    new TrackerEventStructure() } };
                                if (bitReader.ReadBoolean())
                                    gameEvent.data.array[1].array[2].unsignedInt = bitReader.Read(8);
                            }
                            switch (bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // TargetPoint
                                    gameEvent.data.array[2] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                                    break;
                                case 2: // TargetUnit
                                    gameEvent.data.array[2] = new TrackerEventStructure { array = new[] {
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) },
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(8) },
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(32) },
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) },
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(), } };
                                    if (bitReader.ReadBoolean())
                                        gameEvent.data.array[2].array[4].unsignedInt = bitReader.Read(4);
                                    if (bitReader.ReadBoolean())
                                        gameEvent.data.array[2].array[5].unsignedInt = bitReader.Read(4);
                                    gameEvent.data.array[2].array[6].array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } };
                                    break;
                                case 3: // Data
                                    gameEvent.data.array[2] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                                    break;
                            }
                            if (replay.ReplayBuild >= 33684)
                                bitReader.Read(32);
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[3] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CSelectionDeltaEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { unsignedInt = bitReader.Read(4) },
                                new TrackerEventStructure { array = new[] {
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(9) },
                                    new TrackerEventStructure(),
                                    new TrackerEventStructure(),
                                    new TrackerEventStructure(),
                                    new TrackerEventStructure() } } }
                            };
                            switch (bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // Mask
                                    bitReader.Read(bitReader.Read(9));
                                    break;
                                case 2: // OneIndices
                                case 3: // ZeroIndices
                                    gameEvent.data.array[1].array[1] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(9)] };
                                    for (var i = 0; i < gameEvent.data.array[1].array[1].array.Length; i++)
                                        gameEvent.data.array[1].array[1].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(9) };
                                    break;
                            }
                            gameEvent.data.array[1].array[2] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(9)] };
                            for (var i = 0; i < gameEvent.data.array[1].array[2].array.Length; i++)
                                gameEvent.data.array[1].array[2].array[i] = new TrackerEventStructure { array = new[] {
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(16) },
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(8) },
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(8) },
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(9) } } };
                            gameEvent.data.array[1].array[3] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(9)] };
                            for (var i = 0; i < gameEvent.data.array[1].array[3].array.Length; i++)
                                gameEvent.data.array[1].array[3].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CControlGroupUpdateEvent:
                            bitReader.Read(4);
                            bitReader.Read(2);
                            switch(bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // Mask
                                    bitReader.Read(9);
                                    break;
                                case 2: // One Indices
                                    for (var i = 0; i < bitReader.Read(9); i++)
                                        bitReader.Read(9);
                                    break;
                                case 3: // Zero Indices
                                    for (var i = 0; i < bitReader.Read(9); i++)
                                        bitReader.Read(9);
                                    break;
                            }
                            break;
                        case GameEventType.CResourceTradeEvent:
                            bitReader.Read(4);
                            bitReader.Read(32);
                            bitReader.Read(32);
                            bitReader.Read(32);
                            break;
                        case GameEventType.CTriggerChatMessageEvent:
                            gameEvent.data = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(10) };
                            break;
                        case GameEventType.CTriggerPingEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 },
                                new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },
                                new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                            break;
                        case GameEventType.CUnitClickEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CTriggerSkippedEvent:
                            break;
                        case GameEventType.CTriggerSoundLengthQueryEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, new TrackerEventStructure { unsignedInt = bitReader.Read(32) } } };
                            break;
                        case GameEventType.CTriggerSoundOffsetEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CTriggerTransmissionOffsetEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 }, new TrackerEventStructure { unsignedInt = bitReader.Read(32) } } };
                            break;
                        case GameEventType.CTriggerTransmissionCompleteEvent:
                            gameEvent.data = new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 };
                            break;
                        case GameEventType.CCameraUpdateEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[6] };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, new TrackerEventStructure { unsignedInt = bitReader.Read(16) } } };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[1] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[2] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[3] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { vInt = bitReader.Read(8) - 128 };
                            gameEvent.data.array[5] = new TrackerEventStructure { unsignedInt = bitReader.Read(1) };
                            break;
                        case GameEventType.CTriggerPlanetMissionLaunchedEvent:
                            bitReader.Read(32);
                            break;
                        case GameEventType.CTriggerDialogControlEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { vInt = bitReader.Read(32) /* Actually signed - not handled correctly */ },
                                new TrackerEventStructure { vInt = bitReader.Read(32) /* Actually signed - not handled correctly */ },
                                new TrackerEventStructure() } };
                            switch (bitReader.Read(3))
                            {
                                case 0: // None
                                    break;
                                case 1: // Checked
                                    gameEvent.data.array[2].unsignedInt = bitReader.Read(1);
                                    break;
                                case 2: // ValueChanged
                                    gameEvent.data.array[2].unsignedInt = bitReader.Read(32);
                                    break;
                                case 3: // SelectionChanged
                                    gameEvent.data.array[2].vInt = bitReader.Read(32); /* Actually signed - not handled correctly */
                                    break;
                                case 4: // TextChanged
                                    gameEvent.data.array[2].DataType = 2;
                                    gameEvent.data.array[2].blob = bitReader.ReadBlobPrecededWithLength(11);
                                    break;
                                case 5: // MouseButton
                                    gameEvent.data.array[2].unsignedInt = bitReader.Read(32);
                                    break;
                            }
                            break;
                        case GameEventType.CTriggerSoundLengthSyncEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[2] };
                            gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(7)] };
                            for (var i = 0; i < gameEvent.data.array[0].array.Length; i++)
                                gameEvent.data.array[0].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            gameEvent.data.array[1] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(7)] };
                            for (var i = 0; i < gameEvent.data.array[1].array.Length; i++)
                                gameEvent.data.array[1].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CTriggerMouseClickedEvent:
                            bitReader.Read(32);
                            bitReader.ReadBoolean();
                            bitReader.Read(11);
                            bitReader.Read(11);
                            bitReader.Read(20);
                            bitReader.Read(20);
                            bitReader.Read(32);
                            bitReader.Read(8);
                            break;
                        case GameEventType.CTriggerMouseMovedEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { unsignedInt = bitReader.Read(11) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(11) },
                                new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } },
                                new TrackerEventStructure { vInt = bitReader.Read(8) - 128 } } };
                            break;
                        case GameEventType.CTriggerHotkeyPressedEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // May be missing an offset value
                            break;
                        case GameEventType.CTriggerTargetModeUpdateEvent:
                            bitReader.Read(16);
                            bitReader.Read(5);
                            bitReader.Read(8);
                            break;
                        case GameEventType.CTriggerSoundtrackDoneEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CTriggerKeyPressedEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { vInt = bitReader.Read(8) - 128 }, new TrackerEventStructure { vInt = bitReader.Read(8) - 128 } } };
                            break;
                        case GameEventType.CTriggerCutsceneBookmarkFiredEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 }, new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) } } };
                            break;
                        case GameEventType.CTriggerCutsceneEndSceneFiredEvent:
                            gameEvent.data = new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 };
                            break;
                        case GameEventType.CGameUserLeaveEvent:
                            break;
                        case GameEventType.CGameUserJoinEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[5] };
                            gameEvent.data.array[0] = new TrackerEventStructure { unsignedInt = bitReader.Read(2) };
                            gameEvent.data.array[1] = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(8) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[2] = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[3] = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(8) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBytes(40) };
                            break;
                        case GameEventType.CCommandManagerStateEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(2) };
                            if (replay.ReplayBuild >= 33684)
                                if (bitReader.ReadBoolean())
                                    bitReader.Read(32);
                            break;
                        case GameEventType.CCmdUpdateTargetPointEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                            break;
                        case GameEventType.CCmdUpdateTargetUnitEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[7] };
                            gameEvent.data.array[0] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            gameEvent.data.array[1] = new TrackerEventStructure { unsignedInt = bitReader.Read(8) };
                            gameEvent.data.array[2] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            gameEvent.data.array[3] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { unsignedInt = bitReader.Read(4) };
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[5] = new TrackerEventStructure { unsignedInt = bitReader.Read(4) };
                            gameEvent.data.array[6] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                            break;
                        case GameEventType.CHeroTalentSelectedEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CHeroTalentTreeSelectionPanelToggled:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(1) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    bitReader.AlignToByte();
                    gameEvents.Add(gameEvent);
                }
            }

            replay.GameEvents = gameEvents;

            var talentGameEvents = replay.GameEvents.Where(i => i.eventType == GameEventType.CHeroTalentSelectedEvent);
            for (var i = 0; i < replay.ClientList.Length; i++)
                if (replay.ClientList[i] != null)
                    replay.ClientList[i].Talents = talentGameEvents.Where(j => j.playerIndex == i).Select(j => (int)j.data.unsignedInt.Value).OrderBy(j => j).ToArray();

            /* var eventGroups = replay.GameEvents.GroupBy(i => i.eventType);
            string eventGroupData = "";
            foreach (var eventGroup in eventGroups)
            {
                foreach (var eventData in eventGroup)
                    eventGroupData += eventData.ToString() + "\r\n";
                File.WriteAllText(@"C:\HOTSLogs\" + eventGroup.Key + @".txt", eventGroupData);
                eventGroupData = "";
            }
            Console.WriteLine(eventGroups.Count()); */
        }
    }

    public class GameEvent
    {
        public GameEventType eventType;
        public int playerIndex;
        public bool isGlobal = false;
        public int ticksElapsed;
        public TrackerEventStructure data = null;

        public override string ToString()
        {
            return data != null ? data.ToString() : null;
        }
    }

    public enum GameEventType
    {
        CStartGameEvent = 2,
        CUserFinishedLoadingSyncEvent = 5,
        CUserOptionsEvent = 7,
        CBankFileEvent = 9,
        CBankSectionEvent = 10,
        CBankKeyEvent = 11,
        CBankSignatureEvent = 13,
        CCameraSaveEvent = 14,
        CCommandManagerResetEvent = 25,
        CCmdEvent = 27,
        CSelectionDeltaEvent = 28,
        CControlGroupUpdateEvent = 29,
        CResourceTradeEvent = 31,
        CTriggerChatMessageEvent = 32,
        CTriggerPingEvent = 36,
        CUnitClickEvent = 39,
        CTriggerSkippedEvent = 44,
        CTriggerSoundLengthQueryEvent = 45,
        CTriggerSoundOffsetEvent = 46,
        CTriggerTransmissionOffsetEvent = 47,
        CTriggerTransmissionCompleteEvent = 48,
        CCameraUpdateEvent = 49,
        CTriggerPlanetMissionLaunchedEvent = 53,
        CTriggerDialogControlEvent = 55,
        CTriggerSoundLengthSyncEvent = 56,
        CTriggerMouseClickedEvent = 58,
        CTriggerMouseMovedEvent = 59,
        CTriggerHotkeyPressedEvent = 61,
        CTriggerTargetModeUpdateEvent = 62,
        CTriggerSoundtrackDoneEvent = 64,
        CTriggerKeyPressedEvent = 66,
        CTriggerCutsceneBookmarkFiredEvent = 97,
        CTriggerCutsceneEndSceneFiredEvent = 98,
        CGameUserLeaveEvent = 101,
        CGameUserJoinEvent = 102,
        CCommandManagerStateEvent = 103,
        CCmdUpdateTargetPointEvent = 104,
        CCmdUpdateTargetUnitEvent = 105,
        CHeroTalentSelectedEvent = 110,
        CHeroTalentTreeSelectionPanelToggled = 112
    }
}

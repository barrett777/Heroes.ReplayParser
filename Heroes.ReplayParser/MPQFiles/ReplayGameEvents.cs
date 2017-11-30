using System.Linq;

namespace Heroes.ReplayParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class ReplayGameEvents
    {
        public const string FileName = "replay.game.events";

        public static List<GameEvent> Parse(byte[] buffer, Player[] clientList, int replayBuild, int replayVersionMajor)
        {
            // Referenced from https://raw.githubusercontent.com/Blizzard/heroprotocol/master/protocol39445.py

            var gameEvents = new List<GameEvent>();

            var ticksElapsed = 0;
            using (var stream = new MemoryStream(buffer))
            {
                var bitReader = new Streams.BitReader(stream);
                while (!bitReader.EndOfStream)
                {
                    var gameEvent = new GameEvent();
                    ticksElapsed += (int)bitReader.Read(6 + (bitReader.Read(2) << 3));
                    gameEvent.ticksElapsed = ticksElapsed;
                    var playerIndex = (int)bitReader.Read(5);
                    if (playerIndex == 16)
                        gameEvent.isGlobal = true;
                    else
                        gameEvent.player = clientList[playerIndex];

                    gameEvent.eventType = (GameEventType)bitReader.Read(7);
                    switch (gameEvent.eventType)
                    {
                        case GameEventType.CStartGameEvent:
                            break;
                        case GameEventType.CUserFinishedLoadingSyncEvent:
                            break;
                        case GameEventType.CUserOptionsEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_gameFullyDownloaded
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_developmentCheatsEnabled
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_testCheatsEnabled
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_multiplayerCheatsEnabled
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_syncChecksummingEnabled
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_isMapToMapTransition
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_debugPauseEnabled
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_useGalaxyAsserts
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_platformMac
                                new TrackerEventStructure { unsignedInt = bitReader.Read(1) },  // m_cameraFollow
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // m_baseBuildNum
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // m_buildNum
                                new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // m_versionFlags
                                new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(9) } /* m_hotkeyProfile */ } };
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
                            bitReader.Read(3); // m_which
                            bitReader.Read(16); // x
                            bitReader.Read(16); // y
                            break;
                        case GameEventType.CCommandManagerResetEvent:
                            bitReader.Read(32); // m_sequence
                            break;
                        case GameEventType.CGameCheatEvent:
                            // m_target
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[4] };

                            switch (bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // TargetPoint
                                    gameEvent.data.array[0] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                                    break;
                                case 2: // TargetUnit
                                    gameEvent.data.array[0] = new TrackerEventStructure {
                                        array = new[] {
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_targetUnitFlags
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(8) }, // m_timer
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // m_tag
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_snapshotUnitLink
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(), } };
                                    if (bitReader.ReadBoolean())
                                        // m_snapshotControlPlayerId
                                        gameEvent.data.array[0].array[4].unsignedInt = bitReader.Read(4);
                                    if (bitReader.ReadBoolean())
                                        // m_snapshotUpkeepPlayerId
                                        gameEvent.data.array[0].array[5].unsignedInt = bitReader.Read(4);

                                    // m_snapshotPoint
                                    gameEvent.data.array[0].array[6].array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } };
                                    break;
                            }

                            bitReader.Read(32); // m_time
                            Encoding.ASCII.GetString(bitReader.ReadBlobPrecededWithLength(10)); // m_verb
                            Encoding.ASCII.GetString(bitReader.ReadBlobPrecededWithLength(10)); // m_arguments
                            break;
                        case GameEventType.CCmdEvent:
                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[5] };

                            // m_cmdFlags
                            if (replayBuild < 33684)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[22] };
                            else if (replayBuild < 37117)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[23] };
                            else if (replayBuild < 38236)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[24] };
                            else if (replayBuild < 42958)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[25] };
                            else if (replayBuild < 44256)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[24] };
                            else if (replayBuild <= 45635)
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[26] };
                            else if(replayVersionMajor < 2)
								gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[25] };
							else if (replayBuild < 59837 || replayBuild == 59988)
								gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[26] };
							else
								gameEvent.data.array[0] = new TrackerEventStructure { array = new TrackerEventStructure[27] };

							for (var i = 0; i < gameEvent.data.array[0].array.Length; i++)
                                gameEvent.data.array[0].array[i] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(1) };

                            // m_abil
                            if (bitReader.ReadBoolean())
                            {
                                gameEvent.data.array[1] = new TrackerEventStructure {
                                    array = new[] {
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_abilLink
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(5) }, // m_abilCmdIndex
                                    new TrackerEventStructure() } };
                                if (bitReader.ReadBoolean())
                                    // m_abilCmdData
                                    gameEvent.data.array[1].array[2].unsignedInt = bitReader.Read(8);
                            }

                            // m_data
                            switch (bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // TargetPoint
                                    gameEvent.data.array[2] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };
                                    break;
                                case 2: // TargetUnit
                                    gameEvent.data.array[2] = new TrackerEventStructure { array = new[] {
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_targetUnitFlags
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(8) }, // m_timer
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(32) }, // m_tag
                                        new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_snapshotUnitLink
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(),
                                        new TrackerEventStructure(), } };
                                    if (bitReader.ReadBoolean())
                                        // m_snapshotControlPlayerId
                                        gameEvent.data.array[2].array[4].unsignedInt = bitReader.Read(4);
                                    if (bitReader.ReadBoolean())
                                        // m_snapshotUpkeepPlayerId
                                        gameEvent.data.array[2].array[5].unsignedInt = bitReader.Read(4);

                                    // m_snapshotPoint
                                    gameEvent.data.array[2].array[6].array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } };
                                    break;
                                case 3: // Data
                                    gameEvent.data.array[2] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                                    break;
                            }

                            // m_vector
                            if (replayBuild >= 44256 && bitReader.ReadBoolean())
                                new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } };

                            if (replayBuild >= 33684)
                                bitReader.Read(32); // m_sequence
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[3] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // m_otherUnit
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // m_unitGroup
                            break;
                        case GameEventType.CSelectionDeltaEvent:
							gameEvent.data = new TrackerEventStructure { array = new[] {
								new TrackerEventStructure { unsignedInt = bitReader.Read(4) }, // m_controlGroupId

								// m_delta
								new TrackerEventStructure { array = new[] {
									new TrackerEventStructure { unsignedInt = bitReader.Read(replayVersionMajor < 2 ? 9 : 5) }, // m_subgroupIndex
									new TrackerEventStructure(),
									new TrackerEventStructure(),
									new TrackerEventStructure() } } } };

                            // m_removeMask
                            switch (bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // Mask
                                    bitReader.Read(bitReader.Read(replayVersionMajor < 2 ? 9 : 6));
                                    break;
                                case 2: // OneIndices
                                case 3: // ZeroIndices
                                    gameEvent.data.array[1].array[1] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(replayVersionMajor < 2 ? 9 : 6)] };
                                    for (var i = 0; i < gameEvent.data.array[1].array[1].array.Length; i++)
                                        gameEvent.data.array[1].array[1].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(replayVersionMajor < 2 ? 9 : 5) };
                                    break;
                            }

                            // m_addSubgroups
                            gameEvent.data.array[1].array[2] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(replayVersionMajor < 2 ? 9 : 6)] };
                            for (var i = 0; i < gameEvent.data.array[1].array[2].array.Length; i++)
                                gameEvent.data.array[1].array[2].array[i] = new TrackerEventStructure { array = new[] {
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, // m_unitLink
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(8) }, // m_subgroupPriority
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(8) }, // m_intraSubgroupPriority
                                    new TrackerEventStructure { unsignedInt = bitReader.Read(replayVersionMajor < 2 ? 9 : 6) } } }; // m_count

                            // m_addUnitTags
                            gameEvent.data.array[1].array[3] = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(replayVersionMajor < 2 ? 9 : 6)] };
                            for (var i = 0; i < gameEvent.data.array[1].array[3].array.Length; i++)
                                gameEvent.data.array[1].array[3].array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CControlGroupUpdateEvent:
                            bitReader.Read(4); // m_controlGroupIndex

                            // m_controlGroupUpdate
                            if (replayBuild < 36359) // Not sure exactly when this change happened - roughly around here.  This primarily affected 'The Lost Vikings' hero
                                bitReader.Read(2);
                            else
                                bitReader.Read(3);

							// m_mask
							switch(bitReader.Read(2))
                            {
                                case 0: // None
                                    break;
                                case 1: // Mask
                                    bitReader.Read(bitReader.Read(replayVersionMajor < 2 ? 9 : 6));
                                    break;
                                case 2: // OneIndices
                                case 3: // ZeroIndices
                                    gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[bitReader.Read(replayVersionMajor < 2 ? 9 : 6)] };
                                    for (var i = 0; i < gameEvent.data.array.Length; i++)
                                        gameEvent.data.array[i] = new TrackerEventStructure { unsignedInt = bitReader.Read(replayVersionMajor < 2 ? 9 : 5) };
                                    break;
                            }
                            break;
                        case GameEventType.CSelectionSyncCheckEvent:
                            bitReader.Read(4); // m_controlGroupId

                            // m_selectionSyncData
							if (replayVersionMajor < 2)
							{
								bitReader.Read(9); // m_count
								bitReader.Read(9); // m_subgroupCount
								bitReader.Read(9); // m_activeSubgroupIndex
							}
							else
							{
								bitReader.Read(6); // m_count
								bitReader.Read(6); // m_subgroupCount
								bitReader.Read(5); // m_activeSubgroupIndex
							}
                            
                            bitReader.Read(32); // m_unitTagsChecksum
                            bitReader.Read(32); // m_subgroupIndicesChecksum
                            bitReader.Read(32); // m_subgroupsChecksum
                            break;
                        case GameEventType.CResourceTradeEvent:
                            bitReader.Read(4); // m_recipientId
                            bitReader.Read(32); // m_resources, should be offset -2147483648
                            bitReader.Read(32); // m_resources, should be offset -2147483648
                            bitReader.Read(32); // m_resources, should be offset -2147483648
                            break;
                        case GameEventType.CTriggerChatMessageEvent:
                            gameEvent.data = new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(10) };
                            break;
                        case GameEventType.CSetAbsoluteGameSpeedEvent:
                            bitReader.Read(3); // m_speed
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
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // m_unitTag
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
                                // m_target, x/y
                                gameEvent.data.array[0] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(16) }, new TrackerEventStructure { unsignedInt = bitReader.Read(16) } } };
                            if (bitReader.ReadBoolean())
                                // m_distance
                                gameEvent.data.array[1] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                // m_pitch
                                gameEvent.data.array[2] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                // m_yaw
                                gameEvent.data.array[3] = new TrackerEventStructure { unsignedInt = bitReader.Read(16) };
                            if (bitReader.ReadBoolean())
                                // m_reason
                                gameEvent.data.array[4] = new TrackerEventStructure { vInt = bitReader.Read(8) - 128 };

                            // m_follow
                            gameEvent.data.array[5] = new TrackerEventStructure { unsignedInt = bitReader.Read(1) };
                            break;
                        case GameEventType.CTriggerPlanetMissionLaunchedEvent:
                            bitReader.Read(32); // m_difficultyLevel, offset -2147483648
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
                                case 5: // MouseButton or MouseEvent
									if(replayBuild == 57547 || replayBuild > 57589)
										gameEvent.data.array[2].array = new[]
										{
											new TrackerEventStructure { vInt = bitReader.Read(16) }, // m_button
											new TrackerEventStructure { vInt = bitReader.Read(16) } // m_metaKeyFlags
										};
									else
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
                        case GameEventType.CTriggerConversationSkippedEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(1) };
                            break;
                        case GameEventType.CTriggerMouseClickedEvent:
                            bitReader.Read(32); // m_button
                            bitReader.ReadBoolean(); // m_down
                            bitReader.Read(11); // m_posUI X
                            bitReader.Read(11); // m_posUI Y
                            bitReader.Read(20); // m_posWorld X
                            bitReader.Read(20); // m_posWorld Y
                            bitReader.Read(32); // m_posWorld Z (Offset -2147483648)
                            bitReader.Read(8); // m_flags (-128)
                            break;
                        case GameEventType.CTriggerMouseMovedEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] {
								// m_posUI
                                new TrackerEventStructure { unsignedInt = bitReader.Read(11) },
                                new TrackerEventStructure { unsignedInt = bitReader.Read(11) },

								// m_posWorld
                                new TrackerEventStructure { array = new[] { new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 } } },

								// m_flags
                                new TrackerEventStructure { vInt = bitReader.Read(8) - 128 } } };
                            break;
                        case GameEventType.CTriggerHotkeyPressedEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // May be missing an offset value
                            break;
                        case GameEventType.CTriggerTargetModeUpdateEvent:
                            bitReader.Read(16); // m_abilLink
                            bitReader.Read(5); // m_abilCmdIndex
                            bitReader.Read(8); // m_state (-128)
                            break;
                        case GameEventType.CTriggerSoundtrackDoneEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) };
                            break;
                        case GameEventType.CTriggerKeyPressedEvent:
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { vInt = bitReader.Read(8) - 128 }, new TrackerEventStructure { vInt = bitReader.Read(8) - 128 } } };
                            break;
                        case GameEventType.CTriggerCutsceneBookmarkFiredEvent:
                            // m_cutsceneId, m_bookmarkName
                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 }, new TrackerEventStructure { DataType = 2, blob = bitReader.ReadBlobPrecededWithLength(7) } } };
                            break;
                        case GameEventType.CTriggerCutsceneEndSceneFiredEvent:
                            // m_cutsceneId
                            gameEvent.data = new TrackerEventStructure { vInt = bitReader.Read(32) - 2147483648 };
                            break;
                        case GameEventType.CGameUserLeaveEvent:
							// m_leaveReason
							if(replayBuild >= 55929)
								bitReader.Read(5);
							else
								bitReader.Read(4);
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
                            gameEvent.data = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(2) }; // m_state
                            if (replayBuild >= 33684)
                                if (bitReader.ReadBoolean())
                                    // m_sequence
                                    gameEvent.data.array = new[] { new TrackerEventStructure { DataType = 9, vInt = bitReader.Read(8) }, new TrackerEventStructure { DataType = 9, vInt = bitReader.Read(8) }, new TrackerEventStructure { DataType = 9, vInt = bitReader.Read(16) } };
                            break;
                        case GameEventType.CCmdUpdateTargetPointEvent:
                            if (replayBuild >= 40336 && bitReader.ReadBoolean())
                                bitReader.Read(32);

                            gameEvent.data = new TrackerEventStructure { array = new[] { new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { DataType = 9, vInt = bitReader.Read(32) - 2147483648 } } };
                            break;
                        case GameEventType.CCmdUpdateTargetUnitEvent:
                            if (replayBuild >= 40336 && bitReader.ReadBoolean())
                                bitReader.Read(32);

                            gameEvent.data = new TrackerEventStructure { array = new TrackerEventStructure[7] };
                            gameEvent.data.array[0] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(16) }; // m_targetUnitFlags
                            gameEvent.data.array[1] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(8) }; // m_timer
                            gameEvent.data.array[2] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(32) }; // m_tag
                            gameEvent.data.array[3] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(16) }; // m_snapshotUnitLink
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[4] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(4) }; // m_snapshotControlPlayerId
                            if (bitReader.ReadBoolean())
                                gameEvent.data.array[5] = new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(4) }; // m_snapshotUpkeepPlayerId
                            gameEvent.data.array[6] = new TrackerEventStructure { array = new[] { new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { DataType = 7, unsignedInt = bitReader.Read(20) }, new TrackerEventStructure { DataType = 9, vInt = bitReader.Read(32) - 2147483648 } } }; // m_snapshotPoint (x, y, z)
                            break;
                        case GameEventType.CHeroTalentSelectedEvent:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(32) }; // m_index
                            break;
                        case GameEventType.CHeroTalentTreeSelectionPanelToggled:
                            gameEvent.data = new TrackerEventStructure { unsignedInt = bitReader.Read(1) }; // m_shown
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    bitReader.AlignToByte();
                    gameEvents.Add(gameEvent);
                }
            }

            // Uncomment this to write out all replay.game.events to individual text files in the 'C:\HOTSLogs\' folder
            /* var eventGroups = gameEvents.GroupBy(i => i.eventType).Select(i => new { EventType = i.Key, EventCount = i.Count(), Events = i.OrderBy(j => j.TimeSpan).ToArray() }).ToArray();
            foreach (var eventGroup in eventGroups)
                File.WriteAllLines(@"C:\HOTSLogs\" + (int) eventGroup.EventType + " " + eventGroup.EventType + @".txt", eventGroup.Events.Select(i => i.TimeSpan + ": " + i.player + ": " + i).ToArray()); */

            return gameEvents;
        }
    }

    public class GameEvent
    {
        public GameEventType eventType;
        public Player player = null;
        public bool isGlobal = false;
        public int ticksElapsed;
        public TimeSpan TimeSpan { get { return new TimeSpan(0, 0, (int)(ticksElapsed / 16.0)); } }
        public TrackerEventStructure data = null;

        public override string ToString()
        {
            return data != null ? data.ToString() : null;
        }
    }

    public enum GameEventType
    {
        CStartGameEvent = 2,
        CDropOurselvesEvent = 3,
        CUserFinishedLoadingSyncEvent = 5,
        CUserOptionsEvent = 7,
        CBankFileEvent = 9,
        CBankSectionEvent = 10,
        CBankKeyEvent = 11,
        CBankSignatureEvent = 13,
        CCameraSaveEvent = 14,
        CCommandManagerResetEvent = 25,
        CGameCheatEvent = 26,
        CCmdEvent = 27,
        CSelectionDeltaEvent = 28,
        CControlGroupUpdateEvent = 29,
        CSelectionSyncCheckEvent = 30,
        CResourceTradeEvent = 31,
        CTriggerChatMessageEvent = 32,
        CSetAbsoluteGameSpeedEvent = 34,
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
        CTriggerConversationSkippedEvent = 57,
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

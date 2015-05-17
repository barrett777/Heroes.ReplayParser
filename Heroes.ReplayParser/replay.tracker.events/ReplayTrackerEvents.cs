using System.Collections.Generic;
using System.Linq;

namespace Heroes.ReplayParser
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Parses the replay.tracker.events file in the MPQ Archive
    /// </summary>
    public static class ReplayTrackerEvents
    {
        /// <summary> Parses the replay.tracker.events file, applying it to a Replay object. </summary>
        /// <param name="replay"> The replay object to apply the parsed information to. </param>
        /// <param name="buffer"> The buffer containing the replay.tracker.events file. </param>
        /// <param name="parseUnitData"> Determines whether or not to parse unit data </param>
        public static void Parse(Replay replay, byte[] buffer)
        {
            replay.TrackerEvents = new List<TrackerEvent>();

            var currentFrameCount = 0;
            using (var stream = new MemoryStream(buffer))
                using (var reader = new BinaryReader(stream))
                    while (stream.Position < stream.Length)
                    {
                        var intro = reader.ReadBytes(3); // Always 03 00 09 (Edit: Middle digit seems to have at least two possible values)
                        if (intro[0] != 3 || /* intro[1] != 0 || */ intro[2] != 9)
                            throw new Exception("Unexpected data in tracker event");

                        currentFrameCount += (int)TrackerEventStructure.read_vint(reader);

                        var trackerEvent = new TrackerEvent { TimeSpan = new TimeSpan(0, 0, (int)(currentFrameCount / 16.0)) };

                        intro = reader.ReadBytes(1); // Always 09
                        if (intro[0] != 9)
                            throw new Exception("Unexpected data in tracker event");

                        trackerEvent.TrackerEventType = (TrackerEventType)TrackerEventStructure.read_vint(reader);
                        trackerEvent.Data = new TrackerEventStructure(reader);
                        replay.TrackerEvents.Add(trackerEvent);
                    }

            replay.Frames = currentFrameCount;
            replay.ReplayLength = replay.TrackerEvents.Last().TimeSpan;

            // Populate the client list using player indexes
            var playerIndexes = replay.TrackerEvents.Where(i => i.TrackerEventType == ReplayTrackerEvents.TrackerEventType.PlayerSetupEvent && i.Data.dictionary[2].optionalData != null).Select(i => i.Data.dictionary[2].optionalData.vInt.Value).Distinct().OrderBy(i => i).ToArray();
            for (var i = 0; i < playerIndexes.Length; i++)
                // The references between both of these classes are the same on purpose.
                // We want updates to one to propogate to the other.
                replay.ClientList[playerIndexes[i]] = replay.Players[i];
        }

        public enum TrackerEventType
        {
            PlayerStatsEvent = 0,
            /* From sc2reader: Player Stats events are generated for all players that were in the game even if they've since
                left every 10 seconds. An additional set of stats events are generated at the end of the game.
                When a player leaves the game, a single PlayerStatsEvent is generated for that player and no
                one else. That player continues to generate PlayerStatsEvents at 10 second intervals until the
                end of the game.
                In 1v1 games, the above behavior can cause the losing player to have 2 events generated at the
                end of the game. One for leaving and one for the end of the game. */

            UnitBornEvent = 1,
            /* UnitID Index, UnitID Recycle, Unit Type Name, PlayerID with Control, PlayerID with Upkeep, X, Y */

            UnitDiedEvent = 2,
            /* UnitID Index, UnitID Recycle, PlayerID that Killed This, X, Y, Killing UnitID Index, Killing UnitID Recycle */

            UnitOwnerChangeEvent = 3,
            /* UnitID Index, UnitID Recycle, New PlayerID with Control, New PlayerID with Upkeep */

            UnitTypeChangeEvent = 4,
            /* UnitID Index, UnitID Recycle, New Unit Type Name */

            UnknownTrackerEvent1 = 5,
            /* Only one event for the entire match, with a blob containing 'CreepColor'; Also seems to contain who gets Dragon on Dragon Shire */

            UnitPositionsEvent = 8,
            /* First UnitID Index, Items Array (UnitID Index Offset, X, Y) */

            PlayerSetupEvent = 9
            /* PlayerID, Player Type (1=Human, 2=CPU, 3=Neutral, 4=Hostile), UserID, SlotID */
        }
    }

    /// <summary>
    /// Defines a single replay tracker event.
    /// </summary>
    public class TrackerEvent
    {
        /// <summary> Gets or sets the tracker event type. </summary>
        public ReplayTrackerEvents.TrackerEventType TrackerEventType { get; set; }

        /// <summary> Gets or sets the timespan of when the event occurred. </summary>
        public TimeSpan TimeSpan { get; set; }

        /// <summary> Gets or sets the data of the event. </summary>
        public TrackerEventStructure Data { get; set; }

        public override string ToString()
        {
            return TrackerEventType.ToString() + ": " + Data.ToString();
        }
    }

    public class TrackerEventStructure
    {
        public int DataType;
        public TrackerEventStructure[] array = null;
        public Dictionary<int, TrackerEventStructure> dictionary = null;
        public byte[] blob = null;
        public string blobText { get { return blob != null ? Encoding.UTF8.GetString(blob) : null; } }
        public int? choiceFlag = null;
        public TrackerEventStructure choiceData = null;
        public TrackerEventStructure optionalData = null;
        public ulong? unsignedInt = null;
        public long? vInt = null;

        public TrackerEventStructure()
        {

        }

        public TrackerEventStructure(BinaryReader reader)
        {
            DataType = reader.ReadByte();
            switch (DataType)
            {
                case 0x00: // array
                    array = new TrackerEventStructure[read_vint(reader)];
                    for (var i = 0; i < array.Length; i++)
                        array[i] = new TrackerEventStructure(reader);
                    break;
                case 0x01: // bitarray, weird alignment requirements - haven't seen it used yet so not spending time on it
                    /*  bits = self.read_vint()
                        data = self.read_bits(bits) */
                    throw new NotImplementedException();
                case 0x02: // blob
                    blob = reader.ReadBytes((int) read_vint(reader));
                    break;
                case 0x03: // choice
                    choiceFlag = (int) read_vint(reader);
                    choiceData = new TrackerEventStructure(reader);
                    break;
                case 0x04: // optional
                    if (reader.ReadByte() != 0)
                        optionalData = new TrackerEventStructure(reader);
                    break;
                case 0x05: // struct
                    dictionary = new Dictionary<int, TrackerEventStructure>();
                    var dictionarySize = read_vint(reader);
                    for (var i = 0; i < dictionarySize; i++)
                        dictionary[(int) read_vint(reader)] = new TrackerEventStructure(reader);
                    break;
                case 0x06: // u8
                    unsignedInt = reader.ReadByte();
                    break;
                case 0x07: // u32
                    unsignedInt = reader.ReadUInt32();
                    break;
                case 0x08: // u64
                    unsignedInt = reader.ReadUInt64();
                    break;
                case 0x09: // vint
                    vInt = read_vint(reader);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static TrackerEventStructure GetTrackerEventStructure(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
                using (var reader = new BinaryReader(memoryStream))
                    return new TrackerEventStructure(reader);
        }

        public override string ToString()
        {
            switch (DataType)
            {
                case 0x00: // array
                    if (array == null)
                        return null;
                    var returnStringArray = "Array: ";
                    for (var i = 0; i < array.Length; i++)
                        returnStringArray += i + " (" + array[i] + "), ";
                    return returnStringArray.Substring(0, returnStringArray.Length - 2);
                case 0x01: // bitarray, weird alignment requirements
                    throw new NotImplementedException();
                case 0x02: // blob
                    return "Blob: " + blobText;
                case 0x03: // choice
                    return "Choice: Flag: " + choiceFlag + ", Data: " + choiceData;
                case 0x04: // optional
                    return "Optional: " + optionalData;
                case 0x05: // struct
                    var returnStringDictionary = "Dictionary: ";
                    foreach (var key in dictionary.Keys)
                        returnStringDictionary += key + " (" + dictionary[key] + "), ";
                    return returnStringDictionary.Substring(0, returnStringDictionary.Length - 2);
                case 0x06: // u8
                case 0x07: // u32
                case 0x08: // u64
                    return "UInt: " + unsignedInt;
                case 0x09: // vint
                    return "VInt: " + vInt;
                default:
                    throw new NotImplementedException();
            }
        }

        public static long read_vint(BinaryReader reader)
        {
            // Reads a signed integer of variable length
            // Code from https://github.com/ascendedguard/sc2replay-csharp
            long l2 = 0;
            for (var k = 0;; k += 7)
            {
                long l1 = reader.ReadByte();
                l2 |= (l1 & 0x7F) << k;
                if ((l1 & 0x80) == 0)
                    return (l2 & 1L) > 0L ? -(l2 >> 1) : l2 >> 1;
            }
        }
    }
}
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
        public const string FileName = "replay.tracker.events";

        /// <summary> Parses the replay.tracker.events file </summary>
        /// <param name="buffer"> The buffer containing the replay.tracker.events file. </param>
        public static List<TrackerEvent> Parse(byte[] buffer)
        {
            var trackerEvents = new List<TrackerEvent>();

            var currentFrameCount = 0;
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
                while (stream.Position < stream.Length)
                {
                    reader.ReadBytes(3); // Always 03 ?? 09; Middle digit seems to have at least two possible values

                    currentFrameCount += (int)TrackerEventStructure.read_vint(reader);

                    var trackerEvent = new TrackerEvent { TimeSpan = TimeSpan.FromSeconds((int)(currentFrameCount / 16.0)) };

                    reader.ReadBytes(1); // Always 09

                    trackerEvent.TrackerEventType = (TrackerEventType)TrackerEventStructure.read_vint(reader);
                    trackerEvent.Data = new TrackerEventStructure(reader);

                    if (trackerEvent.TrackerEventType == TrackerEventType.StatGameEvent && trackerEvent.Data.dictionary[3].optionalData != null)
                        // m_fixedData is stored in fixed point 20.12 format
                        foreach (var trackerEventArrayItem in trackerEvent.Data.dictionary[3].optionalData.array)
                            trackerEventArrayItem.dictionary[1].vInt = trackerEventArrayItem.dictionary[1].vInt.Value / 4096;

                    trackerEvents.Add(trackerEvent);
                }

            return trackerEvents;
        }

        public enum TrackerEventType
        {
            UnitBornEvent = 1,
            /* UnitID Index, UnitID Recycle, Unit Type Name, PlayerID with Control, PlayerID with Upkeep, X, Y */

            UnitDiedEvent = 2,
            /* UnitID Index, UnitID Recycle, PlayerID that Killed This, X, Y, Killing UnitID Index, Killing UnitID Recycle */

            UnitOwnerChangeEvent = 3,
            /* UnitID Index, UnitID Recycle, New PlayerID with Control, New PlayerID with Upkeep */

            UnitTypeChangeEvent = 4,
            /* UnitID Index, UnitID Recycle, New Unit Type Name */

            UpgradeEvent = 5,
            /* PlayerID, Upgrade Type Name, Count */

            UnitInitEvent = 6,
            /* UnitID, Unit Type Name, PlayerID with Control, PlayerID with Upkeep, X, Y */

            UnitDoneEvent = 7,
            /* UnitID */

            UnitPositionsEvent = 8,
            /* First UnitID Index, Items Array (UnitID Index Offset, X, Y) */

            PlayerSetupEvent = 9,
            /* PlayerID, Player Type (1=Human, 2=CPU, 3=Neutral, 4=Hostile), UserID, SlotID */

            StatGameEvent = 10,
            /* EventName, StringData, InitData, FixedData */

            ScoreResultEvent = 11,
            /* InstanceList (20+ length array of Name/Value pairs) */

            UnitRevivedEvent = 12,
			/* UnitID, X, Y */

			HeroBannedEvent = 13,
			/* Hero, ControllingTeam */

			HeroPickedEvent = 14,
			/* Hero, ControllingPlayer */

			HeroSwappedEvent = 15
			/* Hero, NewControllingPlayer */
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
                    return array != null
                        ? '[' + string.Join(", ", array.Select(i => i != null ? i.ToString() : null)) + ']'
                        : null;
                case 0x01: // bitarray, weird alignment requirements, hasn't been used yet
                    throw new NotImplementedException();
                case 0x02: // blob
                    return '"' + blobText + '"';
                case 0x03: // choice
                    return "Choice: Flag: " + choiceFlag + ", Data: " + choiceData;
                case 0x04: // optional
                    return optionalData != null
                        ? optionalData.ToString()
                        : null;
                case 0x05: // struct
                    return '{' + string.Join(", ", dictionary.Values.Select(i => i != null ? i.ToString() : null)) + '}';
                case 0x06: // u8
                case 0x07: // u32
                case 0x08: // u64
                    return unsignedInt.HasValue
                        ? unsignedInt.Value.ToString()
                        : null;
                case 0x09: // vint
                    return vInt.HasValue
                        ? vInt.Value.ToString()
                        : null;
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
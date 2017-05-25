namespace Heroes.ReplayParser
{
    using System;
    using System.IO;
    using System.Text;

    public class ReplayMessageEvents
    {
        public const string FileName = "replay.message.events";

        /// <summary> Parses the Replay.Messages.Events file. </summary>
        /// <param name="buffer"> Buffer containing the contents of the replay.messages.events file. </param>
        /// <returns> A list of messages parsed from the buffer. </returns>
        public static void Parse(Replay replay, byte[] buffer)
        {
            if (buffer.Length <= 1)
                // Chat has been removed from this replay
                return;

            var ticksElapsed = 0;
            using (var stream = new MemoryStream(buffer))
            {
                var bitReader = new Streams.BitReader(stream);

                while (!bitReader.EndOfStream)
                {
                    var message = new Message();

                    ticksElapsed += (int)bitReader.Read(6 + (bitReader.Read(2) << 3));
                    message.Timestamp = new TimeSpan(0, 0, (int) Math.Round(ticksElapsed / 16.0));

                    var playerIndex = (int)bitReader.Read(5);
                    if (playerIndex != 16)
                    {
                        message.MessageSender = replay.ClientListByUserID[playerIndex];
                        message.PlayerIndex = playerIndex;
                    }

                    message.MessageEventType = (MessageEventType)bitReader.Read(4);
                    switch (message.MessageEventType)
                    {
                        case MessageEventType.SChatMessage:
                            {
                                ChatMessage chatMessage = new ChatMessage();

                                chatMessage.MessageTarget = (MessageTarget)bitReader.Read(3); // m_recipient (the target)
                                chatMessage.Message = Encoding.UTF8.GetString(bitReader.ReadBlobPrecededWithLength(11)); // m_string

                                message.ChatMessage = chatMessage;
                                replay.Messages.Add(message);
                                break;
                            }
                        case MessageEventType.SPingMessage:
                            {
                                PingMessage pingMessage = new PingMessage();

                                pingMessage.MessageTarget = (MessageTarget)bitReader.Read(3); // m_recipient (the target) 

                                pingMessage.XCoordinate = bitReader.ReadInt32() - (-2147483648); // m_point x
                                pingMessage.YCoordinate = bitReader.ReadInt32() - (-2147483648); // m_point y

                                message.PingMessage = pingMessage;
                                replay.Messages.Add(message);
                                break;
                            }
                        case MessageEventType.SLoadingProgressMessage:
                            {
                                // can be used to keep track of how fast/slow players are loading
                                // also includes players who are reloading the game
                                var progress = bitReader.ReadInt32() - (-2147483648); // m_progress
                                break;
                            }
                        case MessageEventType.SServerPingMessage:
                            {
                                break;
                            }
                        case MessageEventType.SReconnectNotifyMessage:
                            {
                                bitReader.Read(2); // m_status; is either a 1 or a 2
                                break;
                            }
                        case MessageEventType.SPlayerAnnounceMessage:
                            {
                                PlayerAnnounceMessage announceMessage = new PlayerAnnounceMessage();

                                announceMessage.AnnouncementType = (AnnouncementType)bitReader.Read(2);

                                switch (announceMessage.AnnouncementType)
                                {
                                    case AnnouncementType.None:
                                        {
                                            break;
                                        }
                                    case AnnouncementType.Ability:
                                        {
                                            AbilityAnnouncment ability = new AbilityAnnouncment();
                                            ability.AbilityLink = bitReader.ReadInt16(); // m_abilLink      
                                            ability.AbilityIndex = (int)bitReader.Read(5); // m_abilCmdIndex
                                            ability.ButtonLink = bitReader.ReadInt16(); // m_buttonLink

                                            announceMessage.AbilityAnnouncement = ability;
                                            break;
                                        }

                                    case AnnouncementType.Behavior: // no idea what triggers this
                                        {
                                            bitReader.ReadInt16(); // m_behaviorLink
                                            bitReader.ReadInt16(); // m_buttonLink
                                            break;
                                        }
                                    case AnnouncementType.Vitals:
                                        {
                                            VitalAnnouncment vital = new VitalAnnouncment();
                                            vital.VitalType = (VitalType)(bitReader.ReadInt16() - (-32768));

                                            announceMessage.VitalAnnouncement = vital;
                                            break;
                                        }
                                    default:
                                        throw new NotImplementedException();
                                }

                                if (replay.ReplayBuild > 45635)
                                    // m_announceLink
                                    bitReader.ReadInt16();

                                bitReader.ReadInt32(); // m_otherUnitTag
                                bitReader.ReadInt32(); // m_unitTag

                                message.PlayerAnnounceMessage = announceMessage;
                                replay.Messages.Add(message);
                                break;
                            }
                        default:
                            throw new NotImplementedException();
                    }

                    bitReader.AlignToByte();
                }
            }
        }

        public enum MessageEventType
        {
            SChatMessage = 0,
            SPingMessage = 1,
            SLoadingProgressMessage = 2,
            SServerPingMessage = 3,
            SReconnectNotifyMessage = 4,
            SPlayerAnnounceMessage = 5
        }

        // m_announcement
        public enum AnnouncementType
        {
            None = 0,
            Ability = 1,
            Behavior = 2,
            Vitals = 3
        }

        public enum MessageTarget
        {
            All = 0,
            Allies = 1,
            Observers = 4,
        }

        public enum VitalType
        {
            Health = 0,
            Mana = 2 // also includes Fury and Brew
        }

        public class AbilityAnnouncment
        {
            public int AbilityIndex { get; set; }
            public int AbilityLink { get; set; }
            public int ButtonLink { get; set; }
        }

        public class BehaviorAnnouncment
        { }

        public class VitalAnnouncment
        {
            public VitalType VitalType { get; set; }
        }

        public class ChatMessage
        {
            public MessageTarget MessageTarget { get; set; }
            public string Message { get; set; }
        }

        // Ping messages include normal pings (no target), targeted pings (such as Player 1 wants to help Player 2), retreat,
        // and the more ping options (on my way, defend, danger, assist)
        // does not include captured camps, hearthing
        // no way to tell which one is which
        public class PingMessage
        {
            public MessageTarget MessageTarget { get; set; }
            public int XCoordinate { get; set; }
            public int YCoordinate { get; set; }
        }

        public class PlayerAnnounceMessage
        {
            public AbilityAnnouncment AbilityAnnouncement { get; set; }
            public BehaviorAnnouncment BehaviorAnnouncement { get; set; }
            public VitalAnnouncment VitalAnnouncement { get; set; }
            public AnnouncementType AnnouncementType { get; set; }
        }
    }
}

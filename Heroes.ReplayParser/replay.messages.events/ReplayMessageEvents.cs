namespace Heroes.ReplayParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Handles all I/O involving the replay.message.events file, which contains in-game chat.
    /// </summary>
    public class ReplayMessageEvents
    {
        /// <summary> Parses the Replay.Messages.Events file. </summary>
        /// <param name="buffer"> Buffer containing the contents of the replay.messages.events file. </param>
        /// <returns> A list of chat messages parsed from the buffer. </returns>
        public static void Parse(Replay replay, byte[] buffer)
        {
            var messages = new List<ChatMessage>();
            using (var stream = new MemoryStream(buffer))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int totalTime = 0;
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // While not EOF
                        var message = new ChatMessage();

                        var time = ParseTimestamp(reader);
                        message.PlayerId = reader.ReadByte();

                        totalTime += time;
                        var opCode = reader.ReadByte();

                        if (opCode == 0x80)
                            reader.ReadBytes(4);
                        else if (opCode == 0x83)
                            reader.ReadBytes(8);
                        else if (opCode == 2 && message.PlayerId <= 10)
                        {
                            if (message.PlayerId == 80)
                                continue;

                            message.MessageTarget = (ChatMessageTarget)(opCode & 7);
                            var length = reader.ReadByte();

                            if ((opCode & 8) == 8)
                                length += 64;

                            if ((opCode & 16) == 16)
                                length += 128;

                            message.Message = Encoding.UTF8.GetString(reader.ReadBytes(length));
                        }
                        else
                        {
                            
                        }

                        if (message.Message != null)
                        {
                            message.Timestamp = new TimeSpan(0, 0, (int)Math.Round(totalTime / 16.0));
                            messages.Add(message);
                        }
                    }
                }
            }

            replay.ChatMessages = messages;
        }

        /// <summary> Reads a Timestamp object, returning the value and incrementing the reader. </summary>
        /// <param name="reader"> The reader, at the position of the Timestamp object.  </param>
        /// <returns> The integer value in the timestamp object.  </returns>
        internal static int ParseTimestamp(BinaryReader reader)
        {
            byte one = reader.ReadByte();
            if ((one & 3) > 0)
            {
                int two = reader.ReadByte();
                two = (short)(((one >> 2) << 8) | two);

                if ((one & 3) >= 2)
                {
                    var tmp = reader.ReadByte();
                    two = (two << 8) | tmp;

                    if ((one & 3) == 3)
                    {
                        tmp = reader.ReadByte();
                        two = (two << 8) | tmp;
                    }
                }

                return two;
            }

            return one >> 2;
        }
    }
}
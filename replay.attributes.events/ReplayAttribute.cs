namespace Heroes.ReplayParser
{
    using System;
    using System.Linq;

    public class ReplayAttribute
    {
        public int Header { get; set; }
        public Heroes.ReplayParser.ReplayAttributeEvents.ReplayAttributeEventType AttributeType { get; set; }
        public int PlayerId { get; set; }
        public byte[] Value { get; set; }

        public static ReplayAttribute Parse(byte[] buffer, int offset)
        {
            var attribute = new ReplayAttribute
            {
                Header = BitConverter.ToInt32(buffer, offset),
                AttributeType = (Heroes.ReplayParser.ReplayAttributeEvents.ReplayAttributeEventType)BitConverter.ToInt32(buffer, offset + 4),
                PlayerId = buffer[offset + 8],
                Value = new byte[4],
            };

            Array.Copy(buffer, offset + 9, attribute.Value, 0, 4);

            return attribute;
        }

        public override string ToString()
        {
            return "Player: " + PlayerId + ", AttributeType: " + AttributeType.ToString() + ", Value: " + System.Text.Encoding.UTF8.GetString(Value.Reverse().ToArray());
        }
    }
}

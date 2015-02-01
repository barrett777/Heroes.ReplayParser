using System;
namespace Heroes.ReplayParser
{
    public class TimelineEvent
    {
        public TimeSpan TimeSpan { get; set; }
        public TimelineEventType TimelineEventType { get; set; }
        public int PlayerID { get; set; }
        public decimal Value { get; set; }
    }

    public enum TimelineEventType
    {
        MapMechanicDragonShireDragon
    }
}

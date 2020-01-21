using System;
using System.Collections.Generic;
using System.Text;

namespace Heroes.ReplayParser
{
    /// <summary>
    /// Parse options, represented as a class.
    /// 
    /// Default options are automatically set, typical use would
    /// be to either use one of the provided static option sets,
    /// or to set your own option set in the initializer
    /// 
    /// i.e. new ParseOption() {
    /// ShouldParseUnits = true
    /// }
    /// </summary>
    public class ParseOptions
    {
        public bool IgnoreErrors { get; set; } = false;
        public bool AllowPTR { get; set; } = false;
        public bool ShouldParseEvents { get; set; } = true;
        public bool ShouldParseUnits { get; set; } = false;
        public bool ShouldParseMouseEvents { get; set; } = false;
        public bool ShouldParseDetailedBattleLobby { get; set; } = true;
        public bool ShouldParseMessageEvents { get; set; } = false;
        public bool ShouldParseStatistics { get; set; } = true;

        /// <summary>
        /// Parsing with all options enabled
        /// </summary>
        public static ParseOptions FullParsing => new ParseOptions()
        {
            AllowPTR = true,
            ShouldParseDetailedBattleLobby = true,
            ShouldParseMouseEvents = true,
            ShouldParseUnits = true,
            ShouldParseMessageEvents = true,
        };

        /// <summary>
        /// Parse as little as possible
        /// </summary>
        public static ParseOptions MinimalParsing => new ParseOptions()
        {
            ShouldParseEvents = false,
            ShouldParseDetailedBattleLobby = false
        };

        /// <summary>
        /// Parsing for typical needs, excludes events, units and mouseevents.
        /// </summary>
        public static ParseOptions TypicalParsing => new ParseOptions();

    }
}

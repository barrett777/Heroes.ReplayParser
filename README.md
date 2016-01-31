# Heroes.ReplayParser
A C# library for parsing Heroes of the Storm replay files (.StormReplay).

Currently developed and used by HOTSLogs.com.

Explanation
================

There is a 'Parse' function in 'DataParser.cs', which parses each file in the .StormReplay container.  Here's a quick summary of contained files we can parse:

**Mpq Header**: Replay Version and Heroes of the Storm Build are stored here

**Replay Init Data**: A lot of game and player options

**Replay Details**: Most of the player details are found here

**Replay Tracker Events**: This has tons of good information on units and statistics

**Replay Attribute Events**: Most of the information in this file is likely also defined elsewhere, but the format of this file is well defined, so it is most convenient to retrieve, and most resistant to new version format changes

**Replay Game Events**: This has every user action performed in game.  Unfortunately it would take a lot of work to get valuable information from this, and may be impossible.  For example, this may tell us that a player tries to use an ability, but it doesn't tell us if someone else interrupted the ability, or if perhaps the player is out of range.

Currently I use this to get Hero talent choices, and estimating some player movement based on some actions.

**Replay Server Battlelobby**: I'm not able to properly parse this, and I'm not sure exactly what data it contains.  Currently I'm only using this to get player's BattleTags, using some horrible, horrible code.  This wasn't mentioned much in Starcraft 2 projects, so I assume it isn't too interesting.

Example Code
================

I've included a simple Console application that shows how to parse replays and access the available data

Special Thanks
================

Most of my progress is a direct result of other people's efforts with the Starcraft 2 replay file format.  Of particular importance is this C# parser which was basically the foundation of this project: https://github.com/ascendedguard/sc2replay-csharp

These projects have also been very helpful in referencing different areas of the replay file format:

https://github.com/Blizzard/heroprotocol

https://github.com/Blizzard/s2protocol

https://github.com/GraylinKim/sc2reader

https://code.google.com/p/phpsc2replay
# Heroes.ReplayParser
A C# library for parsing Heroes of the Storm replay files (.StormReplay).

Currently developed and used by HOTSLogs.com.

Explanation
================

There is a separate 'Parse' function for each file within the .StormReplay container.  Please follow the parsing order in the sample Console application, as some files rely on parsed information from others.  Here is a quick summary of each file we can parse:

**MpqHeader.ParseHeader**

Replay Version and Heroes of the Storm Build are stored here

**ReplayInitData.Parse**

This contains a lot of game and player options

**ReplayDetails.Parse**

This is where most of the player details are found, as well as the map and timestamp the match occurred.

**ReplayTrackerEvents.Parse**

This has a lot of good information on in game Units, and also has placeholders for in game statistics.  I expect Blizzard will fill in the statistics at some point, like they did with Starcraft 2 after 'Heart of the Swarm.'

**ReplayAttributeEvents.Parse**

Most of the information in this file is likely also defined elsewhere, but the format of this file is well defined, so it is most convenient to retrieve, and most resistant to new version format changes.

**ReplayGameEvents.Parse**

This has every user action performed in game.  Unfortunately it would take a lot of work to get valuable information from this, and may be impossible.  For example, this may tell us that a Player tries to use an Ability, but it doesn't tell us if someone else interrupted the Ability, or perhaps the player is out of range.

Currently I use this to get Hero talent choices, and estimating some player movement based on some actions.

**ReplayServerBattlelobby.Parse**

I'm not able to properly parse this.  Currently I'm only using this to get player's BattleTags, using some horrible, horrible code.  There may be other interesting information, I'm not sure.  This wasn't mentioned much in Starcraft 2 projects though.

Example Code
================

I've added a Console application that demonstrates how to parse replays and access the available data.

Special Thanks
================

Most of my progress is a direct result of other people's efforts with the Starcraft 2 replay file format.  Of particular importance is this C# parser which was basically the foundation of this project: https://github.com/ascendedguard/sc2replay-csharp

These projects have also been very helpful in referencing different areas of the replay file format:

https://github.com/Blizzard/heroprotocol

https://github.com/Blizzard/s2protocol

https://github.com/GraylinKim/sc2reader

https://code.google.com/p/phpsc2replay
# Heroes.ReplayParser
A C# library for parsing Heroes of the Storm replay files (.StormReplay).

Currently developed and used by HOTSLogs.com.

**Requirements:** An external tool to extract files from the .StormReplay container, such as MpqLib.dll

Explanation
================

This project is not near complete.  In fact, I've decided to open it to the public in hopes of others helping to complete this.  Many areas of this code skip over unknown bytes of unknown structure, or skip over unknown information in a known structure.  It would be great to know what everything is for, and store useful information in the appropriate Replay or Player objects.

Currently each file within the .StormReplay container has a separate parse function, which you will pass a 'Replay' object, and the bytes from that inner file, however you obtain it.  Currently order does matter, as some rely on information from others.  Here is an example order of parsing, and some explanation of information obtained in each:

**MpqHeader.ParseHeader**

Structure is known, most information is known

Replay Version and Heroes of the Storm Build are stored here

**ReplayInitData.Parse**

Structure is mostly unknown, most information is unknown

I believe this contains hero skins, mounts, and color tints, which would be neat to parse

Lots of game options as well, though most of this is based on Starcraft 2 and probably quite different now

**ReplayDetails.Parse**

Structure is known, most information is known

This is where most of the player details are found, as well as the map and timestamp the match occurred.

**ReplayTrackerEvents.Parse**

Structure is known, most information is known

This currently has a lot of placeholders, but I expect Blizzard will fill this in, once development is further along.

**ReplayAttributeEvents.Parse**

Structure is known, most information is known

Most of the information in this file is likely also defined elsewhere, but the format of this file is well known so it is most convenient to retrieve, and most resistant to new version format changes.

**ReplayGameEvents.Parse**

Structure is known, some information is known

I was pretty hasty in my implementation of these game events.  Many of these events are pretty sloppy.  I hurried because all I really wanted was hero talent choices.

**ReplayServerBattlelobby.Parse**

Structure is mostly unknown, minimal information is known

Currently I'm only using this to get player's BattleTags, using some horrible, horrible code.  There may be other interesting information, I'm not sure.  This wasn't mentioned much in Starcraft 2 projects.

Example Code
================

I've added a console application that demonstrates how to use Heroes.ReplayParser and MpqLib to parse replays and access the available data.

Special Thanks
================

Most of my progress is a direct result of other people's efforts with the Starcraft 2 replay file format.  Of particular importance is this C# parser which was basically the foundation of this project: https://github.com/ascendedguard/sc2replay-csharp

These projects have also been very helpful in referecing different areas of the replay file format:

https://github.com/Blizzard/s2protocol

https://github.com/GraylinKim/sc2reader

https://code.google.com/p/phpsc2replay
namespace Heroes.ReplayParser
{
    using System;
    using System.IO;

    /// <summary> Parses the header at the beginning of the MPQ file structure. </summary>
    public static class MpqHeader
    {
        /// <summary>
        /// Parses the MPQ header on a file to determine version and build numbers.
        /// </summary>
        /// <param name="replay">Replay object to store </param>
        /// <param name="filename">Filename of the file to open.</param>
        public static void ParseHeader(Replay replay, string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open))
                using (var reader = new BinaryReader(fileStream))
                    ParseHeader(replay, reader);
        }

        public static void ParseHeader(Replay replay, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
                using (var reader = new BinaryReader(memoryStream))
                    ParseHeader(replay, reader);
        }

        private static void ParseHeader(Replay replay, BinaryReader reader)
        {
            reader.ReadBytes(3); // 'Magic'
            reader.ReadByte(); // Format
            BitConverter.ToInt32(reader.ReadBytes(4), 0); // Data Max Size
            BitConverter.ToInt32(reader.ReadBytes(4), 0); // Header Offset
            BitConverter.ToInt32(reader.ReadBytes(4), 0); // User Data Header Size

            var headerStructure = new TrackerEventStructure(reader);

            // [0] = Blob, "Heroes of the Storm replay 11" - Strange backward arrow before 11 as well.  I don't think the '11' will change, as I believe it was also always '11' in Starcraft 2 replays.

            replay.ReplayVersion = string.Format("{0}.{1}.{2}.{3}", headerStructure.dictionary[1].dictionary[0].vInt.Value, headerStructure.dictionary[1].dictionary[1].vInt.Value, headerStructure.dictionary[1].dictionary[2].vInt.Value, headerStructure.dictionary[1].dictionary[3].vInt.Value);

			replay.ReplayBuild = (int)headerStructure.dictionary[1].dictionary[4].vInt.Value;

			if(replay.ReplayBuild >= 51978)
				replay.ReplayVersionMajor = (int)headerStructure.dictionary[1].dictionary[1].vInt.Value;
			else
				replay.ReplayVersionMajor = 1;

			if (replay.ReplayBuild >= 39951)
                // 'm_dataBuildNum' may have always been incremented for these smaller 'hotfix' patches, but build 39951 is the first time I've noticed where a Hero's available talent selection has changed in one of these smaller patches
                // We probably want to use this as the most precise build number from now on
                replay.ReplayBuild = (int)headerStructure.dictionary[6].vInt.Value;

            // [2] = VInt, Default 2 - m_type

            replay.Frames = (int)headerStructure.dictionary[3].vInt.Value; // m_elapsedGameLoops

            // [4] = VInt, Default 0 - m_useScaledTime
            // [5] = Depending on replay build, either Blob with gibberish, or array of 16 bytes (basically a Blob), also with gibberish.  Of ~770 pre-wipe replays, there were only 11 distinct blobs, so this is likely a map version hash or something - 'm_ngdpRootKey'
            // [6] = Replay Build (Usually the same as what is in [1], but can be incremented by itself for smaller 'hotfix' patches) - m_dataBuildNum
            // [7] = m_fixedFileHash (Renamed to m_replayCompatibilityHash in build 49582 - same data type)
        }
    }
}

//
// MpqEntry.cs
//
// Authors:
//		Foole (fooleau@gmail.com)
//
// (C) 2006 Foole (fooleau@gmail.com)
// Based on code from StormLib by Ladislav Zezula
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;

namespace Foole.Mpq
{
    [Flags]
    public enum MpqFileFlags : uint
    {
        CompressedPK = 0x100, // AKA Imploded
        CompressedMulti = 0x200,
        Compressed = 0xff00,
        Encrypted = 0x10000,
        BlockOffsetAdjustedKey = 0x020000, // AKA FixSeed
        SingleUnit = 0x1000000,
        FileHasMetadata = 0x04000000, // Appears in WoW 1.10 or newer.  Indicates the file has associated metadata.
        Exists = 0x80000000
    }

    public class MpqEntry
    {
        public uint CompressedSize { get; private set; }
        public uint FileSize { get; private set; }
        public MpqFileFlags Flags { get; internal set; }
        public uint EncryptionSeed { get; internal set; }

        private uint _fileOffset; // Relative to the header offset
        internal uint FilePos { get; private set; } // Absolute position in the file
        private string _filename;

        public static readonly uint Size = 16;

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                EncryptionSeed = CalculateEncryptionSeed();
            }
        }

        public MpqEntry(BinaryReader br, uint headerOffset)
        {
            _fileOffset = br.ReadUInt32();
            FilePos = headerOffset + _fileOffset;
            CompressedSize = br.ReadUInt32();
            FileSize = br.ReadUInt32();
            Flags = (MpqFileFlags)br.ReadUInt32();
            EncryptionSeed = 0;
        }

        private uint CalculateEncryptionSeed()
        {
            if (Filename == null) return 0;

            uint seed = MpqArchive.HashString(Path.GetFileName(Filename), 0x300);
            if ((Flags & MpqFileFlags.BlockOffsetAdjustedKey) == MpqFileFlags.BlockOffsetAdjustedKey)
                seed = (seed + _fileOffset) ^ FileSize;
            return seed;
        }

        public override string ToString()
        {
            if (Filename == null)
            {
                if (!Exists)
                    return "(Deleted file)";
                return string.Format("Unknown file @ {0}", FilePos);
            }
            return Filename;
        }

        public bool IsEncrypted
        {
            get
            {
                return (Flags & MpqFileFlags.Encrypted) != 0;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return (Flags & MpqFileFlags.Compressed) != 0;
            }
        }

        public bool Exists
        {
            get { return Flags != 0; }
        }

        public bool IsSingleUnit
        {
            get { return (Flags & MpqFileFlags.SingleUnit) != 0; }
        }

        // For debugging
        public int FlagsAsInt
        {
            get { return (int)Flags; }
        }
    }
}

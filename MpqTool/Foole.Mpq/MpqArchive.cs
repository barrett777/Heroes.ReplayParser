//
// MpqArchive.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Foole.Mpq
{
	public class MpqArchive : IDisposable, IEnumerable<MpqEntry>
	{
		private MpqHeader _mpqHeader;
		private long _headerOffset;
		private MpqHash[] _hashes;
		private MpqEntry[] _entries;
		
		private static uint[] sStormBuffer;

        internal Stream BaseStream { get; private set; }
        internal int BlockSize { get; private set; }

		static MpqArchive()
		{
			sStormBuffer = BuildStormBuffer();
		}

		public MpqArchive(string filename)
		{
			BaseStream = File.Open(filename, FileMode.Open, FileAccess.Read);
			Init();
		}
		
		public MpqArchive(Stream sourceStream)
		{
            BaseStream = sourceStream;
			Init();
		}

        public MpqArchive(Stream sourceStream, bool loadListfile)
        {
            BaseStream = sourceStream;
            Init();
            if (loadListfile)
                AddListfileFilenames();
        }

		public void Dispose()
		{
            if (BaseStream != null)
                BaseStream.Close();
		}

		private void Init()
		{
			if (LocateMpqHeader() == false)
                throw new MpqParserException("Unable to find MPQ header");

            if (_mpqHeader.HashTableOffsetHigh != 0 || _mpqHeader.ExtendedBlockTableOffset != 0 || _mpqHeader.BlockTableOffsetHigh != 0)
                throw new MpqParserException("MPQ format version 1 features are not supported");

            BinaryReader br = new BinaryReader(BaseStream);

            BlockSize = 0x200 << _mpqHeader.BlockSize;

			// Load hash table
            BaseStream.Seek(_mpqHeader.HashTablePos, SeekOrigin.Begin);
			byte[] hashdata = br.ReadBytes((int)(_mpqHeader.HashTableSize * MpqHash.Size));
			DecryptTable(hashdata, "(hash table)");

			BinaryReader br2 = new BinaryReader(new MemoryStream(hashdata));
			_hashes = new MpqHash[_mpqHeader.HashTableSize];

			for (int i = 0; i < _mpqHeader.HashTableSize; i++)
				_hashes[i] = new MpqHash(br2);

			// Load entry table
            BaseStream.Seek(_mpqHeader.BlockTablePos, SeekOrigin.Begin);
			byte[] entrydata = br.ReadBytes((int)(_mpqHeader.BlockTableSize * MpqEntry.Size));
			DecryptTable(entrydata, "(block table)");

			br2 = new BinaryReader(new MemoryStream(entrydata));
			_entries = new MpqEntry[_mpqHeader.BlockTableSize];

			for (int i = 0; i < _mpqHeader.BlockTableSize; i++)
                _entries[i] = new MpqEntry(br2, (uint)_headerOffset);
		}
		
		private bool LocateMpqHeader()
		{
            BinaryReader br = new BinaryReader(BaseStream);

			// In .mpq files the header will be at the start of the file
			// In .exe files, it will be at a multiple of 0x200
            for (long i = 0; i < BaseStream.Length - MpqHeader.Size; i += 0x200)
			{
                BaseStream.Seek(i, SeekOrigin.Begin);
				_mpqHeader = MpqHeader.FromReader(br);
                if (_mpqHeader != null)
                {
					_headerOffset = i;
                    _mpqHeader.SetHeaderOffset(_headerOffset);
					return true;
				}
			}
			return false;
		}
		
		public MpqStream OpenFile(string filename)
		{
			MpqHash hash;
			MpqEntry entry;

			if (!TryGetHashEntry(filename, out hash))
				throw new FileNotFoundException("File not found: " + filename);

            entry = _entries[hash.BlockIndex];
            if (entry.Filename == null)
                entry.Filename = filename;

            return new MpqStream(this, entry);
		}

        public MpqStream OpenFile(MpqEntry entry)
        {
            return new MpqStream(this, entry);
        }

		public bool FileExists(string filename)
		{
			MpqHash hash;
            
            return TryGetHashEntry(filename, out hash);
		}

        public bool AddListfileFilenames()
        {
            if (!AddFilename("(listfile)")) return false;

            using (Stream s = OpenFile("(listfile)"))
                AddFilenames(s);

            return true;
        }

        public void AddFilenames(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                while (!sr.EndOfStream)
                    AddFilename(sr.ReadLine());
            }
        }

        public bool AddFilename(string filename)
        {
            MpqHash hash;
            if (!TryGetHashEntry(filename, out hash)) return false;

            _entries[hash.BlockIndex].Filename = filename;
            return true;
        }

        public MpqEntry this[int index]
        {
            get { return _entries[index]; }
        }

        public MpqEntry this[string filename]
        {
            get 
            {
                MpqHash hash;
                if (!TryGetHashEntry(filename, out hash)) return null;
                return _entries[hash.BlockIndex];
            }
        }

        public int Count
        { 
            get { return _entries.Length; } 
        }

        public MpqHeader Header
        {
            get { return _mpqHeader; }
        }

		private bool TryGetHashEntry(string filename, out MpqHash hash)
		{
			uint index = HashString(filename, 0);
			index  &= _mpqHeader.HashTableSize - 1;
			uint name1 = HashString(filename, 0x100);
			uint name2 = HashString(filename, 0x200);

			for(uint i = index; i < _hashes.Length; ++i)
			{
				hash = _hashes[i];
                if (hash.Name1 == name1 && hash.Name2 == name2)
                    return true;
			}
            for (uint i = 0; i < index; i++)
            {
                hash = _hashes[i];
                if (hash.Name1 == name1 && hash.Name2 == name2)
                    return true;
            }

            hash = new MpqHash();
            return false;
		}

		internal static uint HashString(string input, int offset)
		{
			uint seed1 = 0x7fed7fed;
			uint seed2 = 0xeeeeeeee;
			
			foreach(char c in input)
			{
				int val = (int)char.ToUpper(c);
				seed1 = sStormBuffer[offset + val] ^ (seed1 + seed2);
				seed2 = (uint)val + seed1 + seed2 + (seed2 << 5) + 3;
			}
			return seed1;
		}
		
		// Used for Hash Tables and Block Tables
		internal static void DecryptTable(byte[] data, string key)
		{
			DecryptBlock(data, HashString(key, 0x300));
		}

		internal static void DecryptBlock(byte[] data, uint seed1)
		{
			uint seed2 = 0xeeeeeeee;

			// NB: If the block is not an even multiple of 4,
			// the remainder is not encrypted
			for (int i = 0; i < data.Length - 3; i += 4)
			{
				seed2 += sStormBuffer[0x400 + (seed1 & 0xff)];

				uint result = BitConverter.ToUInt32(data, i);
				result ^= (seed1 + seed2);

				seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
				seed2 = result + seed2 + (seed2 << 5) + 3;

				data[i + 0] = ((byte)(result & 0xff));
				data[i + 1] = ((byte)((result >> 8) & 0xff));
				data[i + 2] = ((byte)((result >> 16) & 0xff));
				data[i + 3] = ((byte)((result >> 24) & 0xff));
			}
		}
		
		internal static void DecryptBlock(uint[] data, uint seed1)
		{
			uint seed2 = 0xeeeeeeee;

			for (int i = 0; i < data.Length; i++)
			{
				seed2 += sStormBuffer[0x400 + (seed1 & 0xff)];
				uint result = data[i];
				result ^= seed1 + seed2;

				seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
				seed2 = result + seed2 + (seed2 << 5) + 3;
				data[i] = result;
			}
		}

        // This function calculates the encryption key based on
		// some assumptions we can make about the headers for encrypted files
		internal static uint DetectFileSeed(uint value0, uint value1, uint decrypted)
		{
            uint temp = (value0 ^ decrypted) - 0xeeeeeeee;

            for (int i = 0; i < 0x100; i++)
            {
                uint seed1 = temp - sStormBuffer[0x400 + i];
                uint seed2 = 0xeeeeeeee + sStormBuffer[0x400 + (seed1 & 0xff)];
                uint result = value0 ^ (seed1 + seed2);

                if (result != decrypted)
                    continue;

                uint saveseed1 = seed1;

                // Test this result against the 2nd value
                seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
                seed2 = result + seed2 + (seed2 << 5) + 3;

                seed2 += sStormBuffer[0x400 + (seed1 & 0xff)];
                result = value1 ^ (seed1 + seed2);

                if ((result & 0xfffc0000) == 0)
                    return saveseed1;
            }
            return 0;
        }

        private static uint[] BuildStormBuffer()
		{
			uint seed = 0x100001;
			
			uint[] result = new uint[0x500];
			
			for(uint index1 = 0; index1 < 0x100; index1++)
			{
				uint index2 = index1;
				for(int i = 0; i < 5; i++, index2 += 0x100)
				{
					seed = (seed * 125 + 3) % 0x2aaaab;
					uint temp = (seed & 0xffff) << 16;
					seed = (seed * 125 + 3) % 0x2aaaab;

					result[index2]  = temp | (seed & 0xffff);
				}
			}

			return result;
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator<MpqEntry> IEnumerable<MpqEntry>.GetEnumerator()
        {
            foreach (MpqEntry entry in _entries)
                yield return entry;
        }
    }

    // TODO: Possibly incorporate this into MpqArchive
    public class MpqHeader
    {
        public uint ID { get; private set; } // Signature.  Should be 0x1a51504d
        public uint DataOffset { get; private set; } // Offset of the first file.  AKA Header size
        public uint ArchiveSize { get; private set; }
        public ushort MpqVersion { get; private set; } // Most are 0.  Burning Crusade = 1
        public ushort BlockSize { get; private set; } // Size of file block is 0x200 << BlockSize
        public uint HashTablePos { get; private set; }
        public uint BlockTablePos { get; private set; }
        public uint HashTableSize { get; private set; }
        public uint BlockTableSize { get; private set; }

        // Version 1 fields
        // The extended block table is an array of Int16 - higher bits of the offests in the block table.
        public Int64 ExtendedBlockTableOffset { get; private set; }
        public short HashTableOffsetHigh { get; private set; }
        public short BlockTableOffsetHigh { get; private set; }


        public static readonly uint MpqId = 0x1a51504d;
        public static readonly uint Size = 32;

        public static MpqHeader FromReader(BinaryReader br)
        {
            uint id = br.ReadUInt32();
            if (id != MpqId) return null;
            MpqHeader header = new MpqHeader
            {
                ID = id,
                DataOffset = br.ReadUInt32(),
                ArchiveSize = br.ReadUInt32(),
                MpqVersion = br.ReadUInt16(),
                BlockSize = br.ReadUInt16(),
                HashTablePos = br.ReadUInt32(),
                BlockTablePos = br.ReadUInt32(),
                HashTableSize = br.ReadUInt32(),
                BlockTableSize = br.ReadUInt32(),
            };

            if (header.MpqVersion == 1)
            {
                header.ExtendedBlockTableOffset = br.ReadInt64();
                header.HashTableOffsetHigh = br.ReadInt16();
                header.BlockTableOffsetHigh = br.ReadInt16();
            }

            return header;
        }

        public void SetHeaderOffset(long headerOffset)
        {
            HashTablePos += (uint)headerOffset;
            BlockTablePos += (uint)headerOffset;
            if (DataOffset == 0x6d9e4b86) // A protected archive.  Seen in some custom wc3 maps.
                DataOffset = (uint)(MpqHeader.Size + headerOffset);
        }
    }

    internal struct MpqHash
    {
        public uint Name1 { get; private set; }
        public uint Name2 { get; private set; }
        public uint Locale { get; private set; }
        public uint BlockIndex { get; private set; }

        public static readonly uint Size = 16;

        public MpqHash(BinaryReader br)
            : this()
        {
            Name1 = br.ReadUInt32();
            Name2 = br.ReadUInt32();
            Locale = br.ReadUInt32(); // Normally 0 or UInt32.MaxValue (0xffffffff)
            BlockIndex = br.ReadUInt32();
        }
    }
}

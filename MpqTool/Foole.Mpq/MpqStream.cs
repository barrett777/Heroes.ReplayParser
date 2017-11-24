// Support 4 different decompression libraries: DotNetZip, bzip2.net, SharpCompress, SharpZipLib
// Listed in order of decreasing performance, SharpZipLib is considerably slower than the others
//#define WITH_DOTNETZIP
//#define WITH_BZIP2NET
//#define WITH_SHARPCOMPRESS
//#define WITH_SHARPZIPLIB


//
// MpqHuffman.cs
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
    /// <summary>
    /// A Stream based class for reading a file from an MPQ file
    /// </summary>
    public class MpqStream : Stream
    {
        private Stream _stream;
        private int _blockSize;

        private MpqEntry _entry;
        private uint[] _blockPositions;

        private long _position;
        private byte[] _currentData;
        private int _currentBlockIndex = -1;

        internal MpqStream(MpqArchive archive, MpqEntry entry)
        {
            _entry = entry;

            _stream = archive.BaseStream;
            _blockSize = archive.BlockSize;

            if (_entry.IsCompressed && !_entry.IsSingleUnit)
                LoadBlockPositions();
        }

        // Compressed files start with an array of offsets to make seeking possible
        private void LoadBlockPositions()
        {
            int blockposcount = (int)((_entry.FileSize + _blockSize - 1) / _blockSize) + 1;
            // Files with metadata have an extra block containing block checksums
            if ((_entry.Flags & MpqFileFlags.FileHasMetadata) != 0)
                blockposcount++;

            _blockPositions = new uint[blockposcount];

            lock(_stream)
            {
                _stream.Seek(_entry.FilePos, SeekOrigin.Begin);
                BinaryReader br = new BinaryReader(_stream);
                for(int i = 0; i < blockposcount; i++)
                    _blockPositions[i] = br.ReadUInt32();
            }

            uint blockpossize = (uint) blockposcount * 4;

            /*
            if(_blockPositions[0] != blockpossize)
                _entry.Flags |= MpqFileFlags.Encrypted;
             */

            if (_entry.IsEncrypted)
            {
                if (_entry.EncryptionSeed == 0)  // This should only happen when the file name is not known
                {
                    _entry.EncryptionSeed = MpqArchive.DetectFileSeed(_blockPositions[0], _blockPositions[1], blockpossize) + 1;
                    if (_entry.EncryptionSeed == 1)
                        throw new MpqParserException("Unable to determine encyption seed");
                }

                MpqArchive.DecryptBlock(_blockPositions, _entry.EncryptionSeed - 1);

                if (_blockPositions[0] != blockpossize)
                    throw new MpqParserException("Decryption failed");
                if (_blockPositions[1] > _blockSize + blockpossize)
                    throw new MpqParserException("Decryption failed");
            }
        }

        private byte[] LoadBlock(int blockIndex, int expectedLength)
        {
            uint offset;
            int toread;
            uint encryptionseed;

            if (_entry.IsCompressed)
            {
                offset = _blockPositions[blockIndex];
                toread = (int)(_blockPositions[blockIndex + 1] - offset);
            }
            else
            {
                offset = (uint)(blockIndex * _blockSize);
                toread = expectedLength;
            }
            offset += _entry.FilePos;

            byte[] data = new byte[toread];
            lock (_stream)
            {
                _stream.Seek(offset, SeekOrigin.Begin);
                int read = _stream.Read(data, 0, toread);
                if (read != toread)
                    throw new MpqParserException("Insufficient data or invalid data length");
            }

            if (_entry.IsEncrypted && _entry.FileSize > 3)
            {
                if (_entry.EncryptionSeed == 0)
                    throw new MpqParserException("Unable to determine encryption key");

                encryptionseed = (uint)(blockIndex + _entry.EncryptionSeed);
                MpqArchive.DecryptBlock(data, encryptionseed);
            }

            if (_entry.IsCompressed && (toread != expectedLength))
            {
                if ((_entry.Flags & MpqFileFlags.CompressedMulti) != 0)
                    data = DecompressMulti(data, expectedLength);
                else
                    data = PKDecompress(new MemoryStream(data), expectedLength);
            }

            return data;
        }

        #region Stream overrides
        public override bool CanRead
        { get { return true; } }

        public override bool CanSeek
        { get { return true; } }

        public override bool CanWrite
        { get { return false; } }

        public override long Length
        { get { return _entry.FileSize; } }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
            // NOP
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long target;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    target = offset;
                    break;
                case SeekOrigin.Current:
                    target = Position + offset;
                    break;
                case SeekOrigin.End:
                    target = Length + offset;
                    break;
                default:
                    throw new ArgumentException("Origin", "Invalid SeekOrigin");
            }

            if (target < 0)
                throw new ArgumentOutOfRangeException("Attmpted to Seek before the beginning of the stream");
            if (target >= Length)
                throw new ArgumentOutOfRangeException("Attmpted to Seek beyond the end of the stream");

            _position = target;

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength is not supported");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_entry.IsSingleUnit)
                return ReadInternalSingleUnit(buffer, offset, count);

            int toread = count;
            int readtotal = 0;

            while (toread > 0)
            {
                int read = ReadInternal(buffer, offset, toread);
                if (read == 0) break;
                readtotal += read;
                offset += read;
                toread -= read;
            }
            return readtotal;
        }

        // SingleUnit entries can be compressed but are never encrypted
        private int ReadInternalSingleUnit(byte[] buffer, int offset, int count)
        {
            if (_position >= Length)
                return 0;

            if (_currentData == null)
                LoadSingleUnit();

            int bytestocopy = Math.Min((int)(_currentData.Length - _position), count);

            Array.Copy(_currentData, _position, buffer, offset, bytestocopy);

            _position += bytestocopy;
            return bytestocopy;
        }

        private void LoadSingleUnit()
        {
            // Read the entire file into memory
            byte[] filedata = new byte[_entry.CompressedSize];
            lock (_stream)
            {
                _stream.Seek(_entry.FilePos, SeekOrigin.Begin);
                int read = _stream.Read(filedata, 0, filedata.Length);
                if (read != filedata.Length)
                    throw new MpqParserException("Insufficient data or invalid data length");
            }

            if (_entry.CompressedSize == _entry.FileSize)
                _currentData = filedata;
            else
                _currentData = DecompressMulti(filedata, (int)_entry.FileSize);
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            // OW: avoid reading past the contents of the file
            if (_position >= Length)
                return 0;
            
            BufferData();

            int localposition = (int)(_position % _blockSize);
            int bytestocopy = Math.Min(_currentData.Length - localposition, count);
            if (bytestocopy <= 0) return 0;

            Array.Copy(_currentData, localposition, buffer, offset, bytestocopy);

            _position += bytestocopy;
            return bytestocopy;
        }

        public override int ReadByte()
        {
            if (_position >= Length) return -1;

            if (_entry.IsSingleUnit)
                return ReadByteSingleUnit();

            BufferData();

            int localposition = (int)(_position % _blockSize);
            _position++;
            return _currentData[localposition];
        }

        private int ReadByteSingleUnit()
        {
            if (_currentData == null)
                LoadSingleUnit();

            return _currentData[_position++];
        }

        private void BufferData()
        {
            int requiredblock = (int)(_position / _blockSize);
            if (requiredblock != _currentBlockIndex)
            {
                int expectedlength = (int)Math.Min(Length - (requiredblock * _blockSize), _blockSize);
                _currentData = LoadBlock(requiredblock, expectedlength);
                _currentBlockIndex = requiredblock;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Writing is not supported");
        }
        #endregion Strem overrides

        /* Compression types in order:
		 *  10 = BZip2
		 *   8 = PKLib
		 *   2 = ZLib
		 *   1 = Huffman
		 *  80 = IMA ADPCM Stereo
		 *  40 = IMA ADPCM Mono
		 */
        private static byte[] DecompressMulti(byte[] input, int outputLength)
        {
            Stream sinput = new MemoryStream(input);

            byte comptype = (byte)sinput.ReadByte();

            // WC3 onward mosly use Zlib
            // Starcraft 1 mostly uses PKLib, plus types 41 and 81 for audio files
            switch (comptype)
            {
                case 1: // Huffman
                    return MpqHuffman.Decompress(sinput).ToArray();
                case 2: // ZLib/Deflate
                    return ZlibDecompress(sinput, outputLength);
                case 8: // PKLib/Impode
                    return PKDecompress(sinput, outputLength);
                case 0x10: // BZip2
                    return BZip2Decompress(sinput, outputLength);
                case 0x80: // IMA ADPCM Stereo
                    return MpqWavCompression.Decompress(sinput, 2);
                case 0x40: // IMA ADPCM Mono
                    return MpqWavCompression.Decompress(sinput, 1);

                case 0x12:
                    // TODO: LZMA
                    throw new MpqParserException("LZMA compression is not yet supported");

                // Combos
                case 0x22:
                    // TODO: sparse then zlib
                    throw new MpqParserException("Sparse compression + Deflate compression is not yet supported");
                case 0x30:
                    // TODO: sparse then bzip2
                    throw new MpqParserException("Sparse compression + BZip2 compression is not yet supported");
                case 0x41:
                    sinput = MpqHuffman.Decompress(sinput);
                    return MpqWavCompression.Decompress(sinput, 1);
                case 0x48:
                    {
                        byte[] result = PKDecompress(sinput, outputLength);
                        return MpqWavCompression.Decompress(new MemoryStream(result), 1);
                    }
                case 0x81:
                    sinput = MpqHuffman.Decompress(sinput);
                    return MpqWavCompression.Decompress(sinput, 2);
                case 0x88:
                    {
                        byte[] result = PKDecompress(sinput, outputLength);
                        return MpqWavCompression.Decompress(new MemoryStream(result), 2);
                    }
                default:
                    throw new MpqParserException("Compression is not yet supported: 0x" + comptype.ToString("X"));
            }
        }

        private static byte[] BZip2Decompress(Stream data, int expectedLength)
        {
            using (MemoryStream output = new MemoryStream(expectedLength))
            {
#if WITH_DOTNETZIP
                using (var stream = new Ionic.BZip2.BZip2InputStream(data, false))
                {
                    stream.CopyTo(output);
                }
#elif WITH_BZIP2NET
                using (var stream = new Bzip2.BZip2InputStream(data, false))
                {
                    stream.CopyTo(output);
                }
#elif WITH_SHARPCOMPRESS
                using (var stream = new SharpCompress.Compressors.BZip2.BZip2Stream(data, SharpCompress.Compressors.CompressionMode.Decompress))
                {
                    stream.CopyTo(output);
                }
#elif WITH_SHARPZIPLIB
                ICSharpCode.SharpZipLib.BZip2.BZip2.Decompress(data, output, true);
#else
                throw new NotImplementedException("Please define which compression library you want to use");
#endif
                return output.ToArray();
            }
        }

        private static byte[] PKDecompress(Stream data, int expectedLength)
        {
            PKLibDecompress pk = new PKLibDecompress(data);
            return pk.Explode(expectedLength);
        }

        private static byte[] ZlibDecompress(Stream data, int expectedLength)
        {
            using (MemoryStream output = new MemoryStream(expectedLength))
            {
#if WITH_DOTNETZIP
                using (var stream = new Ionic.Zlib.ZlibStream(data, Ionic.Zlib.CompressionMode.Decompress))
                {
                    stream.CopyTo(output);
                }
#elif WITH_SHARPCOMPRESS
                using (var stream = new SharpCompress.Compressors.Deflate.ZlibStream(data, SharpCompress.Compressors.CompressionMode.Decompress))
                {
                    stream.CopyTo(output);
                }
#elif WITH_SHARPZIPLIB
                using (var stream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(data))
                {
                    stream.CopyTo(output);
                }
#else
                throw new NotImplementedException("Please define which compression library you want to use");
#endif
                return output.ToArray();
            }
        }
    }
}

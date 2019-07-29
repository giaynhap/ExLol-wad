using ExportWadLol.Helpers.Cryptography;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ExportWadLol.IO.WAD
{
    public class WADFile : IDisposable
    {
        public byte[] ECDSA { get; private set; }

        private List<WADEntry> _entries = new List<WADEntry>();

        public ReadOnlyCollection<WADEntry> Entries { get; private set; }

        internal Stream _stream { get; private set; }

        internal byte _major;
        internal byte _minor;

        public WADFile()
        {
            this.Entries = this._entries.AsReadOnly();
        }

        public WADFile(string fileLocation) : this(File.OpenRead(fileLocation)) { }


        public WADFile(Stream stream) : this()
        {
            this._stream = stream;
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                string magic = Encoding.ASCII.GetString(br.ReadBytes(2));
                if (magic != "RW")
                {
                    throw new Exception("This is not a valid WAD file");
                }

                this._major = br.ReadByte();
                this._minor = br.ReadByte();
                if (this._major > 3)
                {
                    throw new Exception("This version is not supported");
                }

                uint fileCount = 0;
                //XXHash Checksum of WAD Data (everything after TOC).
                ulong dataChecksum = 0;

                if (this._major == 2)
                {
                    byte ecdsaLength = br.ReadByte();
                    this.ECDSA = br.ReadBytes(ecdsaLength);
                    br.ReadBytes(83 - ecdsaLength);

                    dataChecksum = br.ReadUInt64();
                }
                else if (this._major == 3)
                {
                    this.ECDSA = br.ReadBytes(256);
                    dataChecksum = br.ReadUInt64();
                }

                if (this._major == 1 || this._major == 2)
                {
                    ushort tocStartOffset = br.ReadUInt16();
                    ushort tocFileEntrySize = br.ReadUInt16();
                }

                fileCount = br.ReadUInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    this._entries.Add(new WADEntry(this, br, this._major));
                }
            }
        }

        public void AddEntry(string path, string fileRedirection)
        {
            using (XXHash64 xxHash = XXHash64.Create())
            {
                AddEntry(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower(new CultureInfo("en-US")))), 0), fileRedirection);
            }
        }

         public void AddEntry(string path, byte[] data, bool compressedEntry)
        {
            using (XXHash64 xxHash = XXHash64.Create())
            {
                AddEntry(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower(new CultureInfo("en-US")))), 0), data, compressedEntry);
            }
        }

   
        public void AddEntry(ulong xxHash, string fileRedirection)
        {
            AddEntry(new WADEntry(this, xxHash, fileRedirection));
        }

        public void AddEntry(ulong xxHash, byte[] data, bool compressedEntry)
        {
            AddEntry(new WADEntry(this, xxHash, data, compressedEntry));
        }

        public void AddEntry(WADEntry entry)
        {
            if (!this._entries.Exists(x => x.XXHash == entry.XXHash) && entry._wad == this)
            {
                this._entries.Add(entry);
                this._entries.Sort();
            }
            else
            {
                throw new Exception("An Entry with this path already exists");
            }
        }

      
        public void RemoveEntry(string path)
        {
            using (XXHash64 xxHash = XXHash64.Create())
            {
                RemoveEntry(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path)), 0));
            }
        }

     
        public void RemoveEntry(ulong xxHash)
        {
            if (this._entries.Exists(x => x.XXHash == xxHash))
            {
                this._entries.RemoveAll(x => x.XXHash == xxHash);
            }
        }
        public void RemoveEntry(WADEntry entry)
        {
            this._entries.Remove(entry);
        }
        public void Write(string fileLocation)
        {
            Write(fileLocation, this._major, this._minor);
        }

    
        public void Write(string fileLocation, byte major, byte minor)
        {
            Write(File.Create(fileLocation), major, minor);
        }

    
        public void Write(Stream stream)
        {
            Write(stream, this._major, this._minor);
        }

      
        public void Write(Stream stream, byte major, byte minor)
        {
            if (major > 3)
            {
                throw new Exception("WAD File version: " + major + " either does not support writing or doesn't exist");
            }

            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write(Encoding.ASCII.GetBytes("RW"));
                bw.Write(major);
                bw.Write(minor);

                // Writing signatures 
                if (major == 2)
                {
                    bw.Write(new byte[84]);
                }
                else if (major == 3)
                {
                    bw.Write(new byte[256]);
                }

                // Writing file checksums
                if (major == 2 || major == 3)
                {
                    bw.Write((long)0);
                }

                int tocSize = (major == 1) ? 24 : 32;
                long tocOffset = stream.Position + 4;

                // Writing TOC info
                if (major < 3)
                {
                    tocOffset += 4;
                    bw.Write((ushort)tocOffset);
                    bw.Write((ushort)tocSize);
                }

                bw.Write(this.Entries.Count);

                stream.Seek(tocOffset + (tocSize * this.Entries.Count), SeekOrigin.Begin);

                for (int i = 0; i < this.Entries.Count; i++)
                {
                    WADEntry currentEntry = this.Entries[i];
                    currentEntry._isDuplicated = false;

                    // Finding potential duplicated entry
                    WADEntry duplicatedEntry = null;
                    if (major != 1)
                    {
                        if (currentEntry.Type != EntryType.FileRedirection)
                        {
                            for (int j = 0; j < i; j++)
                            {
                                if (this.Entries[j].SHA.SequenceEqual(currentEntry.SHA))
                                {
                                    currentEntry._isDuplicated = true;
                                    duplicatedEntry = this.Entries[j];
                                    break;
                                }
                            }
                        }
                    }

                    // Writing data
                    if (duplicatedEntry == null)
                    {
                        bw.Write(currentEntry.GetContent(false));
                        currentEntry._newData = null;
                        currentEntry._dataOffset = (uint)stream.Position - currentEntry.CompressedSize;
                    }
                    else
                    {
                        currentEntry._dataOffset = duplicatedEntry._dataOffset;
                    }
                }

                // Write TOC
                stream.Seek(tocOffset, SeekOrigin.Begin);
                foreach (WADEntry wadEntry in this.Entries)
                {
                    wadEntry.Write(bw, major);
                }
            }

            this.Dispose();
            this._stream = stream;
            this._major = major;
            this._minor = minor;
        }
        public void Dispose()
        {
            this._stream?.Close();
            this._stream = null;
        }
    }
}
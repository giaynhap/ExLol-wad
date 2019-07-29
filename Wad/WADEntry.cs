using ExportWadLol.Helpers.Compression;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ExportWadLol.IO.WAD
{

    public class WADEntry : IComparable<WADEntry>
    {
     
        public ulong XXHash { get; private set; }


        public uint CompressedSize { get; internal set; }

        public uint UncompressedSize { get; internal set; }


        public EntryType Type { get; private set; }

        public ushort Unknown1 { get; set; }


        public byte[] SHA { get; internal set; }

        private string _fileRedirection;

       public string FileRedirection
        {
            get => this._fileRedirection;
            set
            {
                if (this.Type == EntryType.FileRedirection)
                {
                    this._fileRedirection = value;
                    this._newData = new byte[value.Length + 4];
                    Buffer.BlockCopy(BitConverter.GetBytes(value.Length), 0, this._newData, 0, 4);
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(value), 0, this._newData, 4, value.Length);
                    this.CompressedSize = (uint)this._newData.Length;
                    this.UncompressedSize = this.CompressedSize;
                    this.SHA = new byte[8];
                }
                else
                {
                    throw new Exception("The current entry is not a FileRedirection entry.");
                }
            }
        }

     
        internal uint _dataOffset;

        internal bool _isDuplicated;

        internal byte[] _newData;

        internal readonly WADFile _wad;
         public WADEntry(WADFile wad, ulong xxHash, byte[] data, bool compressedEntry)
        {
            this._wad = wad;
            this.XXHash = xxHash;
            this.Type = compressedEntry ? (wad._major == 3 ? EntryType.ZStandardCompressed : EntryType.Compressed) : EntryType.Uncompressed;
            this.EditData(data);
        }

        public WADEntry(WADFile wad, ulong xxHash, string fileRedirection)
        {
            this._wad = wad;
            this.XXHash = xxHash;
            this.Type = EntryType.FileRedirection;
            this.FileRedirection = fileRedirection;
        }

       
        public WADEntry(WADFile wad, BinaryReader br, byte major)
        {
            this._wad = wad;
            this.XXHash = br.ReadUInt64();
            this._dataOffset = br.ReadUInt32();
            this.CompressedSize = br.ReadUInt32();
            this.UncompressedSize = br.ReadUInt32();
            this.Type = (EntryType)br.ReadByte();
            this._isDuplicated = br.ReadBoolean();
            this.Unknown1 = br.ReadUInt16();
            if (major >= 2)
            {
                this.SHA = br.ReadBytes(8);
            }

            if (this.Type == EntryType.FileRedirection)
            {
                long currentPosition = br.BaseStream.Position;
                br.BaseStream.Seek(this._dataOffset, SeekOrigin.Begin);
                this._fileRedirection = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
                br.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
            }
        }

        public void EditData(byte[] data)
        {
            if (this.Type == EntryType.FileRedirection)
            {
                throw new Exception("You cannot edit the data of a FileRedirection Entry");
            }
            else if (this.Type == EntryType.Compressed)
            {
                this._newData = Compression.CompressGZip(data);
            }
            else if (this.Type == EntryType.ZStandardCompressed)
            {
                this._newData = Compression.CompressZStandard(data);
            }
            else
            {
                this._newData = data;
            }

            this.CompressedSize = (uint)this._newData.Length;
            this.UncompressedSize = (uint)data.Length;
            using (SHA256 sha256 = SHA256.Create())
            {
                this.SHA = sha256.ComputeHash(this._newData).Take(8).ToArray();
            }
        }

        public void EditData(string stringData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(stringData.Length);
                    bw.Write(Encoding.ASCII.GetBytes(stringData));
                }
                this._newData = ms.ToArray();
            }
            this.CompressedSize = (uint)this._newData.Length;
            this.UncompressedSize = this.CompressedSize;
            this.SHA = new byte[8];
        }

        public byte[] GetContent(bool decompress)
        {
            byte[] dataBuffer = this._newData;
            if (dataBuffer == null)
            {
                dataBuffer = new byte[this.CompressedSize];
                this._wad._stream.Seek(this._dataOffset, SeekOrigin.Begin);
                this._wad._stream.Read(dataBuffer, 0, (int)this.CompressedSize);
            }
            if (this.Type == EntryType.Compressed && decompress)
            {
                return Compression.DecompressGZip(dataBuffer);
            }
            else if (this.Type == EntryType.ZStandardCompressed && decompress)
            {
                return Compression.DecompressZStandard(dataBuffer);
            }
            else
            {
                return dataBuffer;
            }
        }

       
        public void Write(BinaryWriter bw, uint major)
        {
            bw.Write(this.XXHash);
            bw.Write(this._dataOffset);
            bw.Write(this.CompressedSize);
            bw.Write(this.UncompressedSize);
            bw.Write((byte)this.Type);
            bw.Write(this._isDuplicated);
            bw.Write(this.Unknown1);
            if (major >= 2)
            {
                bw.Write(this.SHA);
            }
        }

      
        public int CompareTo(WADEntry other)
        {
            return this.XXHash.CompareTo(other.XXHash);
        }
    }

    public enum EntryType : byte
    {

        Uncompressed,
      
        Compressed,
     
        FileRedirection,
       
        ZStandardCompressed
    }
}
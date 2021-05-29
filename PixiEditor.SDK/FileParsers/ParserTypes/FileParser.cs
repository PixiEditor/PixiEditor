using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PixiEditor.SDK.FileParsers
{
    public abstract class FileParser<T>
    {
        public abstract bool UseBigEndian { get; }

        public virtual Encoding Encoding { get => Encoding.UTF8; }

        public Stream Stream { get; internal set; }

        public FileInfo FileInfo { get; internal set; }

        protected long TotalLenght => Stream.Length;

        protected long Position => Stream.Position;

        internal FileParser() { }

        public abstract T Parse();

        public abstract void Save(T value);

        public void WriteByte(byte value) => Stream.WriteByte(value);

        public void WriteSByte(sbyte value) => Stream.WriteByte((byte)value);

        public void WriteInt16(short value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteUInt16(ushort value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteInt32(int value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteUInt32(uint value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteInt64(long value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteUInt64(ulong value) => WriteToStreamRBIN(BitConverter.GetBytes(value));

        public void WriteString(string value, bool includeLenght = false) => WriteString(value, Encoding, includeLenght);

        public void WriteString(string value, Encoding encoding, bool includeLenght = false)
        {
            byte[] buffer = encoding.GetBytes(value);

            if (includeLenght)
            {
                WriteInt32(buffer.Length);
            }

            WriteBytes(buffer);
        }

        public void WriteBytes(byte[] value) => Stream.Write(value, 0, value.Length);

        public byte ReadByte() => (byte)Stream.ReadByte();

        public sbyte ReadSByte() => (sbyte)Stream.ReadByte();

        public short ReadInt16() => BitConverter.ToInt16(ReadFromStreamRBIN(2), 0);

        public ushort ReadUInt16() => BitConverter.ToUInt16(ReadFromStreamRBIN(2), 0);

        public int ReadInt32() => BitConverter.ToInt32(ReadFromStreamRBIN(4), 0);

        public uint ReadUInt32() => BitConverter.ToUInt32(ReadFromStreamRBIN(4), 0);

        public long ReadInt64() => BitConverter.ToInt64(ReadFromStreamRBIN(8), 0);

        public ulong ReadUInt64() => BitConverter.ToUInt64(ReadFromStreamRBIN(8), 0);

        public string ReadString()
        {
            int stringLenght = ReadInt32();
            return ReadString(stringLenght);
        }

        public string ReadString(int lenght) => Encoding.GetString(ReadBytes(lenght));

        public string ReadString(int lenght, Encoding encoding) => encoding.GetString(ReadBytes(lenght));

        public byte[] ReadBytes()
        {
            int lenght = ReadInt32();
            return ReadBytes(lenght);
        }

        protected byte[] ReadBytes(int lenght)
        {
            byte[] buffer = new byte[lenght];

            int read = Stream.Read(buffer, 0, lenght);

            if (read < lenght)
            {
                throw new EndOfStreamException();
            }

            return buffer;
        }

        private byte[] ReadFromStreamRBIN(int lenght)
        {
            byte[] buffer = ReadBytes(lenght);

            ReverseBufferIfNeeded(buffer);

            return buffer;
        }

        private void WriteToStreamRBIN(byte[] toWrite)
        {
            ReverseBufferIfNeeded(toWrite);

            Stream.Write(toWrite, 0, toWrite.Length);
        }

        private byte[] ReverseBufferIfNeeded(byte[] buffer)
        {
            if (UseBigEndian == BitConverter.IsLittleEndian)
            {
                return buffer.Reverse().ToArray();
            }

            return buffer;
        }
    }
}

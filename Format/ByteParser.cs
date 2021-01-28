using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ToGeometryConverter.Format
{
    public class ByteParser
    {
        protected String location;
        public byte[] b { get; private set; }

        public int position { get; private set; } = 0;

        public ByteParser(String location)
        {
            this.location = location;
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(location, FileMode.Open)))
                {
                    b = reader.ReadBytes((int)reader.BaseStream.Length);
                }
            }
            catch (Exception e)
            {
                b = null;
            }
        }

        public ByteParser(byte[] B)
        {
            this.b = B;
            this.position = 0;
        }

        public byte GetByte()
        {
            return (byte)(b[position++] & 0xff);
        }

        public short GetShort()
        {
            return BitConverter.ToInt16(b, position += 2);
        }

        public float GetFloat()
        {
            return BitConverter.ToSingle(b, position += 4);
        }

        public double GetDouble()
        {
            return BitConverter.ToDouble(b, position += 8);
        }

        public ulong GetLong(bool reverce)
        {
            ulong temp = BitConverter.ToUInt64(
                reverce == true ? this.GetByte(8).Reverse().ToArray() : this.GetByte(8),
               reverce == true ? 0 : position);
            position += 8;
            return temp;
        }

        public int GetInt()
        {
            return BitConverter.ToInt32(b, position += 4);
        }

        public byte[] GetByte(int lenth)
        {
            byte[] temparr = new byte[lenth];
            for (int i = 0; i < lenth; i += 1)
            {
                temparr[i] = b[position + i];
            }
            position += lenth;
            return temparr;
        }


        public string GetString(int length)
        {
            StringBuilder outt = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                outt.Append((char)b[position++]);
            }
            return outt.ToString();
        }

        public void Skip(int times)
        {
            position += times;
        }

        public void Return(int times)
        {
            position -= times;
        }

        public void Reset()
        {
            position = 0;
        }

        public static string ReaderStringLenth(BinaryReader reader, long length)
        {
            StringBuilder outt = new StringBuilder();
            for (long i = 0; i < length; i++)
            {
                outt.Append(reader.ReadChar());
            }
            return outt.ToString();
        }
    }
}

using System;
using System.IO;
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

            using (BinaryReader reader = new BinaryReader(File.Open(location, FileMode.Open)))
            {
                try
                {
                    b = reader.ReadBytes((int)reader.BaseStream.Length);
                }
                catch (Exception e)
                {
                    b = null;

                }
            }

        }

        public byte parseByte()
        {
            return (byte)(b[position++] & 0xff);
        }

        public short parseShort()
        {
            return (short)(b[position++] << 8 | (b[position++] & 0xff));
        }
        public float parseFloat()
        {
            byte[] bts = 
            {
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++]
            };

            return BitConverter.ToSingle(bts, 0);
        }

        public double parseDouble()
        {
            byte[] bts =
            {
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++],
                    b[position++]
            };

            return BitConverter.ToDouble(bts, 0);
        }

        public string parseString(int length)
        {
            StringBuilder outt = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                outt.Append((char)b[position++]);
            }
            return outt.ToString();
        }

        public void skip(int times)
        {
            for (int i = 0; i < times; i++)
            {
                position++;
            }
        }

        public void reset()
        {
            position = 0;
        }
    }
}

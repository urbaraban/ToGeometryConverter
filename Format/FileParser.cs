using System;
using System.IO;
using System.Text;

namespace ToGeometryConverter.Format
{
    public class FileParser
    {
        protected String location;
        protected byte[] b;


        public int position = 0;

        public FileParser(String location)
        {
            this.location = location;

            using (BinaryReader reader = new BinaryReader(File.Open(location, FileMode.Open)))
            {
                try
                {
                    b = reader.ReadBytes((int)reader.BaseStream.Length);
                }
                catch
                {
                    b = null;
                }
            }
        }

        public FileParser(byte[] arr)
        {
            this.b = arr;
        }

        /*
        public FileParser(FileInfo file)
        {
            this(file.getAbsolutePath());
        }
        */

        public byte parseByte()
        {
            return (byte)(b[position++] & 0xff);
        }

        public short parseShort()
        {
            return (short)(b[position++] << 8 | (b[position++] & 0xff));
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

        public void Skip(int times)
        {
            for (int i = 0; i < times; i++)
            {
                position++;
            }
        }

        public void Reset()
        {
            position = 0;
        }
    }
}

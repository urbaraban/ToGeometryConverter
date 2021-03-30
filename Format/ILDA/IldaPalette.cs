using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToGeometryConverter.Format.ILDA
{
    public class IldaPalette
    {
        public String name;
        public String companyName;
        public int totalColors;
        public int paletteNumber;
        public int scannerHead;

        public List<int> colours = new List<int>();

        public IldaPalette()
        {

        }

        public void addColour(int r, int g, int b)
        {
            colours.Add(((r & 0xFF) << 16) + ((g & 0xFF) << 8) + ((b & 0xFF)));
        }

        /**
         * Colors are stored as 32-bit integers, first eight bits are not used, next eight bits are red,
         * following eight bits are green and last eight bits are blue
         *
         * @param index number referring to the palette
         * @return the color in the aforementioned format
         */

        public int getColour(int index)
        {
            if (index >= colours.Count() || index < 0) return 0;
            else return colours[index];
        }

        /**
         * Converts the palette to bytes which can be added in front of an ilda file or stored separately
         * @return array of bytes with ilda-compliant palette
         */

        public byte[] paletteToBytes()
        {
            List<byte> theBytes;
            theBytes = new List<byte>();

            theBytes.Add((byte)'I');       //Bytes 1-4: "ILDA"
            theBytes.Add((byte)'L');
            theBytes.Add((byte)'D');
            theBytes.Add((byte)'A');
            theBytes.Add((byte)0);         //Bytes 5-8: Format Code 2
            theBytes.Add((byte)0);
            theBytes.Add((byte)0);
            theBytes.Add((byte)2);


            for (int i = 0; i < 8; i++)    //Bytes 9-16: Name
            {
                char letter;
                if (name.Length < i + 1) letter = ' ';
                else letter = name[i];
                theBytes.Add((byte)letter);
            }


            if (companyName == null)   //Bytes 17-24: Company Name
            {
                theBytes.Add((byte)'I');
                theBytes.Add((byte)'l');
                theBytes.Add((byte)'d');
                theBytes.Add((byte)'a');
                theBytes.Add((byte)'4');
                theBytes.Add((byte)'P');
                theBytes.Add((byte)'5');
                theBytes.Add((byte)' ');
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    char letter;
                    if (companyName.Length < i + 1) letter = ' ';
                    else letter = companyName[i];
                    theBytes.Add((byte)letter);
                }
            }

            int totalSize = colours.Count();
            if (totalSize < 1) return null;
            if (totalSize > 255) totalSize = 256;

            theBytes.Add((byte)((totalSize >> 8) & 0xff));              //Bytes 25-26: total colours
            theBytes.Add((byte)(totalSize & 0xff)); //Limited to 256 so byte 25 is redundant


            //Bytes 27-28: Palette number
            theBytes.Add((byte)0);    //This better be correct
            theBytes.Add((byte)0);

            theBytes.Add((byte)0);    //Bytes 29-30: Future
            theBytes.Add((byte)0);
            theBytes.Add((byte)scannerHead); //Byte 31: Scanner head
            theBytes.Add((byte)0);    //Also Future


            for (int i = 0; i < Math.Min(256, colours.Count()); i++)    //Rest: colour data
            {
                int colour = colours[i];
                theBytes.Add((byte)((colour >> 16) & 0xFF));
                theBytes.Add((byte)((colour >> 8) & 0xFF));
                theBytes.Add((byte)(colour & 0xFF));
            }

            byte[] bt = new byte[theBytes.Count];
            for (int i = 0; i < theBytes.Count; i++)
            {
                bt[i] = theBytes[i];
            }

            return bt;
        }

        /**
         * Converts this palette to the standard 64 color palette used in most programs
         */

        public void setDefaultPalette()
        {
            name = "Ilda64";
            companyName = "Ilda4P5";
            totalColors = 64;
            paletteNumber = 0;
            scannerHead = 0;

            colours.Clear();
            addColour(255, 0, 0);
            addColour(255, 16, 0);
            addColour(255, 32, 0);
            addColour(255, 48, 0);
            addColour(255, 64, 0);
            addColour(255, 80, 0);
            addColour(255, 96, 0);
            addColour(255, 112, 0);
            addColour(255, 128, 0);
            addColour(255, 144, 0);
            addColour(255, 160, 0);
            addColour(255, 176, 0);
            addColour(255, 192, 0);
            addColour(255, 208, 0);
            addColour(255, 224, 0);
            addColour(255, 240, 0);
            addColour(255, 255, 0);
            addColour(224, 255, 0);
            addColour(192, 255, 0);
            addColour(160, 255, 0);
            addColour(128, 255, 0);
            addColour(96, 255, 0);
            addColour(64, 255, 0);
            addColour(32, 255, 0);
            addColour(0, 255, 0);
            addColour(0, 255, 32);
            addColour(0, 255, 64);
            addColour(0, 255, 96);
            addColour(0, 255, 128);
            addColour(0, 255, 160);
            addColour(0, 255, 192);
            addColour(0, 255, 224);
            addColour(0, 130, 255);
            addColour(0, 114, 255);
            addColour(0, 104, 255);
            addColour(10, 96, 255);
            addColour(0, 82, 255);
            addColour(0, 74, 255);
            addColour(0, 64, 255);
            addColour(0, 32, 255);
            addColour(0, 0, 255);
            addColour(32, 0, 255);
            addColour(64, 0, 255);
            addColour(96, 0, 255);
            addColour(128, 0, 255);
            addColour(160, 0, 255);
            addColour(192, 0, 255);
            addColour(224, 0, 255);
            addColour(255, 0, 255);
            addColour(255, 32, 255);
            addColour(255, 64, 255);
            addColour(255, 96, 255);
            addColour(255, 128, 255);
            addColour(255, 160, 255);
            addColour(255, 192, 255);
            addColour(255, 224, 255);
            addColour(255, 255, 255);
            addColour(255, 224, 224);
            addColour(255, 192, 192);
            addColour(255, 160, 160);
            addColour(255, 128, 128);
            addColour(255, 96, 96);
            addColour(255, 64, 64);
            addColour(15, 32, 32);

        }
    }
}

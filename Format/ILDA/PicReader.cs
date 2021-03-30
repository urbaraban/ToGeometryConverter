using System;


namespace ToGeometryConverter.Format.ILDA
{
    public class PicReader : FileParser
    {
        PicReader(String location) : base(location)
        {

        }

        /**
         * Returns the frame in a .PIC file
         * @param location String that contains the path to the file on disk
         * @return
         */

        public static IldaFrame getFrame(String location)
        {
            PicReader parser = new PicReader(location);
            return parser.getFrame();
        }

        public IldaFrame getFrame()
        {
            int version = b[0];
            int bbp = (version == 1 || version == 0) ? 8 : 11;  //bits per point
            int begin = version == 0 ? 15 : 14;
            IldaFrame frame = new IldaFrame();
            for (int i = begin; i < b.Length; i++)
            {
                if (i <= b.Length - bbp)
                {
                    float x = ((b[i] << 8) & 0xff00) | (b[i + 1] & 0x00ff);
                    float y = ((b[i + 2] << 8) & 0xff00) | (b[i + 3] & 0x00ff);
                    float z = ((b[i + 4] << 8) & 0xff00) | (b[i + 5] & 0x00ff);

                    bool bl = false;           //blanking
                    bool normalVector = false; //ignore normal vectors


                    if ((b[i + 6] & 0x40) == 64) bl = true;
                    if ((b[i + 6] & 0x80) != 128) normalVector = true;
                    int palIndex = b[i + 6] & 0x3F;     //only the last 6 bits are used for the palette colour (64 maximum)

                    if (!normalVector)
                    {
                        if (version == 0 || version == 1)
                        {
                            IldaPoint point = new IldaPoint(x * 0.00003051757f, y * 0.00003051757f, z * 0.00003051757f, palIndex, bl);
                            frame.addPoint(point);
                        }
                        else
                        {
                            IldaPoint point = new IldaPoint(x * 0.00003051757f, y * 0.00003051757f, z * 0.00003051757f, b[i + 8], b[i + 9], b[i + 10], bl);
                            frame.addPoint(point);
                        }

                    }


                }
            }
            frame.palette = version == 0 || version == 1;
            frame.frameName = "PicFrame";
            frame.companyName = "Ilda4P5";

            return frame;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format.ILDA
{ 
    public class IldaReader : FileParser
    {

        //protected List<Integer> framePositions = new List<Integer>();
        public IldaPalette palette;


        public IldaReader(String location) : base(location)
        {

            if (b == null)
            {
                throw new FileNotFoundException("Error: could not read file at " + location);
            }

        }

        public IldaReader(FileInfo file) : base(file.Name)
        {
            if (file.FullName == string.Empty) Console.Write("location empty");
        }

        /**
         * Parse an ilda file from disk
         * Normally only this static method should be required to retrieve all IldaFrames from a file
         * @param location path to the ilda file
         * @return list of all loaded frames
         */

        public static GCCollection ReadFile(String location)
        {
            IldaReader reader = new IldaReader(location);
            List<IldaFrame> ildaFrames = reader.getFramesFromBytes();
            GCCollection gCElements = new GCCollection();
            foreach (IldaFrame ildaFrame in ildaFrames)
            {
                PointsElement points = new PointsElement();
                foreach (IldaPoint ildaPoint in ildaFrame.points)
                {
                    var argb = Convert.ToInt32(ildaPoint.getColour());
                    Color color = Color.FromArgb(argb);
                    int temp = ildaPoint.getColour();
                    points.Add(new GCPoint3D()
                    {
                        X = ildaPoint.X,
                        Y = ildaPoint.Y,
                        Z = ildaPoint.Z,
                        Red = color.R,
                        Green = color.G,
                        Blue = color.B
                    });
                }
                gCElements.Add(points);
            }
            return gCElements;
        }

        public void setPalette(IldaPalette palette)
        {
            this.palette = palette;
        }

        private List<IldaFrame> getFramesFromBytes()
        {
            Reset();
            List<IldaFrame> theFrames = new List<IldaFrame>();
            if (b == null)
            {
                //This should have been caught before
                return null;
            }

            if (b.Length < 32)
            {
                //There isn't even a complete header here!
                Console.Write("Error: file is not long enough to be a valid ILDA file!");
            }

            //Check if the first four bytes read ILDA:


            String hdr = parseString(4);
            if (!hdr.Equals("ILDA"))
            {
                Console.Write("Error: invalid ILDA file, found " + hdr + ", expected ILDA instead");
            }

            Reset();

            loadIldaFrame(theFrames);
            return theFrames;


        }

        /**
         * Iterative method to load ilda frames, the new frames are appended to an List.
         * @param f IldaFrame List where the new frame will be appended
         */

        private void loadIldaFrame(List<IldaFrame> f)
        {
            if (position >= b.Length - 32)
            {
                return;        //no complete header
            }

            //Bytes 0-3: ILDA
            String hdr = parseString(4);
            if (!hdr.Equals("ILDA"))
            {
                return;
            }

            //Bytes 4-6: Reserved
            Skip(3);

            //Byte 7: format code
            int ildaVersion = parseByte();

            //Bytes 8-15: frame name
            String name = parseString(8);

            //Bytes 16-23: company name
            String company = parseString(8);

            //Bytes 24-25: point count
            int pointCount = parseShort();

            //Bytes 26-27: frame number in frames or palette number in palettes
            int frameNumber = parseShort();

            //Bytes 28-29: total frames
            Skip(2);

            //Byte 30: projector number
            int scannerhead = parseByte() & 0xff;

            //Byte 31: Reserved
            Skip(1);

            if (ildaVersion == 2)
            {

                palette = new IldaPalette();

                palette.name = name;
                palette.companyName = company;
                palette.totalColors = pointCount;

                //Byte 30: scanner head.
                palette.scannerHead = scannerhead;


                // ILDA V2: Palette information

                for (int i = 0; i < pointCount; i++)
                {
                    palette.addColour(parseByte(), parseByte(), parseByte());
                }
            }
            else
            {
                IldaFrame frame = new IldaFrame();

                frame.setIldaFormat(ildaVersion);
                frame.setFrameName(name);
                frame.setCompanyName(company);
                frame.setFrameNumber(frameNumber);
                frame.setPalette(ildaVersion == 0 || ildaVersion == 1);

                bool is3D = ildaVersion == 0 || ildaVersion == 4;


                for (int i = 0; i < pointCount; i++)
                {
                    float x = parseShort();
                    float y = parseShort();
                    float z = 0;
                    if (is3D) z = parseShort();
                    bool bl = false;
                    if ((parseByte() & 0x40) == 64) bl = true;
                    if (ildaVersion == 0 || ildaVersion == 1)
                    {
                        IldaPoint point = new IldaPoint(x * 0.00003051757f, y * -0.00003051757f, z * 0.00003051757f, parseByte() & 0xff, bl);
                        frame.addPoint(point);
                    }
                    else if (ildaVersion == 4 || ildaVersion == 5)
                    {
                        int blue = parseByte();
                        int g = parseByte();
                        int r = parseByte();
                        IldaPoint point = new IldaPoint(x * 0.00003051757f, y * -0.00003051757f, z * 0.00003051757f, blue & 0xff, g & 0xff, r & 0xff, bl);
                        frame.addPoint(point);
                    }


                }

                if (frame.isPalette())
                {
                    if (palette == null)
                    {
                        palette = new IldaPalette();
                        palette.setDefaultPalette();
                    }

                    frame.palettePaint(palette);
                }
                f.Add(frame);

                loadIldaFrame(f);
            }
        }


    }
}

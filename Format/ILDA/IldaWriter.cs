using StclLibrary.Laser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToGeometryConverter.Format.ILDA
{
    public class IldaWriter
    {
        public void Write(string location, List<LFrame> lframes, int ildaVersion)
        {
            List<IldaFrame> frames = new List<IldaFrame>();

            foreach (LFrame lFrame in lframes)
            {
                IldaFrame ildaFrame = new IldaFrame();

                foreach(LPoint lPoint in lFrame.Points)
                {
                    ildaFrame.addPoint(new IldaPoint(lPoint.x, lPoint.y, lPoint.z, lPoint.r, lPoint.g, lPoint.b, lPoint.IsBlank));
                }

                ildaFrame.companyName = "tocut";
                ildaFrame.frameNumber = frames.Count + 1;
                ildaFrame.ildaVersion = ildaVersion;
                ildaFrame.scannerHead = 40000;
                ildaFrame.totalFrames = lframes.Count;

                frames.Add(ildaFrame);

            }

            writeFile(ChangeFormatPath(location, "ild"), frames, ildaVersion);
        }

        private string ChangeFormatPath(string filename, string format)
        {
            if (filename.Split('.').Last() != "ild")
            {
                string[] arr = filename.Split('.');
                arr[arr.Length-1] = "ild";
                filename = string.Empty;
                for (int i = 0; i < arr.Length; i++)
                    filename += arr[i] + (i==arr.Length-1 ? string.Empty:".");
            }
            return filename;
        }


        /**
         * Writes a valid ilda file to a certain location with specified format.
         * @param location The path to where the ilda file should be exported
         * @param frames All frames that should be included in the ilda file
         * @param ildaVersion ilda format:
         *                    0 = 3D, palette;
         *                    1 = 2D, palette;
         *                    (2 = palette header);
         *                    (3 = deprecated);
         *                    4 = 3D, RGB;
         *                    5 = 2D, RGB
         */

        public static void writeFile(string location, List<IldaFrame> frames, int ildaVersion)
        {
            if (frames == null) return;

            IldaFrame.fixHeaders(frames);

            byte[] b = getBytesFromFrames(frames, ildaVersion);
            if (b == null) return;

            writeFile(location, b);

        }

        public static void writeFile(String location, byte[] b)
        {
            try
            {
                FileStream file = new FileStream(location, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
                file.Write(b, 0, b.Length);
                file.Close();
            }
            catch
            {
                Console.Write("Error when exporting ilda file: ");
            }
        }

        /**
         * Writes a valid ilda file to a certain location with specified format.
         * It does not check if the specified location has a valid .ild extension.
         * @param location The path to where the ilda file should be exported
         * @param frames All frames that should be included in the ilda file
         * @param palette An IldaPalette that will be appended in front of the ilda file with a format 2 header
         * @param ildaVersion ilda format: should be 0 or 1 since only those two formats use a palette for their colour information
         *                    but nobody is stopping you from appending a palette to a format 4/5 file, though that would be pointless
         */

        public static void writeFile(String location, List<IldaFrame> frames, IldaPalette palette, int ildaVersion)
        {
            if (frames == null) return;

            IldaFrame.fixHeaders(frames);

            byte[] b = getBytesFromFrames(frames, palette, ildaVersion);
            if (b == null) return;

            writeFile(location, b);

        }

        /**
         * Writes a valid ILDA file to the specified location in format 4
         * @param location Where to write the file to
         * @param frames Frames that will go into the file
         */

        public static void writeFile(String location, List<IldaFrame> frames)
        {
            writeFile(location, frames, 4);
        }

        public static byte[] getBytesFromFrames(List<IldaFrame> frames)
        {
            return getBytesFromFrames(frames, 4);
        }

        /**
         * This method returns a byte array which can be exported directly as an ilda file from a palette and an List of IldaFrames.
         * It will insert the palette as a format 2 header before all frames.
         * It assumes the colours already have the correct colour index, no recolourisation happens.
         *
         * @param frames      an List of IldaFrames which get converted to ilda-compliant bytes
         * @param palette     an IldaPalette which gets appended before the laser art in the ilda file
         * @param ildaVersion the ilda format the frames get saved as, can be 0, 1, 4 or 5 but only 0 and 1 use a palette. It makes no sense to export as format 4 or 5 with a palette included.
         * @return ilda compliant byte array which can be directly exported as an ilda file
         */

        public static byte[] getBytesFromFrames(List<IldaFrame> frames, IldaPalette palette, int ildaVersion)
        {
            byte[] pbytes = palette.paletteToBytes();
            byte[] fbytes = getBytesFromFrames(frames, ildaVersion);
            byte[] cbytes = new byte[pbytes.Length + fbytes.Length];
            Array.Copy(pbytes, 0, cbytes, 0, pbytes.Length);
            Array.Copy(fbytes, 0, cbytes, pbytes.Length, fbytes.Length);
            return cbytes;
        }

        /**
         * This method returns a byte array from only an List of IldaFrames. This array can be saved to disk directly as a valid ilda file (binary file).
         * @param frames The frames
         * @param ildaVersion The ilda format version, can be 0, 1, 4 or 5.
         * @return Valid bytes that compose an ilda file
         */

        public static byte[] getBytesFromFrames(List<IldaFrame> frames, int ildaVersion)
        {
            List<Byte> theBytes = new List<Byte>();
            int frameNum = 0;

            if (frames.Count == 0) return null;

            foreach (IldaFrame frame in frames)
            {
                theBytes.Add((byte)'I');
                theBytes.Add((byte)'L');
                theBytes.Add((byte)'D');
                theBytes.Add((byte)'A');
                theBytes.Add((byte)0);
                theBytes.Add((byte)0);
                theBytes.Add((byte)0);

                if (ildaVersion == 0 || ildaVersion == 1 || ildaVersion == 2 || ildaVersion == 4 || ildaVersion == 5)
                    theBytes.Add((byte)ildaVersion);
                else
                    return null;
                

                for (int i = 0; i < 8; i++)    //Bytes 9-16: Name
                {
                    char letter;
                    if (frame.getFrameName().Length < i + 1) letter = ' ';
                    else letter = frame.getFrameName()[i];
                    theBytes.Add((byte)letter);
                }

                if (frame.getCompanyName().Length == 0)   //Bytes 17-24: Company Name
                {
                    theBytes.Add((byte)'I');     //If empty: call it "Ilda4P5"
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
                        if (frame.getCompanyName().Length < i + 1) letter = ' ';
                        else letter = frame.getCompanyName()[i];
                        theBytes.Add((byte)letter);
                    }
                }

                //Bytes 25-26: Total point count
                theBytes.Add((byte)((frame.getPointCount() >> 8) & 0xff));    //This better be correct
                theBytes.Add((byte)(frame.getPointCount() & 0xff));


                //Bytes 27-28: Frame number (automatically increment each frame)
                theBytes.Add((byte)((++frameNum >> 8) & 0xff));    //This better be correct
                theBytes.Add((byte)(frameNum & 0xff));


                //Bytes 29-30: Number of frames
                theBytes.Add((byte)((frames.Count >> 8) & 0xff));    //This better be correct
                theBytes.Add((byte)(frames.Count & 0xff));

                theBytes.Add((byte)(frame.getScannerHead()));    //Byte 31 is scanner head
                theBytes.Add((byte)(0));                    //Byte 32 is future

                foreach (IldaPoint point in frame.getPoints())
                {
                    short posx = (short)point.X;
                    theBytes.Add((byte)((posx >> 8) & 0xff));
                    theBytes.Add((byte)(posx & 0xff));

                    short posy = (short)point.Y;
                    theBytes.Add((byte)((posy >> 8) & 0xff));
                    theBytes.Add((byte)(posy & 0xff));

                    if (ildaVersion == 0 || ildaVersion == 4) //a 3D frame
                    {
                        int posz = (int)point.Z;
                        theBytes.Add((byte)((posz >> 8) & 0xff));
                        theBytes.Add((byte)(posz & 0xff));
                    }
                    //ilda.parent.println(posx + " " + posy + " " + point.blanked);

                    if (point.isBlanked())
                        theBytes.Add((byte)0x40);
                    else
                        theBytes.Add((byte)0);

                    if (ildaVersion == 0 || ildaVersion == 1) theBytes.Add((point.getPalIndex()));
                    else
                    {
                        int c = point.getColour();
                        if (point.isBlanked()) c = 0;  //some programs only use colour information to determine blanking

                        int red = (c >> 16) & 0xFF;  // Faster way of getting red(argb)
                        int green = ((c >> 8) & 0xFF);   // Faster way of getting green(argb)
                        int blue = (c & 0xFF);          // Faster way of getting blue(argb)


                        theBytes.Add((byte)(blue));
                        theBytes.Add((byte)(green));
                        theBytes.Add((byte)(red));
                    }


                }


            }

            //File should always end with a header

            theBytes.Add((byte)'I');
            theBytes.Add((byte)'L');
            theBytes.Add((byte)'D');
            theBytes.Add((byte)'A');
            theBytes.Add((byte)0);
            theBytes.Add((byte)0);
            theBytes.Add((byte)0);
            theBytes.Add((byte)ildaVersion);

            theBytes.Add((byte)'L');
            theBytes.Add((byte)'A');
            theBytes.Add((byte)'S');
            theBytes.Add((byte)'T');
            theBytes.Add((byte)' ');
            theBytes.Add((byte)'O');
            theBytes.Add((byte)'N');
            theBytes.Add((byte)'E');

            theBytes.Add((byte)'I');
            theBytes.Add((byte)'l');
            theBytes.Add((byte)'d');
            theBytes.Add((byte)'a');
            theBytes.Add((byte)'4');
            theBytes.Add((byte)'P');
            theBytes.Add((byte)'5');
            theBytes.Add((byte)' ');

            theBytes.Add((byte)0);
            theBytes.Add((byte)0);

            theBytes.Add((byte)0);
            theBytes.Add((byte)0);

            theBytes.Add((byte)0);
            theBytes.Add((byte)0);

            theBytes.Add((byte)0);

            theBytes.Add((byte)0);

            return theBytes.ToArray();
        }  
    }
}

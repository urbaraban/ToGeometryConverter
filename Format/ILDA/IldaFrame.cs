using System;
using System.Collections.Generic;

namespace ToGeometryConverter.Format.ILDA
{
    public class IldaFrame
    {
        public List<IldaPoint> points = new List<IldaPoint>();
        //The Points in the Frame


        public int ildaVersion = 4;    //Data retrieved from header

        public void setFrameName(string frameName)
        {
            this.frameName = frameName;
        }

        public void setCompanyName(string companyName)
        {
            this.companyName = companyName;
        }

        public void setFrameNumber(int frameNumber)
        {
            this.frameNumber = frameNumber;
        }

        public void setTotalFrames(int totalFrames)
        {
            this.totalFrames = totalFrames;
        }

        public void setScannerHead(int scannerHead)
        {
            this.scannerHead = scannerHead;
        }

        public void setPalette(bool palette)
        {
            this.palette = palette;
        }

        public String frameName = "";
        public String companyName = "Processing";
        public int pointCount;
        public int frameNumber;
        public int totalFrames;
        public int scannerHead;
        public bool palette = false;


        /*ilda frame*/
        public IldaFrame()
        {
        }

        /**
         * Set the ilda version this frame uses.
         * 0 = 3D, palette
         * 1 = 2D, palette
         * 4 = 3D, RGB
         * 5 = 3D, RGB
         * Internally, all frames are 3D and use RGB.
         *
         * @param versionNumber integer, can be 0, 1, 4 or 5
         * @throws IllegalArgumentException when using invalid version number
         */

        public void setIldaFormat(int versionNumber)
        {
            if (versionNumber != 0 && versionNumber != 1 && versionNumber != 4 && versionNumber != 5)
            {
                Console.Write("Unsupported ILDA format " + versionNumber);
            }
            else ildaVersion = versionNumber;
        }

        public void addPoint(IldaPoint point)
        {
            if (point != null) points.Add(point);
        }

        


        /**
         * Renders the frame to a PGraphics to be displayed in the sketch.
         * The PGraphics should be 3D
         * a 2D version might get implemented
         * You must call beginDraw() and endDraw() yourself!
         *
         * @param pg           A reference to the PGraphics (it can't generate its own as this usually results in memory leaks)
         * @param showBlanking Should blanking lines be displayed?
         * @param sizex        Size of the PGraphics element it returns
         * @param sizey
         * @param rotx         Rotation of the frame
         * @param roty
         * @param rotz
         * @return a PGraphics with the frame drawn
         */

       
        public void palettePaint(IldaPalette palette)
        {
            foreach (IldaPoint point in points)
            {
                point.colour = palette.getColour(point.palIndex);
            }
        }

        public List<IldaPoint> getPoints()
        {
            return points;
        }

        public String toString()
        {
            return "This frame has " + points.Count + " points.\nIt's called " + frameName + ".";
        }

        public int getIldaVersion()
        {
            return ildaVersion;
        }

        public String getFrameName()
        {
            return frameName;
        }

        public String getCompanyName()
        {
            return companyName;
        }

        public int getPointCount()
        {
            return pointCount;
        }

        public int getFrameNumber()
        {
            return frameNumber;
        }

        public int getTotalFrames()
        {
            return totalFrames;
        }

        public int getScannerHead()
        {
            return scannerHead;
        }

        public bool isPalette()
        {
            return palette;
        }

        /**
         * Fix the header of the current frame
         * @param frameNumber index of the frame in the animation
         * @param totalFrames total frames in the animation
         * @param frameName name of the frame
         * @param companyName name of the owner/program/company that owns or created the frame
         */

        public void fixHeader(int frameNumber, int totalFrames, String frameName, String companyName)
        {
            fixHeader(this, frameNumber, totalFrames, frameName, companyName);
        }

        /**
         * Static version of fixHeader()
         * See documentation there
         *
         */

        public static void fixHeader(IldaFrame frame, int frameNumber, int totalFrames, String frameName, String companyName)
        {
            frame.frameNumber = frameNumber;
            frame.totalFrames = totalFrames;
            frame.pointCount = frame.points.Count;
            frame.frameName = frameName;
            frame.companyName = companyName;
        }

        /**
         * Fixes the frame headers
         * eg. updates point count, frame number, total frames, ...
         * It leaves the frame name and company name untouched.
         * It assumes the frames form a complete sequence.
         *
         * @param frames A reference to the frames whose headers need to get fixed.
         */

        public static void fixHeaders(List<IldaFrame> frames)
        {
            int i = 1;
            foreach (IldaFrame frame in frames)
                fixHeader(frame, i++, frames.Count, frame.frameName, frame.companyName);

        }

        /**
         * Fixes the frame headers
         * eg.updates point count, frame number, total frames
         * It sets the frame name and company name to the arguments you gave it.
         * It assumes the frames form a complete sequence (for the total frame entry).
         * Call this before writing to an ilda file
         *
         * @param frames      A reference to the frames whose headers need to get fixed.
         * @param frameName   A name you want to give the frame
         * @param companyName Another name
         */

        public static void fixHeaders(List<IldaFrame> frames, String frameName, String companyName)
        {
            int i = 1;
            foreach (IldaFrame frame in frames)
            {

                fixHeader(frame, i++, frames.Count, frameName, companyName);
            }
        }
    }
}

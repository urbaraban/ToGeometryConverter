using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class DCeiling : IFormat
    {
        public string Name { get; } = "DEXCeil";
        public string[] ShortName { get; } = new string[1] { "dc" };

        public GCCollection Get(string Filename, double RoundStep)
        {
            if (File.Exists(Filename))
            {
                GCCollection geometryGroup = new GCCollection();

                using (BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open), Encoding.ASCII))
                {
                    reader.BaseStream.Position += 54;
                    //logo 
                    string m_date = ReadNullTerminatedString(reader);

                    int numDiags = reader.ReadInt32();

                    for (long i = 0; i < numDiags; i++)
                    {
                        reader.BaseStream.Position += 26;
                    }

                    /*m_grid.smPerSeg = (float)*/
                    reader.ReadDouble();
                    int step = reader.ReadInt32();

                    string tempClient = ReadNullTerminatedString(reader);

                    string m_name = ReadNullTerminatedString(reader);
                    string m_number = ReadNullTerminatedString(reader);

                    int numVertex = reader.ReadInt32();
                    bool isSolid = reader.ReadByte() > 0;

                    List<Point3D> points = new List<Point3D>();

                    for (long i = 0; i < numVertex; i++)
                    {

                        bool angle90 = reader.ReadByte() > 0;
                        double x = reader.ReadDouble();
                        double y = reader.ReadDouble();

                        int direct = reader.ReadInt32();

                        points.Add(new Point3D(x, y, 0));
                    }
                    //points.Remove(points[0]);

                    
                    return new GCCollection()
                    {
                        new PointsElement(){
                        Points = points,
                        IsClosed = true,
                        }
                    };
                }
            }
            return null;
        }

        private string ReadNullTerminatedString(System.IO.BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((int)(ch = stream.ReadChar()) != 0)
                str = str + ch;
            return str;
        }
    }
}

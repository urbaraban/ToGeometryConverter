using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    public class DCeiling : IFormat
    {
        public string Name { get; } = "DEXCeil";
        public string[] ShortName { get; } = new string[1] { ".dc" };

        public static GeometryGroup Get(string filename)
        {
            if (File.Exists(filename))
            {
                GeometryGroup geometryGroup = new GeometryGroup();

                using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open), Encoding.ASCII))
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

                    PointCollection points = new PointCollection();

                    for (long i = 0; i < numVertex; i++)
                    {

                        bool angle90 = reader.ReadByte() > 0;
                        double x = reader.ReadDouble();
                        double y = reader.ReadDouble();

                        int direct = reader.ReadInt32();

                        points.Add(new System.Windows.Point(x, y));
                    }

                    Point startPoint = points[0];
                    //points.Remove(points[0]);

                    geometryGroup.Children.Add(
                                Tools.FigureToGeometry(new PathFigure()
                                {
                                    StartPoint = points[0],
                                    Segments = new PathSegmentCollection()
                                    {
                                        new PolyLineSegment(points, true)
                                    },
                                    IsClosed = true
                                }));

                    reader.Close();
                    return geometryGroup;
                }
            }
            return null;
        }

        private static string ReadNullTerminatedString(System.IO.BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((int)(ch = stream.ReadChar()) != 0)
                str = str + ch;
            return str;
        }
    }
}

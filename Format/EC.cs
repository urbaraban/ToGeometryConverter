using System.IO;
using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    public static class EC
    {
        public static string Name = "EasyCeiling";
        public static string Short = ".ec";

        public static GeometryGroup Get(string filename)
        {
            if (File.Exists(filename))
            {
                GeometryGroup geometryGroup = new GeometryGroup();

                ByteParser byteParser = new ByteParser(filename);

                if (byteParser.b != null)
                {
                    string XYFind = byteParser.GetString(byteParser.b.Length).Replace("\0", string.Empty);

                    int startVer = XYFind.IndexOf("VerFile") + 7;

                    double Version = double.Parse(XYFind.Substring(startVer, 5).Replace("\n", string.Empty).Replace('.',','));

                    if (Version < 147)
                    {
                        try
                        {
                            int start = XYFind.IndexOf("<XY>") + 5;
                            int end = XYFind.IndexOf("</XY>");

                            string[] coord = XYFind.Substring(start, end - start - 1).Split('\n');

                            PointCollection points = new PointCollection();

                            for (int i = 0; i < coord.Length; i += 2)
                            {
                                double x = double.Parse(coord[i].Replace('.', ','));
                                double y = double.Parse(coord[i + 1].Replace('.', ','));
                                points.Add(new System.Windows.Point(x, -y));
                            }

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

                            return geometryGroup;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }
         
    }
}

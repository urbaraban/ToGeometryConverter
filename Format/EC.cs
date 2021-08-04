using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    public class EC : IFormat
    {
        public string Name { get; } = "EasyCeiling";
        public string[] ShortName { get; } = new string[1] { "ec" };

        public Tuple<int, int> Progress { get; private set; }

        public async Task<GCCollection> GetAsync(string filename, double RoundStep)
        {
            if (File.Exists(filename))
            {
                ByteParser byteParser = new ByteParser(filename);

                if (byteParser.b != null)
                {
                    string XYFind = byteParser.GetString(byteParser.b.Length).Replace("\0", string.Empty);

                    int startVer = XYFind.IndexOf("VerFile") + 7;

                    double Version = double.Parse(XYFind.Substring(startVer, 5).Replace("\n", string.Empty).Replace('.', ','));

                    if (Version < 147)
                    {
                        try
                        {
                            int start = XYFind.IndexOf("<XY>") + 5;
                            int end = XYFind.IndexOf("</XY>");

                            string[] coord = XYFind.Substring(start, end - start - 1).Split('\n');

                            List<GCPoint3D> points = new List<GCPoint3D>();

                            for (int i = 0; i < coord.Length; i += 2)
                            {
                                double x = double.Parse(coord[i].Replace('.', ','));
                                double y = double.Parse(coord[i + 1].Replace('.', ','));
                                points.Add(new GCPoint3D(x, -y, 0));
                            }

                            return new GCCollection(GCTools.GetName(filename))
                            {
                                new PointsElement()
                                {
                                    Points = points
                                }
                            };

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

using System.Threading.Tasks;
using ToGeometryConverter.Format;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Object.UDP
{
    public static class GCByteReader
    {
        public async static Task<GCCollection> Read(byte[] b)
        {
            if (b != null)
            {
                GCCollection gCElements = new GCCollection();
                ByteParser fileParser = new ByteParser(b);

                string hdr = fileParser.GetString(8);
                if (hdr.Equals("Geometry") == false)
                {
                    return null;
                }

                gCElements.Name = fileParser.GetString(16);

                int PathCount = fileParser.GetShort();

                for (int p = 0; p < PathCount; p++)
                {
                    PointsElement points = new PointsElement();
                    points.IsClosed = fileParser.GetByte() == 0 ? false : true;

                    int pointsCount = fileParser.GetInt();

                    for (int i = 0; i < pointsCount; i++)
                    {
                        points.Add(new GCPoint3D(fileParser.GetDouble(), -fileParser.GetDouble(), 0));
                    }
                    if (points.Points.Count > 0)
                    {
                        gCElements.Add(points);
                    }
                }

                return gCElements;
            }
            return null;
        }
    }
}

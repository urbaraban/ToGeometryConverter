using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    public class DEXCeil : GCFormat
    {
        public DEXCeil() : base("DEXCeil", new string[1] { "dc" }) 
        {
            this.ReadFile = GetAsync;
        }

        private async Task<object> GetAsync(string Filename, double RoundStep)
        {
            if (File.Exists(Filename))
            {
                using BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open), Encoding.ASCII);
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

                List<GCPoint3D> points = new List<GCPoint3D>();

                for (long i = 0; i < numVertex; i++)
                {

                    bool angle90 = reader.ReadByte() > 0;
                    double x = reader.ReadDouble();
                    double y = reader.ReadDouble();

                    int direct = reader.ReadInt32();

                    points.Add(new GCPoint3D(x, y, 0));
                }
                //points.Remove(points[0]);


                return new GCCollection(GCTools.GetName(Filename))
                    {
                        Elements = new List<IGCObject>{ 
                            new PointsElement()
                            {
                                Points = points,
                                IsClosed = true,
                                Name = "Points"
                            }
                        },
                        Name = GCTools.GetName(Filename)
                    };
            }
            return null;
        }

        private string ReadNullTerminatedString(System.IO.BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((int)(ch = stream.ReadChar()) != 0)
                str += ch;
            return str;
        }
    }
}

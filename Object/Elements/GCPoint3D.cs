using System;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Format;

namespace ToGeometryConverter.Object.Elements
{
    public class GCPoint3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public byte Multiplier { get; set; } = 0;

        public Point3D GetPoint3D => new Point3D(this.X, this.Y, this.Z);

        public GCPoint3D() { }

        public GCPoint3D(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Multiplier = 0;
        }

        internal static GCPoint3D Parse(Edge edge)
        {
            return new GCPoint3D(edge.v1.X, edge.v1.Y, edge.v1.Z);
        }
    }
}

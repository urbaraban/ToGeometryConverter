using System.Windows;

namespace ToGeometryConverter.Object
{
    public class RationalBSplinePoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Weight
        {
            get => _pweight;
            set
            {
                _pweight = value;
            }
        }
        private double _pweight = 1d;

        public Point GetPoint => new Point(X, Y);

        public RationalBSplinePoint(double X, double Y, double weight)
        {
            this.X = X;
            this.Y = Y;
            this.Weight = weight;
        }

        public override string ToString() => $"X:{X}; Y:{Y}";
    }
}

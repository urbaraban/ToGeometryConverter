using System.Windows;

namespace ToGeometryConverter.Object
{
    public class RationalBSplinePoint : NotifierBase
    {

        public RationalBSplinePoint(Point myPoint, double weight)
        {
            MyPoint = myPoint;
            Weight = weight;
        }

        private Point pMyPoint = new Point();

        public Point MyPoint
        {
            get { return pMyPoint; }
            set
            {
                SetProperty(ref pMyPoint, value);
            }
        }

        private double pWeight = 1d;

        public double Weight
        {
            get { return pWeight; }
            set
            {
                SetProperty(ref pWeight, value);
            }
        }

    }
}

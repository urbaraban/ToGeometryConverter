using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object
{
    public class NURBS : NotifierBase
    {

        private ObservableCollection<RationalBSplinePoint> pWeightedPointSeries = new ObservableCollection<RationalBSplinePoint>();

        public ObservableCollection<RationalBSplinePoint> WeightedPointSeries
        {
            get { return pWeightedPointSeries; }
            set
            {
                SetProperty(ref pWeightedPointSeries, value);
            }
        }

        private bool pIsBSpline = true;

        public bool IsBSpline
        {
            get { return pIsBSpline; }
            set
            {
                SetProperty(ref pIsBSpline, value);
            }
        }


        /// <summary>
        /// This code is translated to C# from the original C++  code given on page 74-75 in "The NURBS Book" by Les Piegl and Wayne Tiller 
        /// </summary>
        /// <param name="i">Current control pont</param>
        /// <param name="degree">The picewise polynomial degree</param>
        /// <param name="Knot">The knot vector</param>
        /// <param name="step">The value of the current curve point. Valid range from 0 <= step <=1 </param>
        /// <returns>N_{i,degree}(step)</returns>
        private double Nip(int PointIndex, int degree, IList<double> Knot, double step)
        {
            double[] N = new double[degree + 1];
            double saved, temp;

            step = step * Knot.Last();

            int m = Knot.Count - 1;
            if ((PointIndex == 0 && step == Knot[0]) || (PointIndex == (m - degree - 1) && step == Knot[m]))
                return 1;

            if (step < Knot[PointIndex] || step >= Knot[PointIndex + degree + 1])
                return 0;

            for (int j = 0; j <= degree; j++)
            {
                if (step >= Knot[PointIndex + j] && step < Knot[PointIndex + j + 1])
                    N[j] = 1d;
                else
                    N[j] = 0d;
            }

            for (int k = 1; k <= degree; k++)
            {
                if (N[0] == 0)
                    saved = 0d;
                else
                    saved = ((step - Knot[PointIndex]) * N[0]) / (Knot[PointIndex + k] - Knot[PointIndex]);

                for (int j = 0; j < degree - k + 1; j++)
                {
                    double Uleft = Knot[PointIndex + j + 1];
                    double Uright = Knot[PointIndex + j + k + 1];

                    if (N[j + 1] == 0)
                    {
                        N[j] = saved;
                        saved = 0d;
                    }
                    else
                    {
                        temp = N[j + 1] / (Uright - Uleft);
                        N[j] = saved + (Uright - step) * temp;
                        saved = (step - Uleft) * temp;
                    }
                }
            }
            return N[0];
        }

        public PointCollection BSplineCurve(ObservableCollection<RationalBSplinePoint> Points, int Degree, IList<double> KnotVector, double StepSize)
        {

            //lenth
            double lenth = 0;
            Point lastpoint = this.IsBSpline ? BSplinePoint(Points, Degree, KnotVector, 0) : RationalBSplinePoint(Points, Degree, KnotVector, 0);

            for (double i = 0; i < 1; i += 0.01)
            {
                Point temppoint = this.IsBSpline ? BSplinePoint(Points, Degree, KnotVector, i) : RationalBSplinePoint(Points, Degree, KnotVector, i);
                lenth += Math.Sqrt(Math.Pow(temppoint.X - lastpoint.X, 2) + Math.Pow(temppoint.Y - lastpoint.Y, 2));
                lastpoint = temppoint;
            }

            lenth += Math.Sqrt(Math.Pow(Points[Points.Count - 1].MyPoint.X - lastpoint.X, 2) + Math.Pow(Points[Points.Count - 1].MyPoint.Y - lastpoint.Y, 2));

            StepSize = StepSize / lenth;

            //calculate
            PointCollection Result = new PointCollection();
            for (double i = 0; i < 1; i += StepSize)
            {
                if (this.IsBSpline)
                    Result.Add(BSplinePoint(Points, Degree, KnotVector, i));
                else
                    Result.Add(RationalBSplinePoint(Points, Degree, KnotVector, i));
            }

            if (!Result.Contains(Points[Points.Count - 1].MyPoint))
                Result.Add(Points[Points.Count - 1].MyPoint);

            return Result;
        }

        Point BSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, IList<double> KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X * temp;
                y += Points[i].MyPoint.Y * temp;
            }

            Console.WriteLine(x + " " + y);

            return new Point(x, y);
        }

        Point RationalBSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, IList<double> KnotVector, double t)
        {

            double x, y;
            x = 0;
            y = 0;
            double rationalWeight = 0d;

            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t) * Points[i].Weight;
                rationalWeight += temp;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                double temp = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X * Points[i].Weight * temp / rationalWeight;
                y += Points[i].MyPoint.Y * Points[i].Weight * temp / rationalWeight;
            }
            return new Point(x, y);
        }


    }
}

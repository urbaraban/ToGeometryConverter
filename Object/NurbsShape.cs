
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToGeometryConverter.Object
{
    public class NurbsShape
    {
        public bool IsClosed { get; set; }
        public bool IsBSpline { get; set; }

        // private PathGeometry pathGeometry;
        private ObservableCollection<RationalBSplinePoint> _point;
        private int _degree;
        private IList<double> _knotvector;
        private double _stepsize;


        public static implicit operator Geometry(NurbsShape nurbs)
        {
            return nurbs.GetGeometry();
        }

        public NurbsShape(ObservableCollection<RationalBSplinePoint> Points, int Degree, IList<double> KnotVector, double StepSize, bool IsBspline)
        {
            this._point = Points;
            this._degree = Degree;
            this._knotvector = KnotVector;
            this._stepsize = StepSize;
            this.IsBSpline = IsBspline;
        }

        private Geometry GetGeometry()
        {
            PointCollection points = BSplinePoints(this._stepsize);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = points.First();
            points.Remove(points.First());
            pathFigure.Segments.Add(new PolyLineSegment(points, true));

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        private List<Shape> GetArcShapes()
        {
            List<Shape> shapes = new List<Shape>();

            PointCollection points = new PointCollection();

            for (double i = 0; i < 1; i += 0.001)
            {
                points.Add(this.IsBSpline ? BSplinePoint(this._point, this._degree, this._knotvector, i) : RationalBSplinePoint(this._point, this._degree, this._knotvector, i));
            }

            if (points.Count > 2)
            {
                double lastAngel = GetAngleThreePoint(points[0], points[1], points[2]);
                PointCollection ArcCollection = new PointCollection() { points[0], points[1], points[2] };
                for (int i = 3; i < points.Count; i += 1)
                {
                    double tempAngel = GetAngleThreePoint(points[i - 2], points[i - 1], points[i]);

                    if (tempAngel != lastAngel)
                    {
                        shapes.Add(GetArcShape(ArcCollection[0], ArcCollection[ArcCollection.Count / 2 + 1], ArcCollection.Last()));
                        if (i + 2 < points.Count)
                        {
                            lastAngel = GetAngleThreePoint(points[i], points[i + 1], points[i + 2]);
                            ArcCollection = new PointCollection() { points[i], points[i + 1], points[i + 2] };
                            i += 2;
                        }
                        else
                        {
                            ArcCollection.Clear();
                        }
                    }
                    else
                    {
                        ArcCollection.Add(points[i]);
                    }
                }
                if (ArcCollection.Count > 2)
                {
                    shapes.Add(GetArcShape(ArcCollection[0], ArcCollection[ArcCollection.Count / 2 + 1], ArcCollection.Last()));
                }
            }

            return shapes;
        }

        private Path GetArcShape(Point start, Point middle, Point end)
        {
            Point center;
            double radius = 0;

            // Get the perpendicular bisector of (x1, y1) and (x2, y2).
            double x1 = (middle.X + start.X) / 2;
            double y1 = (middle.Y + start.Y) / 2;
            double dy1 = middle.X - start.X;
            double dx1 = -(middle.Y - start.Y);

            // Get the perpendicular bisector of (x2, y2) and (x3, y3).
            double x2 = (end.X + middle.X) / 2;
            double y2 = (end.Y + middle.Y) / 2;
            double dy2 = end.X - middle.X;
            double dx2 = -(end.Y - middle.Y);

            // See where the lines intersect.
            bool lines_intersect, segments_intersect;
            Point intersection, close1, close2;

            FindIntersection(
                new Point(x1, y1), new Point(x1 + dx1, y1 + dy1),
                new Point(x2, y2), new Point(x2 + dx2, y2 + dy2),
                out lines_intersect, out segments_intersect,
                out intersection, out close1, out close2);

            if (!lines_intersect)
            {
                MessageBox.Show("The points are colinear");
                center = new Point(0, 0);
                radius = 0;
            }
            else
            {
                center = intersection;
                double dx = center.X - start.X;
                double dy = center.Y - start.Y;
                radius = (double)Math.Sqrt(dx * dx + dy * dy);
            }

            double angle = GetAngleThreePoint(start, center, end);

            return new Path()
            {
                Data = new PathGeometry()
                {
                    Figures = new PathFigureCollection()
                    {
                        new PathFigure()
                        {
                            StartPoint = start,
                            Segments = new PathSegmentCollection()
                            {
                                new ArcSegment()
                                {
                                    Point = end,
                                    RotationAngle = angle,
                                    SweepDirection = angle < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                                    IsLargeArc = (Math.Abs(angle) % 360) > 180,
                                    Size = new Size(radius, radius)
                                }
                            }
                        }
                    }
                }
            };

        }

        private void FindIntersection(
    Point p1, Point p2, Point p3, Point p4,
    out bool lines_intersect, out bool segments_intersect,
    out Point intersection,
    out Point close_p1, out Point close_p2)
        {
            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (double.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Point(float.NaN, float.NaN);
                close_p1 = new Point(float.NaN, float.NaN);
                close_p2 = new Point(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            double t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new Point(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }


        public PointCollection BSplinePoints(double step)
        {
            //lenth
            double lenth = 0;
            Point lastpoint = this.IsBSpline ? BSplinePoint(this._point, this._degree, this._knotvector, 0) : RationalBSplinePoint(this._point, this._degree, this._knotvector, 0);

            for (double i = 0; i < 1; i += 0.01)
            {
                Point temppoint = this.IsBSpline ? BSplinePoint(this._point, this._degree, this._knotvector, i) : RationalBSplinePoint(this._point, this._degree, this._knotvector, i);
                lenth += Math.Sqrt(Math.Pow(temppoint.X - lastpoint.X, 2) + Math.Pow(temppoint.Y - lastpoint.Y, 2));
                lastpoint = temppoint;
            }

            lenth += Math.Sqrt(Math.Pow(this._point[this._point.Count - 1].MyPoint.X - lastpoint.X, 2) + Math.Pow(this._point[this._point.Count - 1].MyPoint.Y - lastpoint.Y, 2));

            step = step / lenth;

            //calculate
            PointCollection Result = new PointCollection();

            for (double i = 0; i < 1; i += step)
            {
                if (this.IsBSpline)
                    Result.Add(BSplinePoint(this._point, this._degree, this._knotvector, i));
                else
                    Result.Add(RationalBSplinePoint(this._point, this._degree, this._knotvector, i));
            }

            if (!Result.Contains(this._point[this._point.Count - 1].MyPoint))
                Result.Add(this._point[this._point.Count - 1].MyPoint);

            return Result;
        }

        private Point BSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, IList<double> KnotVector, double t)
        {
            double x, y;
            x = 0;
            y = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                double NIP = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X * NIP;
                y += Points[i].MyPoint.Y * NIP;
            }

            return new Point(x, y);
        }

        private Point RationalBSplinePoint(ObservableCollection<RationalBSplinePoint> Points, int degree, IList<double> KnotVector, double t)
        {
            double x, y;
            x = 0;
            y = 0;
            double rationalWeight = 0d;

            for (int i = 0; i < Points.Count; i++)
            {
                double NIP = Nip(i, degree, KnotVector, t) * Points[i].Weight;
                rationalWeight += NIP;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                double NIP = Nip(i, degree, KnotVector, t);
                x += Points[i].MyPoint.X * Points[i].Weight * NIP / rationalWeight;
                y += Points[i].MyPoint.Y * Points[i].Weight * NIP / rationalWeight;
            }
            return new Point(x, y);
        }

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


        private double GetAngleThreePoint(Point Point1, Point Center, Point Point2)
        {
            Vector v1 = Center - Point2;
            Vector v2 = Point1 - Point2;

            return Math.Atan2(v1.X, v1.Y) - Math.Atan2(v2.X, v2.Y);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Vector = System.Windows.Vector;

namespace ToGeometryConverter.Object
{
    public class NurbsShape
    {
        private const double PreciseChordStep = 1.0;
        private const int MaxTessellationPoints = 20000;
        private const int MinTessellationPoints = 64;
        private const double RadialToleranceFactor = 0.02;
        private const double MinRadialTolerance = 0.25;
        private const double MaxArcSweepDeg = 120.0;
        private const double ArcPathFitTolerance = 0.75;

        public bool IsClosed { get; set; }
        public bool IsBSpline { get; set; }

        private readonly List<RationalBSplinePoint> _point;
        private readonly int _degree;
        private readonly IList<double> _knotvector;
        private readonly Geometry geometry;

        public static implicit operator Geometry(NurbsShape nurbs)
        {
            return nurbs.geometry;
        }

        public NurbsShape(List<RationalBSplinePoint> Points, int Degree, IList<double> KnotVector, bool IsBspline)
        {
            this._point = Points;
            this._degree = Degree;
            this._knotvector = KnotVector;
            this.IsBSpline = IsBspline;

            this.geometry = BuildGeometry();
        }

        private PathGeometry BuildGeometry()
        {
            List<Point> tessellated = TessellateCurvePrecise();
            if (tessellated.Count < 2)
            {
                return null;
            }

            return ConvertTessellatedToArcGeometry(tessellated);
        }

        /// <summary>
        /// Точная тесселяция по длине хорды (до MaxTessellationPoints точек).
        /// </summary>
        private List<Point> TessellateCurvePrecise()
        {
            List<Point> points = new List<Point>(MinTessellationPoints) { EvaluatePoint(0.0) };
            double t = 0.0;
            const double minDt = 1e-9;

            while (t < 1.0 - minDt && points.Count < MaxTessellationPoints)
            {
                Point from = points[points.Count - 1];
                double tNext = FindParameterForChord(from, t, PreciseChordStep);
                Point next = EvaluatePoint(tNext);

                if (Distance(from, next) < 1e-9 && tNext >= 1.0 - minDt)
                {
                    break;
                }

                points.Add(next);
                t = tNext;
            }

            int minCount = Math.Max(MinTessellationPoints, EstimateUniformPointCount());
            while (points.Count < minCount && points.Count < MaxTessellationPoints)
            {
                List<Point> uniform = new List<Point>(minCount + 1);
                for (int i = 0; i < minCount; i++)
                {
                    uniform.Add(EvaluatePoint((double)i / (minCount - 1)));
                }

                if (PolylineLength(uniform) > PolylineLength(points))
                {
                    points = uniform;
                }

                break;
            }

            Point endPoint = EvaluatePoint(1.0);
            if (points.Count == 0 || Distance(points[points.Count - 1], endPoint) > 1e-6)
            {
                if (points.Count < MaxTessellationPoints)
                {
                    points.Add(endPoint);
                }
                else
                {
                    points[points.Count - 1] = endPoint;
                }
            }

            return points;
        }

        private int EstimateUniformPointCount()
        {
            const int sampleCount = 40;
            double estimatedLength = 0.0;
            Point prev = EvaluatePoint(0.0);

            for (int i = 1; i <= sampleCount; i++)
            {
                Point curr = EvaluatePoint((double)i / sampleCount);
                estimatedLength += Distance(prev, curr);
                prev = curr;
            }

            return (int)Math.Ceiling(estimatedLength / PreciseChordStep) + 1;
        }

        private static double PolylineLength(IReadOnlyList<Point> points)
        {
            double length = 0.0;
            for (int i = 1; i < points.Count; i++)
            {
                length += Distance(points[i - 1], points[i]);
            }

            return length;
        }

        private double FindParameterForChord(Point from, double tStart, double targetChord)
        {
            if (Distance(from, EvaluatePoint(1.0)) <= targetChord)
            {
                return 1.0;
            }

            double lo = tStart;
            double hi = 1.0;

            while (hi - lo > 1e-8)
            {
                double mid = (lo + hi) * 0.5;
                if (Distance(from, EvaluatePoint(mid)) < targetChord)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return hi;
        }

        /// <summary>
        /// Упрощение тесселяции до дуг: расстояние до центра + знак угла (алгоритм NurbsShape.txt).
        /// </summary>
        private PathGeometry ConvertTessellatedToArcGeometry(List<Point> points)
        {
            PathFigure figure = new PathFigure
            {
                StartPoint = points[0],
                IsClosed = IsClosed
            };

            if (points.Count == 2)
            {
                figure.Segments.Add(new LineSegment(points[1], true));
                return new PathGeometry(new List<PathFigure> { figure });
            }

            double maxChord = CalculateMaxChord(points);
            double maxRadiusAllowed = Math.Max(1.0, maxChord * 50.0);

            int idx = 1;
            while (idx < points.Count)
            {
                if (idx + 1 >= points.Count)
                {
                    figure.Segments.Add(new LineSegment(points[idx], true));
                    break;
                }

                Point p0 = points[idx - 1];
                Point p1 = points[idx];
                Point p2 = points[idx + 1];

                if (!TryFitCircle(p0, p1, p2, out Point center, out double radius)
                    || double.IsNaN(radius)
                    || double.IsInfinity(radius)
                    || radius <= 1e-6
                    || radius > maxRadiusAllowed)
                {
                    figure.Segments.Add(new LineSegment(p1, true));
                    idx++;
                    continue;
                }

                int startIndex = idx - 1;
                int lastIndex = idx + 1;
                double radialTolerance = Math.Max(MinRadialTolerance, radius * RadialToleranceFactor);
                double initialAngle = GetAngleBetween(points[startIndex], center, points[startIndex + 1]);
                int initialSign = Math.Sign(initialAngle);
                if (initialSign == 0)
                {
                    initialSign = 1;
                }

                while (lastIndex + 1 < points.Count)
                {
                    Point candidate = points[lastIndex + 1];
                    double distToCenter = Distance(candidate, center);
                    if (Math.Abs(distToCenter - radius) > radialTolerance)
                    {
                        break;
                    }

                    double angleToCandidate = GetAngleBetween(points[startIndex], center, candidate);
                    if (double.IsNaN(angleToCandidate) || double.IsInfinity(angleToCandidate))
                    {
                        break;
                    }

                    int signToCandidate = Math.Sign(angleToCandidate);
                    if (signToCandidate == 0)
                    {
                        signToCandidate = initialSign;
                    }

                    if (signToCandidate != initialSign)
                    {
                        break;
                    }

                    if (Math.Abs(angleToCandidate) > MaxArcSweepDeg)
                    {
                        break;
                    }

                    int midCandidate = (startIndex + lastIndex + 1) / 2;
                    if (!TryFitCircle(points[startIndex], points[midCandidate], candidate, out Point refitCenter, out double refitRadius)
                        || Math.Abs(refitRadius - radius) > radialTolerance * 2
                        || Distance(refitCenter, center) > radialTolerance * 2)
                    {
                        break;
                    }

                    lastIndex++;
                }

                Point arcStart = points[startIndex];
                ArcSegment arcSegment = null;
                int acceptedLast = startIndex + 1;

                for (int tryLast = lastIndex; tryLast >= startIndex + 2; tryLast--)
                {
                    List<Point> arcPoints = new List<Point>(tryLast - startIndex + 1);
                    for (int j = startIndex; j <= tryLast; j++)
                    {
                        arcPoints.Add(points[j]);
                    }

                    if (GCTools.GetArcSegmentFromList(arcPoints) is ArcSegment candidateArc
                        && !candidateArc.Size.IsEmpty
                        && !double.IsNaN(candidateArc.Size.Width)
                        && candidateArc.Size.Width > 1e-6
                        && ArcMatchesPoints(arcStart, candidateArc, arcPoints))
                    {
                        arcSegment = candidateArc;
                        acceptedLast = tryLast;
                        break;
                    }
                }

                if (arcSegment != null)
                {
                    figure.Segments.Add(arcSegment);
                    idx = acceptedLast + 1;
                    continue;
                }

                for (int k = idx; k <= lastIndex; k++)
                {
                    figure.Segments.Add(new LineSegment(points[k], true));
                }

                idx = lastIndex + 1;
            }

            if (figure.Segments.Count == 0)
            {
                return null;
            }

            return new PathGeometry(new List<PathFigure> { figure });
        }

        private static bool ArcMatchesPoints(Point arcStart, ArcSegment arc, IReadOnlyList<Point> arcPoints)
        {
            if (arcPoints.Count < 2)
            {
                return false;
            }

            PathFigure figure = new PathFigure { StartPoint = arcStart };
            figure.Segments.Add(arc);
            PathGeometry geometry = new PathGeometry(new List<PathFigure> { figure });
            PathGeometry flattened = geometry.GetFlattenedPathGeometry(0.1, ToleranceType.Absolute);

            List<Point> flatPoints = new List<Point>();
            foreach (PathFigure flatFigure in flattened.Figures)
            {
                flatPoints.Add(flatFigure.StartPoint);
                foreach (PathSegment segment in flatFigure.Segments)
                {
                    if (segment is LineSegment line)
                    {
                        flatPoints.Add(line.Point);
                    }
                    else if (segment is PolyLineSegment polyline)
                    {
                        foreach (Point point in polyline.Points)
                        {
                            flatPoints.Add(point);
                        }
                    }
                }
            }

            if (flatPoints.Count == 0)
            {
                return false;
            }

            double radius = arc.Size.Width;
            double tolerance = Math.Max(ArcPathFitTolerance, radius * 0.005);

            foreach (Point sample in arcPoints)
            {
                if (MinDistanceToPolyline(sample, flatPoints) > tolerance)
                {
                    return false;
                }
            }

            Point arcEnd = arcPoints[arcPoints.Count - 1];
            if (Distance(arcEnd, arc.Point) > tolerance)
            {
                return false;
            }

            return true;
        }

        private static double MinDistanceToPolyline(Point point, IReadOnlyList<Point> polyline)
        {
            double min = double.MaxValue;
            for (int i = 1; i < polyline.Count; i++)
            {
                min = Math.Min(min, PerpendicularDistance(point, polyline[i - 1], polyline[i]));
            }

            return min;
        }

        private static double PerpendicularDistance(Point point, Point lineStart, Point lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared < 1e-18)
            {
                return Distance(point, lineStart);
            }

            double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));
            double projX = lineStart.X + t * dx;
            double projY = lineStart.Y + t * dy;
            return Distance(point, new Point(projX, projY));
        }

        private static double CalculateMaxChord(IReadOnlyList<Point> points)
        {
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);
            double dx = maxX - minX;
            double dy = maxY - minY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static bool TryFitCircle(Point start, Point middle, Point end, out Point center, out double radius)
        {
            double x1 = (middle.X + start.X) / 2.0;
            double y1 = (middle.Y + start.Y) / 2.0;
            double dy1 = middle.X - start.X;
            double dx1 = -(middle.Y - start.Y);

            double x2 = (end.X + middle.X) / 2.0;
            double y2 = (end.Y + middle.Y) / 2.0;
            double dy2 = end.X - middle.X;
            double dx2 = -(end.Y - middle.Y);

            double denominator = dy1 * dx2 - dx1 * dy2;
            if (Math.Abs(denominator) < 1e-12)
            {
                center = new Point(double.NaN, double.NaN);
                radius = double.NaN;
                return false;
            }

            double t = ((x1 - x2) * dy2 + (y2 - y1) * dx2) / denominator;
            center = new Point(x1 + dx1 * t, y1 + dy1 * t);
            double dx = center.X - start.X;
            double dy = center.Y - start.Y;
            radius = Math.Sqrt(dx * dx + dy * dy);

            return !double.IsNaN(radius) && !double.IsInfinity(radius) && radius > 1e-6;
        }

        private static double GetAngleBetween(Point from, Point center, Point to)
        {
            Vector v1 = new Vector(from.X - center.X, from.Y - center.Y);
            Vector v2 = new Vector(to.X - center.X, to.Y - center.Y);
            return Vector.AngleBetween(v1, v2);
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private Point EvaluatePoint(double t)
        {
            if (this.IsBSpline)
            {
                return BSplinePoint(this._point, this._degree, this._knotvector, t);
            }

            return RationalBSplinePoint(this._point, this._degree, this._knotvector, t);
        }

        private Point BSplinePoint(IList<RationalBSplinePoint> points, int degree, IList<double> knotVector, double t)
        {
            double x = 0;
            double y = 0;
            for (int i = 0; i < points.Count; i++)
            {
                double nip = Nip(i, degree, knotVector, t);
                x += points[i].X * nip;
                y += points[i].Y * nip;
            }

            return new Point(x, y);
        }

        private Point RationalBSplinePoint(IList<RationalBSplinePoint> points, int degree, IList<double> knotVector, double t)
        {
            double x = 0;
            double y = 0;
            double rationalWeight = 0d;

            for (int i = 0; i < points.Count; i++)
            {
                double nip = Nip(i, degree, knotVector, t) * points[i].Weight;
                rationalWeight += nip;
            }

            const double eps = 1e-12;
            if (Math.Abs(rationalWeight) < eps)
            {
                return new Point(points[0].X, points[0].Y);
            }

            for (int i = 0; i < points.Count; i++)
            {
                double nip = Nip(i, degree, knotVector, t);
                x += points[i].X * points[i].Weight * nip / rationalWeight;
                y += points[i].Y * points[i].Weight * nip / rationalWeight;
            }

            return new Point(x, y);
        }

        private double Nip(int pointIndex, int degree, IList<double> knot, double step)
        {
            double[] n = new double[degree + 1];
            double saved;
            double temp;

            step = step * knot[knot.Count - 1];

            int m = knot.Count - 1;
            if ((pointIndex == 0 && step == knot[0]) || (pointIndex == (m - degree - 1) && step == knot[m]))
            {
                return 1;
            }

            if (step < knot[pointIndex] || step >= knot[pointIndex + degree + 1])
            {
                return 0;
            }

            for (int j = 0; j <= degree; j++)
            {
                double round = Math.Round(step, 6);
                if (step >= knot[pointIndex + j] && round < knot[pointIndex + j + 1])
                {
                    n[j] = 1d;
                }
                else
                {
                    n[j] = 0d;
                }
            }

            const double eps = 1e-12;
            for (int k = 1; k <= degree; k++)
            {
                if (n[0] == 0)
                {
                    saved = 0d;
                }
                else
                {
                    double denom = knot[pointIndex + k] - knot[pointIndex];
                    saved = Math.Abs(denom) < eps ? 0d : ((step - knot[pointIndex]) * n[0]) / denom;
                }

                for (int j = 0; j < degree - k + 1; j++)
                {
                    double uLeft = knot[pointIndex + j + 1];
                    double uRight = knot[pointIndex + j + k + 1];
                    double denom = uRight - uLeft;

                    if (n[j + 1] == 0 || Math.Abs(denom) < eps)
                    {
                        n[j] = saved;
                        saved = 0d;
                    }
                    else
                    {
                        temp = n[j + 1] / denom;
                        n[j] = saved + (uRight - step) * temp;
                        saved = (step - uLeft) * temp;
                    }
                }
            }

            return n[0];
        }
    }
}

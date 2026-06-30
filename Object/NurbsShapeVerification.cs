using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object
{
    public static class NurbsShapeVerification
    {
        public static string RunQuarterCircleTest()
        {
            NurbsShape quarterCircle = CreateQuarterCircleNurbs(100.0);
            PathGeometry geometry = (PathGeometry)(Geometry)quarterCircle;

            if (geometry == null || geometry.Figures.Count == 0)
            {
                return "FAIL: empty geometry";
            }

            PathFigure figure = geometry.Figures[0];
            bool hasArc = figure.Segments.OfType<ArcSegment>().Any();
            bool hasPolyline = figure.Segments.OfType<PolyLineSegment>().Any();
            if (!hasArc && !hasPolyline)
            {
                return "FAIL: expected arc and/or polyline segments";
            }

            Point mid = EvaluateQuarterCircleMidpoint(100.0);
            PathGeometry flattened = geometry.GetFlattenedPathGeometry(0.25, ToleranceType.Absolute);
            double minDistance = double.MaxValue;

            foreach (PathFigure flatFigure in flattened.Figures)
            {
                minDistance = System.Math.Min(minDistance, Distance(flatFigure.StartPoint, mid));
                foreach (PathSegment segment in flatFigure.Segments)
                {
                    if (segment is LineSegment line)
                    {
                        minDistance = System.Math.Min(minDistance, Distance(line.Point, mid));
                    }
                    else if (segment is PolyLineSegment polyline)
                    {
                        foreach (Point point in polyline.Points)
                        {
                            minDistance = System.Math.Min(minDistance, Distance(point, mid));
                        }
                    }
                }
            }

            if (minDistance > 2.0)
            {
                return $"FAIL: midpoint too far from curve ({minDistance:F2})";
            }

            return "PASS";
        }

        private static NurbsShape CreateQuarterCircleNurbs(double radius)
        {
            double w = System.Math.Cos(System.Math.PI / 4.0);
            List<RationalBSplinePoint> points = new List<RationalBSplinePoint>
            {
                new RationalBSplinePoint(radius, 0, 1.0),
                new RationalBSplinePoint(radius, radius, w),
                new RationalBSplinePoint(0, radius, 1.0)
            };

            List<double> knots = new List<double> { 0, 0, 0, 1, 1, 1 };
            return new NurbsShape(points, 2, knots, false);
        }

        private static Point EvaluateQuarterCircleMidpoint(double radius)
        {
            double angle = System.Math.PI / 4.0;
            return new Point(radius * System.Math.Cos(angle), radius * System.Math.Sin(angle));
        }

        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

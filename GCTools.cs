using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using ToGeometryConverter.Object.Elements;
using System.Windows.Media.Media3D;
using System.Numerics;

namespace ToGeometryConverter
{
    public static class GCTools
    {
        public delegate void Logging(string message);
        public static Logging Log;

        public delegate void Progress(int position, int max, string message);
        public static Progress SetProgress;

        public static Point Pftp(System.Drawing.PointF point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point Dxftp(DxfPoint point, DxfVector normal)
        {
            Vector3 VectorNormal = new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
            Vector3 VectorPoint = new Vector3((float)point.X, (float)point.Y, (float)point.Z);
            System.Numerics.Quaternion quaternion = System.Numerics.Quaternion.CreateFromAxisAngle(VectorNormal, 0);
            Vector3 outVector = Vector3.Transform(VectorPoint, quaternion);
            if (normal.Z < 0) 
            { }
            return new Point
            {
                X = outVector.X,
                Y = outVector.Y,
            };
        }


        public static Point DxfLwVtp(DxfLwPolylineVertex point)
        {
            return new Point(point.X, -point.Y);
        }

        /// <summary>
        /// Get Angle in Deg between three points
        /// </summary>
        /// <param name="A">first</param>
        /// <param name="B">middle</param>
        /// <param name="C">end</param>
        /// <returns></returns>
        public static double GetAngleThreePoint(Point A, Point B, Point C)
        {
            Vector AB = B - A;
            Vector CB = B - C;

            return Vector.AngleBetween(AB, CB);
        }

        public static double GatAbsoluteAngle(Point A, Point Center)
        {
            Point C = new Point(Center.X + 100, Center.Y);
            Vector AB = Center - A;
            Vector CB = Center - C;

            return Vector.AngleBetween(AB, CB);
        }


        public static double Lenth2D(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        public static double Lenth3D(GCPoint3D point1, GCPoint3D point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        public static Geometry FigureToGeometry(PathFigure pathFigure)
        {
            return new PathGeometry()
            {
                Figures = new PathFigureCollection()
                {
                    pathFigure
                },
                FillRule = FillRule.Nonzero
            };
        }

        internal static PathSegment GetArcSegmentFromList(List<Point> tesselatePoints)
        {
            Point A = tesselatePoints[0];
            Point B = tesselatePoints[tesselatePoints.Count / 2];
            Point C = tesselatePoints.Last();


            double offset = Math.Pow(B.X, 2) + Math.Pow(B.Y, 2);
            double bc = (Math.Pow(A.X, 2) + Math.Pow(A.Y, 2) - offset) / 2.0;
            double cd = (offset - Math.Pow(C.X, 2) - Math.Pow(C.Y, 2)) / 2.0;
            double det = (A.X - B.X) * (B.Y - C.Y) - (B.X - C.X) * (A.Y - B.Y);

            double idet = 1 / det;

            Point Center = new Point((bc * (B.Y - C.Y) - cd * (A.Y - B.Y)) * idet, (cd * (A.X - B.X) - bc * (B.X - C.X)) * idet);

            double radius = Math.Sqrt(Math.Pow(B.X - Center.X, 2) + Math.Pow(B.Y - Center.Y, 2));

            bool isLarge = Math.Abs(GCTools.GetAngleThreePoint(A, Center, C)) < Math.Abs((GCTools.GetAngleThreePoint(B, Center, C) + GCTools.GetAngleThreePoint(A, Center, B)));

            SweepDirection sweepDirection = (GCTools.GetAngleThreePoint(A, Center, C) < 0 && isLarge == false) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

            double rotationAngle = Math.Abs(Math.Abs(GCTools.GetAngleThreePoint(A, Center, C) % 360) - (sweepDirection == SweepDirection.Counterclockwise ? 0 : 360));

            return new ArcSegment(C, new Size(radius, radius), rotationAngle,
                isLarge, sweepDirection, true);

        }


        public static List<PointsElement> GetGeometryPoints(Geometry geometry, double RoundStep, double RadiusEdge)
        {
            List<PointsElement> pointsObjects = new List<PointsElement>();

            if (geometry is PathGeometry pathGeometry)
            {
                pointsObjects.AddRange(GetPathPoint(pathGeometry));
            }
            else if (geometry is GeometryGroup geometryGroup)
            {
                foreach (Geometry geometryG in geometryGroup.Children)
                {
                    pointsObjects.AddRange(GCTools.GetGeometryPoints(geometryG, RoundStep, RadiusEdge));
                }
            }
            else if (geometry is EllipseGeometry ellipseGeometry)
            {
                PointsElement ellipseObj = new PointsElement();

                double C_x = ellipseGeometry.Center.X, C_y = ellipseGeometry.Center.Y, w = ellipseGeometry.RadiusX, h = ellipseGeometry.RadiusY;

                double step = (Math.PI * 2) / ((Math.PI * (w + h)) / RoundStep);

                for (double t = 0; t <= 2 * Math.PI; t += step)
                {
                    ellipseObj.Add(new GCPoint3D()
                    {
                        X = C_x + (w / 2) * Math.Cos(t),
                        Y = C_y + (h / 2) * Math.Sin(t)
                    });
                }
                pointsObjects.Add(ellipseObj);
            }
            else if (geometry is LineGeometry lineGeometry)
            {
                pointsObjects.Add(new PointsElement()
                {
                    Points = new List<GCPoint3D>()
                            {
                                new GCPoint3D(lineGeometry.StartPoint.X, lineGeometry.StartPoint.Y, 0),
                                new GCPoint3D(lineGeometry.EndPoint.X, lineGeometry.EndPoint.Y, 0)
                            }
                });
            }
            
            return pointsObjects;

            List<PointsElement> GetPathPoint(PathGeometry pathGeometry)
            {
                List<PointsElement> PathPointList = new List<PointsElement>();
                foreach (PathFigure figure in pathGeometry.Figures)
                {
                    PointsElement lObject = new PointsElement();
                    lObject.IsClosed = figure.IsClosed;
                    lObject.Add(new GCPoint3D(figure.StartPoint.X, figure.StartPoint.Y, 0));

                    Point LastPoint = figure.StartPoint;

                    if (figure.Segments.Count > 0)
                    {
                        foreach (PathSegment segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case BezierSegment bezierSegment:
                                    lObject.AddRange(
                                        BezieByStep(
                                            LastPoint, bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, RoundStep));
                                    LastPoint = bezierSegment.Point3;
                                    break;

                                case PolyBezierSegment polyBezierSegment:
                                    for (int i = 0; i < polyBezierSegment.Points.Count - 2; i += 3)
                                    {
                                        lObject.AddRange(
                                            BezieByStep(
                                                LastPoint, polyBezierSegment.Points[i], polyBezierSegment.Points[i + 2], polyBezierSegment.Points[i + 1], RoundStep));
                                        LastPoint = polyBezierSegment.Points[i + 1];
                                    }
                                    break;

                                case LineSegment lineSegment:
                                    lObject.Add(new GCPoint3D(lineSegment.Point.X, lineSegment.Point.Y, 0));
                                    LastPoint = lineSegment.Point;
                                    break;

                                case PolyLineSegment polyLineSegment:
                                    for (int i = 0; i < polyLineSegment.Points.Count; i++)
                                    {
                                        lObject.Add(new GCPoint3D(polyLineSegment.Points[i].X, polyLineSegment.Points[i].Y, 0));
                                        LastPoint = polyLineSegment.Points.Last();
                                    }
                                    break;

                                case PolyQuadraticBezierSegment polyQuadraticBezier:
                                    for (int i = 0; i < polyQuadraticBezier.Points.Count - 1; i += 2)
                                    {
                                        lObject.AddRange(
                                            QBezierByStep(
                                                LastPoint, polyQuadraticBezier.Points[i], polyQuadraticBezier.Points[i + 1], RoundStep));
                                        LastPoint = polyQuadraticBezier.Points[i + 1];
                                    }
                                    break;

                                case QuadraticBezierSegment quadraticBezierSegment:
                                    lObject.AddRange(
                                        QBezierByStep(
                                            LastPoint, quadraticBezierSegment.Point1, quadraticBezierSegment.Point2, RoundStep));
                                    LastPoint = quadraticBezierSegment.Point2;
                                    break;

                                case ArcSegment arcSegment:
                                    SweepDirection sweepDirection = arcSegment.SweepDirection;

                                    foreach (Point lPoint3D in CircleByStep(
                                            LastPoint, arcSegment.Point, arcSegment.Size.Width,
                                            RadiusEdge, sweepDirection, RoundStep, arcSegment.RotationAngle))
                                    {
                                        lObject.Add(new GCPoint3D(lPoint3D.X, lPoint3D.Y, 0));
                                    }
                                    LastPoint = arcSegment.Point;
                                    break;
                                default:
                                    Console.WriteLine($"Unkom type segment: {segment.GetType().Name}");
                                    break;
                            }
                        }
                    }
                    if (lObject.Count > 0)
                        PathPointList.Add(lObject);
                }
                return PathPointList;
            }
        }


        /// <summary>
        /// interpolation Qbezier
        /// </summary>
        public static PointsElement QBezierByStep(Point StartPoint, Point ControlPoint, Point EndPoint, double CRS)
        {
            GCPoint3D LastPoint = new GCPoint3D(StartPoint.X, StartPoint.Y, 0);
            double Lenth = 0;
            for (int t = 1; t < 100; t++)
            {
                GCPoint3D tempPoint = GetPoint((double)t / 99);
                Lenth += GCTools.Lenth3D(LastPoint, tempPoint);
                LastPoint = tempPoint;
            }

            int CountStep = (int)(Lenth / (CRS)) >= 2 ? (int)(Lenth / CRS) : 2;

            PointsElement tempObj = new PointsElement();

            for (int t = 0; t < CountStep; t++)
            {
                tempObj.Add(GetPoint((double)t / (CountStep - 1)));
            }

            return tempObj;

            GCPoint3D GetPoint(double t)
            {
                return new GCPoint3D(
                    (1 - t) * (1 - t) * StartPoint.X + 2 * (1 - t) * t * ControlPoint.X + t * t * EndPoint.X,
                   (1 - t) * (1 - t) * StartPoint.Y + 2 * (1 - t) * t * ControlPoint.Y + t * t * EndPoint.Y, 
                   0);
            }
        }

        /// <summary>
        /// interpolation bezier
        /// </summary>
        public static PointsElement BezieByStep(Point point0, Point point1, Point point2, Point point3, double CRS)
        {
            double Lenth = 0;
            GCPoint3D LastPoint = new GCPoint3D(point1.X, point1.Y, 0);

            for (int t = 0; t < 100; t++)
            {
                GCPoint3D tempPoint = GetPoint((double)t / 99);
                Lenth += GCTools.Lenth3D(LastPoint, tempPoint);
                LastPoint = tempPoint;
            }

            PointsElement tempObj = new PointsElement();

            int CountStep = (int)(Lenth / CRS) >= 2 ? (int)(Lenth / CRS) : 2;

            for (int t = 0; t < CountStep; t++)
            {
                tempObj.Add(GetPoint((double)t / (CountStep - 1)));
            }

            return tempObj;

            GCPoint3D GetPoint(double t)
            {
                return new GCPoint3D(
                    ((1 - t) * (1 - t) * (1 - t)) * point0.X
                           + 3 * ((1 - t) * (1 - t)) * t * point1.X
                           + 3 * (1 - t) * (t * t) * point2.X
                           + (t * t * t) * point3.X,
                    ((1 - t) * (1 - t) * (1 - t)) * point0.Y
                       + 3 * ((1 - t) * (1 - t)) * t * point1.Y
                       + 3 * (1 - t) * (t * t) * point2.Y
                       + (t * t * t) * point3.Y,
                    0);
            }
        }

        /// <summary>
        /// interpolation Circle or arc
        /// </summary>
        public static PointCollection CircleByStep(Point StartPoint, Point EndPoint, double radius, double radiusEdge, SweepDirection clockwise, double CRS, double Delta = 360)
        {
            Delta *= Math.PI / 180;

            PointCollection lObject = new PointCollection();

            if (Delta != 0)
            {
                Point Center = GetCenterArc(StartPoint, EndPoint, radius, clockwise == SweepDirection.Clockwise, Delta > Math.PI && clockwise == SweepDirection.Counterclockwise);

                double StartAngle = Math.PI * 2 - Math.Atan2(StartPoint.Y - Center.Y, StartPoint.X - Center.X);

                double koeff = (radius / radiusEdge) < 0.3 ? 0.3 : (radius / radiusEdge);
                koeff = (radius / radiusEdge) > 3 ? 3 : (radius / radiusEdge);

                double RadianStep = Delta / (int)((Delta * radius) / CRS);

                if (double.IsInfinity(RadianStep) == true) RadianStep = 1;

                for (double radian = 0; radian <= Delta * 1.005; radian += RadianStep)
                {
                    double Angle = (StartAngle + (clockwise == SweepDirection.Counterclockwise ? radian : -radian)) % (2 * Math.PI);

                    lObject.Add(new Point(
                        Center.X + (radius * Math.Cos(Angle)),
                        Center.Y - (radius * Math.Sin(Angle))
                        ));
                }
            }
            else
            {
                if (clockwise == SweepDirection.Counterclockwise)
                {
                    lObject.Add(StartPoint);
                    lObject.Add(EndPoint);
                }
                else
                {
                    lObject.Add(EndPoint);
                    lObject.Add(StartPoint);
                }

            }

            return lObject;

            Point GetCenterArc(Point StartPt, Point EndPt, double r, bool Clockwise, bool large)
            {
                double radsq = r * r;
                double q = Math.Sqrt(Math.Pow(EndPt.X - StartPt.X, 2) + Math.Pow(EndPt.Y - StartPt.Y, 2));
                double x3 = (StartPt.X + EndPt.X) / 2;
                double y3 = (StartPt.Y + EndPt.Y) / 2;
                double d1 = 0;
                double d2 = 0;
                if (radsq > 0)
                {
                    d1 = Math.Sqrt(radsq - ((q / 2) * (q / 2))) * ((StartPt.Y - EndPt.Y) / q) * (large ? -1 : 1);
                    d2 = Math.Sqrt(radsq - ((q / 2) * (q / 2))) * ((EndPt.X - StartPt.X) / q) * (large ? -1 : 1);
                }
                return new Point(
                    x3 + (Clockwise ? d1 : -d1),
                    y3 + (Clockwise ? d2 : -d2)
                    );
            }
        }

        public static GeometryGroup GetPointsGeometries(List<PointsElement> ListPoints)
        {
            GeometryGroup geometries = new GeometryGroup();

            foreach (PointsElement obj in ListPoints)
            {
                PathFigure pathFigure = new PathFigure()
                {
                    StartPoint = new Point(obj.Points[0].X, obj.Points[0].Y),
                    IsClosed = obj.IsClosed
                };
                for (int i = 1; i < obj.Points.Count; i += 1)
                {
                    pathFigure.Segments.Add(new LineSegment(new Point(obj.Points[i].X, obj.Points[i].Y), true));
                }

                geometries.Children.Add(GCTools.FigureToGeometry(pathFigure));
            }

            return geometries;
        }

        public static List<PointsElement> TransformPoint(Transform3D Transform, List<PointsElement> InnerList)
        {
            List<PointsElement> OutList = new List<PointsElement>();
            foreach (PointsElement ListPoints in InnerList)
            {
                PointsElement OutPoints = new PointsElement()
                {
                    IsClosed = ListPoints.IsClosed
                };
                foreach (GCPoint3D point in ListPoints)
                {
                    if (Transform != null)
                    {
                        Transform.TryTransform(point.GetPoint3D, out Point3D result);
                        OutPoints.Add(result);
                    }
                    else
                    {
                        OutPoints.Add(point.GetPoint3D);
                    }
                }
                OutList.Add(OutPoints);
            }
            return OutList;
        }

        public static string GetName(string Filepath)
        {
            return Filepath.Split('\\').Last();
        }

        public static GCFormat GetConverter(string Filename, ICollection<GCFormat> formats)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (GCFormat format in formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm.ToLower() == InFileFormat.ToLower())
                    {
                        return format;
                    }
                }
            }
            return null;
        }
    }
}

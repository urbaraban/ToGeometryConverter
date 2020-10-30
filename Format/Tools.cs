using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToGeometryConverter.Format
{
    public static class Tools
    {
        //public static PathGeometry Geometry { get; set; }

        public static System.Windows.Point Pftp(PointF point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static System.Windows.Point Dxftp(DxfPoint point)
        {
            return new System.Windows.Point(point.X, -point.Y);
        }

        public static System.Windows.Point DxfCtp(DxfControlPoint point)
        {
            return new System.Windows.Point(point.Point.X, -point.Point.Y);
        }

        public static System.Windows.Point DxfLwVtp(DxfLwPolylineVertex point)
        {
            return new System.Windows.Point(point.X, -point.Y);
        }

        /// <summary>
        /// Get Angle in Deg between three points
        /// </summary>
        /// <param name="A">first</param>
        /// <param name="B">middle</param>
        /// <param name="C">end</param>
        /// <returns></returns>
        public static double GetAngleThreePoint(System.Windows.Point A, System.Windows.Point B, System.Windows.Point C)
        {
            Vector AB = B - A;
            Vector CB = B - C;

            return Vector.AngleBetween(AB, CB);
        }

        public static double GatAbsoluteAngle(System.Windows.Point A, System.Windows.Point Center)
        {
            System.Windows.Point C = new System.Windows.Point(Center.X + 100, Center.Y);
            Vector AB = Center - A;
            Vector CB = Center - C;

            return Vector.AngleBetween(AB, CB);
        }


        public static double Lenth(System.Windows.Point point1, System.Windows.Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        public static Path FigureToShape(PathFigure pathFigure)
        {
            return GeometryToShape(new PathGeometry()
            {
                Figures = new PathFigureCollection()
                {
                    pathFigure
                },
                FillRule = FillRule.Nonzero 
            });;
        }

        public static Path GeometryToShape(Geometry geometry)
        {
            return new Path
            {
                Data = geometry
            }; 
        }

        internal static PathSegment GetArcSegmentFromList(List<System.Windows.Point> tesselatePoints)
        {
            System.Windows.Point A = tesselatePoints[0];
            System.Windows.Point B = tesselatePoints[tesselatePoints.Count / 2];
            System.Windows.Point C = tesselatePoints.Last();


            double offset = Math.Pow(B.X, 2) + Math.Pow(B.Y, 2);
            double bc = (Math.Pow(A.X, 2) + Math.Pow(A.Y, 2) - offset) / 2.0;
            double cd = (offset - Math.Pow(C.X, 2) - Math.Pow(C.Y, 2)) / 2.0;
            double det = (A.X - B.X) * (B.Y - C.Y) - (B.X - C.X) * (A.Y - B.Y);

            double idet = 1 / det;

            System.Windows.Point Center = new System.Windows.Point((bc * (B.Y - C.Y) - cd * (A.Y - B.Y)) * idet, (cd * (A.X - B.X) - bc * (B.X - C.X)) * idet);

            double radius = Math.Sqrt(Math.Pow(B.X - Center.X, 2) + Math.Pow(B.Y - Center.Y, 2));

            bool isLarge = Math.Abs(Tools.GetAngleThreePoint(A, Center, C)) < Math.Abs((Tools.GetAngleThreePoint(B, Center, C) + Tools.GetAngleThreePoint(A, Center, B)));

            SweepDirection sweepDirection = (Tools.GetAngleThreePoint(A, Center, C) < 0 && isLarge == false) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

            double rotationAngle = Math.Abs(Math.Abs(Tools.GetAngleThreePoint(A, Center, C) % 360) - (sweepDirection == SweepDirection.Counterclockwise ? 0 : 360));

            return new ArcSegment(C, new System.Windows.Size(radius, radius), rotationAngle,
                isLarge, sweepDirection, true);

        }

        internal static Shape GetEllipseFromList(List<System.Windows.Point> tesselatePoints)
        {
            return Tools.GeometryToShape(new EllipseGeometry());
        }
    }
}

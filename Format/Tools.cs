using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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

        public static System.Windows.Point Dbltp(double x, double y)
        {
            return new System.Windows.Point(x, y);
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
    }
}

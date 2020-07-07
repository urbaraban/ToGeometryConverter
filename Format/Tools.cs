using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    public static class Tools
    {
        public static PathGeometry Geometry;

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

        public static void FindInterContour(PathFigure innerFigure)
        {
            List<string> inArr = GetPArr(innerFigure.ToString());

            switch (innerFigure.GetType().FullName)
            {
                case "System.Windows.Media.PolyQuadraticBezierSegment":
                    break;
            }

            if (!innerFigure.IsClosed) 
            {

                foreach (PathFigure figure in Geometry.Figures)
                {
                    if (!figure.IsClosed)
                    {
                        List<string> tempArr = GetPArr(figure.ToString());

                        int MinInd = Math.Min(inArr.First().Length, tempArr.Last().Length) - 1;

                        if (CheckPointString(tempArr.Last().Split(';'),inArr.First().Split(';')))
                        {
                            for (int i = 0; i < innerFigure.Segments.Count; i++)
                                figure.Segments.Add(innerFigure.Segments[i]);
                            return;

                        }
                        else if (CheckPointString(tempArr.First().Split(';'), inArr.Last().Split(';')))
                        {
                            figure.StartPoint = innerFigure.StartPoint;
                            for (int i = innerFigure.Segments.Count - 1; i > -1; i--)
                                figure.Segments.Insert(0, innerFigure.Segments[i]);
                            return;
                        }
                        else if (CheckPointString(tempArr.First().Split(';'), inArr.First().Split(';')))
                        {
                            Console.WriteLine("invertFirst " + tempArr.First() + " " + tempArr.First());
                            Geometry.Figures.Add(innerFigure);
                            return;
                        }
                        else if (CheckPointString(tempArr.Last().Split(';'), inArr.Last().Split(';')))
                        {
                            Console.WriteLine("InvertLast " + tempArr.Last() + " " + tempArr.Last());
                            Geometry.Figures.Add(innerFigure);
                            return;
                        }
                    }
                }
            }

            Geometry.Figures.Add(innerFigure);

            bool CheckPointString(string[] Point1, string[] Point2)
            {
                int min0 = Math.Min(Point1[0].Length, Point2[0].Length) - 4;
                int min1 = Math.Min(Point1[1].Length, Point2[1].Length) - 4;
                double P1X = double.Parse(Point1[0]);
                double P1Y = double.Parse(Point1[1]);
                double P2X = double.Parse(Point2[0]);
                double P2Y = double.Parse(Point2[1]);

                return Math.Round(P1X, 2) == Math.Round(P2X, 2) && Math.Round(P1Y, 2) == Math.Round(P2Y, 2);
            }

            List<string> GetPArr(string str)
            {
                string[] ArrStr = Regex.Split(str, @"[a-zA-Z]+");

                List<string> ListStr = new List<string>();

                for (int i = 0; i < ArrStr.Length; i++)
                {
                    if (ArrStr[i] != string.Empty)
                    {
                        string[] PointArr = ArrStr[i].Split(';');

                        if (PointArr.Length >= 2)
                        for (int j = PointArr.Length - 2; j < PointArr.Length; j += 2)
                        {
                            ListStr.Add(PointArr[j].Split(' ').Last() + ";" + PointArr[j + 1].Split(' ').Last());
                        }
                    }
                }

                return ListStr;
            }
        }

        public static PathGeometry MakeTransform (PathGeometry inGmtr)
        {
            TransformGroup Transform = new TransformGroup();

            TranslateTransform Translate = new TranslateTransform();
            RotateTransform Rotate = new RotateTransform();
            ScaleTransform Scale = new ScaleTransform();
            
            Transform.Children.Add(Scale);
            Transform.Children.Add(Rotate);
            Transform.Children.Add(Translate);

            Rotate.CenterX = inGmtr.Bounds.X + Tools.Geometry.Bounds.Width / 2;
            Rotate.CenterY = inGmtr.Bounds.Y + Tools.Geometry.Bounds.Height / 2;

            inGmtr.Transform = Transform;

            return inGmtr;
        }
    }
}

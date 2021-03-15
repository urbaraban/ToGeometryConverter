using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class DXF
    {
        public static string Name = "DXF";
        public static string Short = ".dxf";

        public static GeometryGroup Get(string filename, double CRS)

        {
            DxfFile dxfFile;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
                {
                    dxfFile = DxfFile.Load(fs);
                };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                dxfFile = null;
            }


            if (dxfFile != null)
            {
                return ParseEntities(dxfFile.Entities);
            }

            return null;

            GeometryGroup ParseEntities(IList<DxfEntity> entitys)
            {
                GeometryGroup geometryGroup = new GeometryGroup();

                foreach (DxfEntity entity in entitys)
                {
                    switch (entity.EntityType)
                    {
                        case DxfEntityType.Point:
                            /* DxfPoint dxfPoint = (DxfPoint)entity;
                             geometry.AddGeometry(new LineGeometry(new Point(dxfPoint.X, dxfPoint.Y), new Point(dxfPoint.X, dxfPoint.Y)));*/
                            break;

                        case DxfEntityType.Insert:
                            DxfInsert dxfInsert = (DxfInsert)entity;
                            foreach (DxfBlock dxfBlock in dxfFile.Blocks)
                            {
                                if (dxfInsert.Name == dxfBlock.Name)
                                {
                                    ParseEntities(dxfBlock.Entities);
                                }
                            }
                            break;

                        /*case DxfEntityType.Text:
                            DxfText dxfText = (DxfText)entity;
                            FormattedText formatted = new FormattedText(dxfText.Value,
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                            new Typeface("Tahoma"), dxfText.TextHeight * 5, Brushes.Black);
                            geometryGroup.Children.Add(formatted.BuildGeometry(new Point(dxfText.Location.X, dxfText.Location.Y)));
                            break;
                        case DxfEntityType.MText:
                            DxfMText dxfMText = (DxfMText)entity;
                            FormattedText Mformatted = new FormattedText(dxfMText.Text,
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                            new Typeface("Tahoma"), dxfMText.InitialTextHeight, Brushes.Black);
                            geometryGroup.Children.Add(Mformatted.BuildGeometry(new Point(dxfMText.InsertionPoint.X, dxfMText.InsertionPoint.Y)));
                            break;*/

                        case DxfEntityType.Line:
                            DxfLine line = (DxfLine)entity;
                            geometryGroup.Children.Add(new LineGeometry(Tools.Dxftp(line.P1), Tools.Dxftp(line.P2)));
                            break;

                        case DxfEntityType.Helix:
                            DxfHelix dxfHelix = (DxfHelix)entity;

                            PointCollection points = new PointCollection(GetSpiralPoints(dxfHelix, CRS));

                            geometryGroup.Children.Add(
                                Tools.FigureToGeometry(new PathFigure() 
                                { 
                                    StartPoint = Tools.Dxftp(dxfHelix.AxisBasePoint),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new PolyLineSegment(points, true)
                                    }
                                }));
                            break;

                        case DxfEntityType.MLine:
                            DxfMLine dxfMLine = (DxfMLine)entity;
                            PathFigure MLineFigure = new PathFigure();
                            MLineFigure.StartPoint = Tools.Dxftp(dxfMLine.Vertices[0]);

                            //Идем по точкам
                            for (int i = 1; i < dxfMLine.Vertices.Count; i++)
                                MLineFigure.Segments.Add(new LineSegment(
                                    Tools.Dxftp(dxfMLine.Vertices[i % dxfMLine.Vertices.Count]), true));

                            MLineFigure.IsClosed = MLineFigure.IsClosed;
                            geometryGroup.Children.Add(
                                Tools.FigureToGeometry(MLineFigure));

                            break;

                        case DxfEntityType.Arc:
                            DxfArc dxfArc = (DxfArc)entity;

                            PathFigure ArcContour = new PathFigure();
                            ArcContour.StartPoint = Tools.Dxftp(dxfArc.GetPointFromAngle(dxfArc.StartAngle));

                            DxfPoint arcPoint2 = dxfArc.GetPointFromAngle(dxfArc.EndAngle);

                            ArcContour.Segments.Add(
                                                new ArcSegment(
                                                    Tools.Dxftp(arcPoint2),
                                                new Size(dxfArc.Radius, dxfArc.Radius),
                                                (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360,
                                                (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360 > 180,
                                                SweepDirection.Counterclockwise,
                                                true));

                            geometryGroup.Children.Add(Tools.FigureToGeometry(ArcContour));
                            break;

                        case DxfEntityType.Circle:
                            DxfCircle dxfCircle = (DxfCircle)entity;
                            geometryGroup.Children.Add(new EllipseGeometry(Tools.Dxftp(dxfCircle.Center), dxfCircle.Radius, dxfCircle.Radius));
                            break;

                        case DxfEntityType.Ellipse:
                            DxfEllipse dxfEllipse = (DxfEllipse)entity;
                            double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);
                            geometryGroup.Children.Add(
                                new EllipseGeometry(Tools.Dxftp(dxfEllipse.Center),
                                dxfEllipse.MajorAxis.Length,
                                dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio,
                                new RotateTransform(MajorAngle * 180 / Math.PI))); 
                            break;

                        case DxfEntityType.LwPolyline:
                            DxfLwPolyline dxfLwPolyline = (DxfLwPolyline)entity;
                            PathFigure lwPolyLineFigure = new PathFigure();

                            lwPolyLineFigure.StartPoint = Tools.DxfLwVtp(dxfLwPolyline.Vertices[0]);

                            for (int i = 0; i < dxfLwPolyline.Vertices.Count - 1; i += 1)
                            {
                                double radian = Math.Atan(dxfLwPolyline.Vertices[i].Bulge) * 4;
                                double radius = CalBulgeRadius(Tools.DxfLwVtp(dxfLwPolyline.Vertices[i]), Tools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]), dxfLwPolyline.Vertices[i].Bulge);

                                lwPolyLineFigure.Segments.Add(
                                    new ArcSegment(
                                        Tools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]),
                                        new Size(radius, radius),
                                        Math.Abs(radian * 180 / Math.PI),
                                        Math.Abs(dxfLwPolyline.Vertices[i].Bulge) > 1,
                                        dxfLwPolyline.Vertices[i].Bulge < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                        true
                                    ));

                                //points1.Add(Tools.DxfLwVtp(dxfLwPolyline.Vertices[i % dxfLwPolyline.Vertices.Count]));
                            }

                            lwPolyLineFigure.IsClosed = dxfLwPolyline.IsClosed;

                            geometryGroup.Children.Add(Tools.FigureToGeometry(lwPolyLineFigure));

                            break;

                        case DxfEntityType.Polyline:
                            DxfPolyline dxfPolyline = (DxfPolyline)entity;

                            PathFigure polyLineFigure = new PathFigure();
                            polyLineFigure.StartPoint = Tools.Dxftp(dxfPolyline.Vertices[0].Location);

                            for (int i = 1; i < dxfPolyline.Vertices.Count; i++)
                            {
                                polyLineFigure.Segments.Add(new LineSegment(Tools.Dxftp(dxfPolyline.Vertices[i].Location), true));
                            }

                            polyLineFigure.IsClosed = dxfPolyline.IsClosed;

                            geometryGroup.Children.Add(Tools.FigureToGeometry(polyLineFigure));
                            break;

                        case DxfEntityType.Spline:
                            DxfSpline dxfSpline = (DxfSpline)entity;

                            ObservableCollection<RationalBSplinePoint> rationalBSplinePoints = new ObservableCollection<RationalBSplinePoint>();

                            foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                            {
                                rationalBSplinePoints.Add(new RationalBSplinePoint(Tools.Dxftp(controlPoint.Point), controlPoint.Weight));
                            }
                            geometryGroup.Children.Add(new NurbsShape(rationalBSplinePoints, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, CRS, dxfSpline.IsRational == true));

                            break;
                    }

                }

                return geometryGroup;
            }
        }

        private static double CalBulgeRadius(System.Windows.Point point1, System.Windows.Point point2, double bulge)
        {
            // Calculate the vertex angle
            double cicleAngle = Math.Atan(bulge) * 4;

            //the distance between two points
            double pointLen = Tools.Lenth(point1, point2);
            //According to the normal value back
            double radius = (pointLen / 2) / Math.Sin(cicleAngle / 2);

            if (double.IsInfinity(radius))
            {
                return 0;
            }
            else
            {
                return Math.Abs(radius);
            }

        }

        /// <summary>
        /// Get point from helix by step
        /// </summary>
        /// <param name="dxfHelix"></param>
        /// <returns></returns>
        private static List<Point> GetSpiralPoints(DxfHelix dxfHelix, double CRS)
        {
            double StartAngle = Math.Atan2(dxfHelix.AxisBasePoint.Y - dxfHelix.StartPoint.Y, dxfHelix.AxisBasePoint.X - dxfHelix.StartPoint.X);

            double HelixRadius = Tools.Lenth(new Point(dxfHelix.AxisBasePoint.X, dxfHelix.AxisBasePoint.Y), new Point(dxfHelix.StartPoint.X, dxfHelix.StartPoint.Y));

            // Get the points.
            List<Point> points = new List<Point>();

            int steps = (int)((2 * Math.PI * HelixRadius * dxfHelix.NumberOfTurns) /CRS);

            double dtheta = (Math.PI * 2 * dxfHelix.NumberOfTurns) / steps * (dxfHelix.IsRightHanded ? -1 : 1);    // Five degrees.

            for (int i = 1; Math.Abs(dtheta * i)/ (Math.PI * 2) <= dxfHelix.NumberOfTurns; i++)
            {
                double theta = (StartAngle + dtheta * i);
                Console.WriteLine($"Th: {theta.ToString()}/{Math.PI * 2 * dxfHelix.NumberOfTurns}");
                // Calculate r.
                double r = HelixRadius / steps  * i;

                Console.WriteLine($"R: {r.ToString()}/{HelixRadius}");

                // Convert to Cartesian coordinates.
                double x = r * Math.Cos(theta);
                double y = -r * Math.Sin(theta);

                // Center.

                   x += dxfHelix.AxisBasePoint.X;
                   y -= dxfHelix.AxisBasePoint.Y;
                
                // Create the point.
                points.Add(new Point(x, y));
            }
            //points.Reverse();
            return points;
        }
    }
}


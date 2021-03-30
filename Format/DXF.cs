﻿using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public class DXF : IFormat
    {
        string IFormat.Name => "DXF";
        string[] IFormat.ShortName => new string[1] { "dxf" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public GCCollection Get(string filename, double CRS)
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

            GCCollection ParseEntities(IList<DxfEntity> entitys)
            {
                GCCollection gccollection = new GCCollection();

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
                            gccollection.Add(new GeometryElement(new LineGeometry(GCTools.Dxftp(line.P1), GCTools.Dxftp(line.P2))));
                            break;

                        case DxfEntityType.Helix:
                            DxfHelix dxfHelix = (DxfHelix)entity;

                            PointCollection points = new PointCollection(GetSpiralPoints(dxfHelix, CRS));

                            gccollection.Add(
                                new GeometryElement(
                                GCTools.FigureToGeometry(new PathFigure() 
                                { 
                                    StartPoint = GCTools.Dxftp(dxfHelix.AxisBasePoint),
                                    Segments = new PathSegmentCollection()
                                    {
                                        new PolyLineSegment(points, true)
                                    }
                                })));
                            break;

                        case DxfEntityType.MLine:
                            DxfMLine dxfMLine = (DxfMLine)entity;
                            PathFigure MLineFigure = new PathFigure();
                            MLineFigure.StartPoint = GCTools.Dxftp(dxfMLine.Vertices[0]);

                            //Идем по точкам
                            for (int i = 1; i < dxfMLine.Vertices.Count; i++)
                                MLineFigure.Segments.Add(new LineSegment(
                                    GCTools.Dxftp(dxfMLine.Vertices[i % dxfMLine.Vertices.Count]), true));

                            MLineFigure.IsClosed = MLineFigure.IsClosed;
                            gccollection.Add(
                                new GeometryElement(
                                GCTools.FigureToGeometry(MLineFigure)));

                            break;

                        case DxfEntityType.Arc:
                            DxfArc dxfArc = (DxfArc)entity;

                            PathFigure ArcContour = new PathFigure();
                            ArcContour.StartPoint = GCTools.Dxftp(dxfArc.GetPointFromAngle(dxfArc.StartAngle));

                            DxfPoint arcPoint2 = dxfArc.GetPointFromAngle(dxfArc.EndAngle);

                            ArcContour.Segments.Add(
                                                new ArcSegment(
                                                    GCTools.Dxftp(arcPoint2),
                                                new Size(dxfArc.Radius, dxfArc.Radius),
                                                (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360,
                                                (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360 > 180,
                                                SweepDirection.Counterclockwise,
                                                true));

                            gccollection.Add(new GeometryElement(
                                GCTools.FigureToGeometry(ArcContour)));
                            break;

                        case DxfEntityType.Circle:
                            DxfCircle dxfCircle = (DxfCircle)entity;
                            gccollection.Add(new GeometryElement(
                                new EllipseGeometry(GCTools.Dxftp(dxfCircle.Center), dxfCircle.Radius, dxfCircle.Radius)));
                            break;

                        case DxfEntityType.Ellipse:
                            DxfEllipse dxfEllipse = (DxfEllipse)entity;
                            double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);
                            gccollection.Add(
                                new GeometryElement(
                                new EllipseGeometry(GCTools.Dxftp(dxfEllipse.Center),
                                dxfEllipse.MajorAxis.Length,
                                dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio,
                                new RotateTransform(MajorAngle * 180 / Math.PI)))); 
                            break;

                        case DxfEntityType.LwPolyline:
                            DxfLwPolyline dxfLwPolyline = (DxfLwPolyline)entity;
                            PathFigure lwPolyLineFigure = new PathFigure();

                            lwPolyLineFigure.StartPoint = GCTools.DxfLwVtp(dxfLwPolyline.Vertices[0]);

                            for (int i = 0; i < dxfLwPolyline.Vertices.Count - 1; i += 1)
                            {
                                double radian = Math.Atan(dxfLwPolyline.Vertices[i].Bulge) * 4;
                                double radius = CalBulgeRadius(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i]), GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]), dxfLwPolyline.Vertices[i].Bulge);

                                lwPolyLineFigure.Segments.Add(
                                    new ArcSegment(
                                        GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]),
                                        new Size(radius, radius),
                                        Math.Abs(radian * 180 / Math.PI),
                                        Math.Abs(dxfLwPolyline.Vertices[i].Bulge) > 1,
                                        dxfLwPolyline.Vertices[i].Bulge < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                        true
                                    ));

                                //points1.Add(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i % dxfLwPolyline.Vertices.Count]));
                            }

                            lwPolyLineFigure.IsClosed = dxfLwPolyline.IsClosed;

                            gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(lwPolyLineFigure)));

                            break;

                        case DxfEntityType.Polyline:
                            DxfPolyline dxfPolyline = (DxfPolyline)entity;

                            PathFigure polyLineFigure = new PathFigure();
                            polyLineFigure.StartPoint = GCTools.Dxftp(dxfPolyline.Vertices[0].Location);

                            for (int i = 1; i < dxfPolyline.Vertices.Count; i++)
                            {
                                polyLineFigure.Segments.Add(new LineSegment(GCTools.Dxftp(dxfPolyline.Vertices[i].Location), true));
                            }

                            polyLineFigure.IsClosed = dxfPolyline.IsClosed;

                            gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(polyLineFigure)));
                            break;

                        case DxfEntityType.Spline:
                            DxfSpline dxfSpline = (DxfSpline)entity;

                            ObservableCollection<RationalBSplinePoint> rationalBSplinePoints = new ObservableCollection<RationalBSplinePoint>();

                            foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                            {
                                rationalBSplinePoints.Add(new RationalBSplinePoint(GCTools.Dxftp(controlPoint.Point), controlPoint.Weight));
                            }
                            gccollection.Add(new GeometryElement(new NurbsShape(rationalBSplinePoints, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, CRS, dxfSpline.IsRational == true)));

                            break;
                    }

                }

                return gccollection;
            }
        }

        private double CalBulgeRadius(System.Windows.Point point1, System.Windows.Point point2, double bulge)
        {
            // Calculate the vertex angle
            double cicleAngle = Math.Atan(bulge) * 4;

            //the distance between two points
            double pointLen = GCTools.Lenth2D(point1, point2);
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
        private List<Point> GetSpiralPoints(DxfHelix dxfHelix, double CRS)
        {
            double StartAngle = Math.Atan2(dxfHelix.AxisBasePoint.Y - dxfHelix.StartPoint.Y, dxfHelix.AxisBasePoint.X - dxfHelix.StartPoint.X);

            double HelixRadius = GCTools.Lenth2D(new Point(dxfHelix.AxisBasePoint.X, dxfHelix.AxisBasePoint.Y), new Point(dxfHelix.StartPoint.X, dxfHelix.StartPoint.Y));

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


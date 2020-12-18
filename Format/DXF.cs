using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ToGeometryConverter.Object;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class DXF
    {
        public static List<Shape> Get(string filename, double CRS)

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

            List<Shape> ParseEntities(IList<DxfEntity> entitys)
            {
                List<Shape> geometryGroup = new List<Shape>();

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

                        case DxfEntityType.Line:
                            DxfLine line = (DxfLine)entity;
                            PathFigure contour = new PathFigure();
                            contour.StartPoint = Tools.Dxftp(line.P1);
                            contour.Segments.Add(new LineSegment(Tools.Dxftp(line.P2), true));
                            geometryGroup.Add(
                                Tools.FigureToShape(contour));
                            break;

                        case DxfEntityType.Helix:
                            DxfHelix dxfHelix = (DxfHelix)entity;

                            PointCollection points = new PointCollection(GetSpiralPoints(dxfHelix));

                            geometryGroup.Add(
                                Tools.FigureToShape(new PathFigure() 
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
                            geometryGroup.Add(
                                Tools.FigureToShape(MLineFigure));

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

                            geometryGroup.Add(Tools.FigureToShape(ArcContour));
                            break;

                        case DxfEntityType.Circle:
                            DxfCircle dxfCircle = (DxfCircle)entity;
                            geometryGroup.Add(new Path{
                               Data = new EllipseGeometry(Tools.Dxftp(dxfCircle.Center), dxfCircle.Radius, dxfCircle.Radius)
                            });
                            break;

                        case DxfEntityType.Ellipse:
                            DxfEllipse dxfEllipse = (DxfEllipse)entity;

                            double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);

                            geometryGroup.Add(new Path{
                                Data = new EllipseGeometry(Tools.Dxftp(dxfEllipse.Center),
                                dxfEllipse.MajorAxis.Length,
                                dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio,
                                new RotateTransform(MajorAngle * 180 / Math.PI))
                            }); 
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

                            geometryGroup.Add(Tools.FigureToShape(lwPolyLineFigure));

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

                            geometryGroup.Add(Tools.FigureToShape(polyLineFigure));
                            break;

                        case DxfEntityType.Spline:
                            DxfSpline dxfSpline = (DxfSpline)entity;

                            ObservableCollection<RationalBSplinePoint> rationalBSplinePoints = new ObservableCollection<RationalBSplinePoint>();

                            foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                            {
                                rationalBSplinePoints.Add(new RationalBSplinePoint(Tools.Dxftp(controlPoint.Point), controlPoint.Weight));
                            }
                            geometryGroup.Add(new NurbsShape(rationalBSplinePoints, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, CRS, dxfSpline.IsRational == true));

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

        private static List<Point> GetSpiralPoints(DxfHelix dxfHelix)
        {
            double StartAngle = Math.PI * 2 - Math.Atan2(dxfHelix.AxisBasePoint.Y - dxfHelix.StartPoint.Y, dxfHelix.AxisBasePoint.X - dxfHelix.StartPoint.X);

            double HelixRadius = Math.Max(Math.Abs(dxfHelix.AxisBasePoint.X - dxfHelix.StartPoint.X), Math.Abs(dxfHelix.AxisBasePoint.Y - dxfHelix.StartPoint.Y));

            double EndAngle = StartAngle + 2 * Math.PI * dxfHelix.NumberOfTurns;

            // Get the points.
            List<Point> points = new List<Point>();

            double dtheta = (dxfHelix.NumberOfTurns * Math.PI * 2)/ 20 * (dxfHelix.IsRightHanded ? 1 : -1);    // Five degrees.
            for (int i = 1; i <= 20; i++)
            {
                double theta = (StartAngle + dtheta * i);
                // Calculate r.
                double r = HelixRadius * theta / (2 * Math.PI);

                // Convert to Cartesian coordinates.
                double x = r * Math.Cos(theta);
                double y = -r * Math.Sin(theta);

                // Center.

                   x += dxfHelix.AxisBasePoint.X;
                   y -= dxfHelix.AxisBasePoint.Y;
                
                // Create the point.
                points.Add(new Point(x, y));
            }
            points.Reverse();
            return points;
        }
    }
}


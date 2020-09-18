using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class DXF
    {
        public static PathGeometry Get(string filename, double CRS)

        {
            DxfFile dxfFile;
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    dxfFile = DxfFile.Load(fs);
                };
            }
            catch
            {
                dxfFile = null;
            }
            

            if (dxfFile != null)
            {
                Tools.Geometry = new PathGeometry();

                ParseEntities(dxfFile.Entities);

                return Tools.MakeTransform(Tools.Geometry);
            }

            return null;

            void ParseEntities(IList<DxfEntity> entitys)
            {
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
                            Tools.FindInterContour(contour);
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
                            Tools.FindInterContour(MLineFigure);
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

                            Tools.FindInterContour(ArcContour);
                            break;

                        case DxfEntityType.Circle:
                            DxfCircle dxfCircle = (DxfCircle)entity;
                            Tools.Geometry.AddGeometry(new EllipseGeometry(Tools.Dxftp(dxfCircle.Center), dxfCircle.Radius, dxfCircle.Radius));
                            break;

                        case DxfEntityType.Ellipse:
                            DxfEllipse dxfEllipse = (DxfEllipse)entity;

                            double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);

                            Tools.Geometry.AddGeometry(new EllipseGeometry(Tools.Dxftp(dxfEllipse.Center),
                                dxfEllipse.MajorAxis.Length,
                                dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio,
                                new RotateTransform(MajorAngle * 180 / Math.PI)));
                            break;

                        case DxfEntityType.LwPolyline:
                            DxfLwPolyline dxfLwPolyline = (DxfLwPolyline)entity;
                            PathFigure lwPolyLineFigure = new PathFigure();

                            lwPolyLineFigure.StartPoint = Tools.DxfLwVtp(dxfLwPolyline.Vertices[0]);

                            for (int i = 0; i < dxfLwPolyline.Vertices.Count - 1; i+=1)
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

                            Tools.FindInterContour(lwPolyLineFigure);

                             break;

                        case DxfEntityType.Polyline:
                            DxfPolyline dxfPolyline = (DxfPolyline)entity;

                            PathFigure polyLineFigure = new PathFigure();
                            polyLineFigure.StartPoint = Tools.Dxftp(dxfPolyline.Vertices[0].Location);

                            for (int i = 1; i < dxfPolyline.Vertices.Count; i++)
                                polyLineFigure.Segments.Add(new LineSegment(Tools.Dxftp(dxfPolyline.Vertices[i].Location), true));

                            polyLineFigure.IsClosed = dxfPolyline.IsClosed;

                            Tools.FindInterContour(polyLineFigure);
                            break;

                        case DxfEntityType.Spline:
                            DxfSpline dxfSpline = (DxfSpline)entity;

                            NURBS nurbs = new NURBS();
                            nurbs.IsBSpline = true;

                            foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                                nurbs.WeightedPointSeries.Add(new RationalBSplinePoint(Tools.Dxftp(controlPoint.Point), controlPoint.Weight));

                            PointCollection points = nurbs.BSplineCurve(nurbs.WeightedPointSeries, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, CRS);

                            PathFigure NURBSCurve = new PathFigure();

                            NURBSCurve.StartPoint = points[0];
                            for (int i = 1; i < points.Count; i++)
                            {
                                NURBSCurve.Segments.Add(new LineSegment(points[i], true));
                            }

                            NURBSCurve.IsClosed = dxfSpline.IsClosed;

                            if (!NURBSCurve.IsClosed)
                                NURBSCurve.Segments.Add(new LineSegment(points.Last(), true));

                            Tools.FindInterContour(NURBSCurve);
                            break;
                    }

                }
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
    }
}

using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public class DXF : GCFormat
    {
        public DXF() : base("DXF", new string[1] { "dxf" }) { }

        public override Get ReadFile => GetAsync;

        private async Task<object> GetAsync(string filename, double CRS)
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
                GCTools.Log?.Invoke($"Loaded dxf: {filename}", "GCTool");
                return await Task<object>.Run(async () =>
                {
                    GCCollection elements = new GCCollection(filename.Split('\\').Last());
                    IList<DxfLayer> dxfLayers = dxfFile.Layers;

                    if (dxfFile.Layers.Count == 0)
                    {
                        dxfLayers = GetEntityLayer(dxfFile.Entities);
                    }

                    foreach (DxfLayer layer in dxfLayers)
                    {
                        elements.Add(ParseEntities(dxfFile.Entities, dxfFile.Blocks, layer.Name, CRS));
                    }

                    return elements;
                });
            }

            return null;
        }

        private IList<DxfLayer> GetEntityLayer(IList<DxfEntity> entitys)
        {
            List<DxfLayer> dxfLayers = new List<DxfLayer>();
            foreach (DxfEntity entity in entitys)
            {
                if (CheckLayer(entity.Layer) == false)
                {
                    dxfLayers.Add(new DxfLayer(entity.Layer));
                }
            }

            bool CheckLayer (string LayerName)
            {
                foreach (DxfLayer layer in dxfLayers)
                {
                    if (layer.Name == LayerName) return true;
                }

                return false;
            }
            return dxfLayers;
        }

        private GCCollection ParseEntities(IList<DxfEntity> entitys, IList<DxfBlock> blocks, string LayerName, double CRS)
        {
            GCCollection gccollection = new GCCollection(LayerName);

            foreach (DxfEntity entity in entitys)
            {

                int index = entitys.IndexOf(entity);
                GCTools.SetProgress?.Invoke(index, entitys.Count - 1, $"Parse DXF {index}/{entitys.Count - 1}");

                if (entity.Layer == LayerName || string.IsNullOrEmpty(LayerName))
                {
                    if (entity is DxfInsert dxfInsert)
                    {
                        foreach (DxfBlock dxfBlock in blocks)
                        {
                            if (dxfInsert.Name == dxfBlock.Name)
                            {
                                gccollection.AddRange(ParseEntities(dxfBlock.Entities, blocks, LayerName, CRS));
                            }
                        }
                    }
                    else
                    {
                        IGCObject obj = ParseObject(entity, CRS);
                        if (obj != null) {
                            gccollection.Add(obj);
                        }
                    }
                }
            }
            GCTools.SetProgress?.Invoke(0, 99, string.Empty);

            return gccollection;
        }

        private static IGCObject ParseObject(DxfEntity entity, double CRS)
        {
            switch (entity)
            {
                case DxfText dxfText:
                    Point point = GCTools.Dxftp(dxfText.Location, dxfText.Normal);
                    return new TextElement(dxfText.Value, 16, new System.Windows.Media.Media3D.Point3D(point.X, point.Y, 0));


                case DxfMText MText:
                    Point Mpoint = GCTools.Dxftp(MText.InsertionPoint, new DxfVector(0, 0, 1));
                    return new TextElement(MText.Text, 16,
                                    new System.Windows.Media.Media3D.Point3D(Mpoint.X, Mpoint.Y, 0));

                case DxfLine line:
                    return new GeometryElement(
                        new LineGeometry() {
                            StartPoint = GCTools.Dxftp(line.P1, new DxfVector(0, 0, 1)),
                            EndPoint = GCTools.Dxftp(line.P2, new DxfVector(0, 0, 1))
                        },
                        entity.EntityType.ToString());

                case DxfHelix dxfHelix:
                    PointCollection points = new PointCollection(GetSpiralPoints(dxfHelix, CRS));
                    return new GeometryElement(GCTools.FigureToGeometry(new PathFigure()
                        {
                            StartPoint = GCTools.Dxftp(dxfHelix.AxisBasePoint, new DxfVector(0,0,1)),
                            Segments = new PathSegmentCollection()
                            {
                                        new PolyLineSegment(points, true)
                            }
                        }), entity.EntityType.ToString());

                case DxfMLine dxfMLine:
                    PathFigure MLineFigure = new PathFigure
                    {
                        StartPoint = GCTools.Dxftp(dxfMLine.Vertices[0], dxfMLine.Normal)
                    };

                    //Идем по точкам
                    for (int i = 1; i < dxfMLine.Vertices.Count; i++)
                        MLineFigure.Segments.Add(new LineSegment(
                            GCTools.Dxftp(dxfMLine.Vertices[i % dxfMLine.Vertices.Count], dxfMLine.Normal), true));

                    MLineFigure.IsClosed = MLineFigure.IsClosed;
                    return new GeometryElement(GCTools.FigureToGeometry(MLineFigure), entity.EntityType.ToString());


                case DxfArc dxfArc:
                    PathFigure ArcContour = new PathFigure
                    {
                        StartPoint = GCTools.Dxftp(dxfArc.GetPointFromAngle(dxfArc.StartAngle), dxfArc.Normal)
                    };

                    DxfPoint arcPoint2 = dxfArc.GetPointFromAngle(dxfArc.EndAngle);

                    ArcContour.Segments.Add(
                                        new ArcSegment(
                                            GCTools.Dxftp(arcPoint2, dxfArc.Normal),
                                        new Size(dxfArc.Radius, dxfArc.Radius),
                                        (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360,
                                        (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360 > 180,
                                         dxfArc.Normal.Z < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                        true));

                    return new GeometryElement(GCTools.FigureToGeometry(ArcContour), entity.EntityType.ToString());

                case DxfCircle dxfCircle:
                    return new GeometryElement(
                        new EllipseGeometry(
                            GCTools.Dxftp(dxfCircle.Center, dxfCircle.Normal), 
                            dxfCircle.Radius * 2, 
                            dxfCircle.Radius * 2), entity.EntityType.ToString());

                case DxfEllipse dxfEllipse:
                    double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);
                    return new GeometryElement(
                        new EllipseGeometry(GCTools.Dxftp(dxfEllipse.Center, dxfEllipse.Normal),
                        dxfEllipse.MajorAxis.Length,
                        dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio,
                        new RotateTransform(MajorAngle * 180 / Math.PI)), entity.EntityType.ToString());

                case DxfLwPolyline dxfLwPolyline:
                    PathFigure lwPolyLineFigure = new PathFigure
                    {
                        StartPoint = GCTools.DxfLwVtp(dxfLwPolyline.Vertices[0])
                    };

                    for (int i = 0; i < dxfLwPolyline.Vertices.Count - 1; i += 1)
                    {
                        double radian = Math.Atan(dxfLwPolyline.Vertices[i].Bulge) * 4;
                        double radius = CalBulgeRadius(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i]), GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]), dxfLwPolyline.Vertices[i].Bulge);

                        if (radian != 0 || radius != 0)
                        {
                            lwPolyLineFigure.Segments.Add(
                                new ArcSegment(
                                    GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]),
                                    new Size(radius, radius),
                                    Math.Abs(radian * 180 / Math.PI),
                                    Math.Abs(dxfLwPolyline.Vertices[i].Bulge) > 1,
                                    dxfLwPolyline.Vertices[i].Bulge < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                    true
                                ));
                        }
                        else
                        {
                            lwPolyLineFigure.Segments.Add(
                                new LineSegment(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1]), true));
                        }

                        //points1.Add(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i % dxfLwPolyline.Vertices.Count]));
                    }

                    lwPolyLineFigure.IsClosed = dxfLwPolyline.IsClosed;
                    return new GeometryElement(GCTools.FigureToGeometry(lwPolyLineFigure), entity.EntityType.ToString());


                case DxfPolyline dxfPolyline:
                    PathFigure polyLineFigure = new PathFigure();
                    polyLineFigure.StartPoint = GCTools.Dxftp(dxfPolyline.Vertices[0].Location, dxfPolyline.Normal);

                    for (int i = 1; i < dxfPolyline.Vertices.Count; i++)
                    {
                        polyLineFigure.Segments.Add(new LineSegment(GCTools.Dxftp(dxfPolyline.Vertices[i].Location, dxfPolyline.Normal), true));
                    }

                    polyLineFigure.IsClosed = dxfPolyline.IsClosed;

                    return new GeometryElement(GCTools.FigureToGeometry(polyLineFigure), entity.EntityType.ToString());


                case DxfSpline dxfSpline:
                    ObservableCollection<RationalBSplinePoint> rationalBSplinePoints = new ObservableCollection<RationalBSplinePoint>();

                    foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                    {
                        rationalBSplinePoints.Add(new RationalBSplinePoint(GCTools.Dxftp(controlPoint.Point, dxfSpline.Normal), controlPoint.Weight));
                    }
                    return new GeometryElement(new NurbsShape(rationalBSplinePoints, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, CRS, dxfSpline.IsRational == true), entity.EntityType.ToString());

                default:
                    return null;

            }
        }

        private static double CalBulgeRadius(System.Windows.Point point1, System.Windows.Point point2, double bulge)
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
        private static List<Point> GetSpiralPoints(DxfHelix dxfHelix, double CRS)
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
                Console.WriteLine($"Th: {theta}/{Math.PI * 2 * dxfHelix.NumberOfTurns}");
                // Calculate r.
                double r = HelixRadius / steps  * i;

                Console.WriteLine($"R: {r}/{HelixRadius}");

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


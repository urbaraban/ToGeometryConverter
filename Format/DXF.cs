using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public class DXF : GCFormat
    {
        public DXF() : base("DXF", new string[1] { ".dxf" }) { }

        public override Get ReadFile => GetAsync;

        private async Task<object> GetAsync(string filename)
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

            var scaleFactor = GetScaleFactor(dxfFile.Header.DefaultDrawingUnits);

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
                        GCCollection layercollect = ParseEntities(dxfFile.Entities, dxfFile.Blocks, layer.Name, dxfFile.Header.InsertionBase, scaleFactor);
                        if (layercollect.Count > 0)
                        {
                            elements.Add(layercollect);
                        }

                    }
                    return elements;
                });
            }

            return null;
        }

        private double GetScaleFactor(DxfUnits defaultDrawingUnits)
        {
            switch (defaultDrawingUnits)
            {
                case DxfUnits.Inches:
                    return 25.4; // 1 inch = 25.4 mm
                case DxfUnits.Feet:
                    return 304.8; // 1 foot = 304.8 mm
                case DxfUnits.Millimeters:
                    return 1.0; // 1 mm = 1 mm
                case DxfUnits.Centimeters:
                    return 10.0; // 1 cm = 10 mm
                case DxfUnits.Meters:
                    return 1000.0; // 1 m = 1000 mm
                default:
                    return 1.0; // Default scale factor if not specified
            }
        }

        //private double GetScaleFactor(DxfPlotSettings dxfPlotSettings)
        //{
        //    switch (dxfPlotSettings)
        //    {
        //        case 
        //    }
        //}

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

        private GCCollection ParseEntities(IList<DxfEntity> entitys, IList<DxfBlock> blocks, string LayerName, DxfPoint location, double scaleFactor, bool insert = false)
        {
            GCCollection gccollection = new GCCollection(LayerName);

            var lentity = entitys.Where(x => x.Layer == LayerName || insert == true).ToList();

            for (int i = 0; i < lentity.Count; i += 1)
            {
                GCTools.SetProgress?.Invoke(i, lentity.Count() - 1, $"Parse DXF {i}/{lentity.Count - 1}");

                if (lentity[i] is DxfInsert dxfInsert)
                {
                    foreach (DxfBlock dxfBlock in blocks)
                    {
                        if (dxfInsert.Name == dxfBlock.Name)
                        {
                            DxfPoint insert_location = new DxfPoint(
                                (location.X + dxfInsert.Location.X) * scaleFactor,
                                (location.Y + dxfInsert.Location.Y) * scaleFactor,
                                (location.Z + dxfInsert.Location.Z) * scaleFactor);
                            gccollection.AddRange(ParseEntities(dxfBlock.Entities, blocks, LayerName, insert_location, scaleFactor, true));
                        }
                    }
                }
                else
                {
                    IGCObject obj = ParseObject(lentity[i], location, scaleFactor);
                    if (obj != null)
                    {
                        gccollection.Add(obj);
                    }
                }

            }
            GCTools.SetProgress?.Invoke(0, 99, string.Empty);

            return gccollection;
        }

        private static IGCObject ParseObject(DxfEntity entity, DxfPoint Location, double scaleFactor)
        {
            switch (entity)
            {
                case DxfText dxfText:
                    Point point = GCTools.Dxftp(dxfText.Location, dxfText.Normal, Location, scaleFactor);
                    return new TextElement(dxfText.Value, 16, new Point3D(point.X, point.Y, 0));


                case DxfMText MText:
                    Point Mpoint = GCTools.Dxftp(MText.InsertionPoint, new DxfVector(0, 0, 1), Location, scaleFactor);
                    return new TextElement(MText.Text, 16,
                                    new Point3D(Mpoint.X, Mpoint.Y, 0));

                case DxfLine line:
                    return new GeometryElement(
                        new LineGeometry() {
                            StartPoint = GCTools.Dxftp(line.P1, new DxfVector(0, 0, 1), Location, scaleFactor),
                            EndPoint = GCTools.Dxftp(line.P2, new DxfVector(0, 0, 1), Location, scaleFactor)
                        },
                        entity.EntityType.ToString());

                case DxfHelix dxfHelix:
                    PointCollection points = new PointCollection(GetSpiralPoints(dxfHelix));
                    return new GeometryElement(GCTools.FigureToGeometry(new PathFigure()
                        {
                            StartPoint = GCTools.Dxftp(dxfHelix.AxisBasePoint, new DxfVector(0,0,1), Location, scaleFactor),
                            Segments = new PathSegmentCollection()
                            {
                                new PolyLineSegment(points, true)
                            }
                        }), entity.EntityType.ToString());

                case DxfMLine dxfMLine:
                    PathFigure MLineFigure = new PathFigure
                    {
                        StartPoint = GCTools.Dxftp(dxfMLine.Vertices[0], dxfMLine.Normal, Location, scaleFactor)
                    };

                    //Идем по точкам
                    for (int i = 1; i < dxfMLine.Vertices.Count; i++)
                        MLineFigure.Segments.Add(new LineSegment(
                            GCTools.Dxftp(dxfMLine.Vertices[i % dxfMLine.Vertices.Count], dxfMLine.Normal, Location, scaleFactor), true));

                    MLineFigure.IsClosed = MLineFigure.IsClosed;
                    return new GeometryElement(GCTools.FigureToGeometry(MLineFigure), entity.EntityType.ToString());


                case DxfArc dxfArc:
                    PathFigure ArcContour = new PathFigure
                    {
                        StartPoint = GCTools.Dxftp(dxfArc.GetPointFromAngle(dxfArc.StartAngle), dxfArc.Normal, Location, scaleFactor)
                    };

                    DxfPoint arcPoint2 = dxfArc.GetPointFromAngle(dxfArc.EndAngle);

                    ArcContour.Segments.Add(
                                        new ArcSegment(
                                            GCTools.Dxftp(arcPoint2, dxfArc.Normal, Location, scaleFactor),
                                        new Size(dxfArc.Radius * scaleFactor, dxfArc.Radius * scaleFactor),
                                        (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360,
                                        (360 + dxfArc.EndAngle - dxfArc.StartAngle) % 360 > 180,
                                         dxfArc.Normal.Z < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                        true));

                    return new GeometryElement(GCTools.FigureToGeometry(ArcContour), entity.EntityType.ToString());

                case DxfCircle dxfCircle:
                    return new GeometryElement(
                        new EllipseGeometry(
                            GCTools.Dxftp(dxfCircle.Center, dxfCircle.Normal, Location, scaleFactor), 
                            dxfCircle.Radius * 2 * scaleFactor, 
                            dxfCircle.Radius * 2 * scaleFactor), entity.EntityType.ToString());

                case DxfEllipse dxfEllipse:
                    double MajorAngle = (Math.PI * 2 - Math.Atan((dxfEllipse.MajorAxis.Y) / (dxfEllipse.MajorAxis.X))) % (2 * Math.PI);
                    return new GeometryElement(
                        new EllipseGeometry(GCTools.Dxftp(dxfEllipse.Center, dxfEllipse.Normal, Location, scaleFactor),
                        dxfEllipse.MajorAxis.Length * scaleFactor,
                        dxfEllipse.MajorAxis.Length * dxfEllipse.MinorAxisRatio * scaleFactor,
                        new RotateTransform(MajorAngle * 180 / Math.PI)), entity.EntityType.ToString());

                case DxfLwPolyline dxfLwPolyline:
                    PathFigure lwPolyLineFigure = new PathFigure
                    {
                        StartPoint = GCTools.DxfLwVtp(dxfLwPolyline.Vertices[0], scaleFactor),
                        IsClosed = false
                    };

                    for (int i = 0; i < dxfLwPolyline.Vertices.Count - 1; i += 1)
                    {
                        var vertice = dxfLwPolyline.Vertices[i];

                        var point1 = GCTools.DxfLwVtp(vertice, scaleFactor);
                        var point2 = GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1], scaleFactor);

                        double radian = Math.Abs(Math.Atan(vertice.Bulge)) * 4;
                        double radius = CalBulgeRadius(
                            point1,
                            point2,
                            vertice.Bulge) * scaleFactor;

                        if (radian != 0 || radius != 0)
                        {
                            var direction = vertice.Bulge < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
                            double angle = radian * 180 / Math.PI % 360;
                            bool isLarge = angle > 180;

                            lwPolyLineFigure.Segments.Add(
                                new ArcSegment(point2, new Size(radius, radius), angle, isLarge, direction, true));
                        }
                        else
                        {
                            lwPolyLineFigure.Segments.Add(
                                new LineSegment(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i + 1], scaleFactor), true));
                        }

                        //points1.Add(GCTools.DxfLwVtp(dxfLwPolyline.Vertices[i % dxfLwPolyline.Vertices.Count]));
                    }

                    lwPolyLineFigure.IsClosed = dxfLwPolyline.IsClosed;
                    return new GeometryElement(GCTools.FigureToGeometry(lwPolyLineFigure), entity.EntityType.ToString());


                case DxfPolyline dxfPolyline:
                    PathFigure polyLineFigure = new PathFigure();
                    polyLineFigure.StartPoint = GCTools.Dxftp(dxfPolyline.Vertices[0].Location, dxfPolyline.Normal, Location, scaleFactor);

                    for (int i = 1; i < dxfPolyline.Vertices.Count; i++)
                    {
                        polyLineFigure.Segments.Add(new LineSegment(GCTools.Dxftp(dxfPolyline.Vertices[i].Location, dxfPolyline.Normal, Location, scaleFactor), true));
                    }

                    polyLineFigure.IsClosed = dxfPolyline.IsClosed;

                    return new GeometryElement(GCTools.FigureToGeometry(polyLineFigure), entity.EntityType.ToString());


                case DxfSpline dxfSpline:
                    ObservableCollection<RationalBSplinePoint> rationalBSplinePoints = new ObservableCollection<RationalBSplinePoint>();

                    foreach (DxfControlPoint controlPoint in dxfSpline.ControlPoints)
                    { 
                        Point point1 = GCTools.Dxftp(controlPoint.Point, dxfSpline.Normal, Location, scaleFactor);
                        rationalBSplinePoints.Add(new RationalBSplinePoint(point1.X, point1.Y, controlPoint.Weight));
                    }
                    NurbsShape nurbsShape = new NurbsShape(rationalBSplinePoints, dxfSpline.DegreeOfCurve, dxfSpline.KnotValues, dxfSpline.IsRational == true);
                    return new GeometryElement(nurbsShape, entity.EntityType.ToString());

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
        private static List<Point> GetSpiralPoints(DxfHelix dxfHelix)
        {
            double StartAngle = Math.Atan2(dxfHelix.AxisBasePoint.Y - dxfHelix.StartPoint.Y, dxfHelix.AxisBasePoint.X - dxfHelix.StartPoint.X);

            double HelixRadius = GCTools.Lenth2D(new Point(dxfHelix.AxisBasePoint.X, dxfHelix.AxisBasePoint.Y), new Point(dxfHelix.StartPoint.X, dxfHelix.StartPoint.Y));

            // Get the points.
            List<Point> points = new List<Point>();

            int steps = (int)((2 * Math.PI * HelixRadius * dxfHelix.NumberOfTurns));

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


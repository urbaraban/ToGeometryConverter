using Svg;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public class SVG : IFormat
    {
        public string Name { get; } = "Vector";
        public string[] ShortName { get; } = new string[1] { "svg" };

        public GCCollection Get(string filepath, double RoundStep)
        {           
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());

            return switchCollection(svgDoc.Children);

            GCCollection switchCollection(SvgElementCollection elements)
            {
                GCCollection gccollection = new GCCollection();

                foreach (SvgElement svgElement in elements)
                {
                    if (svgElement.Visibility == "visible")
                    {
                        switch (svgElement)
                        {
                            case SvgPolygon polygon:
                                PathFigure pathPolygon = new PathFigure();
                                pathPolygon.StartPoint = new Point(polygon.Points[0], polygon.Points[1]);

                                for (int i = 2; i < polygon.Points.Count; i += 2)
                                {
                                    pathPolygon.Segments.Add(new LineSegment(new Point(polygon.Points[i], polygon.Points[i + 1]), true));
                                }
                                pathPolygon.IsClosed = !(svgElement is SvgPolyline);
                                gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(pathPolygon)));
                                break;

                            case SvgCircle circle:
                                gccollection.Add(new GeometryElement(new EllipseGeometry(new Point(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius)));
                                break;

                            case SvgText text:
                                gccollection.Add(new TextElement(text.Text, text.FontSize,
                                    new System.Windows.Media.Media3D.Point3D(text.Bounds.X, text.Bounds.Y, 0)));
                                break;

                            case SvgEllipse ellipse:
                                gccollection.Add(new GeometryElement(new EllipseGeometry(new Point(ellipse.CenterX, ellipse.CenterY), ellipse.RadiusX, ellipse.RadiusY)));
                                break;

                            case SvgRectangle rectangle:
                                gccollection.Add(new GeometryElement(new RectangleGeometry(new Rect(new Point(rectangle.X, rectangle.Y), new Size(rectangle.Width, rectangle.Height)))));
                                break;

                            case SvgLine line:
                                PathFigure pathLine = new PathFigure();
                                pathLine.StartPoint = new Point(line.StartX, line.StartY);
                                pathLine.Segments.Add(new LineSegment(new Point(line.EndX, line.EndY), true));
                                gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(pathLine)));
                                break;

                            case SvgPath path:
                                PathGeometry pathPath = new PathGeometry();
                                PathFigure contourPath = new PathFigure();
                                contourPath.IsClosed = contourPath.IsClosed;
                                for (int i = 0; i < path.PathData.Count; i++)
                                {
                                    switch (path.PathData[i].GetType().FullName)
                                    {
                                        case "Svg.Pathing.SvgMoveToSegment":
                                            Svg.Pathing.SvgMoveToSegment svgMoveTo = (Svg.Pathing.SvgMoveToSegment)path.PathData[i];
                                            pathPath.Figures.Add(contourPath);
                                            contourPath = new PathFigure();
                                            contourPath.StartPoint = GCTools.Pftp(svgMoveTo.Start);
                                            contourPath.IsClosed = true;
                                            break;

                                        case "Svg.Pathing.SvgQuadraticCurveSegment":
                                            Svg.Pathing.SvgQuadraticCurveSegment svgQuadraticCurve = (Svg.Pathing.SvgQuadraticCurveSegment)path.PathData[i];
                                            PolyQuadraticBezierSegment polyQuadraticBezierSegment = new PolyQuadraticBezierSegment();

                                            polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.ControlPoint));
                                            polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.End));

                                            contourPath.Segments.Add(polyQuadraticBezierSegment);

                                            break;

                                        case "Svg.Pathing.SvgArcSegment":
                                            Svg.Pathing.SvgArcSegment svgArcSeg = (Svg.Pathing.SvgArcSegment)path.PathData[i];
                                            contourPath.Segments.Add(
                                                new ArcSegment(
                                                     GCTools.Pftp(svgArcSeg.End),
                                                new Size(svgArcSeg.RadiusX, svgArcSeg.RadiusY), svgArcSeg.Angle, false,
                                                svgArcSeg.Sweep == Svg.Pathing.SvgArcSweep.Positive ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                                true));
                                            break;

                                        case "Svg.Pathing.SvgLineSegment":
                                            Svg.Pathing.SvgLineSegment svgLineSegment = (Svg.Pathing.SvgLineSegment)path.PathData[i];
                                            contourPath.Segments.Add(new LineSegment(GCTools.Pftp(svgLineSegment.End), true));
                                            break;

                                        case "Svg.Pathing.SvgCubicCurveSegment":
                                            Svg.Pathing.SvgCubicCurveSegment svgCubicCurve = (Svg.Pathing.SvgCubicCurveSegment)path.PathData[i];
                                            contourPath.Segments.Add(new BezierSegment(
                                                 GCTools.Pftp(svgCubicCurve.FirstControlPoint),
                                               GCTools.Pftp(svgCubicCurve.SecondControlPoint),
                                              GCTools.Pftp(svgCubicCurve.End), true));
                                            break;

                                        case "Svg.Pathing.SvgClosePathSegment":
                                            pathPath.Figures.Add(contourPath);
                                            break;
                                    }
                                }
                                if (!pathPath.Figures.Equals(contourPath))
                                    pathPath.Figures.Add(contourPath);

                                break;

                            case SvgGroup group:
                                foreach (IGCElement element in switchCollection(group.Children))
                                {
                                    gccollection.Add(element);
                                }
                                break;
                        }
                    }
                }

                return gccollection;
            }
        }
    }
}

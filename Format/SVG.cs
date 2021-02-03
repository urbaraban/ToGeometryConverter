using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class SVG
    {
        public static string Name = "Vector";
        public static string Short = ".svg";
        public static GeometryGroup Get(string filepath, bool Tesselate)
        {           
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());

            return switchCollection(svgDoc.Children);

            GeometryGroup switchCollection(SvgElementCollection elements)
            {
                GeometryGroup geometryGroup = new GeometryGroup();

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
                                pathPolygon.IsClosed = true;
                                geometryGroup.Children.Add(Tools.FigureToGeometry(pathPolygon));
                                break;

                            case SvgCircle circle:
                                geometryGroup.Children.Add(new EllipseGeometry(new Point(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius));
                                break;

                            case SvgEllipse ellipse:
                                geometryGroup.Children.Add(new EllipseGeometry(new Point(ellipse.CenterX, ellipse.CenterY), ellipse.RadiusX, ellipse.RadiusY));
                                break;

                            case SvgRectangle rectangle:
                                geometryGroup.Children.Add(new RectangleGeometry(new Rect(new Point(rectangle.X, rectangle.Y), new Size(rectangle.Width, rectangle.Height))));
                                break;

                            case SvgLine line:
                                PathFigure pathLine = new PathFigure();
                                pathLine.StartPoint = new Point(line.StartX, line.StartY);
                                pathLine.Segments.Add(new LineSegment(new Point(line.EndX, line.EndY), true));
                                geometryGroup.Children.Add(Tools.FigureToGeometry(pathLine));
                                break;

                            case SvgPath path:
                                PathGeometry pathPath = new PathGeometry();
                                PathFigure contourPath = new PathFigure();
                                contourPath.IsClosed = true;
                                for (int i = 0; i < path.PathData.Count; i++)
                                {
                                    switch (path.PathData[i].GetType().FullName)
                                    {
                                        case "Svg.Pathing.SvgMoveToSegment":
                                            Svg.Pathing.SvgMoveToSegment svgMoveTo = (Svg.Pathing.SvgMoveToSegment)path.PathData[i];
                                            pathPath.Figures.Add(contourPath);
                                            contourPath = new PathFigure();
                                            contourPath.StartPoint = Tools.Pftp(svgMoveTo.Start);
                                            contourPath.IsClosed = true;
                                            break;

                                        case "Svg.Pathing.SvgQuadraticCurveSegment":
                                            Svg.Pathing.SvgQuadraticCurveSegment svgQuadraticCurve = (Svg.Pathing.SvgQuadraticCurveSegment)path.PathData[i];
                                            PolyQuadraticBezierSegment polyQuadraticBezierSegment = new PolyQuadraticBezierSegment();

                                            polyQuadraticBezierSegment.Points.Add(Tools.Pftp(svgQuadraticCurve.ControlPoint));
                                            polyQuadraticBezierSegment.Points.Add(Tools.Pftp(svgQuadraticCurve.End));

                                            contourPath.Segments.Add(polyQuadraticBezierSegment);

                                            break;

                                        case "Svg.Pathing.SvgArcSegment":
                                            Svg.Pathing.SvgArcSegment svgArcSeg = (Svg.Pathing.SvgArcSegment)path.PathData[i];
                                            contourPath.Segments.Add(
                                                new ArcSegment(
                                                     Tools.Pftp(svgArcSeg.End),
                                                new Size(svgArcSeg.RadiusX, svgArcSeg.RadiusY), svgArcSeg.Angle, false,
                                                svgArcSeg.Sweep == Svg.Pathing.SvgArcSweep.Positive ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                                true));
                                            break;

                                        case "Svg.Pathing.SvgLineSegment":
                                            Svg.Pathing.SvgLineSegment svgLineSegment = (Svg.Pathing.SvgLineSegment)path.PathData[i];
                                            contourPath.Segments.Add(new LineSegment(Tools.Pftp(svgLineSegment.End), true));
                                            break;

                                        case "Svg.Pathing.SvgCubicCurveSegment":
                                            Svg.Pathing.SvgCubicCurveSegment svgCubicCurve = (Svg.Pathing.SvgCubicCurveSegment)path.PathData[i];
                                            contourPath.Segments.Add(new BezierSegment(
                                                 Tools.Pftp(svgCubicCurve.FirstControlPoint),
                                               Tools.Pftp(svgCubicCurve.SecondControlPoint),
                                              Tools.Pftp(svgCubicCurve.End), true));
                                            break;

                                        case "Svg.Pathing.SvgClosePathSegment":
                                            pathPath.Figures.Add(contourPath);
                                            break;
                                    }
                                }
                                if (!pathPath.Figures.Equals(contourPath))
                                    pathPath.Figures.Add(contourPath);

                                geometryGroup.Children.Add(pathPath);
                                break;

                            case SvgGroup group:
                                foreach (Geometry geometry in switchCollection(group.Children).Children)
                                {
                                    geometryGroup.Children.Add(geometry);
                                }
                                break;
                        }
                    }
                }

                return geometryGroup;
            }
        }
    }
}

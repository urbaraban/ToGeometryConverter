using Svg;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class SVG
    {
        public static PathGeometry Get(string filepath)
        {           
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());

            Tools.Geometry = new PathGeometry();

            switchCollection(svgDoc.Children);

            void switchCollection(SvgElementCollection elements)
            {
                foreach (SvgElement svgElement in elements)
                {
                    if (svgElement.Visibility == "visible")
                    {
                      switch (svgElement.GetType().FullName)
                        {
                            case "Svg.SvgPolygon":
                                SvgPolygon polygon = (SvgPolygon)svgElement;
                                PathFigure pathPolygon = new PathFigure();

                                pathPolygon.StartPoint = Tools.Dbltp(polygon.Points[0], polygon.Points[1]);

                                for (int i = 2; i < polygon.Points.Count; i += 2)
                                {
                                    pathPolygon.Segments.Add(new LineSegment(Tools.Dbltp(polygon.Points[i].Value, polygon.Points[i + 1].Value), true));
                                }
                                pathPolygon.IsClosed = true;
                                Tools.FindInterContour(pathPolygon);
                                break;

                            case "Svg.SvgCircle":
                                SvgCircle circle = (SvgCircle)svgElement;
                                Tools.Geometry.AddGeometry(new EllipseGeometry(Tools.Dbltp(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius));
                                break;

                            case "Svg.SvgEllipse":
                                SvgEllipse ellipse = (SvgEllipse)svgElement;
                                Tools.Geometry.AddGeometry(new EllipseGeometry(Tools.Dbltp(ellipse.CenterX, ellipse.CenterY), ellipse.RadiusX, ellipse.RadiusY));
                                break;

                            case "Svg.SvgRectangle":
                                SvgRectangle rectangle = (SvgRectangle)svgElement;
                                Tools.Geometry.AddGeometry(new RectangleGeometry(new Rect(Tools.Dbltp(rectangle.X, rectangle.Y), new Size(rectangle.Width, rectangle.Height))));
                                break;

                            case "Svg.SvgLine":
                                SvgLine line = (SvgLine)svgElement;
                                PathFigure pathLine = new PathFigure();
                                pathLine.StartPoint = Tools.Dbltp(line.StartX, line.StartY);
                                pathLine.Segments.Add(new LineSegment(Tools.Dbltp(line.EndX, line.EndY), true));
                                Tools.FindInterContour(pathLine);
                                break;

                            case "Svg.SvgPath":
                                SvgPath path = (SvgPath)svgElement;
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
  
                                Tools.Geometry.AddGeometry(pathPath);
                                break;

                            case "Svg.SvgGroup":
                                SvgGroup group = (SvgGroup)svgElement;
                                switchCollection(group.Children);
                                break;
                        }
                    }
                }
            }

            if (Tools.Geometry.Figures.Count > 0)
                return Tools.MakeTransform(Tools.Geometry);
            else
                return null;
        }
    }
}

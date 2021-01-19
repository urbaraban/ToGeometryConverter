using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public static class SVG
    {
        public static List<Shape> Get(string filepath, bool Tesselate)
        {           
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());

            return switchCollection(svgDoc.Children);

            List<Shape> switchCollection(SvgElementCollection elements)
            {
                List<Shape> geometryGroup = new List<Shape>();

                foreach (SvgElement svgElement in elements)
                {
                    if (svgElement.Visibility == "visible")
                    {
                        switch (svgElement.GetType().FullName)
                        {
                            case "Svg.SvgPolygon":
                                SvgPolygon polygon = (SvgPolygon)svgElement;
                                PathFigure pathPolygon = new PathFigure();

                                pathPolygon.StartPoint = new Point(polygon.Points[0], polygon.Points[1]);

                                for (int i = 2; i < polygon.Points.Count; i += 2)
                                {
                                    if (Tesselate && (polygon.Points.Count - i > 3))
                                    {
                                        double lastAngle = Tools.GetAngleThreePoint(new Point(polygon.Points[i - 2], polygon.Points[i - 1]), new Point(polygon.Points[i], polygon.Points[i + 1]), new Point(polygon.Points[i + 2], polygon.Points[i + 3]));

                                        if (Math.Abs(lastAngle) > 110)
                                        {
                                            List<Point> TesselatePoints = new List<Point>();
                                            TesselatePoints.Add(new Point(polygon.Points[i - 2], polygon.Points[i - 1]));
                                            TesselatePoints.Add(new Point(polygon.Points[i], polygon.Points[i + 1]));

                                            for (int j = i + 2; polygon.Points.Count - j > 1; j += 2)
                                            {
                                                double tempAngle = Tools.GetAngleThreePoint(TesselatePoints[TesselatePoints.Count - 2], TesselatePoints[TesselatePoints.Count - 1], new Point(polygon.Points[j], polygon.Points[j + 1]));

                                                if (Math.Abs(Math.Round(lastAngle) - Math.Round(tempAngle)) < 1)
                                                {
                                                    TesselatePoints.Add(new Point(polygon.Points[j], polygon.Points[j + 1]));
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }

                                            if (TesselatePoints.Count >= 4)
                                            {
                                                if (TesselatePoints.First().X == TesselatePoints.Last().X && TesselatePoints.First().Y == TesselatePoints.Last().Y)
                                                {
                                                    geometryGroup.Add(Tools.GetEllipseFromList(TesselatePoints));
                                                    i += TesselatePoints.Count * 2;
                                                }
                                                else
                                                {
                                                    pathPolygon.Segments.Add(Tools.GetArcSegmentFromList(TesselatePoints));
                                                    i += (TesselatePoints.Count - 1) * 2;
                                                }
                                            }
                                            else
                                            {
                                                for (int k = 0; k < TesselatePoints.Count - 1; k += 1)
                                                {
                                                    pathPolygon.Segments.Add(new LineSegment(TesselatePoints[k], true));
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            pathPolygon.Segments.Add(new LineSegment(new Point(polygon.Points[i], polygon.Points[i + 1]), true));
                                        }
                                    }
                                    else
                                    {
                                        pathPolygon.Segments.Add(new LineSegment(new Point(polygon.Points[i], polygon.Points[i + 1]), true));
                                    }
                                }
                                pathPolygon.IsClosed = true;
                                geometryGroup.Add(Tools.FigureToShape(pathPolygon));
                                break;

                            case "Svg.SvgCircle":
                                SvgCircle circle = (SvgCircle)svgElement;
                                geometryGroup.Add(new Path
                                {
                                    Data = new EllipseGeometry(new Point(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius)
                                });
                                break;

                            case "Svg.SvgEllipse":
                                SvgEllipse ellipse = (SvgEllipse)svgElement;
                                geometryGroup.Add(new Path
                                {
                                    Data = new EllipseGeometry(new Point(ellipse.CenterX, ellipse.CenterY), ellipse.RadiusX, ellipse.RadiusY)
                                });
                                break;

                            case "Svg.SvgRectangle":
                                SvgRectangle rectangle = (SvgRectangle)svgElement;
                                geometryGroup.Add(
                                    new Path
                                    {
                                        Data = new RectangleGeometry(new Rect(new Point(rectangle.X, rectangle.Y), new Size(rectangle.Width, rectangle.Height)))
                                    });
                                break;

                            case "Svg.SvgLine":
                                SvgLine line = (SvgLine)svgElement;
                                PathFigure pathLine = new PathFigure();
                                pathLine.StartPoint = new Point(line.StartX, line.StartY);
                                pathLine.Segments.Add(new LineSegment(new Point(line.EndX, line.EndY), true));
                                geometryGroup.Add(Tools.FigureToShape(pathLine));
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

                                geometryGroup.Add(new Path
                                {
                                    Data = pathPath
                                }); 
                                break;

                            case "Svg.SvgGroup":
                                SvgGroup group = (SvgGroup)svgElement;
                                geometryGroup.AddRange(switchCollection(group.Children));
                                break;
                        }
                    }
                }

                return geometryGroup;
            }
        }
    }
}

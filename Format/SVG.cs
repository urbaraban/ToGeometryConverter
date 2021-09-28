﻿using Svg;
using Svg.Pathing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ToGeometryConverter.Format
{
    public class SVG : GCFormat
    {
        public SVG() : base("SVG Vector", new string[1] { "svg" }) { }

        public override Get ReadFile => GetAsync;


        private async Task<object> GetAsync(string filepath, double RoundStep)
        {           
            SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());

            return await SwitchCollection(svgDoc.Children, GCTools.GetName(filepath));
        }

        private async Task<GCCollection> SwitchCollection(SvgElementCollection elements, string Name)
        {
            GCCollection gccollection = new GCCollection(Name);
            this.SetProgress?.Invoke(0, elements.Count, "Parse SVG");
            foreach (SvgElement svgElement in elements)
            {
                int index = elements.IndexOf(svgElement);
                this.SetProgress?.Invoke(index, elements.Count - 1, $"Parse SVG {index}/{elements.Count - 1}");

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
                            gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(pathPolygon), svgElement.ID));
                            break;

                        case SvgCircle circle:
                            gccollection.Add(new GeometryElement(new EllipseGeometry(new Point(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius), svgElement.ID));
                            break;

                        case SvgText text:
                            gccollection.Add(new TextElement(text.Text, text.FontSize,
                                new System.Windows.Media.Media3D.Point3D(text.Bounds.X, text.Bounds.Y, 0)));
                            break;

                        case SvgEllipse ellipse:
                            gccollection.Add(new GeometryElement(new EllipseGeometry(new Point(ellipse.CenterX, ellipse.CenterY), ellipse.RadiusX, ellipse.RadiusY), svgElement.ID));
                            break;

                        case SvgRectangle rectangle:
                            gccollection.Add(new GeometryElement(new RectangleGeometry(new Rect(new Point(rectangle.X, rectangle.Y), new Size(rectangle.Width, rectangle.Height))), svgElement.ID));
                            break;

                        case SvgLine line:
                            PathFigure pathLine = new PathFigure();
                            pathLine.StartPoint = new Point(line.StartX, line.StartY);
                            pathLine.Segments.Add(new LineSegment(new Point(line.EndX, line.EndY), true));
                            gccollection.Add(new GeometryElement(GCTools.FigureToGeometry(pathLine), svgElement.ID));
                            break;

                        case SvgPath path:
                            PathGeometry pathPath = new PathGeometry();
                            PathFigure contourPath = new PathFigure();
                            contourPath.IsClosed = contourPath.IsClosed;
                            for (int i = 0; i < path.PathData.Count; i++)
                            {
                                switch (path.PathData[i])
                                {
                                    case SvgMoveToSegment svgMoveTo:
                                        pathPath.Figures.Add(contourPath);
                                        contourPath = new PathFigure();
                                        contourPath.StartPoint = GCTools.Pftp(svgMoveTo.Start);
                                        contourPath.IsClosed = true;
                                        break;

                                    case SvgQuadraticCurveSegment svgQuadraticCurve:
                                        PolyQuadraticBezierSegment polyQuadraticBezierSegment = new PolyQuadraticBezierSegment();

                                        polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.ControlPoint));
                                        polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.End));

                                        contourPath.Segments.Add(polyQuadraticBezierSegment);

                                        break;

                                    case SvgArcSegment svgArcSeg:
                                        contourPath.Segments.Add(
                                            new ArcSegment(
                                                 GCTools.Pftp(svgArcSeg.End),
                                            new Size(svgArcSeg.RadiusX, svgArcSeg.RadiusY), svgArcSeg.Angle, false,
                                            svgArcSeg.Sweep == Svg.Pathing.SvgArcSweep.Positive ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                                            true));
                                        break;

                                    case SvgLineSegment svgLineSegment:
                                        contourPath.Segments.Add(new LineSegment(GCTools.Pftp(svgLineSegment.End), true));
                                        break;

                                    case SvgCubicCurveSegment svgCubicCurve:
                                        contourPath.Segments.Add(new BezierSegment(
                                             GCTools.Pftp(svgCubicCurve.FirstControlPoint),
                                           GCTools.Pftp(svgCubicCurve.SecondControlPoint),
                                          GCTools.Pftp(svgCubicCurve.End), true));
                                        break;

                                    case SvgClosePathSegment segment:
                                        pathPath.Figures.Add(contourPath);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (!pathPath.Figures.Equals(contourPath))
                                pathPath.Figures.Add(contourPath);
                            gccollection.Add(new GeometryElement(pathPath, svgElement.ID));
                            break;

                        case SvgGroup group:
                            gccollection.Add(await SwitchCollection(group.Children, group.ID));
                            break;
                    }
                }
            }

            this.SetProgress?.Invoke(0, 99, string.Empty);
            return gccollection;
        }

    }
}

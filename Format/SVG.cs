using Svg;
using Svg.Pathing;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public SVG() : base("SVG Vector", new string[1] { ".svg" }) { }

        public override Get ReadFile => GetAsync;

        private async Task<object> GetAsync(string filepath)
        {
            return await Task<object>.Run(() =>
            {
                if (File.Exists(filepath) == true)
                {
                    SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(filepath, new Dictionary<string, string>());
                    GCTools.Log?.Invoke($"Load {this.Name} file: {filepath}", "GCTool");
                    GCCollection retcollection = SwitchCollection(svgDoc.Children, GCTools.GetName(filepath));

                    retcollection.Add(new TextElement(Path.GetFileName(filepath), 10,
                        new System.Windows.Media.Media3D.Point3D(
                            retcollection.Bounds.BottomLeft.X, 
                            retcollection.Bounds.BottomLeft.Y + 500, 0)));
                    return retcollection;
                }
                return null;
            });
        }

        public async Task<object> Parse(string text)
        {
            return await Task.Run(() =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                MemoryStream stream = new MemoryStream(bytes);
                SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(stream);
                return SwitchCollection(svgDoc.Children, "Clipboard");
            });
        }

        private GCCollection SwitchCollection(SvgElementCollection elements, string Name)
        {
            GCCollection gccollection = new GCCollection(Name);
            GCTools.SetProgress?.Invoke(0, elements.Count, "Parse SVG");
            if (elements.Count < 300)
            {
                foreach (SvgElement svgElement in elements)
                {
                    int index = elements.IndexOf(svgElement);
                    GCTools.SetProgress?.Invoke(index, elements.Count - 1, $"Parse SVG {index}/{elements.Count - 1}");

                    if (svgElement.Visibility.ToLower() == "visible")
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
                                PathGeometry pathGeometry = ParseSvgPath(path);
                                gccollection.Add(new GeometryElement(pathGeometry, svgElement.ID));
                                break;

                            case SvgGroup group:
                                gccollection.Add(SwitchCollection(group.Children, group.ID));
                                break;
                        }
                    }
                }
            }
            else
            {
                GCTools.Log($"{Name}: Elements count > 300 !!!", "GCTool");
            }

            GCTools.SetProgress?.Invoke(0, 99, string.Empty);
            return gccollection;
        }

        private static PathGeometry ParseSvgPath(SvgPath path)
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            for (int i = 0; i < path.PathData.Count; i++)
            {
                switch (path.PathData[i])
                {
                    case SvgMoveToSegment svgMoveTo:
                        pathFigure = new PathFigure
                        {
                            StartPoint = GCTools.Pftp(svgMoveTo.Start),
                            IsClosed = true
                        };
                        break;

                    case SvgQuadraticCurveSegment svgQuadraticCurve:
                        PolyQuadraticBezierSegment polyQuadraticBezierSegment = new PolyQuadraticBezierSegment();

                        polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.ControlPoint));
                        polyQuadraticBezierSegment.Points.Add(GCTools.Pftp(svgQuadraticCurve.End));

                        pathFigure.Segments.Add(polyQuadraticBezierSegment);
                        break;

                    case SvgArcSegment svgArcSeg:
                        pathFigure.Segments.Add(
                            new ArcSegment(
                                 GCTools.Pftp(svgArcSeg.End),
                            new Size(svgArcSeg.RadiusX, svgArcSeg.RadiusY), svgArcSeg.Angle, false,
                            svgArcSeg.Sweep == Svg.Pathing.SvgArcSweep.Positive ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                            true));
                        break;

                    case SvgLineSegment svgLineSegment:
                        pathFigure.Segments.Add(new LineSegment(GCTools.Pftp(svgLineSegment.End), true));
                        break;

                    case SvgCubicCurveSegment svgCubicCurve:
                        pathFigure.Segments.Add(new BezierSegment(
                             GCTools.Pftp(svgCubicCurve.FirstControlPoint),
                           GCTools.Pftp(svgCubicCurve.SecondControlPoint),
                          GCTools.Pftp(svgCubicCurve.End), true));
                        break;

                    case SvgClosePathSegment segment:
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    default:
                        break;
                }
            }

            return pathGeometry;
        }

    }
}

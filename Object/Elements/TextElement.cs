using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object.Elements
{
    public class TextElement : IGCElement
    {
        public string Name
        {
            get => this.Text;
            set => this.Text = value;
        }
        public bool IsClosed { get; set; } = true;

        public string Text 
        {
            get => _text;
            set
            {
                this._text = value;

            }
        }
        private string _text = string.Empty;

        public Point3D Point { get; set; }

        public double Size { get; set; }

        public Task<Geometry> MyGeometry { get; private set; }

        public Rect Bounds => this.bounds;
        private Rect bounds;

        private FormattedText formattedText;

        public TextElement(string Text, double Size, Point3D Point)
        {
            this.Text = Text;
            this.Point = Point;
            this.Size = Size;
            this.formattedText = new FormattedText(Text,
                                CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                new Typeface("Tahoma"), Math.Max(Size, 1) * 5, Brushes.Black);

            MyGeometry = new Task<Geometry>(() => { return formattedText.BuildGeometry(new Point(this.Point.X, this.Point.Y)); });
            this.bounds = Bounds;
        }

        public List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge) => new List<PointsElement>();

        public Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            Transform.TryTransform(this.Point, out Point3D point);
            return this.formattedText.BuildGeometry(new Point(point.X, point.Y));
        }
    }
}

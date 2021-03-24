using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object
{
    public class TextElement : IGCElement
    {
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

        public Geometry GetGeometry { get; private set; }

        public Rect Bounds => GetGeometry.Bounds;

        public TextElement(string Text, double Size, Point3D Point)
        {
            this.Text = Text;
            this.Point = Point;

            FormattedText formatted = new FormattedText(Text,
                                CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                new Typeface("Tahoma"), Size * 5, Brushes.Black);
            GetGeometry = formatted.BuildGeometry(new Point(this.Point.X, this.Point.Y));
        }

        public List<PointsElement> GetPointCollection(bool GetChar, double RoundStep, double RoundEdge) => new List<PointsElement>();
    }
}

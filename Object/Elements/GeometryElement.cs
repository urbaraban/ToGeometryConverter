using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object.Elements
{
    public class GeometryElement : IGCElement
    {
        public string Name { get; set; }
        public Geometry MyGeometry { get; set; }

        public bool IsClosed { get; set; }

        public Rect Bounds => this.bounds;
        private Rect bounds;

        public GeometryElement(Geometry geometry, string Name)
        {
            MyGeometry = geometry;
            MyGeometry.Freeze();
            bounds = geometry.Bounds;
            this.Name = Name;
        }

        public List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            return GCTools.TransformPoint(Transform, GCTools.GetGeometryPoints(MyGeometry, RoundStep, RoundEdge)); 
        }

        public Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            return GCTools.GetPointsGeometries(this.GetPointCollection(Transform, RoundStep, RoundEdge));
        }

        public string ToString() => MyGeometry.ToString();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object
{
    public class GeometryElement : IGCElement
    {
        public Geometry MyGeometry { get; set; }

        public bool IsClosed { get; set; }

        public GeometryElement(Geometry geometry)
        {
            MyGeometry = geometry;
        }

        public Geometry GetGeometry { 
            get => MyGeometry;  
        }

        public List<PointsElement> GetPointCollection(bool GetChar, double RoundStep, double RoundEdge)
        {
            return GCTools.GetGeometryPoints(MyGeometry, RoundStep, RoundEdge);
        }

        public Rect Bounds => new Rect();
    }
}

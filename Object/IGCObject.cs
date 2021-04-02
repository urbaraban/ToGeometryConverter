using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Object
{
    public interface IGCObject
    {
        //Geometry GetGeometry { get; }

        List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge);

        Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge);

        Rect Bounds { get; }
    }
}

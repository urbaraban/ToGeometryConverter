using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object
{
    public interface IGCElement : IGCObject
    {
        Geometry GetGeometry { get; }
        bool IsClosed { get; set; }
    }
}

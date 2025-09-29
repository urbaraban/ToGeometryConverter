using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object.Elements
{
    public interface IGCElement : IGCObject
    { 
        bool IsClosed { get; set; }
    }
}

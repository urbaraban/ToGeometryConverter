using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object.Elements
{
    public interface IGCElement : IGCObject
    { 
        string Name { get; set; }
        bool IsClosed { get; set; }
    }
}

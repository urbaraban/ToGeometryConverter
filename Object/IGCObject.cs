using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ToGeometryConverter.Object
{
    public interface IGCObject
    {
        //Geometry GetGeometry { get; }

        List<PointsElement> GetPointCollection(bool GetChar, double RoundStep, double RoundEdge);



        Rect Bounds { get; }
    }
}

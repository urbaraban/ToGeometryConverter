﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object.Elements
{
    public class GeometryElement : IGCElement
    {
        public Geometry MyGeometry { get; set; }

        public bool IsClosed { get; set; }

        public GeometryElement(Geometry geometry)
        {
            MyGeometry = geometry;
        }

        public List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            return GCTools.TransformPoint(Transform, GCTools.GetGeometryPoints(MyGeometry, RoundStep, RoundEdge));
        }

        public Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge)
        {
           return GCTools.GetPointsGeometries(this.GetPointCollection(Transform, RoundStep, RoundEdge));
        }

        public Rect Bounds => MyGeometry.Bounds;
    }
}
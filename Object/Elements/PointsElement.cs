using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object.Elements
{
    public class PointsElement : IList<GCPoint3D>, IGCElement
    {
        public string Name { get; set; }

        public List<GCPoint3D> Points
        {
            get => this._points;
            set
            {
                this._points = value;
            }
        }
        private List<GCPoint3D> _points = new List<GCPoint3D>();

        public List<Point3D> GetPoints3D
        {
            get
            {
                List<Point3D> point3Ds = new List<Point3D>();
                foreach (GCPoint3D gCPoint3D in Points)
                {
                    point3Ds.Add(new Point3D(gCPoint3D.X, gCPoint3D.Y, gCPoint3D.Z));
                }
                return point3Ds;
            }
        }

        public List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            PointsElement points = new PointsElement() { IsClosed = this.IsClosed };
            foreach (GCPoint3D point in this._points)
            {
                Transform.TryTransform(point.GetPoint3D, out Point3D result);
                points.Add(result);
            }
            return new List<PointsElement>() { points };
        }


        public Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            return GCTools.GetPointsGeometries(this.GetPointCollection(Transform, RoundStep, RoundEdge));
        }

        public Rect Bounds 
        {
            get
            {
                double minX = Points[0].X, minY = Points[0].Y, minZ = Points[0].Z, maxX = Points[0].X, maxY = Points[0].Y, maxZ = Points[0].Z;
                foreach(GCPoint3D GCPoint3D in Points)
                {
                    minX = Math.Min(minX, GCPoint3D.X);
                    maxX = Math.Max(maxX, GCPoint3D.X);
                    minZ = Math.Min(minY, GCPoint3D.Y);
                    maxY = Math.Max(maxY, GCPoint3D.Y);
                    minZ = Math.Min(minZ, GCPoint3D.Z);
                    maxZ = Math.Max(maxZ, GCPoint3D.Z);
                }
                return new Rect(new Point(minX, minY), new Point(maxX, maxY));
            }
        }

        public bool IsClosed { get; set; }

        public override string ToString() => this.Count.ToString();


        #region IList<GCPoint3D>
        public GCPoint3D this[int index] { get => ((IList<GCPoint3D>)Points)[index]; set => ((IList<GCPoint3D>)Points)[index] = value; }

        public int Count => ((ICollection<GCPoint3D>)Points).Count;

        public bool IsReadOnly => ((ICollection<GCPoint3D>)Points).IsReadOnly;


        public void Add(GCPoint3D item)
        {
            ((ICollection<GCPoint3D>)Points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<GCPoint3D>)Points).Clear();
        }

        public bool Contains(GCPoint3D item)
        {
            return ((ICollection<GCPoint3D>)Points).Contains(item);
        }

        public void CopyTo(GCPoint3D[] array, int arrayIndex)
        {
            ((ICollection<GCPoint3D>)Points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<GCPoint3D> GetEnumerator()
        {
            return ((IEnumerable<GCPoint3D>)Points).GetEnumerator();
        }

        public int IndexOf(GCPoint3D item)
        {
            return ((IList<GCPoint3D>)Points).IndexOf(item);
        }

        public void Insert(int index, GCPoint3D item)
        {
            ((IList<GCPoint3D>)Points).Insert(index, item);
        }

        public bool Remove(GCPoint3D item) => ((ICollection<GCPoint3D>)Points).Remove(item);

        public void RemoveAt(int index) => ((IList<GCPoint3D>)Points).RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Points).GetEnumerator();

        internal void AddRange(List<GCPoint3D> points) => Points.AddRange(points);

        internal void AddRange(PointsElement points) => Points.AddRange(points);

        internal void Add(Point3D result)
        {
            this.Points.Add(new GCPoint3D(result.X, result.Y, result.Z));
        }
        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Object
{
    public class PointsElement : IList<Point3D>, IGCElement
    {
        public List<Point3D> Points
        {
            get => this._points;
            set
            {
                this._points = value;
            }
        }
        private List<Point3D> _points = new List<Point3D>();

        //public Geometry GetGeometry => throw new NotImplementedException();

        public List<PointsElement> GetPointCollection(bool GetChar, double RoundStep, double RoundEdge) => 
            new List<PointsElement>() { this };

        public Geometry GetGeometry { get; private set; }

        public Rect Bounds 
        {
            get
            {
                double minX = Points[0].X, minY = Points[0].Y, minZ = Points[0].Z, maxX = Points[0].X, maxY = Points[0].Y, maxZ = Points[0].Z;
                foreach(Point3D point3D in Points)
                {
                    minX = Math.Min(minX, point3D.X);
                    maxX = Math.Max(maxX, point3D.X);
                    minZ = Math.Min(minY, point3D.Y);
                    maxY = Math.Max(maxY, point3D.Y);
                    minZ = Math.Min(minZ, point3D.Z);
                    maxZ = Math.Max(maxZ, point3D.Z);
                }
                return new Rect(new Point(minX, minY), new Point(maxX, maxY));
            }
        }

        public bool IsClosed { get; set; }


        #region IList<Point3D>
        public Point3D this[int index] { get => ((IList<Point3D>)Points)[index]; set => ((IList<Point3D>)Points)[index] = value; }

        public int Count => ((ICollection<Point3D>)Points).Count;

        public bool IsReadOnly => ((ICollection<Point3D>)Points).IsReadOnly;


        public void Add(Point3D item)
        {
            ((ICollection<Point3D>)Points).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Point3D>)Points).Clear();
        }

        public bool Contains(Point3D item)
        {
            return ((ICollection<Point3D>)Points).Contains(item);
        }

        public void CopyTo(Point3D[] array, int arrayIndex)
        {
            ((ICollection<Point3D>)Points).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Point3D> GetEnumerator()
        {
            return ((IEnumerable<Point3D>)Points).GetEnumerator();
        }

        public int IndexOf(Point3D item)
        {
            return ((IList<Point3D>)Points).IndexOf(item);
        }

        public void Insert(int index, Point3D item)
        {
            ((IList<Point3D>)Points).Insert(index, item);
        }

        public bool Remove(Point3D item) => ((ICollection<Point3D>)Points).Remove(item);

        public void RemoveAt(int index) => ((IList<Point3D>)Points).RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Points).GetEnumerator();

        internal void AddRange(List<Point3D> points) => Points.AddRange(points);

        internal void AddRange(PointsElement points) => Points.AddRange(points);
        #endregion
    }
}

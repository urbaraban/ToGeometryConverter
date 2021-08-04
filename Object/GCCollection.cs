using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Object
{
    public class GCCollection : IGCObject, IList<IGCObject>
    {
        public List<IGCObject> Elements = new List<IGCObject>();

        public string Name { get; set; } = string.Empty;

        public int IndexSelectElement = -1;

        public Rect Bounds
        {
            get
            {
                if (Elements.Count > 0)
                {
                    Rect first = Elements[0].Bounds;
                    double minX = first.X, minY = first.Y, /*minZ = first.Z,*/ maxX = first.X, maxY = first.Y; /*maxZ = first.Z*/

                    foreach (IGCObject gCObject in Elements)
                    {
                        if ((gCObject is TextElement) == false)
                        {
                            minX = Math.Min(minX, gCObject.Bounds.TopLeft.X);
                            maxX = Math.Max(maxX, gCObject.Bounds.BottomRight.X);
                            minY = Math.Min(minY, gCObject.Bounds.TopLeft.Y);
                            maxY = Math.Max(maxY, gCObject.Bounds.BottomRight.Y);
                            //minZ = Math.Min(minZ, point3D.Z);
                            //maxZ = Math.Max(maxZ, point3D.Z);
                        }
                    }
                    return new Rect(new Point(minX, minY), new Point(maxX, maxY));
                }
                return new Rect();
            }
        }

        public GCCollection(string Name)
        {
            this.Name = Name;
        }

        public List<PointsElement> GetPointCollection(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            List<PointsElement> points = new List<PointsElement>();
            foreach(IGCElement element in this.Elements)
            {
                if ((element is TextElement) == false)
                {
                    points.AddRange(element.GetPointCollection(Transform, RoundStep, RoundEdge));
                }
            }
            return points;
        }

        internal void AddRange(GCCollection gCCollection)
        {
            foreach (IGCElement gCElement in gCCollection)
            {
                this.Add(gCElement);
            }
        }

        public void AddRange(List<IGCElement> pointsElements) => Elements.AddRange(pointsElements);

        public void AddRange(GeometryGroup geometryGroup)
        {
            foreach(Geometry geometry in geometryGroup.Children)
            {
                Elements.Add(new GeometryElement(geometry, geometry.GetType().Name));
            }
        }

        public Geometry GetGeometry(Transform3D Transform, double RoundStep, double RoundEdge)
        {
            GeometryGroup geometryGroup = new GeometryGroup();
            foreach (IGCElement element in this.Elements)
            {
                geometryGroup.Children.Add(element.GetGeometry(Transform, RoundStep, RoundEdge));
            }

            return geometryGroup;
        }

        #region IList<IGCObject>
        public IGCObject this[int index] { get => ((IList<IGCObject>)Elements)[index]; set => ((IList<IGCObject>)Elements)[index] = value; }

        public int Count => ((ICollection<IGCObject>)Elements).Count;

        public bool IsReadOnly => ((ICollection<IGCObject>)Elements).IsReadOnly;

        public void Add(IGCObject item)
        {
            ((ICollection<IGCObject>)Elements).Add(item);
        }

        public void Clear()
        {
            ((ICollection<IGCObject>)Elements).Clear();
        }

        public bool Contains(IGCObject item)
        {
            return ((ICollection<IGCObject>)Elements).Contains(item);
        }

        public void CopyTo(IGCObject[] array, int arrayIndex)
        {
            ((ICollection<IGCObject>)Elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IGCObject> GetEnumerator()
        {
            return ((IEnumerable<IGCObject>)Elements).GetEnumerator();
        }

        public int IndexOf(IGCObject item)
        {
            return ((IList<IGCObject>)Elements).IndexOf(item);
        }

        public void Insert(int index, IGCObject item)
        {
            ((IList<IGCObject>)Elements).Insert(index, item);
        }

        public bool Remove(IGCObject item)
        {
            return ((ICollection<IGCObject>)Elements).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IGCObject>)Elements).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Elements).GetEnumerator();
        }
        #endregion
    }
}

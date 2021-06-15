using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Object
{
    public class GCCollection : IGCObject, IList<IGCElement>
    {
        public List<IGCElement> Elements = new List<IGCElement>();

        public string Name = string.Empty;

        public int IndexSelectElement = -1;

        public Rect Bounds
        {
            get
            {
                if (Elements.Count > 0)
                {
                    Rect first = Elements[0].Bounds;
                    double minX = first.X, minY = first.Y, /*minZ = first.Z,*/ maxX = first.X, maxY = first.Y; /*maxZ = first.Z*/

                    foreach (IGCElement gCObject in Elements)
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

        public void AddRange(List<IGCElement> pointsElements) => Elements.AddRange(pointsElements);

        public void AddRange(GeometryGroup geometryGroup)
        {
            foreach(Geometry geometry in geometryGroup.Children)
            {
                Elements.Add(new GeometryElement(geometry));
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

        #region IList<IGCElement>
        public IGCElement this[int index] { get => ((IList<IGCElement>)Elements)[index]; set => ((IList<IGCElement>)Elements)[index] = value; }

        public int Count => ((ICollection<IGCElement>)Elements).Count;

        public bool IsReadOnly => ((ICollection<IGCElement>)Elements).IsReadOnly;

        public void Add(IGCElement item)
        {
            ((ICollection<IGCElement>)Elements).Add(item);
        }

        public void Clear()
        {
            ((ICollection<IGCElement>)Elements).Clear();
        }

        public bool Contains(IGCElement item)
        {
            return ((ICollection<IGCElement>)Elements).Contains(item);
        }

        public void CopyTo(IGCElement[] array, int arrayIndex)
        {
            ((ICollection<IGCElement>)Elements).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IGCElement> GetEnumerator()
        {
            return ((IEnumerable<IGCElement>)Elements).GetEnumerator();
        }

        public int IndexOf(IGCElement item)
        {
            return ((IList<IGCElement>)Elements).IndexOf(item);
        }

        public void Insert(int index, IGCElement item)
        {
            ((IList<IGCElement>)Elements).Insert(index, item);
        }

        public bool Remove(IGCElement item)
        {
            return ((ICollection<IGCElement>)Elements).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IGCElement>)Elements).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Elements).GetEnumerator();
        }
        #endregion
    }
}

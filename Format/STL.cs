using IxMilia.Stl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    public class STL : IFormat
    {
        public string Name => "STL";

        public string[] ShortName => new string[1] { "stl" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public GCCollection Get(string Filename, double RoundStep)
        {
            GCCollection gCElements = new GCCollection();

            try
            {
                using (FileStream fs = new FileStream(Filename, FileMode.Open))
                {
                    StlFile stlFile = StlFile.Load(fs);

                    List<Polygon> polygons = new List<Polygon>();

                    Progressed?.Invoke(this, new Tuple<int, int>(1,4));

                    foreach (StlTriangle stlTriangle in stlFile.Triangles)
                    {
                        polygons.Add(
                            new Polygon(
                            new Edge(stlTriangle.Vertex1, stlTriangle.Vertex2),
                            new Edge(stlTriangle.Vertex3, stlTriangle.Vertex1),
                            new Edge(stlTriangle.Vertex2, stlTriangle.Vertex3),
                            stlTriangle.Normal.Normalize()
                            ));
                    }

                    List<Edge> edges = GetPlaces(polygons);

                        gCElements.AddRange(GetContourPlaces(edges));
                    

                }
            }
            catch
            {
                return null;
            }

            return gCElements;
        }

        private static List<Edge> FindBoundary(List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = result.Count - 1; i > 0; i -= 1)
            {
                for (int n = i - 1; n >= 0; n -= 1)
                {
                    if (EqalseEdge(result[i], result[n]) == true)
                    {
                        // shared edge so remove both
                        //result.RemoveAt(i);
                        result.RemoveAt(n);
                        n--;
                        break;
                    }
                }
            }
            return result;
        }

        private static List<Edge> GetPlaces(List<Polygon> Polygons)
        {
            //Remove edge in polygons with angle beetwin < 10 grad
            for (int first = 0; first < Polygons.Count - 1; first += 1)
            {
                for (int second = first + 1; second < Polygons.Count; second += 1)
                {
                    if (Vector3D.AngleBetween(Polygons[first].normal, Polygons[second].normal) < 15)
                    {
                        for (int h = 0; h < Polygons[first].edges.Count; h += 1)
                        {
                            for (int z = 0; z < Polygons[second].edges.Count; z += 1)
                            {
                                if (Polygons[first].edges[h] != null && Polygons[second].edges[z] != null)
                                {
                                    if (EqalseEdge(Polygons[first].edges[h], Polygons[second].edges[z]) == true)
                                    {
                                        Polygons[first].edges[h] = null;
                                        Polygons[second].edges[z] = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<Edge> EdgePlace = new List<Edge>();
            foreach (Polygon polygon in Polygons)
            {
                foreach (Edge edge in polygon.edges)
                {
                    if (edge != null)
                    {
                        EdgePlace.Add(edge);
                    }
                }
            }

            return EdgePlace;
        }

        private static List<Edge> SortEdges(List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = 0; i < result.Count - 2; i++)
            {
                Edge E = result[i];
                for (int n = i + 1; n < result.Count; n++)
                {
                    Edge a = result[n];
                    if (EqalseVertex(E.v2, a.v1) == true)
                    {
                        // in this case they are already in order so just continoue with the next one
                        if (n == i + 1)
                            break;
                        // if we found a match, swap them with the next one after "i"
                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }
            return result;
        }

        private static List<PointsElement> GetContourPlaces(List<Edge> Edges)
        {
            List<Edge> SortEdges = STL.SortEdges(Edges);

            List<List<Edge>> Places = new List<List<Edge>>();
            List<Edge> PlaceEdges = new List<Edge>() { SortEdges[0] };

            for (int i = 1; i < SortEdges.Count; i += 1)
            {
                if (EqalseVertex(SortEdges[i].v1, PlaceEdges[PlaceEdges.Count - 1].v2) == true)
                {
                    PlaceEdges.Add(SortEdges[i]);
                }
                else
                {
                    Places.Add(PlaceEdges);
                    PlaceEdges = new List<Edge>() { SortEdges[i] };
                }
            }
            if (Places.IndexOf(PlaceEdges) == -1) Places.Add(PlaceEdges);

            List<PointsElement> elements = new List<PointsElement>();

            foreach (List<Edge> place in Places)
            {
                PointsElement element = new PointsElement() { IsClosed = (EqalseVertex(place[0].v1, place[place.Count - 1].v2)) };
                for (int i = 0; i < place.Count; i += 1)
                {
                    element.Add(new Point3D(SortEdges[i].v1.X, SortEdges[i].v1.Y, SortEdges[i].v1.Z));
                }

                 elements.Add(element);
            }

            return elements;
        }

        private static PointsElement GetElementEdges(List<Edge> edges)
        {
            PointsElement element = new PointsElement() { IsClosed = true };
            foreach (Edge edge in edges)
            {
                element.Add(new Point3D(edge.v1.X, edge.v1.Y, edge.v1.Z));
            }
            return element;
        }

        public static bool EqalseVertex(StlVertex stlVertex1, StlVertex stlVertex2) =>
            Math.Sqrt(
                Math.Pow(stlVertex2.X - stlVertex1.X, 2) + 
                Math.Pow(stlVertex2.Y - stlVertex1.Y, 2) + 
                Math.Pow(stlVertex2.Z - stlVertex1.Z, 2)) < 0.1;

        public static bool EqalseEdge(Edge edge1, Edge edge2) =>
            ((EqalseVertex(edge1.v1, edge2.v1) && EqalseVertex(edge1.v2, edge2.v2)) ||
            (EqalseVertex(edge1.v2, edge2.v1) && EqalseVertex(edge1.v1, edge2.v2)));

        public static bool CheckPolygonPlace(List<Polygon> BasePolygons, Polygon CheckPolygon, bool CheckEdge)
        {
            if (CheckEdge == true)
            {
                foreach(Polygon BasePolygon in BasePolygons)
                {
                    foreach(Edge BaseEdge in BasePolygon.edges)
                    {
                        foreach(Edge edgeCheck in CheckPolygon.edges)
                        {
                            if (Vector3D.AngleBetween(BasePolygon.normal, CheckPolygon.normal) == 0 &&
                                EqalseEdge(BaseEdge, edgeCheck) == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                return Vector3D.AngleBetween(BasePolygons[0].normal, CheckPolygon.normal) < 1;
            }

            return false;
        }
            


    }

    public class Edge
    {
        public StlVertex v1;
        public StlVertex v2;

        public Edge(StlVertex v1, StlVertex v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public Edge GetInverse => new Edge(this.v2, this.v1 );

        public override string ToString() => $"{v1.X}:{v1.Y}:{v1.Z};   {v2.X}:{v2.Y}:{v2.Z}";
    }

    public struct Polygon
    {
        public List<Edge> edges;
        public Vector3D normal;

        public Polygon(Edge edge1, Edge edge2, Edge edge3, StlNormal Normal)
        {
            edges = new List<Edge>()
            {
                edge1,
                edge2,
                edge3
            };

            this.normal = new Vector3D (Normal.X, Normal.Y, Normal.Z);
        }
    }
}

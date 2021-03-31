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

            using (FileStream fs = new FileStream(Filename, FileMode.Open))
            {
                StlFile stlFile = StlFile.Load(fs);

                List<Polygon> polygons = new List<Polygon>();

                Progressed?.Invoke(this, new Tuple<int, int>(1, 4));

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

                List<List<Edge>> edges = GetPlaces(polygons);

                foreach (List<Edge> place in edges)
                {
                    if (place.Count > stlFile.Triangles.Count / 20)
                    {
                        gCElements.AddRange(GetContourPlaces(place));
                        //gCElements.Add(GetContour(STL.SortEdges(place)));
                    }
                }

                fs.Close();
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

        private static List<List<Edge>> GetPlaces(List<Polygon> Polygons)
        {
            ToGCLogger.Set(0, Polygons.Count, "Выделяем поверхности");
            //Remove edge in polygons with angle beetwin < 10 grad
            for (int first = 0; first < Polygons.Count - 1; first += 1)
            {
                ToGCLogger.Set(first, Polygons.Count, $"{first}/{Polygons.Count}");
                for (int second = first + 1; second < Polygons.Count; second += 1)
                {
                    if (Vector3D.AngleBetween(Polygons[first].normal, Polygons[second].normal) < 20)
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
                if (Polygons[first].edges[0] == null && Polygons[first].edges[1] == null && Polygons[first].edges[2] == null)
                {
                    Polygons.RemoveAt(first);
                    first -= 1;
                }
            }

            //sort polygon by places with similar normal
            List<List<Polygon>> PlacesPolygons = new List<List<Polygon>> { new List<Polygon> { Polygons[0] } };
            for (int first = 1; first < Polygons.Count; first += 1)
            {
                Polygon polygon = Polygons[first];
                foreach (List<Polygon> place in PlacesPolygons)
                {
                    if (Vector3D.AngleBetween(place[0].normal, polygon.normal) <= 1)
                    {
                        place.Add(polygon);
                        Polygons.Remove(polygon);
                        first -= 1;
                        break;
                    }
                }
                if (Polygons.IndexOf(polygon) > -1)
                {
                    PlacesPolygons.Add(new List<Polygon> { Polygons[first] });
                    Polygons.Remove(polygon);
                    first -= 1;
                }
            }

            List<List<Edge>> EdgePlace = new List<List<Edge>>();

            foreach (List<Polygon> polygons in PlacesPolygons)
            {
                List<Edge> edges = new List<Edge>();
                foreach (Polygon polygon in polygons)
                {
                    foreach (Edge edge in polygon.edges)
                    {
                        if (edge != null)
                        {
                            edges.Add(edge);
                        }
                    }
                }
                if (edges.Count > 2) EdgePlace.Add(edges);
            }

            ToGCLogger.End();
            return EdgePlace;
        }

        private static List<Edge> SortEdges(List<Edge> aEdges)
        {
            Console.WriteLine($"Sort {aEdges.Count} edges");
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = 0; i < result.Count - 1; i++)
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
            List<PointsElement> points = new List<PointsElement>(); 

            for (int i = 1; i < SortEdges.Count - 1; i += 1)
            {
                if (EqalseVertex(SortEdges[i].v1, SortEdges[i - 1].v2, true) == false)
                {
                    if (i > 2)
                    {
                        points.Add(GetContour(SortEdges.GetRange(0, i)));
                    }
                    SortEdges.RemoveRange(0, i);
                    i = 1;
                }
            }
            if (SortEdges.Count > 2)
            {
                points.Add(GetContour(SortEdges));
            }

            return points;
        }

        public static PointsElement GetContour(List<Edge> edges)
        {
            PointsElement points = new PointsElement() { IsClosed = true };
            foreach(Edge edge in edges)
            {
                points.Add(GCPoint3D.Parse(edge));
            }
            return points;
        }

        public static bool EqalseVertex(StlVertex stlVertex1, StlVertex stlVertex2, bool log = false)
        {
            double lenth = Math.Sqrt(
                Math.Pow(stlVertex2.X - stlVertex1.X, 2) +
                Math.Pow(stlVertex2.Y - stlVertex1.Y, 2) +
                Math.Pow(stlVertex2.Z - stlVertex1.Z, 2));
            if (log == true)
                Console.WriteLine(lenth.ToString());
            return lenth == 0;
        }

        public static bool EqalseEdge(Edge edge1, Edge edge2) =>
            ((EqalseVertex(edge1.v1, edge2.v1) && EqalseVertex(edge1.v2, edge2.v2)) ||
            (EqalseVertex(edge1.v2, edge2.v1) && EqalseVertex(edge1.v1, edge2.v2)));

          


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

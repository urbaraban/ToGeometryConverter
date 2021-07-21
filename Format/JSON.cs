using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    class JSON : IFormat
    {
        public string Name => "json";

        public string[] ShortName => new string[1] { "json" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            string openStream = File.ReadAllText(Filename);
            JObject Main = (JObject)JsonConvert.DeserializeObject(openStream);

            JToken Contours = Main["coordinates_of_points_with_shrinkage"];

            GCCollection collection = new GCCollection();
            foreach (JToken contour in Contours)
            {
                PointsElement element = new PointsElement() { IsClosed = true };

                JToken points = contour["points"];

                foreach (JToken point in points)
                {
                    element.Add(new GCPoint3D()
                    {
                        X = double.Parse(point["x"].ToString()),
                        Y = double.Parse(point["y"].ToString()),
                    });
                }

                collection.Add(element);
            }

            return collection;
        }
    }
}

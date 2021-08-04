using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    class MetaFile : IFormat
    {
        public string Name => "MetaFile";

        public string[] ShortName => new string[2] { "wmf", "emf" };

        public Tuple<int, int> Progress { get; private set; }

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            GCCollection elements = new GCCollection(GCTools.GetName(Filename));


            return elements;
        }
    }
}

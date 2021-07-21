using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    class MetaFile : IFormat
    {
        public string Name => "MetaFile";

        public string[] ShortName => new string[2] { "wmf", "emf" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            GCCollection elements = new GCCollection();
            Metafile metafile = new Metafile(Filename);
            if (metafile != null)
            {
                foreach (PropertyItem item in metafile.PropertyItems)
                {

                }
            }
            return elements;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToGeometryConverter.Format;
using ToGeometryConverter.Object;

namespace ToGeometryConverter
{
    public static class ToGC
    {
        public static event EventHandler<Tuple<int, int>> Progressed;

        private static List<IFormat> Formats = new List<IFormat>()
        {
            new SVG(),
            new DXF(),
            new DCeiling(),
            new STL(),
            new ILD(),
            //new IGES(),
            //new PDF()
        };

        public async static Task<GCCollection> AsyncGet(string Filename, double RoundStep)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (IFormat format in Formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm.ToLower() == InFileFormat.ToLower())
                    {
                        return format.Get(Filename, RoundStep);
                    }
                }
            }
            return null;
        }

        public static string Filter
        {
            get
            {
                string _filter = string.Empty;
                string _allformat = string.Empty;

                foreach (IFormat format in Formats)
                {
                    foreach (string frm in format.ShortName)
                    {
                        _allformat += $"*.{frm};";
                    }
                }
                _filter += $"All Format ({_allformat}) | {_allformat}";

                foreach (IFormat format in Formats)
                {
                    _allformat = string.Empty;
                    foreach (string frm in format.ShortName)
                    {
                        _allformat += $"*.{frm};";
                    }
                    _filter += $" | {format.Name}({_allformat}) | {_allformat}";
                }

                _filter += " | All Files (*.*)|*.*";

                return _filter;
            }
        }
    }
}

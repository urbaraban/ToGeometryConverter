using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ToGeometryConverter.Format;

namespace ToGeometryConverter
{
    public static class ToGC
    {
        private static List<IFormat> Formats = new List<IFormat>()
        {
            new SVG(),
            new DXF(),
            new DCeiling()
        };

        public static GeometryGroup? Get(string Filename, double RoundStep)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (IFormat format in Formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm == InFileFormat) return format.Get(Filename, RoundStep);
                }
            }
            return null;
        }

        //"(.frw; .cdw; .svg; .dxf; .stp; .ild; .ec)|*.frw; *.cdw; *.svg; *.dxf, *.stp, *.ild, *.ec| All Files (*.*)|*.*";

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

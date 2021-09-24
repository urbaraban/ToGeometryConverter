using System;
using System.Collections.Generic;
using System.Linq;

namespace ToGeometryConverter
{
    public static class ToGC
    {
        /*public static List<GCFormat> Formats { get; } = new List<GCFormat>()
        {
            new SVG(),
            new DXF(),
            new DEXCeil(),
            new STL(),
            new ILD(),
            new MetaFile(),
            new JSON()
        };*/

        public static GCFormat GetConverter(string Filename, ICollection<GCFormat> formats)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (GCFormat format in formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm.ToLower() == InFileFormat.ToLower())
                    {
                        return format;
                    }
                }
            }
            return null;
        }

        public static string GetAllFormatsFilter(ICollection<GCFormat> formats)
        {
            string _allformat = string.Empty;
            //add standart format
            foreach (GCFormat format in formats)
            {
                foreach (string frm in format.ShortName)
                {
                    _allformat += $"*.{frm};";
                }
            }

            return $"All Format ({_allformat}) | {_allformat}";
        }

        public static string GetFilter(GCFormat[] AddFormat, ICollection<GCFormat> formats)
        {
            string _filter = string.Empty;

            _filter += GetAllFormatsFilter(formats);

            foreach (GCFormat format in formats)
            {
                string strformat = string.Empty;
                foreach (string frm in format.ShortName)
                {
                    strformat += $"*.{frm};";
                }
                _filter += $" | {format.Name}({strformat}) | {strformat}";
            }

            _filter += " | All Files (*.*)|*.*";

            return _filter;
        }
    }

    public static class ToGCLogger
    {
        public static event EventHandler<ProgBarMessage> Progressed;

        public static void Set(int Value, int MaxValue, string Message)
        {
            Progressed?.Invoke(null, new ProgBarMessage(Value, MaxValue, Message));
        }
        
        public static void End()
        {
            Progressed?.Invoke(null, new ProgBarMessage(0, 1, string.Empty));
        }
    }

    public struct ProgBarMessage
    {
        public int v;
        public int m;
        public string t;
        
        public ProgBarMessage(int Value, int MaxValue, string Text)
        {
            this.v = Value;
            this.m = MaxValue;
            this.t = Text;
        }
    }
}

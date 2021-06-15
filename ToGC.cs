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

        public async static Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (IFormat format in Formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm.ToLower() == InFileFormat.ToLower())
                    {
                        return await format.GetAsync(Filename, RoundStep);
                    }
                }
            }
            return null;
        }

        private static void Format_Progressed(object sender, Tuple<int, int> e)
        {
            throw new NotImplementedException();
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

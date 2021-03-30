using System;
using ToGeometryConverter.Format.ILDA;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class ILD : IFormat
    {
        public string Name => "ILDA";

        public string[] ShortName => new string[1] { "ild" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public GCCollection Get(string Filename, double RoundStep)
        {
            return IldaReader.ReadFile(Filename);
        }
    }
}

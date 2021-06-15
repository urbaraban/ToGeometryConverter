using System;
using System.Threading.Tasks;
using ToGeometryConverter.Format.ILDA;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class ILD : IFormat
    {
        public string Name => "ILDA";

        public string[] ShortName => new string[1] { "ild" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            return IldaReader.ReadFile(Filename);
        }
    }
}

using System;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    interface IFormat
    {
        public event EventHandler<Tuple<int, int>> Progressed;

        public string Name { get; }
        public string[] ShortName { get; }

        public Task<GCCollection> GetAsync(string Filename, double RoundStep);
    }
}

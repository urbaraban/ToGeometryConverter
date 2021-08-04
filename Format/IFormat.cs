using System;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public interface IFormat
    {
        public Tuple<int, int> Progress { get; }

        public string Name { get; }
        public string[] ShortName { get; }

        public Task<GCCollection> GetAsync(string Filename, double RoundStep);
    }
}

using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    interface IFormat
    {
        public string Name { get; }
        public string[] ShortName { get; }

        public GCCollection Get(string Filename, double RoundStep);
    }
}

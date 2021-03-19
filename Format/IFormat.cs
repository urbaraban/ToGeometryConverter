using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    interface IFormat
    {
        public string Name { get; }
        public string[] ShortName { get; }

        public GeometryGroup Get(string Filename, double RoundStep);
    }
}

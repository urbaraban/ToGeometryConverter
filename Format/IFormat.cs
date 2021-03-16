using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    interface IFormat
    {
        string Name { get; }
        string[] ShortName { get; }
    }
}

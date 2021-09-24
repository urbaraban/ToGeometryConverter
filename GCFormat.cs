using System;
using System.Threading.Tasks;

namespace ToGeometryConverter
{
    public class GCFormat
    {
        public delegate Task<object> Get(string filepath, double RoundStep);
        public Get ReadFile { get; set; }

        public delegate Tuple<int, int> Progress(int position, int max, string message);

        public string Name { get; private set; }
        public string[] ShortName { get; private set; }


        public GCFormat(string Name, string[] ShortName)
        {
            this.Name = Name;
            this.ShortName = ShortName;
        }
    }
}

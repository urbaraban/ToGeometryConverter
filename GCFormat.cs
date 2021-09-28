using System.Threading.Tasks;

namespace ToGeometryConverter
{
    public class GCFormat
    {
        public delegate void Progress(int position, int max, string message);
        public Progress SetProgress;

        public delegate Task<object> Get(string filepath, double RoundStep);
        public virtual Get ReadFile { get; set; }

        public string Name { get; private set; }
        public string[] ShortName { get; private set; }


        public GCFormat(string Name, string[] ShortName)
        {
            this.Name = Name;
            this.ShortName = ShortName;
        }
    }
}

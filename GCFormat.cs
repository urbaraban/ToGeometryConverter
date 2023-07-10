using System.Threading.Tasks;

namespace ToGeometryConverter
{
    public class GCFormat
    {
        public delegate Task<object> Get(string filepath);
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

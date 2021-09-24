using System;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class MetaFile : GCFormat
    {
        public MetaFile() : base("MetaFile", new string[2] { "wmf", "emf" }) 
        {
            this.ReadFile = GetAsync;
        }


        public async Task<object> GetAsync(string Filename, double RoundStep)
        {
            GCCollection elements = new GCCollection(GCTools.GetName(Filename));

            return elements;
        }
    }
}

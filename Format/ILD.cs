using System;
using System.Threading.Tasks;
using ToGeometryConverter.Format.ILDA;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class ILD : GCFormat
    {
        public ILD() : base("ILDA", new string[1] { "ild" }) 
        {
            this.ReadFile = GetAsync;
        }

        private static async Task<object> GetAsync(string Filename, double RoundStep)
        {
            return await IldaReader.ReadFile(Filename);
        }
    }
}

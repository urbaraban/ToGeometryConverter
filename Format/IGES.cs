using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToGeometryConverter.Format
{
    class IGES : IFormat
    {
        public string Name { get; } = "IGES";
        public string[] ShortName { get; } = new string[2] { ".igs", ".iges" };

    }
}

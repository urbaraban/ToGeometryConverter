using IxMilia.Iges;
using IxMilia.Iges.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ToGeometryConverter.Format
{
    public class IGES : IFormat
    {
        public string Name { get; } = "IGES";
        public string[] ShortName { get; } = new string[2] { "igs", "iges" };


        public GeometryGroup Get(string filename, double RoundStep)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                IgesFile igesFile = IgesFile.Load(fs);


                // if on >= NETStandard1.3 you can use:
                // IgesFile igesFile = IgesFile.Load(@"C:\Path\To\File.iges");

                foreach (IgesEntity entity in igesFile.Entities)
                {
                    switch (entity.EntityType)
                    {
                        case IgesEntityType.Line:
                            IgesLine line = (IgesLine)entity;
                            // ...
                            break;
                            // ...
                    }
                }
            }
            return null;
        }
    }
}

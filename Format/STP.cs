using IxMilia.Step;
using IxMilia.Step.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;

namespace ToGeometryConverter.Format
{
    class STP : IFormat
    {
        public string Name => "STP";

        string[] IFormat.ShortName => new string[2] { "stp", "step" };

        public event EventHandler<Tuple<int, int>> Progressed;

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
        {
            GCCollection gCElements = new GCCollection();
            StepFile stepFile;
            using (FileStream fs = new FileStream(Filename, FileMode.Open))
            {
                stepFile = StepFile.Load(fs);

                foreach (StepRepresentationItem item in stepFile.Items)
                {
                    switch (item.ItemType)
                    {
                        case StepItemType.Line:
                            StepLine line = (StepLine)item;
                            PointsElement points = new PointsElement();
                            points.Add(new GCPoint3D(line.Point.X, line.Point.Y, line.Point.Z));
                            break;
                    }
                }
            }
            return gCElements;
        }
    }
}

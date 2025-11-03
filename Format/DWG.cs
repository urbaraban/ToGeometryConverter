using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using CSMath;
using IxMilia.Dxf;
using IxMilia.Dxf.Blocks;
using IxMilia.Dxf.Entities;
using IxMilia.Dxf.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ToGeometryConverter.Object;
using ToGeometryConverter.Object.Elements;
using Point = ACadSharp.Entities.Point;

namespace ToGeometryConverter.Format
{
    public class DWG : GCFormat
    {
        public DWG() : base("DWG", new string[1] { ".dwg" }) { }

        public override Get ReadFile => GetAsync;

        private async Task<object> GetAsync(string filename)
        {
            CadDocument doc = DwgReader.Read(filename, onNotification);

            GCTools.Log?.Invoke($"Loaded dxf: {filename}", "GCTool");

            foreach(var layer in doc.Layers)
            {
                ParseEntities(doc.Entities, doc.BlockRecords, layer.Name, new Point());
            }


            return null;
        }


        private GCCollection ParseEntities(CadObjectCollection<Entity> entitys, BlockRecordsTable blocks, string LayerName, Point location, bool isInsert = false)
        {
            GCCollection gccollection = new GCCollection(LayerName);

            var lentity = entitys.Where(x => x.Layer.Name == LayerName || isInsert == true).ToList();

            for (int i = 0; i < lentity.Count; i += 1)
            {
                GCTools.SetProgress?.Invoke(i, lentity.Count() - 1, $"Parse DXF {i}/{lentity.Count - 1}");

                if (lentity[i] is Insert insert)
                {

                    //foreach (var block in blocks)
                    //{
                    //    if (insert.Block.Name == block.Name)
                    //    {
                    //        DxfPoint insert_location = new DxfPoint(
                    //            (location..X + insert.Location.X) * scaleFactor,
                    //            (location.Y + insert.Location.Y) * scaleFactor,
                    //            (location.Z + insert.Location.Z) * scaleFactor);
                    //        gccollection.AddRange(ParseEntities(dxfBlock.Entities, blocks, LayerName, insert_location, scaleFactor, true));
                    //    }
                    //}
                }
                else
                {
                    IGCObject obj = ParseObject(lentity[i], new DxfPoint());
                    if (obj != null)
                    {
                        gccollection.Add(obj);
                    }
                }

            }
            GCTools.SetProgress?.Invoke(0, 99, string.Empty);

            return gccollection;
        }

        private static IGCObject ParseObject(Entity entity, DxfPoint Location)
        {
            Debug.WriteLine(entity.ToString());
            switch (entity)
            {
                case Line line:
                    return null;
                default:
                    return null;
            }
        }

        private static void onNotification(object sender, NotificationEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static Point3D dwgPoint(XYZ point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }
    }
}

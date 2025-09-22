using ACadSharp;
using ACadSharp.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    public class DWG : GCFormat
    {
        public DWG() : base("DWG", new string[1] { ".dwg" }) { }

        public override Get ReadFile => GetAsync;

        private async Task<object> GetAsync(string filename)
        {
            CadDocument doc = DwgReader.Read(filename, onNotification);

            return null;
        }

        private static void onNotification(object sender, NotificationEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

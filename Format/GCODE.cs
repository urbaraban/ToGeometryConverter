using Oxage.Wmf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    internal class GCODE : GCFormat
    {
        public GCODE() : base("GCODE", new string[4] { ".gcode", ".mpt", ".mpf", ".nc" })
        {
            this.ReadFile = GetAsync;
        }


        public async Task<object> GetAsync(string Filename, double RoundStep)
        {
            PdfDocument pdfDocument = PdfReader.Open(Filename);

            foreach (PdfPage page in pdfDocument.Pages)
            {
                foreach (PdfContent content in page.Contents)
                {

                }
            }

            return null;
        }
    }
}


using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Threading.Tasks;
using ToGeometryConverter.Object;

namespace ToGeometryConverter.Format
{
    class PDF : IFormat
    {
        public string Name => "PDF";

        public string[] ShortName => new string[1] { "pdf" };

        public Tuple<int, int> Progress { get; private set; }

        public async Task<GCCollection> GetAsync(string Filename, double RoundStep)
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

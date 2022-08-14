using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System.Threading.Tasks;

namespace ToGeometryConverter.Format
{
    class PDF : GCFormat
    {
        public PDF() : base("PDF", new string[1] { ".pdf" }) 
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

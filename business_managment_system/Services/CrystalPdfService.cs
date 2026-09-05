using System.Data;

namespace business_managment_system.Services
{
    /// <summary>
    /// Kept so the project builds without SAP Crystal Reports.
    /// PDFs come from <see cref="TablePdfWriter"/>.
    /// </summary>
    public class CrystalPdfService
    {
        public byte[] ExportTable(DataTable table, string title, string templateName)
        {
            return TablePdfWriter.FromTable(table, title);
        }
    }
}

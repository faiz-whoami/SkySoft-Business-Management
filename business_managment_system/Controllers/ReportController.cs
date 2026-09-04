using System;
using System.Web.Mvc;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class ReportController : Controller
    {
        private readonly ReportService _reports;

        public ReportController()
            : this(new ReportService())
        {
        }

        public ReportController(ReportService reports)
        {
            _reports = reports;
        }

        public ActionResult Index(string partyType, int? transactionId, int? year)
        {
            ViewBag.Title = "Reports";
            return View(_reports.GetIndex(partyType, transactionId, year));
        }

        [HttpGet]
        public ActionResult PartyDirectory(string partyType)
        {
            return Pdf(
                () => _reports.PartyDirectoryPdf(partyType),
                "SkySoft-Partner-Directory.pdf");
        }

        [HttpGet]
        public ActionResult TransactionDetail(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                TempData["Error"] = "Enter a transaction ID.";
                return RedirectToAction("Index");
            }

            return Pdf(
                () => _reports.TransactionDetailPdf(id.Value),
                "SkySoft-Transaction-" + id.Value + ".pdf");
        }

        [HttpGet]
        public ActionResult MonthlySummary(int? year)
        {
            return Pdf(
                () => _reports.MonthlySummaryPdf(year),
                year.HasValue
                    ? "SkySoft-Monthly-Summary-" + year.Value + ".pdf"
                    : "SkySoft-Monthly-Summary.pdf");
        }

        private ActionResult Pdf(Func<byte[]> build, string fileName)
        {
            try
            {
                var bytes = build();
                return File(bytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}

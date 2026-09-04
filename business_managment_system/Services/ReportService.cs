using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reports;
        private readonly CrystalPdfService _crystal;

        public ReportService()
            : this(new ReportRepository(), new CrystalPdfService())
        {
        }

        public ReportService(ReportRepository reports, CrystalPdfService crystal)
        {
            _reports = reports;
            _crystal = crystal;
        }

        public ReportIndexViewModel GetIndex(string partyType, int? transactionId, int? year)
        {
            var years = _reports.GetSummaryYears();
            if (years.Count == 0)
            {
                years.Add(DateTime.Today.Year);
            }

            var crystalReady = _crystal.IsAvailable();
            return new ReportIndexViewModel
            {
                CrystalAvailable = crystalReady,
                CrystalMessage = crystalReady ? null : CrystalPdfService.MissingCrystalMessage(),
                PartyType = partyType,
                TransactionId = transactionId,
                Year = year,
                Years = years
            };
        }

        public byte[] PartyDirectoryPdf(string partyType)
        {
            var table = _reports.GetPartyDirectory(partyType);
            var title = string.IsNullOrWhiteSpace(partyType)
                ? "Business Partner Directory"
                : "Business Partner Directory — " + partyType.Trim();
            return _crystal.ExportTable(table, title, "PartyDirectory");
        }

        public byte[] TransactionDetailPdf(int transactionId)
        {
            var detail = _reports.GetTransactionDetail(transactionId);
            var table = ReportRepository.FlattenTransaction(detail);
            if (table.Rows.Count == 0)
            {
                throw new InvalidOperationException("Transaction was not found.");
            }

            return _crystal.ExportTable(table, "Transaction Detail #" + transactionId, "TransactionDetail");
        }

        public byte[] MonthlySummaryPdf(int? year)
        {
            var table = _reports.GetMonthlySummary(year);
            var title = year.HasValue
                ? "Monthly Transaction Summary — " + year.Value
                : "Monthly Transaction Summary";
            return _crystal.ExportTable(table, title, "MonthlySummary");
        }
    }
}

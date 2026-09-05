using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reports;

        public ReportService()
            : this(new ReportRepository())
        {
        }

        public ReportService(ReportRepository reports)
        {
            _reports = reports;
        }

        public ReportIndexViewModel GetIndex(string partyType, int? transactionId, int? year)
        {
            var years = _reports.GetSummaryYears();
            if (years.Count == 0)
            {
                years.Add(DateTime.Today.Year);
            }

            return new ReportIndexViewModel
            {
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
                : "Business Partner Directory - " + partyType.Trim();
            return TablePdfWriter.FromTable(table, title);
        }

        public byte[] TransactionDetailPdf(int transactionId)
        {
            var detail = _reports.GetTransactionDetail(transactionId);
            var table = ReportRepository.FlattenTransaction(detail);
            if (table.Rows.Count == 0)
            {
                throw new InvalidOperationException("Transaction was not found.");
            }

            return TablePdfWriter.FromTable(table, "Transaction Detail #" + transactionId);
        }

        public byte[] MonthlySummaryPdf(int? year)
        {
            var table = _reports.GetMonthlySummary(year);
            var title = year.HasValue
                ? "Monthly Transaction Summary - " + year.Value
                : "Monthly Transaction Summary";
            return TablePdfWriter.FromTable(table, title);
        }
    }
}

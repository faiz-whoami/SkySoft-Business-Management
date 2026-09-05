SkySoft Crystal Reports (.rpt)

These three files are the assignment report templates:

  PartyDirectory.rpt      FR-REPT-01  Business Partner Directory
  TransactionDetail.rpt   FR-REPT-02  Transaction Detail Statement
  MonthlySummary.rpt      FR-REPT-03  Monthly Transaction Summary

They are SAP Crystal Reports binary files. They cannot be written by hand.
This development PC does not have Crystal Reports installed, so the .rpt
files are created on the first PDF export on any machine that has
"SAP Crystal Reports, version for Visual Studio" (CRforVS / 13.0.4000.0).

After that first export, copy the three .rpt files from the web project's
Reports folder into source control. They will then open in the Crystal
Reports designer and be used by the MVC app.

Data each report must bind to (ADO.NET DataTable / SetDataSource):

1) PartyDirectory
   PartyType, PartyId, Name, Email, Phone, Address
   Source: dbo.sp_Report_PartyDirectory

2) TransactionDetail
   TransactionId, TransactionType, TransactionStatus, TransactionDate,
   PartyName, TotalAmount, RecordedBy, Description, Quantity, UnitPrice, LineTotal
   Source: dbo.sp_Report_TransactionDetail (flattened header + lines)

3) MonthlySummary
   TxnYear, TxnMonth, TransactionType, TransactionCount, TotalAmount
   Source: dbo.sp_Report_MonthlySummary

If Crystal is not installed, the site still downloads a PDF from TablePdfWriter
using the same stored procedures.

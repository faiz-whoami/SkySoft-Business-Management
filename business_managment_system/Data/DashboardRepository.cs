using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class DashboardRepository
    {
        public DashboardViewModel GetDashboard()
        {
            var model = new DashboardViewModel();
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);
            model.MonthLabel = monthStart.ToString("MMMM yyyy");

            using (var connection = DbConnectionFactory.Create())
            {
                connection.Open();
                LoadSummary(connection, model, monthStart, nextMonth);
                model.RecentTransactions = LoadRecentTransactions(connection);
            }

            return model;
        }

        private static void LoadSummary(SqlConnection connection, DashboardViewModel model, DateTime monthStart, DateTime nextMonth)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Customer WHERE IsActive = 1) AS ActiveCustomers,
                    (SELECT COUNT(*) FROM dbo.Supplier WHERE IsActive = 1) AS ActiveSuppliers,
                    (SELECT COUNT(*) FROM dbo.Vendor WHERE IsActive = 1) AS ActiveVendors,
                    ISNULL((
                        SELECT SUM(t.TotalAmount)
                        FROM dbo.[Transaction] t
                        INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                        INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                        WHERE tt.Name = N'Sale'
                          AND ts.Name <> N'Cancelled'
                          AND t.TransactionDate >= @MonthStart
                          AND t.TransactionDate < @NextMonth
                    ), 0) AS MonthSales;";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@MonthStart", monthStart);
                command.Parameters.AddWithValue("@NextMonth", nextMonth);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.ActiveCustomers = reader.GetInt32(0);
                        model.ActiveSuppliers = reader.GetInt32(1);
                        model.ActiveVendors = reader.GetInt32(2);
                        model.MonthSalesTotal = reader.GetDecimal(3);
                    }
                }
            }
        }

        private static IList<DashboardTransactionRow> LoadRecentTransactions(SqlConnection connection)
        {
            const string sql = @"
                SELECT TOP 5
                    t.TransactionId,
                    tt.Name AS TransactionType,
                    ts.Name AS StatusName,
                    t.TransactionDate,
                    COALESCE(c.Name, s.Name, v.Name) AS PartyName,
                    t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                LEFT JOIN dbo.Customer c ON c.CustomerId = t.CustomerId
                LEFT JOIN dbo.Supplier s ON s.SupplierId = t.SupplierId
                LEFT JOIN dbo.Vendor v ON v.VendorId = t.VendorId
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<DashboardTransactionRow>();
            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add(new DashboardTransactionRow
                    {
                        TransactionId = reader.GetInt32(0),
                        TransactionType = reader.GetString(1),
                        Status = reader.GetString(2),
                        TransactionDate = reader.GetDateTime(3),
                        PartyName = reader.IsDBNull(4) ? "—" : reader.GetString(4),
                        TotalAmount = reader.GetDecimal(5)
                    });
                }
            }

            return rows;
        }
    }
}

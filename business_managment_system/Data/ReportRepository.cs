using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace business_managment_system.Data
{
    public class ReportRepository
    {
        public DataTable GetPartyDirectory(string partyType)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand("dbo.sp_Report_PartyDirectory", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PartyType", (object)NullIfEmpty(partyType) ?? DBNull.Value);
                return FillTable(command, "PartyDirectory");
            }
        }

        public DataSet GetTransactionDetail(int transactionId)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand("dbo.sp_Report_TransactionDetail", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TransactionId", transactionId);
                var data = new DataSet("TransactionDetail");
                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(data);
                }

                if (data.Tables.Count > 0)
                {
                    data.Tables[0].TableName = "Header";
                }

                if (data.Tables.Count > 1)
                {
                    data.Tables[1].TableName = "Lines";
                }

                return data;
            }
        }

        public DataTable GetMonthlySummary(int? year)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand("dbo.sp_Report_MonthlySummary", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Year", (object)year ?? DBNull.Value);
                return FillTable(command, "MonthlySummary");
            }
        }

        public IList<int> GetSummaryYears()
        {
            const string sql = @"
                SELECT DISTINCT TxnYear
                FROM dbo.vw_MonthlyTransactionSummary
                ORDER BY TxnYear DESC;";

            var years = new List<int>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        years.Add(reader.GetInt32(0));
                    }
                }
            }

            return years;
        }

        public static DataTable FlattenTransaction(DataSet detail)
        {
            var table = new DataTable("TransactionDetail");
            table.Columns.Add("TransactionId", typeof(int));
            table.Columns.Add("TransactionType", typeof(string));
            table.Columns.Add("TransactionStatus", typeof(string));
            table.Columns.Add("TransactionDate", typeof(DateTime));
            table.Columns.Add("PartyName", typeof(string));
            table.Columns.Add("TotalAmount", typeof(decimal));
            table.Columns.Add("RecordedBy", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("UnitPrice", typeof(decimal));
            table.Columns.Add("LineTotal", typeof(decimal));

            if (detail == null || detail.Tables.Count == 0 || detail.Tables[0].Rows.Count == 0)
            {
                return table;
            }

            var header = detail.Tables[0].Rows[0];
            var lines = detail.Tables.Count > 1 ? detail.Tables[1] : null;
            if (lines == null || lines.Rows.Count == 0)
            {
                table.Rows.Add(
                    header["TransactionId"],
                    header["TransactionType"],
                    header["TransactionStatus"],
                    header["TransactionDate"],
                    header["PartyName"],
                    header["TotalAmount"],
                    header["RecordedBy"],
                    DBNull.Value,
                    DBNull.Value,
                    DBNull.Value,
                    DBNull.Value);
                return table;
            }

            foreach (DataRow line in lines.Rows)
            {
                table.Rows.Add(
                    header["TransactionId"],
                    header["TransactionType"],
                    header["TransactionStatus"],
                    header["TransactionDate"],
                    header["PartyName"],
                    header["TotalAmount"],
                    header["RecordedBy"],
                    line["Description"],
                    line["Quantity"],
                    line["UnitPrice"],
                    line["LineTotal"]);
            }

            return table;
        }

        private static DataTable FillTable(SqlCommand command, string tableName)
        {
            var table = new DataTable(tableName);
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(table);
            }

            table.TableName = tableName;
            return table;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

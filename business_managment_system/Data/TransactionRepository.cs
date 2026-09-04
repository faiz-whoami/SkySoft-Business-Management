using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class TransactionRepository
    {
        public int? GetTypeIdByName(string name)
        {
            const string sql = @"SELECT TransactionTypeId FROM dbo.TransactionType WHERE Name = @Name;";
            return QueryNullableInt(sql, name);
        }

        public int? GetStatusIdByName(string name)
        {
            const string sql = @"SELECT TransactionStatusId FROM dbo.TransactionStatus WHERE Name = @Name;";
            return QueryNullableInt(sql, name);
        }

        public IList<LookupOption> GetTypes()
        {
            return QueryLookups("SELECT TransactionTypeId, Name FROM dbo.TransactionType ORDER BY Name;");
        }

        public IList<LookupOption> GetStatuses()
        {
            return QueryLookups("SELECT TransactionStatusId, Name FROM dbo.TransactionStatus ORDER BY Name;");
        }

        public IList<PartyOption> GetActiveCustomers()
        {
            return QueryParties("SELECT CustomerId, Name FROM dbo.Customer WHERE IsActive = 1 ORDER BY Name;");
        }

        public IList<PartyOption> GetActiveSuppliers()
        {
            return QueryParties("SELECT SupplierId, Name FROM dbo.Supplier WHERE IsActive = 1 ORDER BY Name;");
        }

        public IList<PartyOption> GetActiveVendors()
        {
            return QueryParties("SELECT VendorId, Name FROM dbo.Vendor WHERE IsActive = 1 ORDER BY Name;");
        }

        public bool CustomerIsActive(int id)
        {
            return ExistsActive("SELECT COUNT(1) FROM dbo.Customer WHERE CustomerId = @Id AND IsActive = 1;", id);
        }

        public bool SupplierIsActive(int id)
        {
            return ExistsActive("SELECT COUNT(1) FROM dbo.Supplier WHERE SupplierId = @Id AND IsActive = 1;", id);
        }

        public bool VendorIsActive(int id)
        {
            return ExistsActive("SELECT COUNT(1) FROM dbo.Vendor WHERE VendorId = @Id AND IsActive = 1;", id);
        }

        public IList<ProductOption> GetActiveProducts()
        {
            const string sql = @"
                SELECT ProductId, SKU, ProductName, UnitPrice
                FROM dbo.Product
                WHERE IsActive = 1
                ORDER BY ProductName;";

            var items = new List<ProductOption>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new ProductOption
                        {
                            ProductId = reader.GetInt32(0),
                            Sku = reader.IsDBNull(1) ? null : reader.GetString(1),
                            ProductName = reader.GetString(2),
                            UnitPrice = reader.GetDecimal(3)
                        });
                    }
                }
            }

            return items;
        }

        public IList<TransactionListRow> Search(string term, string typeName, string statusName)
        {
            const string sql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, COALESCE(c.Name, s.Name, v.Name) AS PartyName,
                       u.FullName AS RecordedBy, t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                INNER JOIN dbo.EndUser u ON u.EndUserId = t.CreatedByUserId
                LEFT JOIN dbo.Customer c ON c.CustomerId = t.CustomerId
                LEFT JOIN dbo.Supplier s ON s.SupplierId = t.SupplierId
                LEFT JOIN dbo.Vendor v ON v.VendorId = t.VendorId
                WHERE (@TypeName IS NULL OR tt.Name = @TypeName)
                  AND (@StatusName IS NULL OR ts.Name = @StatusName)
                  AND (
                        @Term IS NULL
                        OR CAST(t.TransactionId AS NVARCHAR(20)) LIKE '%' + @Term + '%'
                        OR c.Name LIKE '%' + @Term + '%'
                        OR s.Name LIKE '%' + @Term + '%'
                        OR v.Name LIKE '%' + @Term + '%'
                      )
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<TransactionListRow>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TypeName", (object)NullIfEmpty(typeName) ?? DBNull.Value);
                command.Parameters.AddWithValue("@StatusName", (object)NullIfEmpty(statusName) ?? DBNull.Value);
                command.Parameters.AddWithValue("@Term", (object)NullIfEmpty(term) ?? DBNull.Value);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new TransactionListRow
                        {
                            TransactionId = reader.GetInt32(0),
                            TransactionType = reader.GetString(1),
                            Status = reader.GetString(2),
                            TransactionDate = reader.GetDateTime(3),
                            PartyName = reader.IsDBNull(4) ? "—" : reader.GetString(4),
                            RecordedBy = reader.GetString(5),
                            TotalAmount = reader.GetDecimal(6)
                        });
                    }
                }
            }

            return rows;
        }

        public TransactionDetailsViewModel GetDetails(int id)
        {
            const string headerSql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, COALESCE(c.Name, s.Name, v.Name) AS PartyName,
                       u.FullName AS RecordedBy, t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                INNER JOIN dbo.EndUser u ON u.EndUserId = t.CreatedByUserId
                LEFT JOIN dbo.Customer c ON c.CustomerId = t.CustomerId
                LEFT JOIN dbo.Supplier s ON s.SupplierId = t.SupplierId
                LEFT JOIN dbo.Vendor v ON v.VendorId = t.VendorId
                WHERE t.TransactionId = @TransactionId;";

            TransactionDetailsViewModel model = null;
            using (var connection = DbConnectionFactory.Create())
            {
                connection.Open();
                using (var command = new SqlCommand(headerSql, connection))
                {
                    command.Parameters.AddWithValue("@TransactionId", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        model = new TransactionDetailsViewModel
                        {
                            TransactionId = reader.GetInt32(0),
                            TransactionType = reader.GetString(1),
                            Status = reader.GetString(2),
                            TransactionDate = reader.GetDateTime(3),
                            PartyName = reader.IsDBNull(4) ? "—" : reader.GetString(4),
                            RecordedBy = reader.GetString(5),
                            TotalAmount = reader.GetDecimal(6)
                        };
                    }
                }

                model.Lines = GetLines(connection, id);
            }

            return model;
        }

        public int CreateWithItems(
            int transactionTypeId,
            int? customerId,
            int? supplierId,
            int? vendorId,
            int createdByUserId,
            DateTime? transactionDate,
            string itemsJson)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand("dbo.sp_CreateTransactionWithItems", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TransactionTypeId", transactionTypeId);
                command.Parameters.AddWithValue("@CustomerId", (object)customerId ?? DBNull.Value);
                command.Parameters.AddWithValue("@SupplierId", (object)supplierId ?? DBNull.Value);
                command.Parameters.AddWithValue("@VendorId", (object)vendorId ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedByUserId", createdByUserId);
                command.Parameters.Add("@ItemsJson", SqlDbType.NVarChar, -1).Value = itemsJson ?? "[]";
                command.Parameters.AddWithValue("@TransactionDate", (object)transactionDate ?? DBNull.Value);
                var newId = command.Parameters.Add("@NewTransactionId", SqlDbType.Int);
                newId.Direction = ParameterDirection.Output;

                connection.Open();
                command.ExecuteNonQuery();
                return (int)newId.Value;
            }
        }

        public void SetStatus(int transactionId, int statusId)
        {
            const string sql = @"
                UPDATE dbo.[Transaction]
                SET TransactionStatusId = @StatusId,
                    ModifiedDate = SYSDATETIME()
                WHERE TransactionId = @TransactionId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TransactionId", transactionId);
                command.Parameters.AddWithValue("@StatusId", statusId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static IList<TransactionLineRow> GetLines(SqlConnection connection, int transactionId)
        {
            const string sql = @"
                SELECT TransactionItemId, ProductId, Description, Quantity, UnitPrice, LineTotal
                FROM dbo.TransactionItem
                WHERE TransactionId = @TransactionId
                ORDER BY TransactionItemId;";

            var lines = new List<TransactionLineRow>();
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TransactionId", transactionId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(new TransactionLineRow
                        {
                            TransactionItemId = reader.GetInt32(0),
                            ProductId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                            Description = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            UnitPrice = reader.GetDecimal(4),
                            LineTotal = reader.GetDecimal(5)
                        });
                    }
                }
            }

            return lines;
        }

        private static int? QueryNullableInt(string sql, string name)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Name", name ?? string.Empty);
                connection.Open();
                var result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        private static IList<LookupOption> QueryLookups(string sql)
        {
            var items = new List<LookupOption>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new LookupOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return items;
        }

        private static IList<PartyOption> QueryParties(string sql)
        {
            var items = new List<PartyOption>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new PartyOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return items;
        }

        private static bool ExistsActive(string sql, int id)
        {
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

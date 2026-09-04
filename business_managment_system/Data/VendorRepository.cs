using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class VendorRepository
    {
        public IList<Vendor> Search(string name, bool? isActive)
        {
            const string sql = @"
                SELECT VendorId, Name, Email, Phone, Address, ServiceType, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Vendor
                WHERE (@Name IS NULL OR Name LIKE '%' + @Name + '%')
                  AND (@IsActive IS NULL OR IsActive = @IsActive)
                ORDER BY Name;";

            var items = new List<Vendor>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Name", (object)NullIfEmpty(name) ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", (object)isActive ?? DBNull.Value);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(Map(reader));
                    }
                }
            }

            return items;
        }

        public Vendor GetById(int id)
        {
            const string sql = @"
                SELECT VendorId, Name, Email, Phone, Address, ServiceType, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Vendor
                WHERE VendorId = @VendorId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@VendorId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Create(Vendor vendor)
        {
            const string sql = @"
                INSERT INTO dbo.Vendor (Name, Email, Phone, Address, ServiceType, IsActive)
                VALUES (@Name, @Email, @Phone, @Address, @ServiceType, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                AddSaveParameters(command, vendor);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(Vendor vendor)
        {
            const string sql = @"
                UPDATE dbo.Vendor
                SET Name = @Name,
                    Email = @Email,
                    Phone = @Phone,
                    Address = @Address,
                    ServiceType = @ServiceType,
                    ModifiedDate = SYSDATETIME()
                WHERE VendorId = @VendorId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@VendorId", vendor.VendorId);
                AddSaveParameters(command, vendor);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE dbo.Vendor
                SET IsActive = @IsActive,
                    ModifiedDate = SYSDATETIME()
                WHERE VendorId = @VendorId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@VendorId", id);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IList<PartyTransactionRow> GetTransactions(int vendorId)
        {
            const string sql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                WHERE t.VendorId = @VendorId
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<PartyTransactionRow>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@VendorId", vendorId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new PartyTransactionRow
                        {
                            TransactionId = reader.GetInt32(0),
                            TransactionType = reader.GetString(1),
                            Status = reader.GetString(2),
                            TransactionDate = reader.GetDateTime(3),
                            TotalAmount = reader.GetDecimal(4)
                        });
                    }
                }
            }

            return rows;
        }

        private static void AddSaveParameters(SqlCommand command, Vendor vendor)
        {
            command.Parameters.AddWithValue("@Name", vendor.Name);
            command.Parameters.AddWithValue("@Email", (object)NullIfEmpty(vendor.Email) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object)NullIfEmpty(vendor.Phone) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object)NullIfEmpty(vendor.Address) ?? DBNull.Value);
            command.Parameters.AddWithValue("@ServiceType", (object)NullIfEmpty(vendor.ServiceType) ?? DBNull.Value);
        }

        private static Vendor Map(IDataRecord reader)
        {
            return new Vendor
            {
                VendorId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                ServiceType = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsActive = reader.GetBoolean(6),
                CreatedDate = reader.GetDateTime(7),
                ModifiedDate = reader.GetDateTime(8)
            };
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

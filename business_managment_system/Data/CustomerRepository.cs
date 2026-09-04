using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class CustomerRepository
    {
        public IList<Customer> Search(string name, bool? isActive)
        {
            const string sql = @"
                SELECT CustomerId, Name, Email, Phone, Address, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Customer
                WHERE (@Name IS NULL OR Name LIKE '%' + @Name + '%')
                  AND (@IsActive IS NULL OR IsActive = @IsActive)
                ORDER BY Name;";

            var items = new List<Customer>();
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

        public Customer GetById(int id)
        {
            const string sql = @"
                SELECT CustomerId, Name, Email, Phone, Address, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Customer
                WHERE CustomerId = @CustomerId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public int Create(Customer customer)
        {
            const string sql = @"
                INSERT INTO dbo.Customer (Name, Email, Phone, Address, IsActive)
                VALUES (@Name, @Email, @Phone, @Address, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                AddSaveParameters(command, customer);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(Customer customer)
        {
            const string sql = @"
                UPDATE dbo.Customer
                SET Name = @Name,
                    Email = @Email,
                    Phone = @Phone,
                    Address = @Address,
                    ModifiedDate = SYSDATETIME()
                WHERE CustomerId = @CustomerId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                AddSaveParameters(command, customer);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE dbo.Customer
                SET IsActive = @IsActive,
                    ModifiedDate = SYSDATETIME()
                WHERE CustomerId = @CustomerId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", id);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IList<CustomerTransactionRow> GetTransactions(int customerId)
        {
            const string sql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                WHERE t.CustomerId = @CustomerId
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<CustomerTransactionRow>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new CustomerTransactionRow
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

        private static void AddSaveParameters(SqlCommand command, Customer customer)
        {
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Email", (object)NullIfEmpty(customer.Email) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object)NullIfEmpty(customer.Phone) ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object)NullIfEmpty(customer.Address) ?? DBNull.Value);
        }

        private static Customer Map(IDataRecord reader)
        {
            return new Customer
            {
                CustomerId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive = reader.GetBoolean(5),
                CreatedDate = reader.GetDateTime(6),
                ModifiedDate = reader.GetDateTime(7)
            };
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

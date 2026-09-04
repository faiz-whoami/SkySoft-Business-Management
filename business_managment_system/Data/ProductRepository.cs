using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class ProductRepository
    {
        public IList<Product> Search(string term, bool? isActive)
        {
            const string sql = @"
                SELECT ProductId, SKU, ProductName, UnitPrice, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Product
                WHERE (@Term IS NULL OR ProductName LIKE '%' + @Term + '%' OR SKU LIKE '%' + @Term + '%')
                  AND (@IsActive IS NULL OR IsActive = @IsActive)
                ORDER BY ProductName;";

            var items = new List<Product>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Term", (object)NullIfEmpty(term) ?? DBNull.Value);
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

        public Product GetById(int id)
        {
            const string sql = @"
                SELECT ProductId, SKU, ProductName, UnitPrice, IsActive, CreatedDate, ModifiedDate
                FROM dbo.Product
                WHERE ProductId = @ProductId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProductId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public bool SkuExists(string sku, int? excludeProductId)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return false;
            }

            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Product
                WHERE SKU = @SKU
                  AND (@ExcludeId IS NULL OR ProductId <> @ExcludeId);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SKU", sku.Trim());
                command.Parameters.AddWithValue("@ExcludeId", (object)excludeProductId ?? DBNull.Value);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int Create(Product product)
        {
            const string sql = @"
                INSERT INTO dbo.Product (SKU, ProductName, UnitPrice, IsActive)
                VALUES (@SKU, @ProductName, @UnitPrice, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                AddSaveParameters(command, product);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(Product product)
        {
            const string sql = @"
                UPDATE dbo.Product
                SET SKU = @SKU,
                    ProductName = @ProductName,
                    UnitPrice = @UnitPrice,
                    ModifiedDate = SYSDATETIME()
                WHERE ProductId = @ProductId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProductId", product.ProductId);
                AddSaveParameters(command, product);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE dbo.Product
                SET IsActive = @IsActive,
                    ModifiedDate = SYSDATETIME()
                WHERE ProductId = @ProductId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProductId", id);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IList<ProductUsageRow> GetUsages(int productId)
        {
            const string sql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, ti.Quantity, ti.UnitPrice, ti.LineTotal
                FROM dbo.TransactionItem ti
                INNER JOIN dbo.[Transaction] t ON t.TransactionId = ti.TransactionId
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                WHERE ti.ProductId = @ProductId
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<ProductUsageRow>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProductId", productId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new ProductUsageRow
                        {
                            TransactionId = reader.GetInt32(0),
                            TransactionType = reader.GetString(1),
                            Status = reader.GetString(2),
                            TransactionDate = reader.GetDateTime(3),
                            Quantity = reader.GetInt32(4),
                            UnitPrice = reader.GetDecimal(5),
                            LineTotal = reader.GetDecimal(6)
                        });
                    }
                }
            }

            return rows;
        }

        private static void AddSaveParameters(SqlCommand command, Product product)
        {
            command.Parameters.AddWithValue("@SKU", (object)NullIfEmpty(product.Sku) ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProductName", product.ProductName);
            command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
        }

        private static Product Map(IDataRecord reader)
        {
            return new Product
            {
                ProductId = reader.GetInt32(0),
                Sku = reader.IsDBNull(1) ? null : reader.GetString(1),
                ProductName = reader.GetString(2),
                UnitPrice = reader.GetDecimal(3),
                IsActive = reader.GetBoolean(4),
                CreatedDate = reader.GetDateTime(5),
                ModifiedDate = reader.GetDateTime(6)
            };
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}

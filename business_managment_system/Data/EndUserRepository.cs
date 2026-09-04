using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using business_managment_system.Models;

namespace business_managment_system.Data
{
    public class EndUserRepository
    {
        public EndUser GetByUsername(string username)
        {
            const string sql = @"
                SELECT u.EndUserId, u.Username, u.PasswordHash, u.FullName, u.IsActive, r.RoleName
                FROM dbo.EndUser u
                INNER JOIN dbo.Role r ON r.RoleId = u.RoleId
                WHERE u.Username = @Username";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", username ?? string.Empty);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new EndUser
                    {
                        EndUserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        FullName = reader.GetString(3),
                        IsActive = reader.GetBoolean(4),
                        RoleName = reader.GetString(5)
                    };
                }
            }
        }

        public IList<EndUser> Search(string term, bool? isActive)
        {
            const string sql = @"
                SELECT u.EndUserId, u.Username, u.FullName, u.RoleId, r.RoleName,
                       u.IsActive, u.CreatedDate, u.ModifiedDate
                FROM dbo.EndUser u
                INNER JOIN dbo.Role r ON r.RoleId = u.RoleId
                WHERE (@Term IS NULL OR u.Username LIKE '%' + @Term + '%' OR u.FullName LIKE '%' + @Term + '%')
                  AND (@IsActive IS NULL OR u.IsActive = @IsActive)
                ORDER BY u.FullName;";

            var items = new List<EndUser>();
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
                        items.Add(MapDirectory(reader));
                    }
                }
            }

            return items;
        }

        public EndUser GetById(int id)
        {
            const string sql = @"
                SELECT u.EndUserId, u.Username, u.FullName, u.RoleId, r.RoleName,
                       u.IsActive, u.CreatedDate, u.ModifiedDate
                FROM dbo.EndUser u
                INNER JOIN dbo.Role r ON r.RoleId = u.RoleId
                WHERE u.EndUserId = @EndUserId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EndUserId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapDirectory(reader) : null;
                }
            }
        }

        public bool UsernameExists(string username, int? excludeUserId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.EndUser
                WHERE Username = @Username
                  AND (@ExcludeId IS NULL OR EndUserId <> @ExcludeId);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", username.Trim());
                command.Parameters.AddWithValue("@ExcludeId", (object)excludeUserId ?? DBNull.Value);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public IList<RoleOption> GetRoles()
        {
            const string sql = @"SELECT RoleId, RoleName FROM dbo.Role ORDER BY RoleName;";
            var roles = new List<RoleOption>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new RoleOption
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        });
                    }
                }
            }

            return roles;
        }

        public int CountActiveAdmins()
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.EndUser u
                INNER JOIN dbo.Role r ON r.RoleId = u.RoleId
                WHERE r.RoleName = N'Admin' AND u.IsActive = 1;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Create(EndUser user)
        {
            const string sql = @"
                INSERT INTO dbo.EndUser (Username, PasswordHash, FullName, RoleId, IsActive)
                VALUES (@Username, @PasswordHash, @FullName, @RoleId, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(EndUser user, bool updatePassword)
        {
            var sql = updatePassword
                ? @"
                    UPDATE dbo.EndUser
                    SET Username = @Username,
                        FullName = @FullName,
                        RoleId = @RoleId,
                        PasswordHash = @PasswordHash,
                        ModifiedDate = SYSDATETIME()
                    WHERE EndUserId = @EndUserId;"
                : @"
                    UPDATE dbo.EndUser
                    SET Username = @Username,
                        FullName = @FullName,
                        RoleId = @RoleId,
                        ModifiedDate = SYSDATETIME()
                    WHERE EndUserId = @EndUserId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EndUserId", user.EndUserId);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@FullName", user.FullName);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                if (updatePassword)
                {
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE dbo.EndUser
                SET IsActive = @IsActive,
                    ModifiedDate = SYSDATETIME()
                WHERE EndUserId = @EndUserId;";

            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EndUserId", id);
                command.Parameters.AddWithValue("@IsActive", isActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IList<PartyTransactionRow> GetTransactions(int endUserId)
        {
            const string sql = @"
                SELECT t.TransactionId, tt.Name AS TransactionType, ts.Name AS StatusName,
                       t.TransactionDate, t.TotalAmount
                FROM dbo.[Transaction] t
                INNER JOIN dbo.TransactionType tt ON tt.TransactionTypeId = t.TransactionTypeId
                INNER JOIN dbo.TransactionStatus ts ON ts.TransactionStatusId = t.TransactionStatusId
                WHERE t.CreatedByUserId = @EndUserId
                ORDER BY t.TransactionDate DESC, t.TransactionId DESC;";

            var rows = new List<PartyTransactionRow>();
            using (var connection = DbConnectionFactory.Create())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EndUserId", endUserId);
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

        private static EndUser MapDirectory(IDataRecord reader)
        {
            return new EndUser
            {
                EndUserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                FullName = reader.GetString(2),
                RoleId = reader.GetInt32(3),
                RoleName = reader.GetString(4),
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

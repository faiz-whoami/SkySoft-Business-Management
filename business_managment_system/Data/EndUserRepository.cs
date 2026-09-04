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
    }
}

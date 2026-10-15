using CustomerApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;

namespace CustomerApi.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("dbo.usp_AppUser_GetByUsername", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new AppUser
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}

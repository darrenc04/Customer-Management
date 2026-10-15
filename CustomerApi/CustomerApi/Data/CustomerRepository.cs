using CustomerApi.Exceptions;
using CustomerApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CustomerApi.Data
{
    public class CustomerRepository
    {
        private const int DuplicateUsernameErrorNumber = 50001;
        private const int CustomerNotFoundErrorNumber = 50002;
        private const int UniqueConstraintErrorNumber = 2627;
        private const int UniqueIndexErrorNumber = 2601;

        private readonly string _connectionString;

        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public Task<List<Customer>> GetAllAsync()
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_GetAll");
                return await ReadCustomerAsync(connection, command);
            });
        }

        public Task<List<Customer>> SearchAsync(string searchText)
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_Search");
                command.Parameters.Add("@SearchText", SqlDbType.NVarChar, 100).Value = searchText;
                return await ReadCustomerAsync(connection, command);
            });
        }

        public Task<Customer> GetByIdAsync(int customerId)
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_GetById");
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = customerId;
                var customers = await ReadCustomerAsync(connection, command);
                return customers.FirstOrDefault();
            });
        }

        public Task<int> InsertAsync(Customer customer)
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_Insert");
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = customer.Username;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = customer.Age;
                command.Parameters.Add("@Contact", SqlDbType.NVarChar, 12).Value = customer.Contact;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = customer.Email;
                command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 128).Value = customer.CreatedBy;
                await connection.OpenAsync();

                return (int)await command.ExecuteScalarAsync();
            });
        }

        public Task UpdateAsync(Customer customer)
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_Update");
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = customer.CustomerID;
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = customer.Username;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = customer.Age;
                command.Parameters.Add("@Contact", SqlDbType.NVarChar, 12).Value = customer.Contact;
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = customer.Email;
                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
                return true;
            });
        }

        public Task DeleteAsync(int customerId)
        {
            return RunAsync(async () =>
            {
                await using var connection = new SqlConnection(_connectionString);
                await using var command = CreatedCommand(connection, "dbo.usp_Customer_Delete");
                command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = customerId;
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                return true;
            });
        }

        public static SqlCommand CreatedCommand(SqlConnection connection, string storedProcedureName)
        {
            return new SqlCommand(storedProcedureName, connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
        }

        private static async Task<T> RunAsync<T>(Func<Task<T>> databaseAction)
        {
            try
            {
                return await databaseAction();
            }
            catch (SqlException ex) when (ex.Number is DuplicateUsernameErrorNumber or UniqueConstraintErrorNumber or UniqueIndexErrorNumber)
            {
                throw new DuplicateUsernameException();
            }
            catch (SqlException ex) when (ex.Number == CustomerNotFoundErrorNumber)
            {
                throw new CustomerNotFoundException();
            }

        }

        private static async Task<List<Customer>> ReadCustomerAsync(SqlConnection connection, SqlCommand command)
        {
            var customers = new List<Customer>();
            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(MapCustomer(reader));
            }
            return customers;
        }

        private static Customer MapCustomer(SqlDataReader reader)
        {
            int lastUpdateOrdinal = reader.GetOrdinal("LastUpdate");
            return new Customer
            {
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                Contact = reader.GetString(reader.GetOrdinal("Contact")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastUpdate = reader.IsDBNull(lastUpdateOrdinal) ? null : reader.GetDateTime(lastUpdateOrdinal)
            };
        }
    }
}

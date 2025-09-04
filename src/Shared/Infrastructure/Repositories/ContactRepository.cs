using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using ContactModels;

namespace Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly string _connectionString;

        public ContactRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string não encontrada");
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<Contact?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Nome, DDD, NumeroCelular, Email, DataCriacao, DataAtualizacao 
                FROM Contacts 
                WHERE Id = @Id";

            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Contact>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Contact>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var offset = (page - 1) * pageSize;
            var whereClause = string.Empty;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause = @"WHERE Nome LIKE @Search 
                               OR Email LIKE @Search 
                               OR DDD LIKE @Search 
                               OR NumeroCelular LIKE @Search";
                parameters.Add("Search", $"%{search}%");
            }

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var sql = $@"
                SELECT Id, Nome, DDD, NumeroCelular, Email, DataCriacao, DataAtualizacao 
                FROM Contacts 
                {whereClause}
                ORDER BY DataCriacao DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Contact>(sql, parameters);
        }

        public async Task<int> GetTotalCountAsync(string? search = null)
        {
            var whereClause = string.Empty;
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause = @"WHERE Nome LIKE @Search 
                               OR Email LIKE @Search 
                               OR DDD LIKE @Search 
                               OR NumeroCelular LIKE @Search";
                parameters.Add("Search", $"%{search}%");
            }

            var sql = $"SELECT COUNT(*) FROM Contacts {whereClause}";

            using var connection = CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }

        public async Task<Contact> CreateAsync(Contact contact)
        {
            const string sql = @"
                INSERT INTO Contacts (Nome, DDD, NumeroCelular, Email, DataCriacao)
                OUTPUT INSERTED.Id, INSERTED.Nome, INSERTED.DDD, INSERTED.NumeroCelular, 
                       INSERTED.Email, INSERTED.DataCriacao, INSERTED.DataAtualizacao
                VALUES (@Nome, @DDD, @NumeroCelular, @Email, @DataCriacao)";

            using var connection = CreateConnection();
            return await connection.QuerySingleAsync<Contact>(sql, contact);
        }

        public async Task<Contact> UpdateAsync(Contact contact)
        {
            const string sql = @"
                UPDATE Contacts 
                SET Nome = @Nome, 
                    DDD = @DDD, 
                    NumeroCelular = @NumeroCelular, 
                    Email = @Email, 
                    DataAtualizacao = @DataAtualizacao
                OUTPUT INSERTED.Id, INSERTED.Nome, INSERTED.DDD, INSERTED.NumeroCelular, 
                       INSERTED.Email, INSERTED.DataCriacao, INSERTED.DataAtualizacao
                WHERE Id = @Id";

            using var connection = CreateConnection();
            return await connection.QuerySingleAsync<Contact>(sql, contact);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Contacts WHERE Id = @Id";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Contacts WHERE Id = @Id";

            using var connection = CreateConnection();
            var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<Contact?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT Id, Nome, DDD, NumeroCelular, Email, DataCriacao, DataAtualizacao 
                FROM Contacts 
                WHERE Email = @Email";

            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Contact>(sql, new { Email = email });
        }
    }
}


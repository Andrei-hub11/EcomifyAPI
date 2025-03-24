using System.Data;

using Microsoft.Extensions.Configuration;

using Npgsql;

namespace EcomifyAPI.Infrastructure.Data;

internal class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration, string? connectionString = null)
    {
        _connectionString = connectionString ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration),
            "Connection string 'DefaultConnection' not found.");
    }
    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}
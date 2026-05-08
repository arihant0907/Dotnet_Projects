using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using DeviceHealthService.Domain.Entities;
using DeviceHealthService.Domain.Interfaces;
using MySql.Data.MySqlClient;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public DbConnectionFactory(IOptions<DatabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IDbConnection> CreateConnectionAsync(string? tenantId = null)
    {
        var connectionString = ResolveConnectionString(tenantId);

        IDbConnection connection = _settings.Provider switch
        {
            "SqlServer" => new SqlConnection(connectionString),
            "MySql" => new MySqlConnection(connectionString),
            _ => throw new NotSupportedException("Unsupported DB provider")
        };

        if (connection.State != ConnectionState.Open) 
            await ((dynamic)connection).OpenAsync();

        Console.WriteLine($"Created or opend connection");

        return connection;
    }

    private string ResolveConnectionString(string? tenantId)
    {
        if (!string.IsNullOrEmpty(tenantId) &&
            _settings.ConnectionStrings.ContainsKey(tenantId))
        {
            return _settings.ConnectionStrings[tenantId];
        }

        return _settings.ConnectionStrings["Default"];
    }
}
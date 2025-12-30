using System.Data;
using Dapper;
using Npgsql;

namespace ProductOutboxPublisher.Service;

public static class ProductOutboxSingletonDatabase
{
    private static string? _connectionString;
    private static bool _dataReaderState = true;

    static ProductOutboxSingletonDatabase()
    {
        _connectionString = Environment.GetEnvironmentVariable("ProductOutboxConnection");
    }

    public static bool DataReaderState => _dataReaderState;

    public static void Initialize(string connectionString) =>
        _connectionString = connectionString;

    private static NpgsqlConnection CreateConnection() => new(_connectionString);

    public static async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        await using var connection = CreateConnection();
        return await connection.QueryAsync<T>(sql, param);
    }

    public static async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        await using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    public static void DataReaderReady()
    {
        _dataReaderState = true;
    }
    
    public static void DataReaderBusy()
    {
        _dataReaderState = false;
    }
}
using System.Data;
using System.Data.SqlClient;
using Application.Common.Abstractions;

namespace Infrastructure.Persistence;

internal sealed class SqlConnectionFactory(string connectionString)
    : ISqlConnectionFactory, IDisposable
{
    private IDbConnection _connection;
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateNewConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection;
    }

    public void Dispose()
    {
        if (_connection is not null && _connection.State is ConnectionState.Open)
        {
            _connection.Dispose();
        }
    }

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public IDbConnection GetOpenConnection()
    {
        if (_connection is null || _connection.State is not ConnectionState.Open)
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    public IDbTransaction BeginTransaction()
    {
        if (_connection is null || _connection.State is not ConnectionState.Open)
            throw new InvalidOperationException("Connection must be open to begin a transaction.");

        return _connection.BeginTransaction();
    }

    public void CommitTransaction(IDbTransaction transaction)
    {
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));

        transaction.Commit();
    }

    public void RollbackTransaction(IDbTransaction transaction)
    {
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));

        transaction.Rollback();
    }

}

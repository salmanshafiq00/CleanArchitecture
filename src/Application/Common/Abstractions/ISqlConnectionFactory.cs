using System.Data;

namespace Application.Common.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();

    IDbConnection CreateNewConnection();

    string GetConnectionString();

    IDbTransaction BeginTransaction();

    void CommitTransaction(IDbTransaction transaction);

    void RollbackTransaction(IDbTransaction transaction);


}

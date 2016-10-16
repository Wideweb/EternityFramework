using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EternityFramework.DataAccess
{
    public interface IDbQuery : IDisposable
    {
        SqlTransaction BeginTransaction();
        Task ExecuteCommandAsync(string queryString, params SqlParameter[] parameters);
        Task<long> ExecuteInsertCommandAsync(string queryString, params SqlParameter[] parameters);
        Task<T> ExecuteQueryAsync<T>(string queryString, params SqlParameter[] parameters) where T : struct;
        Task<T> ExecuteReaderAsync<T>(string queryString, Func<SqlDataReader, T> readFunc, params SqlParameter[] parameters);
        bool TableExists(string tableName);
    }
}

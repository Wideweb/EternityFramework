﻿using EternityFramework.Utils;
using System;
using System.Collections;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EternityFramework.DataAccess
{
    public class DbQuery : IDbQuery
    {
        private SqlTransaction currentTransaction;
        private SqlConnection connection;
        private bool disposed = false;

        public DbQuery(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        public SqlTransaction BeginTransaction()
        {
            currentTransaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
            return currentTransaction;
        }

        public object Get(string sqlQuery, Type type)
        {
            if (type.GetTypeInfo().IsValueType || type == typeof(String))
            {
                return ExecuteQuery(sqlQuery);
            }

            if (TypeSystem.IsList(type))
            {
                var elementType = TypeSystem.GetElementType(type);
                return ExecuteReader(sqlQuery, sdr =>
                {
                    var data = TypeSystem.CreateList(elementType);
                    while (sdr.Read())
                    {
                        data.Add(Load(sdr, elementType));
                    }
                    return data;
                });
            }

            return ExecuteReader(sqlQuery, sdr =>
            {
                if (sdr.Read())
                {
                    return Load(sdr, type);
                }
                return null;
            });
        }

        public T Get<T>(string sqlQuery)
        {
            return (T)Get(sqlQuery, typeof(T));
        }

        public Task ExecuteCommandAsync(string queryString, params SqlParameter[] parameters)
        {
            return ExecuteAsync(queryString, command =>
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQueryAsync();
            });
        }

        public void ExecuteCommand(string queryString, params SqlParameter[] parameters)
        {
            Execute(queryString, command =>
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            });
        }

        public Task<long> ExecuteInsertCommandAsync(string queryString, params SqlParameter[] parameters)
        {
            queryString += "; SELECT CAST(scope_identity() AS bigint)";
            return ExecuteAsync(queryString, async command =>
            {
                command.Parameters.AddRange(parameters);
                return (long)await command.ExecuteScalarAsync();
            });
        }

        public Task<T> ExecuteQueryAsync<T>(string queryString, params SqlParameter[] parameters) where T : struct
        {
            return ExecuteAsync(queryString, async command =>
            {
                command.Parameters.AddRange(parameters);
                return (T)await command.ExecuteScalarAsync();
            });
        }

        public T ExecuteQuery<T>(string queryString, params SqlParameter[] parameters) where T : struct
        {
            return Execute(queryString, command =>
            {
                command.Parameters.AddRange(parameters);
                return (T)command.ExecuteScalar();
            });
        }

        public Task<object> ExecuteQueryAsync(string queryString, params SqlParameter[] parameters)
        {
            return ExecuteAsync(queryString, async command =>
            {
                command.Parameters.AddRange(parameters);
                return await command.ExecuteScalarAsync();
            });
        }

        public object ExecuteQuery(string queryString, params SqlParameter[] parameters)
        {
            return Execute(queryString, command =>
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            });
        }

        public Task<T> ExecuteReaderAsync<T>(string queryString, Func<SqlDataReader, T> readFunc, params SqlParameter[] parameters)
        {
            return ExecuteAsync(queryString, async command =>
            {
                command.Parameters.AddRange(parameters);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    return readFunc(reader);
                }
            });
        }

        public T ExecuteReader<T>(string queryString, Func<SqlDataReader, T> readFunc, params SqlParameter[] parameters)
        {
            return Execute(queryString, command =>
            {
                command.Parameters.AddRange(parameters);
                using (var reader = command.ExecuteReader())
                {
                    return readFunc(reader);
                }
            });
        }

        private async Task<T> ExecuteAsync<T>(string queryString, Func<SqlCommand, Task<T>> func)
        {
            if (currentTransaction != null && currentTransaction.Connection != null)
            {
                return await ExecuteAsync(queryString, func, currentTransaction);
            }

            using (var transaction = BeginTransaction())
            {
                try
                {
                    var result = await ExecuteAsync(queryString, func, transaction);
                    transaction.Commit();
                    return result;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        private T Execute<T>(string queryString, Func<SqlCommand, T> func)
        {
            if (currentTransaction != null && currentTransaction.Connection != null)
            {
                return Execute(queryString, func, currentTransaction);
            }

            using (var transaction = BeginTransaction())
            {
                try
                {
                    var result = Execute(queryString, func, transaction);
                    transaction.Commit();
                    return result;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        private Task<T> ExecuteAsync<T>(string queryString, Func<SqlCommand, Task<T>> func, SqlTransaction transaction)
        {
            var command = new SqlCommand(queryString, transaction.Connection);
            command.Transaction = transaction;
            return func(command);
        }

        private T Execute<T>(string queryString, Func<SqlCommand, T> func, SqlTransaction transaction)
        {
            var command = new SqlCommand(queryString, transaction.Connection);
            command.Transaction = transaction;
            return func(command);
        }

        public bool TableExists(string tableName)
        {
            bool exists;

            try
            {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                var cmd = $"select case when exists((select * from information_schema.tables where table_name = '{tableName}')) then 1 else 0 end";
                exists = ExecuteQuery<int>(cmd) > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
                try
                {
                    // Other RDBMS.  Graceful degradation
                    exists = true;
                    var cmdOthers = $"select 1 from '{tableName}' where 1 = 0";
                    ExecuteCommand(cmdOthers);
                }
                catch
                {
                    exists = false;
                }
            }

            return exists;
        }

        public T Load<T>(SqlDataReader sqlDataReader)
        {
            return (T)Load(sqlDataReader, typeof(T));
        }

        public object Load(SqlDataReader sqlDataReader, Type type)
        {
            try
            {
                var instance = Activator.CreateInstance(type);
                var properties = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(it => it.CanWrite);

                foreach (var property in properties)
                {
                    var value = sqlDataReader[property.Name];
                    property.SetValue(instance, value == DBNull.Value ? null : value);
                }
                return instance;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                if (currentTransaction != null)
                {
                    currentTransaction.Dispose();
                }
                connection.Dispose();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~DbQuery()
        {
            Dispose(false);
        }
    }
}

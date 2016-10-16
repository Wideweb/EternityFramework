using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace EternityFramework.DataAccess.Gateway
{
    public abstract class RowDataGatewayBase
    {
        public long Id { get; set; }

        protected DbQuery dbQuery { get; set; }

        public RowDataGatewayBase(DbQuery dbQuery)
        {
            this.dbQuery = dbQuery;
        }

        public virtual void Save()
        {
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(it => it.Name != nameof(Id));

            var parameters = properties
                .Select(it => new SqlParameter
                {
                    ParameterName = $"@{it.Name}",
                    Value = it.GetValue(this, null)
                });

            var tableName = RowDataGatewayHelper.GetTableName(GetType());
            var propertiesString = string.Join(", ", properties.Select(it => it.Name));
            var parametersString = string.Join(", ", parameters.Select(it => it.ParameterName));

            dbQuery.ExecuteCommand($"INSERT INTO {tableName}({propertiesString}) VALUES({parametersString})", parameters.ToArray());
        }

        public virtual void Delete()
        {
            var tableName = RowDataGatewayHelper.GetTableName(GetType());
            dbQuery.ExecuteCommand($"DELETE FROM {tableName} WHERE {nameof(Id)} = @{nameof(Id)}", new SqlParameter {
                ParameterName = $"@{nameof(Id)}",
                Value = Id
            });
        }

        public virtual bool Load(SqlDataReader sqlDataReader)
        {
            if (!sqlDataReader.Read())
            {
                return false;
            }

            try
            {
                var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    property.SetValue(this, sqlDataReader[property.Name]);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}

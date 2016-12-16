using EternityFramework.DataComponents.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace EternityFramework.Utils
{
    public static class DbEntityHelper
    {
        public static IEnumerable<PropertyInfo> GetProperties(Type entityType, bool excludeId = true)
        {
            return entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(it => it.CanWrite)
                .Where(it => it.Name != "Id" || !excludeId);
        }

        public static IEnumerable<SqlParameter> GetSqlParameters<TData>(TData entity, bool excludeId = true)
        {
            return GetProperties(entity.GetType(), excludeId)
                .Select(it => new SqlParameter
                {
                    ParameterName = $"@{it.Name}",
                    Value = it.GetValue(entity, null) ?? DBNull.Value
                });
        }

        public static string GetPropertiesString(Type entityType)
        {
            return string.Join(", ", GetProperties(entityType).Select(it => it.Name));
        }

        public static string GetUpdatePropertiesString<TData>(TData entity)
        {
            var pairs = GetProperties(entity.GetType())
                .Zip(GetSqlParameters(entity), (prop, param) => $"{prop.Name}={param.ParameterName}");

            return string.Join(", ", pairs);
        }

        public static string GetSqlParametersString<TData>(TData entity)
        {
            return string.Join(", ", GetSqlParameters(entity).Select(it => it.ParameterName));
        }

        public static string GetDbTableName(Type entityType)
        {
            var attr = entityType.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            return attr == null ? entityType.Name : attr.Name;
        }
    }
}

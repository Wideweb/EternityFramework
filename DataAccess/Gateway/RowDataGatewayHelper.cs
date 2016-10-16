using EternityFramework.DataComponents.DataAnnotations;
using System;
using System.Reflection;

namespace EternityFramework.DataAccess.Gateway
{
    internal static class RowDataGatewayHelper
    {
        public static string GetTableName(Type gatewayType)
        {
            if (!typeof(RowDataGatewayBase).IsAssignableFrom(gatewayType))
            {
                throw new ArgumentException($"Wrong gateway type {gatewayType.Name}");
            }

            var attr = gatewayType.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            return attr == null ? gatewayType.Name : attr.Name;
        }
    }
}

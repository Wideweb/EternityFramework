using EternityFramework.Migrations.Attributes;
using System;
using System.Reflection;

namespace EternityFramework.Migrations.Services
{
    internal static class DbMigrationHelper
    {
        public static long GetMigrationOrder(Type migrationType)
        {
            if (!typeof(DbMigration).IsAssignableFrom(migrationType))
            {
                throw new ArgumentException($"Wrong gateway type {migrationType.Name}");
            }

            var attr = migrationType.GetTypeInfo().GetCustomAttribute<MigrationOrderAttribute>();

            if (attr == null)
            {
                throw new ArgumentException($"Migration order is not specified for {migrationType.Name} migration");
            }

            return attr.MigrationOrder;
        }
    }
}

using DbMigratorCore.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace EternityFramework.Migrations.Services
{
    internal class MigrationProvider
    {
        private string migrationsAssemblyName;

        public MigrationProvider(string migrationsAssemblyName)
        {
            this.migrationsAssemblyName = migrationsAssemblyName;
        }

        public IOrderedEnumerable<Type> GetMigrationsAfter(long migrationOrder)
        {
            return GetMigrations(it => DbMigrationHelper.GetMigrationOrder(it) > migrationOrder);
        }

        public IOrderedEnumerable<Type> GetMigrationsBefore(long migrationOrder)
        {
            return GetMigrations(it => DbMigrationHelper.GetMigrationOrder(it) < migrationOrder);
        }

        public IOrderedEnumerable<Type> GetMigrations(Func<Type, bool> predicate)
        {
            return LoadAssembly()
                .GetTypes()
                .Where(it => typeof(DbMigration).IsAssignableFrom(it) && predicate(it))
                .OrderBy(it => DbMigrationHelper.GetMigrationOrder(it));
        }

        private Assembly LoadAssembly()
        {
            return new AssemblyLoader().LoadFromAssemblyPath(migrationsAssemblyName);
        }
    }
}

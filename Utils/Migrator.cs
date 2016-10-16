using EternityFramework.DataAccess;
using EternityFramework.DataAccess.Gateway;
using EternityFramework.Migrations;
using EternityFramework.Migrations.ModelGateway;
using EternityFramework.Migrations.Services;
using System;
using System.Linq;

namespace EternityFramework.Utils
{
    public class Migrator
    {
        private readonly DbQuery dbQuery;
        private readonly MigrationProvider migrationProvider;
        private readonly GatewayFinder gatewayFinder;

        public Migrator(string migrationsAssemblyName, string connectionString)
        {
            dbQuery = new DbQuery(connectionString);
            migrationProvider = new MigrationProvider(migrationsAssemblyName);
            gatewayFinder = new GatewayFinder(dbQuery);
        }

        public void ApplyMigrations()
        {
            long lastMigrationOrder = 0;
            if(!dbQuery.TableExists(RowDataGatewayHelper.GetTableName(typeof(MigrationHistoryGateway))))
            {
                new AddedMigrationHistoryEntity { DbQuery = dbQuery }.Up();
            }
            else
            {
                lastMigrationOrder = GetLastMigrationOrder();
            }

            var migrations = migrationProvider.GetMigrationsAfter(lastMigrationOrder);
            HandleMigrations(migrations, (migration, type) =>
            {
                migration.Up();
                new MigrationHistoryGateway(dbQuery)
                {
                    MigrationName = type.Name,
                    MigrationOrder = DbMigrationHelper.GetMigrationOrder(type)
                }.Save();
            });
        }

        public void Downgrade(long toMigrationOrder)
        {
            if (!dbQuery.TableExists(RowDataGatewayHelper.GetTableName(typeof(MigrationHistoryGateway))))
            {
                throw new InvalidOperationException("Migration history wasn't found.");
            }

            var migrations = migrationProvider.GetMigrationsAfter(toMigrationOrder);
            HandleMigrations(migrations, (migration, type) =>
            {
                migration.Down();
                gatewayFinder.FindAndDeleteMigrationsAfter(toMigrationOrder);
            });
        }

        private void HandleMigrations(IOrderedEnumerable<Type> migrations, Action<DbMigration, Type> commnad)
        {
            using (var transaction = dbQuery.BeginTransaction())
            {
                try
                {
                    foreach (var migration in migrations)
                    {
                        var migrationInstance = (DbMigration)Activator.CreateInstance(migration);
                        migrationInstance.DbQuery = dbQuery;
                        commnad(migrationInstance, migration);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    transaction.Rollback();
                }
            }
        }

        private long GetLastMigrationOrder()
        {
            var lastAppliedMigration = gatewayFinder.FindLastMigration();
            return lastAppliedMigration == null ? -1 : lastAppliedMigration.MigrationOrder;
        }
    }
}

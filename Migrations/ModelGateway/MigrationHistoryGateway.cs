using EternityFramework.DataAccess;
using EternityFramework.DataAccess.Gateway;
using EternityFramework.DataComponents.DataAnnotations;

namespace EternityFramework.Migrations.ModelGateway
{
    [Table(Name = "MigrationHistory")]
    public class MigrationHistoryGateway : RowDataGatewayBase
    {
        public string MigrationName { get; set; }
        public long MigrationOrder { get; set; }

        public MigrationHistoryGateway(DbQuery dbQuery) : base(dbQuery) { }
    }
}

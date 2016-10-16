namespace EternityFramework.Migrations
{
    public class AddedMigrationHistoryEntity : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                 CREATE TABLE [dbo].[MigrationHistory](
	                [Id] [bigint] IDENTITY(1,1) NOT NULL,
	                [MigrationName] [nvarchar](max) NULL,
	                [MigrationOrder] [bigint] NOT NULL,
                 CONSTRAINT [PK_MigrationHistory] PRIMARY KEY CLUSTERED ([Id] ASC)"
            );
        }

        public override void Down()
        {
            Sql(@"DROP TABLE [dbo].[MigrationHistory]");
        }
    }
}

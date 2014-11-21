using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GeneralizeRemoteCalendarFunctionality : KwasantDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._RemoteCalendarServiceInterfaceTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.Id);

            SeedConstants<RemoteCalendarServiceInterface>("dbo._RemoteCalendarServiceInterfaceTemplate");

            RenameColumn("dbo.RemoteCalendarProviders", "CalDAVEndPoint", "EndPoint");
            AddColumn("dbo.RemoteCalendarProviders", "Interface", c => c.Int(nullable: false));

            Sql(string.Format("UPDATE dbo.RemoteCalendarProviders SET Interface = {0} WHERE Name = N'Google'", RemoteCalendarServiceInterface.CalDAV));

            CreateIndex("dbo.RemoteCalendarProviders", "Interface");
            AddForeignKey("dbo.RemoteCalendarProviders", "Interface", "dbo._RemoteCalendarServiceInterfaceTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            RenameColumn("dbo.RemoteCalendarProviders", "EndPoint", "CalDAVEndPoint");
            DropForeignKey("dbo.RemoteCalendarProviders", "Interface", "dbo._RemoteCalendarServiceInterfaceTemplate");
            DropIndex("dbo.RemoteCalendarProviders", new[] { "Interface" });
            DropColumn("dbo.RemoteCalendarProviders", "Interface");
            DropTable("dbo._RemoteCalendarServiceInterfaceTemplate");
        }
    }
}

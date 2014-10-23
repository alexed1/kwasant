namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRemoteCalendarLinkFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RemoteCalendarLinks", "RemoteCalendarHref", c => c.String());
            AddColumn("dbo.RemoteCalendarLinks", "IsDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RemoteCalendarLinks", "IsDisabled");
            DropColumn("dbo.RemoteCalendarLinks", "RemoteCalendarHref");
        }
    }
}

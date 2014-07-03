namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCalendars : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "CalendarID", c => c.Int(nullable: false));
            CreateIndex("dbo.Events", "CalendarID");
            AddForeignKey("dbo.Events", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: true);
            DropColumn("dbo.Calendars", "PersonId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Calendars", "PersonId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Events", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Events", new[] { "CalendarID" });
            DropColumn("dbo.Events", "CalendarID");
        }
    }
}

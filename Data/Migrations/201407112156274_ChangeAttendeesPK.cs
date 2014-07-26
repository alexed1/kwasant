namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeAttendeesPK : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Attendees");
            AddPrimaryKey("dbo.Attendees", new[] { "Id", "EventID" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Attendees");
            AddPrimaryKey("dbo.Attendees", "Id");
        }
    }
}

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAttendeeToNegotiationTemp : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "NegotiationID" });
            DropColumn("dbo.Attendees", "NegotiationID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attendees", "NegotiationID", c => c.Int(nullable: false));
            CreateIndex("dbo.Attendees", "NegotiationID");
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id", cascadeDelete: true);
        }
    }
}

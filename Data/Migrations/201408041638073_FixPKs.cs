namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixPKs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "Id", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "Id" });
            DropPrimaryKey("dbo.Attendees");
            AddColumn("dbo.Attendees", "NegotiationID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Attendees", "Id");
            CreateIndex("dbo.Attendees", "NegotiationID");
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "NegotiationID" });
            DropPrimaryKey("dbo.Attendees");
            DropColumn("dbo.Attendees", "NegotiationID");
            AddPrimaryKey("dbo.Attendees", new[] { "Id", "EventID" });
            CreateIndex("dbo.Attendees", "Id");
            AddForeignKey("dbo.Attendees", "Id", "dbo.Negotiations", "Id", cascadeDelete: true);
        }
    }
}

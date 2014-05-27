namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendees", "EmailAddress_Id", c => c.Int());
            CreateIndex("dbo.Attendees", "EmailAddress_Id");
            AddForeignKey("dbo.Attendees", "EmailAddress_Id", "dbo.EmailAddresses", "Id");
            DropColumn("dbo.Attendees", "EmailAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attendees", "EmailAddress", c => c.String());
            DropForeignKey("dbo.Attendees", "EmailAddress_Id", "dbo.EmailAddresses");
            DropIndex("dbo.Attendees", new[] { "EmailAddress_Id" });
            DropColumn("dbo.Attendees", "EmailAddress_Id");
        }
    }
}

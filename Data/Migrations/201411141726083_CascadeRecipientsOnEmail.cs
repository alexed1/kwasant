namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeRecipientsOnEmail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses");
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            DropIndex("dbo.Recipients", new[] { "EmailAddressID" });
            AlterColumn("dbo.Recipients", "EmailID", c => c.Int(nullable: false));
            AlterColumn("dbo.Recipients", "EmailAddressID", c => c.Int(nullable: false));
            CreateIndex("dbo.Recipients", "EmailID");
            CreateIndex("dbo.Recipients", "EmailAddressID");
            AddForeignKey("dbo.Recipients", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropIndex("dbo.Recipients", new[] { "EmailAddressID" });
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            AlterColumn("dbo.Recipients", "EmailAddressID", c => c.Int());
            AlterColumn("dbo.Recipients", "EmailID", c => c.Int());
            CreateIndex("dbo.Recipients", "EmailAddressID");
            CreateIndex("dbo.Recipients", "EmailID");
            AddForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses", "Id");
            AddForeignKey("dbo.Recipients", "EmailID", "dbo.Emails", "Id");
        }
    }
}

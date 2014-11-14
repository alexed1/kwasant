namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeRecipientsOnEmail : DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM dbo.Recipients WHERE EmailID IS NULL");
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            AlterColumn("dbo.Recipients", "EmailID", c => c.Int(nullable: false));
            CreateIndex("dbo.Recipients", "EmailID");
            AddForeignKey("dbo.Recipients", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            AlterColumn("dbo.Recipients", "EmailID", c => c.Int());
            CreateIndex("dbo.Recipients", "EmailID");
            AddForeignKey("dbo.Recipients", "EmailID", "dbo.Emails", "Id");
        }
    }
}

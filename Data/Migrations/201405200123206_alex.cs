namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alex : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attachments", "ContentID", c => c.String());
            AddColumn("dbo.Attachments", "BoundaryEmbedded", c => c.Boolean(nullable: false));
            AddColumn("dbo.Emails", "HTMLText", c => c.String());
            AddColumn("dbo.Emails", "PlainText", c => c.String());
            DropColumn("dbo.Emails", "Text");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Emails", "Text", c => c.String());
            DropColumn("dbo.Emails", "PlainText");
            DropColumn("dbo.Emails", "HTMLText");
            DropColumn("dbo.Attachments", "BoundaryEmbedded");
            DropColumn("dbo.Attachments", "ContentID");
        }
    }
}

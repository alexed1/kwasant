namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDateRecieved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "DateReceived", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "DateReceived");
        }
    }
}

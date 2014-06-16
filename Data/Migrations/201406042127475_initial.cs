namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Emails", "Status", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Emails", "Status", c => c.Int(nullable: false));
        }
    }
}

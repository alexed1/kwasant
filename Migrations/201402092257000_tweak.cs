namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tweak : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "Name", c => c.String());
            AddColumn("dbo.Users", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Name");
            DropColumn("dbo.Services", "Name");
        }
    }
}

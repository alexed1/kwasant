namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tweak22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "UserId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profiles", "UserId");
        }
    }
}

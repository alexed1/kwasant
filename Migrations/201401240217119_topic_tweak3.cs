namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class topic_tweak3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.Users");
            AddPrimaryKey("dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Users");
            AddPrimaryKey("dbo.Users", "Id");
            AlterColumn("dbo.Users", "Id", c => c.Int(nullable: false, identity: true));
        }
    }
}

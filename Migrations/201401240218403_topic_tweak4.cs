namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class topic_tweak4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Topics", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.Topics");
            AddPrimaryKey("dbo.Topics", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Topics");
            AddPrimaryKey("dbo.Topics", "Id");
            AlterColumn("dbo.Topics", "Id", c => c.Int(nullable: false, identity: true));
        }
    }
}

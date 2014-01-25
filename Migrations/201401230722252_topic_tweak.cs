namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class topic_tweak : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Topics", "name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Topics", "name");
        }
    }
}

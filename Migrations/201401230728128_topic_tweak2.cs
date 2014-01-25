namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class topic_tweak2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Topics", "Name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Topics", "Name", c => c.String());
        }
    }
}

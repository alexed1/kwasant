namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class foo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Queues", "foo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Queues", "foo");
        }
    }
}

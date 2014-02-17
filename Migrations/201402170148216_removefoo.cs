namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removefoo : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Queues", "foo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Queues", "foo", c => c.String());
        }
    }
}

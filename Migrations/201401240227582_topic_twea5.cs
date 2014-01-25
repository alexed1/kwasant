namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class topic_twea5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Queue_Id", c => c.Int());
            CreateIndex("dbo.Users", "Queue_Id");
            AddForeignKey("dbo.Users", "Queue_Id", "dbo.Queues", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "Queue_Id", "dbo.Queues");
            DropIndex("dbo.Users", new[] { "Queue_Id" });
            DropColumn("dbo.Users", "Queue_Id");
        }
    }
}

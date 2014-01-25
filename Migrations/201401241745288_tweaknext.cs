namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tweaknext : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "Queue_ID", "dbo.Queues");
            DropIndex("dbo.Users", new[] { "Queue_ID" });
            AlterColumn("dbo.Queues", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Users", "Queue_Id", c => c.Int());
            DropPrimaryKey("dbo.Queues");
            AddPrimaryKey("dbo.Queues", "Id");
            CreateIndex("dbo.Users", "Queue_Id");
            AddForeignKey("dbo.Users", "Queue_Id", "dbo.Queues", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "Queue_Id", "dbo.Queues");
            DropIndex("dbo.Users", new[] { "Queue_Id" });
            DropPrimaryKey("dbo.Queues");
            AddPrimaryKey("dbo.Queues", "ID");
            AlterColumn("dbo.Users", "Queue_Id", c => c.Int());
            AlterColumn("dbo.Queues", "Id", c => c.Int(nullable: false, identity: true));
            CreateIndex("dbo.Users", "Queue_ID");
            AddForeignKey("dbo.Users", "Queue_ID", "dbo.Queues", "ID");
        }
    }
}

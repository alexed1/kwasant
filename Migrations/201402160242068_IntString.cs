namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IntString : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Queues", "MessageList", c => c.String());
            DropColumn("dbo.Queues", "MessageList_SerializedValue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Queues", "MessageList_SerializedValue", c => c.String());
            DropColumn("dbo.Queues", "MessageList");
        }
    }
}

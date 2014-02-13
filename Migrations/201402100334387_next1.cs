namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class next1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Queues", "MessageList_SerializedValue", c => c.String());
            DropColumn("dbo.Queues", "List_SerializedValue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Queues", "List_SerializedValue", c => c.String());
            DropColumn("dbo.Queues", "MessageList_SerializedValue");
        }
    }
}

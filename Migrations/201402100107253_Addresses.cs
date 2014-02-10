namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addresses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceName = c.String(),
                        Body = c.String(),
                        Message_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Messages", t => t.Message_Id)
                .Index(t => t.Message_Id);
            
            AddColumn("dbo.Messages", "Sender_Id", c => c.Int());
            CreateIndex("dbo.Messages", "Sender_Id");
            AddForeignKey("dbo.Messages", "Sender_Id", "dbo.Addresses", "Id");
            DropColumn("dbo.Messages", "SenderId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "SenderId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Messages", "Sender_Id", "dbo.Addresses");
            DropForeignKey("dbo.Addresses", "Message_Id", "dbo.Messages");
            DropIndex("dbo.Messages", new[] { "Sender_Id" });
            DropIndex("dbo.Addresses", new[] { "Message_Id" });
            DropColumn("dbo.Messages", "Sender_Id");
            DropTable("dbo.Addresses");
        }
    }
}

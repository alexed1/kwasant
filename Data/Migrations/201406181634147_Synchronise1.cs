namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Synchronise1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Questions", "ClarificationRequestId", "dbo.ClarificationRequests");
            DropForeignKey("dbo.ClarificationRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.ClarificationRequests", "BookingRequestId", "dbo.BookingRequests");
            DropIndex("dbo.Questions", new[] { "ClarificationRequestId" });
            DropIndex("dbo.ClarificationRequests", new[] { "Id" });
            DropIndex("dbo.ClarificationRequests", new[] { "BookingRequestId" });
            DropTable("dbo.Questions");
            DropTable("dbo.ClarificationRequests");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClarificationRequests",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        BookingRequestId = c.Int(nullable: false),
                        ClarificationStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClarificationRequestId = c.Int(),
                        Status = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        Response = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ClarificationRequests", "BookingRequestId");
            CreateIndex("dbo.ClarificationRequests", "Id");
            CreateIndex("dbo.Questions", "ClarificationRequestId");
            AddForeignKey("dbo.ClarificationRequests", "BookingRequestId", "dbo.BookingRequests", "Id");
            AddForeignKey("dbo.ClarificationRequests", "Id", "dbo.Emails", "Id");
            AddForeignKey("dbo.Questions", "ClarificationRequestId", "dbo.ClarificationRequests", "Id");
        }
    }
}

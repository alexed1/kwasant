namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClarificationRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClarificationQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClarificationRequestId = c.Int(),
                        Status = c.Int(nullable: false),
                        Text = c.String(),
                        Response = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClarificationRequests", t => t.ClarificationRequestId)
                .Index(t => t.ClarificationRequestId);
            
            CreateTable(
                "dbo.ClarificationRequests",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        BookingRequestId = c.Int(nullable: false),
                        ClarificationStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestId)
                .Index(t => t.Id)
                .Index(t => t.BookingRequestId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClarificationRequests", "BookingRequestId", "dbo.BookingRequests");
            DropForeignKey("dbo.ClarificationRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.ClarificationQuestions", "ClarificationRequestId", "dbo.ClarificationRequests");
            DropIndex("dbo.ClarificationRequests", new[] { "BookingRequestId" });
            DropIndex("dbo.ClarificationRequests", new[] { "Id" });
            DropIndex("dbo.ClarificationQuestions", new[] { "ClarificationRequestId" });
            DropTable("dbo.ClarificationRequests");
            DropTable("dbo.ClarificationQuestions");
        }
    }
}

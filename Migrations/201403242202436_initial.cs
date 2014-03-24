namespace Shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        email_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.email_Id)
                .Index(t => t.email_Id);
            
            CreateTable(
                "dbo.EmailAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Email_Id = c.Int(),
                        Email_Id1 = c.Int(),
                        Email_Id2 = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Email_Id)
                .ForeignKey("dbo.Emails", t => t.Email_Id1)
                .ForeignKey("dbo.Emails", t => t.Email_Id2)
                .Index(t => t.Email_Id)
                .Index(t => t.Email_Id1)
                .Index(t => t.Email_Id2);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                        Subject = c.String(),
                        Status = c.String(),
                        Sender_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.Sender_Id)
                .Index(t => t.Sender_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailAddresses", "Email_Id2", "dbo.Emails");
            DropForeignKey("dbo.Emails", "Sender_Id", "dbo.EmailAddresses");
            DropForeignKey("dbo.EmailAddresses", "Email_Id1", "dbo.Emails");
            DropForeignKey("dbo.EmailAddresses", "Email_Id", "dbo.Emails");
            DropForeignKey("dbo.Customers", "email_Id", "dbo.EmailAddresses");
            DropIndex("dbo.Emails", new[] { "Sender_Id" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id2" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id1" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id" });
            DropIndex("dbo.Customers", new[] { "email_Id" });
            DropTable("dbo.Emails");
            DropTable("dbo.EmailAddresses");
            DropTable("dbo.Customers");
        }
    }
}

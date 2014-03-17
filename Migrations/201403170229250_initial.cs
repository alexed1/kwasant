namespace Shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        Address = c.String(),
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
                        Sender_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.Sender_Id)
                .Index(t => t.Sender_Id);
            
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Registrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Profile_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Profiles", t => t.Profile_Id)
                .Index(t => t.Profile_Id);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        foobar = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Registrations", "Profile_Id", "dbo.Profiles");
            DropForeignKey("dbo.EmailAddresses", "Email_Id2", "dbo.Emails");
            DropForeignKey("dbo.Emails", "Sender_Id", "dbo.EmailAddresses");
            DropForeignKey("dbo.EmailAddresses", "Email_Id1", "dbo.Emails");
            DropForeignKey("dbo.EmailAddresses", "Email_Id", "dbo.Emails");
            DropIndex("dbo.Registrations", new[] { "Profile_Id" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id2" });
            DropIndex("dbo.Emails", new[] { "Sender_Id" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id1" });
            DropIndex("dbo.EmailAddresses", new[] { "Email_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.Services");
            DropTable("dbo.Registrations");
            DropTable("dbo.Profiles");
            DropTable("dbo.Emails");
            DropTable("dbo.EmailAddresses");
        }
    }
}

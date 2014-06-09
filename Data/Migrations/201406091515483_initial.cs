namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        AspNetUserRolesDO_UserId = c.String(maxLength: 128),
                        AspNetUserRolesDO_RoleId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUserRoles", t => new { t.AspNetUserRolesDO_UserId, t.AspNetUserRolesDO_RoleId })
                .Index(t => t.Name, unique: true, name: "RoleNameIndex")
                .Index(t => new { t.AspNetUserRolesDO_UserId, t.AspNetUserRolesDO_RoleId });
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.EmailAddresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Address, unique: true, name: "IX_EmailAddress_Address");
            
            CreateTable(
                "dbo.Recipients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmailID = c.Int(nullable: false),
                        EmailAddressID = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID, cascadeDelete: true)
                .Index(t => t.EmailID)
                .Index(t => t.EmailAddressID);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Subject = c.String(),
                        HTMLText = c.String(),
                        PlainText = c.String(),
                        DateReceived = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        FromID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.FromID)
                .Index(t => t.FromID);
            
            CreateTable(
                "dbo.StoredFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginalName = c.String(),
                        StoredName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Location = c.String(),
                        Status = c.String(),
                        Transparency = c.String(),
                        Class = c.String(),
                        Description = c.String(),
                        Priority = c.Int(nullable: false),
                        Sequence = c.Int(nullable: false),
                        Summary = c.String(),
                        Category = c.String(),
                        CreatedByID = c.String(nullable: false, maxLength: 128),
                        IsAllDay = c.Boolean(nullable: false),
                        BookingRequestID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID)
                .ForeignKey("dbo.Users", t => t.CreatedByID)
                .Index(t => t.CreatedByID)
                .Index(t => t.BookingRequestID);
            
            CreateTable(
                "dbo.Attendees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        EmailAddressID = c.Int(nullable: false),
                        EventID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID, cascadeDelete: true)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .Index(t => t.EmailAddressID)
                .Index(t => t.EventID);
            
            CreateTable(
                "dbo.Instructions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Category = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Calendars",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PersonId = c.Int(nullable: false),
                        OwnerID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.OwnerID)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "dbo.CommunicationConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        ToAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackingStatuses",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ForeignTableName = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Id, t.ForeignTableName });
            
            CreateTable(
                "dbo.BookingRequestInstruction",
                c => new
                    {
                        BookingRequestID = c.Int(nullable: false),
                        InstructionID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookingRequestID, t.InstructionID })
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID, cascadeDelete: true)
                .ForeignKey("dbo.Instructions", t => t.InstructionID, cascadeDelete: true)
                .Index(t => t.BookingRequestID)
                .Index(t => t.InstructionID);
            
            CreateTable(
                "dbo.EventEmail",
                c => new
                    {
                        EmailID = c.Int(nullable: false),
                        EventID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EmailID, t.EventID })
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.Events", t => t.EventID, cascadeDelete: true)
                .Index(t => t.EmailID)
                .Index(t => t.EventID);
            
            CreateTable(
                "dbo.Attachments",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        EmailID = c.Int(nullable: false),
                        Type = c.String(),
                        ContentID = c.String(),
                        BoundaryEmbedded = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StoredFiles", t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.EmailID);
            
            CreateTable(
                "dbo.BookingRequests",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                        BookingStatus = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        EmailAddressID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.EmailAddresses", t => t.EmailAddressID, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.EmailAddressID, unique: true, name: "IX_User_EmailAddress");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Users", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.BookingRequests", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Attachments", "Id", "dbo.StoredFiles");
            DropForeignKey("dbo.Calendars", "OwnerID", "dbo.Users");
            DropForeignKey("dbo.Recipients", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Recipients", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Emails", "FromID", "dbo.EmailAddresses");
            DropForeignKey("dbo.EventEmail", "EventID", "dbo.Events");
            DropForeignKey("dbo.EventEmail", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.Events", "CreatedByID", "dbo.Users");
            DropForeignKey("dbo.Events", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestInstruction", "InstructionID", "dbo.Instructions");
            DropForeignKey("dbo.BookingRequestInstruction", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" }, "dbo.AspNetUserRoles");
            DropIndex("dbo.Users", "IX_User_EmailAddress");
            DropIndex("dbo.Users", new[] { "Id" });
            DropIndex("dbo.BookingRequests", new[] { "User_Id" });
            DropIndex("dbo.BookingRequests", new[] { "Id" });
            DropIndex("dbo.Attachments", new[] { "EmailID" });
            DropIndex("dbo.Attachments", new[] { "Id" });
            DropIndex("dbo.EventEmail", new[] { "EventID" });
            DropIndex("dbo.EventEmail", new[] { "EmailID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "InstructionID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "BookingRequestID" });
            DropIndex("dbo.Calendars", new[] { "OwnerID" });
            DropIndex("dbo.Attendees", new[] { "EventID" });
            DropIndex("dbo.Attendees", new[] { "EmailAddressID" });
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            DropIndex("dbo.Events", new[] { "CreatedByID" });
            DropIndex("dbo.Emails", new[] { "FromID" });
            DropIndex("dbo.Recipients", new[] { "EmailAddressID" });
            DropIndex("dbo.Recipients", new[] { "EmailID" });
            DropIndex("dbo.EmailAddresses", "IX_EmailAddress_Address");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", new[] { "AspNetUserRolesDO_UserId", "AspNetUserRolesDO_RoleId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Users");
            DropTable("dbo.BookingRequests");
            DropTable("dbo.Attachments");
            DropTable("dbo.EventEmail");
            DropTable("dbo.BookingRequestInstruction");
            DropTable("dbo.TrackingStatuses");
            DropTable("dbo.CommunicationConfigurations");
            DropTable("dbo.Calendars");
            DropTable("dbo.Instructions");
            DropTable("dbo.Attendees");
            DropTable("dbo.Events");
            DropTable("dbo.StoredFiles");
            DropTable("dbo.Emails");
            DropTable("dbo.Recipients");
            DropTable("dbo.EmailAddresses");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}

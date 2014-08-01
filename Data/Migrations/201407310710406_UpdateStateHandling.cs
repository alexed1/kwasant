using System.Linq;
using Data.Constants;
using Data.Interfaces;
using StructureMap;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateStateHandling : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Events", "StateID", "dbo.EventStatuses");
            DropForeignKey("dbo.BookingRequests", "BRState", "dbo.BookingRequestStates");
            DropIndex("dbo.Events", new[] { "StateID" });
            DropIndex("dbo.BookingRequests", new[] { "BRState" });
            RenameColumn(table: "dbo.ClarificationRequests", name: "CRState", newName: "ClarificationRequestStateID");
            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_CRState", newName: "IX_ClarificationRequestStateID");
            CreateTable(
                "dbo.EmailParticipantTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
              "dbo.EmailStatusRows",
              c => new
              {
                  Id = c.Int(nullable: false, identity: true),
                  Name = c.String(),
              })
              .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.QuestionStatusRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CommunicationTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackingStatusRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackingTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.BookingRequests", "BookingRequestStateID", c => c.Int(nullable: false));
            AddColumn("dbo.Emails", "EmailStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.Events", "EventStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.Recipients", "EmailParticipantTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.Questions", "QuestionStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationConfigurations", "CommunicationTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "TrackingTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "TrackingStatusID", c => c.Int(nullable: false));

            Sql(@"Update dbo.BookingRequests SET BookingRequestStateID = BRState");
            Sql(@"Update dbo.Emails SET EmailStatusID = EmailStatus");
            Sql(@"Update dbo.Events SET EventStatusID = StateID");
            Sql(@"Update dbo.Recipients SET EmailParticipantTypeID = (Type - 1)"); // -1, because we moved from 2-3-4 to 1-2-3
            Sql(@"Update dbo.Questions SET QuestionStatusID = (Status + 1)"); // +1, because it was originally 0indexed
            Sql(@"Update dbo.CommunicationConfigurations SET CommunicationTypeID = (Type + 1)"); // +1, because it was originally 0indexed
            Sql(@"Update dbo.TrackingStatuses SET TrackingTypeID = Type");
            Sql(@"Update dbo.TrackingStatuses SET TrackingStatusID = Status");

            
            //We need to seed all this before updating the DB
            Sql(@"
SET IDENTITY_INSERT dbo.EmailStatusRows ON;
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Dispatched + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Invalid + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Processed + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Queued + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.SendCriticalError + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.SendRejected + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Sent + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Unprocessed + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailStatus.Unstarted + @",'')
SET IDENTITY_INSERT dbo.EmailStatusRows OFF;");

Sql(@"
SET IDENTITY_INSERT dbo.EmailParticipantTypeRows ON;
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.Bcc + @",'')
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.Cc + @",'')
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.To + @",'')
SET IDENTITY_INSERT dbo.EmailParticipantTypeRows OFF;");

Sql(@"
SET IDENTITY_INSERT dbo.CommunicationTypeRows ON;
INSERT INTO dbo.CommunicationTypeRows (Id, Name) 
VALUES (" + CommunicationType.Email + @",'')
INSERT INTO dbo.CommunicationTypeRows (Id, Name) 
VALUES (" + CommunicationType.Sms + @",'')
SET IDENTITY_INSERT dbo.CommunicationTypeRows OFF;");

Sql(@"
SET IDENTITY_INSERT dbo.TrackingStatusRows ON;
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.Completed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.Invalid + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.PendingClarification + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.Processed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.Unprocessed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingStatus.Unstarted + @",'')
SET IDENTITY_INSERT dbo.TrackingStatusRows OFF;");

Sql(@"
SET IDENTITY_INSERT dbo.TrackingTypeRows ON;
INSERT INTO dbo.TrackingTypeRows (Id, Name) 
VALUES (" + TrackingType.BookingState + @",'')
INSERT INTO dbo.TrackingTypeRows (Id, Name) 
VALUES (" + TrackingType.TestState + @",'')
SET IDENTITY_INSERT dbo.TrackingTypeRows OFF;");
            
            CreateIndex("dbo.Emails", "EmailStatusID");
            CreateIndex("dbo.Events", "EventStatusID");
            CreateIndex("dbo.Recipients", "EmailParticipantTypeID");
            CreateIndex("dbo.Questions", "QuestionStatusID");
            CreateIndex("dbo.CommunicationConfigurations", "CommunicationTypeID");
            CreateIndex("dbo.TrackingStatuses", "TrackingTypeID");
            CreateIndex("dbo.TrackingStatuses", "TrackingStatusID");
            CreateIndex("dbo.BookingRequests", "BookingRequestStateID");

            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: false);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CommunicationConfigurations", "CommunicationTypeID", "dbo.CommunicationTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates", "Id", cascadeDelete: false);

            Sql(@"
ALTER TABLE dbo.BookingRequests
DROP CONSTRAINT [FK_dbo.BookingRequests_dbo.BookingRequestStatuses_BookingRequestStatusID]");
            DropColumn("dbo.BookingRequests", "BRState");
            DropColumn("dbo.Emails", "EmailStatus");
            DropColumn("dbo.Events", "StateID");
            DropColumn("dbo.Recipients", "Type");
            DropColumn("dbo.Questions", "Status");
            DropColumn("dbo.CommunicationConfigurations", "Type");
            DropColumn("dbo.TrackingStatuses", "Type");
            DropColumn("dbo.TrackingStatuses", "Status");

        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClarificationRequestStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ServiceAuthorizationTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookingRequestStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventSyncStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventStatuses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TrackingStatuses", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationConfigurations", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Questions", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Recipients", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Events", "StateID", c => c.Int(nullable: false));
            AddColumn("dbo.Emails", "EmailStatus", c => c.Int(nullable: false));
            AddColumn("dbo.BookingRequests", "BRState", c => c.Int(nullable: false));
            DropForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.CommunicationConfigurations", "CommunicationTypeID", "dbo.CommunicationTypeRows");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropIndex("dbo.BookingRequests", new[] { "BookingRequestStateID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingStatusID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingTypeID" });
            DropIndex("dbo.CommunicationConfigurations", new[] { "CommunicationTypeID" });
            DropIndex("dbo.Questions", new[] { "QuestionStatusID" });
            DropIndex("dbo.Recipients", new[] { "EmailParticipantTypeID" });
            DropIndex("dbo.Events", new[] { "EventStatusID" });
            DropIndex("dbo.Emails", new[] { "EmailStatusID" });
            DropColumn("dbo.TrackingStatuses", "TrackingStatusID");
            DropColumn("dbo.TrackingStatuses", "TrackingTypeID");
            DropColumn("dbo.CommunicationConfigurations", "CommunicationTypeID");
            DropColumn("dbo.Questions", "QuestionStatusID");
            DropColumn("dbo.Recipients", "EmailParticipantTypeID");
            DropColumn("dbo.Events", "EventStatusID");
            DropColumn("dbo.Emails", "EmailStatusID");
            DropColumn("dbo.BookingRequests", "BookingRequestStateID");
            DropTable("dbo.TrackingTypeRows");
            DropTable("dbo.TrackingStatusRows");
            DropTable("dbo.CommunicationTypeRows");
            DropTable("dbo.ServiceAuthorizationTypes");
            DropTable("dbo.BookingRequestStates");
            DropTable("dbo.QuestionStatusRows");
            DropTable("dbo.ClarificationRequestStates");
            DropTable("dbo.EventSyncStatus");
            DropTable("dbo.EventStatuses");
            DropTable("dbo.EventCreateTypes");
            DropTable("dbo.EmailParticipantTypeRows");
            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_ClarificationRequestStateID", newName: "IX_CRState");
            RenameColumn(table: "dbo.ClarificationRequests", name: "ClarificationRequestStateID", newName: "CRState");
            CreateIndex("dbo.BookingRequests", "BRState");
            CreateIndex("dbo.Events", "StateID");
            AddForeignKey("dbo.BookingRequests", "BRState", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "StateID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.EmailStatusRows", newName: "EventCreateTypes");
        }
    }
}

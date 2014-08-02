namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class TurnOffIdentityInsert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Negotiations", "NegotiationStateID", c => c.Int());
            CreateIndex("dbo.Negotiations", "NegotiationStateID");

            AddColumn("dbo.Negotiations", "BookingRequestID", c => c.Int());
            CreateIndex("dbo.Negotiations", "BookingRequestID");
            AddForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests", "Id");

            AddColumn("dbo.Answers", "AnswerStatusID", c => c.Int());
            CreateIndex("dbo.Answers", "AnswerStatusID");            

            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus");
            DropForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropPrimaryKey("dbo.EmailStatusRows");
            DropPrimaryKey("dbo.EmailParticipantTypeRows");
            DropPrimaryKey("dbo.EventCreateTypes");
            DropPrimaryKey("dbo.EventStatuses");
            DropPrimaryKey("dbo.EventSyncStatus");
            DropPrimaryKey("dbo.ClarificationRequestStates");
            DropPrimaryKey("dbo.NegotiationStateRows");
            DropPrimaryKey("dbo.QuestionStatusRows");
            DropPrimaryKey("dbo.BookingRequestStates");
            DropPrimaryKey("dbo.ServiceAuthorizationTypes");
            DropPrimaryKey("dbo.TrackingStatusRows");
            DropPrimaryKey("dbo.TrackingTypeRows");
            DropPrimaryKey("dbo.AnswerStatusRows");

            //Ridiculous that EF makes us do this, but it doesn't handle turning off identity insert on columns properly. We need to do it manually

            AddColumn("dbo.EmailStatusRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.EmailParticipantTypeRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.EventCreateTypes", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.EventStatuses", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.EventSyncStatus", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.ClarificationRequestStates", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.NegotiationStateRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionStatusRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.BookingRequestStates", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceAuthorizationTypes", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatusRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingTypeRows", "TempID", c => c.Int(nullable: false));
            AddColumn("dbo.AnswerStatusRows", "TempID", c => c.Int(nullable: false));

            Sql(@"Update dbo.EmailStatusRows SET TempID = Id");
            Sql(@"Update dbo.EmailParticipantTypeRows SET TempID = Id");
            Sql(@"Update dbo.EventCreateTypes SET TempID = Id");
            Sql(@"Update dbo.EventStatuses SET TempID = Id");
            Sql(@"Update dbo.EventSyncStatus SET TempID = Id");
            Sql(@"Update dbo.ClarificationRequestStates SET TempID = Id");
            Sql(@"Update dbo.NegotiationStateRows SET TempID = Id");
            Sql(@"Update dbo.QuestionStatusRows SET TempID = Id");
            Sql(@"Update dbo.BookingRequestStates SET TempID = Id");
            Sql(@"Update dbo.ServiceAuthorizationTypes SET TempID = Id");
            Sql(@"Update dbo.TrackingStatusRows SET TempID = Id");
            Sql(@"Update dbo.TrackingTypeRows SET TempID = Id");
            Sql(@"Update dbo.AnswerStatusRows SET TempID = Id");

            DropColumn("dbo.EmailStatusRows", "Id");
            DropColumn("dbo.EmailParticipantTypeRows", "Id");
            DropColumn("dbo.EventCreateTypes", "Id");
            DropColumn("dbo.EventStatuses", "Id");
            DropColumn("dbo.EventSyncStatus", "Id");
            DropColumn("dbo.ClarificationRequestStates", "Id");
            DropColumn("dbo.NegotiationStateRows", "Id");
            DropColumn("dbo.QuestionStatusRows", "Id");
            DropColumn("dbo.BookingRequestStates", "Id");
            DropColumn("dbo.ServiceAuthorizationTypes", "Id");
            DropColumn("dbo.TrackingStatusRows", "Id");
            DropColumn("dbo.TrackingTypeRows", "Id");
            DropColumn("dbo.AnswerStatusRows", "Id");
            
            AddColumn("dbo.EmailStatusRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.EmailParticipantTypeRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.EventCreateTypes", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.EventStatuses", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.EventSyncStatus", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.ClarificationRequestStates", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.NegotiationStateRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionStatusRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.BookingRequestStates", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceAuthorizationTypes", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatusRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingTypeRows", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.AnswerStatusRows", "Id", c => c.Int(nullable: false));

            Sql(@"Update dbo.EmailStatusRows SET Id = TempID");
            Sql(@"Update dbo.EmailParticipantTypeRows SET Id = TempID");
            Sql(@"Update dbo.EventCreateTypes SET Id = TempID");
            Sql(@"Update dbo.EventStatuses SET Id = TempID");
            Sql(@"Update dbo.EventSyncStatus SET Id = TempID");
            Sql(@"Update dbo.ClarificationRequestStates SET Id = TempID");
            Sql(@"Update dbo.NegotiationStateRows SET Id = TempID");
            Sql(@"Update dbo.QuestionStatusRows SET Id = TempID");
            Sql(@"Update dbo.BookingRequestStates SET Id = TempID");
            Sql(@"Update dbo.ServiceAuthorizationTypes SET Id = TempID");
            Sql(@"Update dbo.TrackingStatusRows SET Id = TempID");
            Sql(@"Update dbo.TrackingTypeRows SET Id = TempID");
            Sql(@"Update dbo.AnswerStatusRows SET Id = TempID");
            
            DropColumn("dbo.EmailStatusRows", "TempID");
            DropColumn("dbo.EmailParticipantTypeRows", "TempID");
            DropColumn("dbo.EventCreateTypes", "TempID");
            DropColumn("dbo.EventStatuses", "TempID");
            DropColumn("dbo.EventSyncStatus", "TempID");
            DropColumn("dbo.ClarificationRequestStates", "TempID");
            DropColumn("dbo.NegotiationStateRows", "TempID");
            DropColumn("dbo.QuestionStatusRows", "TempID");
            DropColumn("dbo.BookingRequestStates", "TempID");
            DropColumn("dbo.ServiceAuthorizationTypes", "TempID");
            DropColumn("dbo.TrackingStatusRows", "TempID");
            DropColumn("dbo.TrackingTypeRows", "TempID");
            DropColumn("dbo.AnswerStatusRows", "TempID");
            
            AddPrimaryKey("dbo.EmailStatusRows", "Id");
            AddPrimaryKey("dbo.EmailParticipantTypeRows", "Id");
            AddPrimaryKey("dbo.EventCreateTypes", "Id");
            AddPrimaryKey("dbo.EventStatuses", "Id");
            AddPrimaryKey("dbo.EventSyncStatus", "Id");
            AddPrimaryKey("dbo.ClarificationRequestStates", "Id");
            AddPrimaryKey("dbo.NegotiationStateRows", "Id");
            AddPrimaryKey("dbo.QuestionStatusRows", "Id");
            AddPrimaryKey("dbo.BookingRequestStates", "Id");
            AddPrimaryKey("dbo.ServiceAuthorizationTypes", "Id");
            AddPrimaryKey("dbo.TrackingStatusRows", "Id");
            AddPrimaryKey("dbo.TrackingTypeRows", "Id");
            AddPrimaryKey("dbo.AnswerStatusRows", "Id");
            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes");
            DropForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates");
            DropForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropPrimaryKey("dbo.AnswerStatusRows");
            DropPrimaryKey("dbo.TrackingTypeRows");
            DropPrimaryKey("dbo.TrackingStatusRows");
            DropPrimaryKey("dbo.ServiceAuthorizationTypes");
            DropPrimaryKey("dbo.BookingRequestStates");
            DropPrimaryKey("dbo.QuestionStatusRows");
            DropPrimaryKey("dbo.NegotiationStateRows");
            DropPrimaryKey("dbo.ClarificationRequestStates");
            DropPrimaryKey("dbo.EventSyncStatus");
            DropPrimaryKey("dbo.EventStatuses");
            DropPrimaryKey("dbo.EventCreateTypes");
            DropPrimaryKey("dbo.EmailParticipantTypeRows");
            DropPrimaryKey("dbo.EmailStatusRows");
            AlterColumn("dbo.AnswerStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.TrackingTypeRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.TrackingStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ServiceAuthorizationTypes", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.BookingRequestStates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.QuestionStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.NegotiationStateRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ClarificationRequestStates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventSyncStatus", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventStatuses", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventCreateTypes", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EmailParticipantTypeRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EmailStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.AnswerStatusRows", "Id");
            AddPrimaryKey("dbo.TrackingTypeRows", "Id");
            AddPrimaryKey("dbo.TrackingStatusRows", "Id");
            AddPrimaryKey("dbo.ServiceAuthorizationTypes", "Id");
            AddPrimaryKey("dbo.BookingRequestStates", "Id");
            AddPrimaryKey("dbo.QuestionStatusRows", "Id");
            AddPrimaryKey("dbo.NegotiationStateRows", "Id");
            AddPrimaryKey("dbo.ClarificationRequestStates", "Id");
            AddPrimaryKey("dbo.EventSyncStatus", "Id");
            AddPrimaryKey("dbo.EventStatuses", "Id");
            AddPrimaryKey("dbo.EventCreateTypes", "Id");
            AddPrimaryKey("dbo.EmailParticipantTypeRows", "Id");
            AddPrimaryKey("dbo.EmailStatusRows", "Id");
            AddForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: true);
        }
    }
}

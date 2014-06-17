namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class SetupDefaultEmailAndBookingStatuses : DbMigration
    {
        public override void Up()
        {
            Sql(@"

update BookingRequests set BookingStatus = 'Unprocessed'

DECLARE @emailAddressID INT
SET @emailAddressID = (select TOP(1) ID from EmailAddresses where name = 'Kwasant Scheduling Services' and Address = 'scheduling@kwasant.com')
update emails set EmailStatus = 2
update emails set EmailStatus = 4 where FromID is not null and fromid = @emailAddressID");
        }
        
        public override void Down()
        {
        }
    }
}

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class DeleteEmailsFromOurselves : DbMigration
    {
        public override void Up()
        {
            Sql(@"
DELETE FROM Envelopes
WHERE EmailID IN (
	SELECT ID FROM Emails
		WHERE FromID IN (
		select Id from EmailAddresses
		WHERE Address like 'info@kwasant.com'
	)
)

DELETE FROM BookingRequests
WHERE ID IN (
	SELECT ID FROM Emails
		WHERE FromID IN (
		select Id from EmailAddresses
		WHERE Address like 'info@kwasant.com'
	)
)

DELETE FROM Emails WHERE ID IN (
	SELECT ID FROM Emails
		WHERE FromID IN (
		select Id from EmailAddresses
		WHERE Address like 'info@kwasant.com'
	)
)");
        }
        
        public override void Down()
        {
        }
    }
}

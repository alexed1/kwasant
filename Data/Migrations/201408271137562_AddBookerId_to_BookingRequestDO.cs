namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookerId_to_BookingRequestDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookingRequests", "BookerId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookingRequests", "BookerId");
        }
    }
}

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBooker_to_BookingRequests : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookingRequests", "BookerID", c => c.String(maxLength: 128));
            CreateIndex("dbo.BookingRequests", "BookerID");
            AddForeignKey("dbo.BookingRequests", "BookerID", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookingRequests", "BookerID", "dbo.Users");
            DropIndex("dbo.BookingRequests", new[] { "BookerID" });
            DropColumn("dbo.BookingRequests", "BookerID");
        }
    }
}

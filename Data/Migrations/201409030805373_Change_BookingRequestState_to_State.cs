namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change_BookingRequestState_to_State : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestState", newName: "State");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestState", newName: "IX_State");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BookingRequests", name: "IX_State", newName: "IX_BookingRequestState");
            RenameColumn(table: "dbo.BookingRequests", name: "State", newName: "BookingRequestState");
        }
    }
}

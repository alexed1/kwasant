namespace Shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class syncfusion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppointmentTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AppointmentTables");
        }
    }
}

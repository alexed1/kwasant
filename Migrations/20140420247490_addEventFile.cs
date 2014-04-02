namespace Shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEventFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventFile",
                c => new
                {
                    Id = c.String(),                    
                    Body = c.String(),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.EventFile");
        }
    }
}

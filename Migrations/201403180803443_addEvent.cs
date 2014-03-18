namespace Shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Duration = c.Time(nullable: false, precision: 7),
                        IsAllDay = c.Boolean(nullable: false),
                        Location = c.String(),
                        Status = c.Int(nullable: false),
                        Transparency = c.Int(nullable: false),
                        Class = c.String(),
                        Description = c.String(),
                        Priority = c.Int(nullable: false),
                        Sequence = c.Int(nullable: false),
                        Summary = c.String(),
                        UID = c.String(),
                        Name = c.String(),
                        Line = c.Int(nullable: false),
                        Column = c.Int(nullable: false),
                        Group = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Events");
        }
    }
}

namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Registrations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrations", "Profile_Id", c => c.Int());
            CreateIndex("dbo.Registrations", "Profile_Id");
            AddForeignKey("dbo.Registrations", "Profile_Id", "dbo.Profiles", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Registrations", "Profile_Id", "dbo.Profiles");
            DropIndex("dbo.Registrations", new[] { "Profile_Id" });
            DropColumn("dbo.Registrations", "Profile_Id");
        }
    }
}

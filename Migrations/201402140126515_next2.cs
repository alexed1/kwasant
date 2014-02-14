namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class next2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "foobar", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "foobar");
        }
    }
}

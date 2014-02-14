namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class next : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Addresses", "foo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Addresses", "foo");
        }
    }
}

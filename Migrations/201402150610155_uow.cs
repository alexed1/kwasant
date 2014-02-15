namespace shnexy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uow : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Addresses", "foo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Addresses", "foo", c => c.String());
        }
    }
}

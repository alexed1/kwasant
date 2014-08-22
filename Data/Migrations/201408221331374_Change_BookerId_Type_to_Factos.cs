namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change_BookerId_Type_to_Factos : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Facts", "BookerId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Facts", "BookerId", c => c.Int(nullable: false));
        }
    }
}

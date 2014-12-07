namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveTemplateDescriptionsToMemory : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Envelopes", "TemplateDescription");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Envelopes", "TemplateDescription", c => c.String());
        }
    }
}

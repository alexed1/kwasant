using Data.Repositories;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTemplateDescriptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Envelopes", "TemplateDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Envelopes", "TemplateDescription");
        }
    }
}

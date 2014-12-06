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
            const string sqlFormat = @"UPDATE dbo.Envelopes SET TemplateDescription = '{0}' WHERE TemplateName = '{1}'";
            foreach (var kvp in EnvelopeRepository.TemplateDescriptionMapping)
            {
                Sql(String.Format(sqlFormat, kvp.Value, kvp.Key));
            }
        }
        
        public override void Down()
        {
            DropColumn("dbo.Envelopes", "TemplateDescription");
        }
    }
}

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Notes_From_IncidentDO : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Incidents", "Notes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Incidents", "Notes", c => c.String());
        }
    }
}

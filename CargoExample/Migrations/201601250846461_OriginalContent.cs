namespace CargoExample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OriginalContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ContentItems", "OriginalContent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ContentItems", "OriginalContent");
        }
    }
}

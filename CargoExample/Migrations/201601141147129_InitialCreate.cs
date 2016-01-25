namespace CargoExample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContentItems",
                c => new
                    {
                        Location = c.String(nullable: false, maxLength: 200),
                        Key = c.String(nullable: false, maxLength: 200),
                        Content = c.String(nullable: false),
                    })
                .PrimaryKey(t => new { t.Location, t.Key });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ContentItems");
        }
    }
}

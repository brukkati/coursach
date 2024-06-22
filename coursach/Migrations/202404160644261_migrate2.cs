namespace coursach.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migrate2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Items", "ImageUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Items", "ImageUrl", c => c.String());
        }
    }
}

﻿namespace coursach.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "ImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "ImageUrl");
        }
    }
}

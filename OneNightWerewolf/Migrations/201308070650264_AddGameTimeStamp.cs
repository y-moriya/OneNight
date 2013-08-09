namespace OneNightWerewolf.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameTimeStamp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Games", "Version", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Games", "Version");
        }
    }
}

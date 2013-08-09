namespace OneNightWerewolf.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPlayerCommited : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Players", "Commited", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Players", "Commited");
        }
    }
}

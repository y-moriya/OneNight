namespace OneNightWerewolf.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Int(nullable: false, identity: true),
                        GameName = c.String(),
                        PlayerNum = c.Int(nullable: false),
                        Phase = c.Int(nullable: false),
                        SecretCards = c.String(),
                        Cards = c.String(),
                        WerewolfWon = c.Boolean(nullable: false),
                        Creator = c.String(),
                        CreatedAt = c.DateTime(),
                        StartedAt = c.DateTime(),
                        DayAt = c.DateTime(),
                        JudgeAt = c.DateTime(),
                        VoteAt = c.DateTime(),
                        ClosedAt = c.DateTime(),
                        DeletedAt = c.DateTime(),
                        NextUpdate = c.DateTime(),
                    })
                .PrimaryKey(t => t.GameId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        PlayerId = c.Int(nullable: false, identity: true),
                        PlayerName = c.String(),
                        OriginalCardId = c.Int(nullable: false),
                        CurrentCardId = c.Int(nullable: false),
                        VotePlayerId = c.Int(nullable: false),
                        GameId = c.Int(nullable: false),
                        Won = c.Boolean(nullable: false),
                        Executed = c.Boolean(nullable: false),
                        SkillTarget = c.Int(nullable: false),
                        SkillResult = c.String(),
                    })
                .PrimaryKey(t => t.PlayerId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        DeletedAt = c.DateTime(),
                        GameId = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                        PlayerName = c.String(),
                        MessageType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MessageId)
                .ForeignKey("dbo.Games", t => t.GameId, cascadeDelete: true)
                .Index(t => t.GameId);
            
            CreateTable(
                "dbo.Entries",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GameId = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GameId })
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Games", t => t.GameId, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => t.PlayerId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GameId)
                .Index(t => t.PlayerId);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Entries", new[] { "PlayerId" });
            DropIndex("dbo.Entries", new[] { "GameId" });
            DropIndex("dbo.Entries", new[] { "UserId" });
            DropIndex("dbo.Messages", new[] { "GameId" });
            DropForeignKey("dbo.Entries", "PlayerId", "dbo.Players");
            DropForeignKey("dbo.Entries", "GameId", "dbo.Games");
            DropForeignKey("dbo.Entries", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.Messages", "GameId", "dbo.Games");
            DropTable("dbo.UserProfile");
            DropTable("dbo.Entries");
            DropTable("dbo.Messages");
            DropTable("dbo.Players");
            DropTable("dbo.Games");
        }
    }
}

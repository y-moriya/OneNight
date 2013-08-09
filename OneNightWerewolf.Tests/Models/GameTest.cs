using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneNightWerewolf.Models;

namespace OneNightWerewolf.Tests.Models
{
    [TestClass]
    public class GameTest
    {
        [TestMethod]
        public void GameTest1()
        {
            GameModel game = new GameModel("Game1");
            PlayerModel p1 = new PlayerModel("Player1");
            PlayerModel p2 = new PlayerModel("Player2");
            PlayerModel p3 = new PlayerModel("Player3");

            game.AddPlayer(p1);
            game.AddPlayer(p2);
            game.AddPlayer(p3);

            Assert.AreEqual(3, game.PlayerNum);
        }

        [TestMethod]
        public void GameTest2()
        {
            GameModel game = new GameModel(1);
            Assert.AreEqual(3, game.PlayerNum);
        }

        [TestMethod]
        public void GameTest3()
        {
            GameModel game = new GameModel(1);
            game.UpdatePhase();

            Assert.AreEqual(Phase.Night, game.Game.Phase);
        }

        [TestMethod]
        public void GameTest4()
        {
            GameModel game = new GameModel(1);
            var seer = game.Players.Find(p => p.OriginalCard is SeerCard);
            var wolf = game.Players.Find(p => p.OriginalCard is WerewolfCard);
            var thief = game.Players.Find(p => p.OriginalCard is ThiefCard);

            Assert.AreEqual(1, seer.Player.PlayerId);
            Assert.AreEqual(2, wolf.Player.PlayerId);
            Assert.AreEqual(3, thief.Player.PlayerId);

            game.UseSkill(seer.Player.PlayerId, -1);
            Assert.AreEqual(-1, seer.Player.SkillTarget);
            Assert.AreEqual(4, seer.Player.GetSkillResults()[0]);
            Assert.AreEqual(1, seer.Player.GetSkillResults()[1]);

            game.UseSkill(wolf.Player.PlayerId, -1);
            Assert.AreEqual(0, wolf.Player.SkillTarget);
            Assert.AreEqual(0, wolf.Player.GetSkillResults().Count);

            game.UseSkill(thief.Player.PlayerId, 1);
            Assert.AreEqual(1, thief.Player.SkillTarget);
            Assert.AreEqual(2, thief.Player.GetSkillResults()[0]);
            Assert.IsTrue(thief.CurrentCard is SeerCard);
        }

        [TestMethod]
        public void GameTest5()
        {
            GameModel game = new GameModel(2);
            game.UpdatePhase();

            Assert.AreEqual(Phase.Day, game.Game.Phase);
        }

        [TestMethod]
        public void GameTest6()
        {
            GameModel game = new GameModel(2);
            var seer = game.Players.Find(p => p.OriginalCard is SeerCard);
            var wolf = game.Players.Find(p => p.OriginalCard is WerewolfCard);
            var thief = game.Players.Find(p => p.OriginalCard is ThiefCard);
            
            game.Vote(seer.Player.PlayerId, wolf.Player.PlayerId);
            Assert.AreEqual(wolf.Player.PlayerId, seer.Player.VotePlayerId);

            game.Vote(seer.Player.PlayerId, thief.Player.PlayerId);
            Assert.AreEqual(thief.Player.PlayerId, seer.Player.VotePlayerId);

            game.Vote(wolf.Player.PlayerId, thief.Player.PlayerId);
            Assert.AreEqual(thief.Player.PlayerId, wolf.Player.VotePlayerId);

        }

        [TestMethod]
        public void GameTest7()
        {
            GameModel game = new GameModel(2);
            game.UpdatePhase();

            Assert.AreEqual(Phase.Epilogue, game.Game.Phase);
        }
    }
}

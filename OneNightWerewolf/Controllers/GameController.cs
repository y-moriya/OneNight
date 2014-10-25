using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OneNightWerewolf.Models;
using OneNightWerewolf.Filters;
using Microsoft.AspNet.SignalR;
using OneNightWerewolf.Hubs;

namespace OneNightWerewolf.Controllers
{
    public class GameController : Controller
    {
        private GamesContext db = new GamesContext();

        //
        // GET: /Game/
        [GamesFilter]
        public ActionResult Night(int id)
        {
            var game = new GameModel(id);
            if (game.DoUpdate())
            {
                game.UpdatePhase();
            }

            ViewBag.ReturnUrl = Url.Action("Night", new { id = id });
            return View(game);
        }

        public ActionResult Messages(int id)
        {
            var game = new GameModel(id);
            var player = game.Players.Find(p => p.Player.PlayerUserName == User.Identity.Name);
            int playerId = 0;
            if (player != null)
            {
                playerId = player.Player.PlayerId;
            }

            var messages = game.GetMessages(playerId);
            if (game.Game.Phase == Phase.Close)
            {
                messages = messages.OrderBy(m => m.MessageId).ToList();
            }

            return PartialView(messages);
        }

        public ActionResult PartialIndex()
        {
            if (db.Games.Count(g => g.Phase < Phase.Close) == 0)
            {
                return PartialView(db.Games.OrderByDescending(g => g.GameId).Take(10));
            }
            return PartialView(db.Games.Where(g => g.Phase < Phase.Close).OrderByDescending(g => g.GameId));
        }

        public ActionResult PlayersList(int id)
        {
            var game = new GameModel(id);

            return PartialView(game.Players);
        }

        public ActionResult PlayersForm(int id)
        {
            var game = new GameModel(id);
            var player = game.Players.Find(p => p.Player.PlayerUserName == User.Identity.Name);

            return PartialView(player);
        }

        //
        // GET: /Game/Details/5

        public ActionResult Details(int id = 0)
        {
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }

            var messages = db.Messages.Where(m => m.Game.GameId == id);
            ViewBag.Messages = messages;
            ViewBag.Players = db.Players.Where(p => p.GameId == id);
            ViewBag.GameId = id;
            ViewBag.Users = db.UserProfiles.Select(u => new SelectListItem()
            {
                Text = u.UserName,
                Value = u.UserName
            });
            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                return RedirectToAction("Night", new { id = message.GameId });
            }

            if (message.PlayerUserName != User.Identity.Name)
            {
                return RedirectToAction("Night", new { id = message.GameId });
            }

            if (ModelState.IsValid)
            {
                var game = new GameModel(message.GameId);
                game.SendMessage(message);
                return RedirectToAction("Night", new { id = message.GameId });
            }

            return RedirectToAction("Night", new { id = message.GameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vote(int gameId, int playerId, int targetId)
        {
            var game = new GameModel(gameId);
            game.Vote(playerId, targetId);

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UseSkill(int gameId, int playerId, int targetId = 0)
        {
            var game = new GameModel(gameId);
            game.UseSkill(playerId, targetId);

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Commit(int playerId)
        {
            var player = db.Players.Find(playerId);
            player.Commited = !player.Commited;
            db.SaveChanges();

            return RedirectToAction("Night", new { id = player.GameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Entry(int gameId)
        {
            var game = new GameModel(gameId);
            var player = new PlayerModel(db.GetUserProfile(User.Identity.Name));
            game.AddPlayer(player);

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Exit(int gameId, int playerId)
        {
            var game = new GameModel(gameId);
            game.RemovePlayer(playerId);

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestUpdate(int gameId)
        {
            var game = new GameModel(gameId);
            game.UpdatePhase();

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Start(int gameId)
        {
            var game = new GameModel(gameId);
            if (game.Game.Phase > Phase.Prologue)
            {
                return RedirectToAction("Night", new { id = gameId });
            }
            game.UpdatePhase();
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            if (game.Game.Phase > Phase.Prologue)
            {
                string msg = "ゲームが開始されました。画面を更新します。";
                context.Clients.Group(gameId.ToString()).Reload(msg);
            }

            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Auto(int PlayerNum)
        {
            var game = new GameModel("AutoTestGame");
            for (int i = 0; i < PlayerNum; i++)
            {
                var player = new PlayerModel(new UserProfile() { UserName = "User" + i, Name = "User" + i });
                game.AddPlayer(player);
            }

            // 開始
            game.UpdatePhase();

            // 夜終了
            game.UpdatePhase();

            // 昼終了
            game.UpdatePhase();

            // 投票終了
            game.UpdatePhase();

            // クローズ
            game.UpdatePhase();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Game/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Game/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Game game)
        {
            if (ModelState.IsValid)
            {
                db.Games.Add(game);
                db.SaveChanges();

                var gm = new GameModel(game.GameId);
                return RedirectToAction("Night", new { id = game.GameId });
            }

            return View(game);
        }

        //
        // GET: /Game/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            return View(game);
        }

        //
        // POST: /Game/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Game game)
        {
            if (ModelState.IsValid)
            {
                db.Entry(game).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(game);
        }

        //
        // GET: /Game/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Game game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }
            return View(game);
        }

        //
        // POST: /Game/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Game game = db.Games.Find(id);
            db.Games.Remove(game);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Record(string id)
        {
            string name = User.Identity.Name;
            if (!string.IsNullOrEmpty(id))
            {
                name = id;
            }
            var players = db.Players.Where(p => p.PlayerUserName == name);
            Record record = new Record(name, players, players.SelectMany(p => db.Games.Where(g => g.GameId == p.GameId)));
            return View(record);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
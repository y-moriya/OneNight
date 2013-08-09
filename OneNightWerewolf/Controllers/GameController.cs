using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OneNightWerewolf.Models;
using OneNightWerewolf.Filters;

namespace OneNightWerewolf.Controllers
{
    public class GameController : Controller
    {
        private GamesContext db = new GamesContext();

        //
        // GET: /Game/

        public ActionResult Night(int id)
        {
            var game = new GameModel(id);
            if (game.DoUpdate())
            {
                game.UpdatePhase();
            }

            return View(game);
        }

        public ActionResult Messages(int id)
        {
            var game = new GameModel(id);
            var player = game.Players.Find(p => p.Player.PlayerName == User.Identity.Name);
            int playerId = 0;
            if (player != null)
            {
                playerId = player.Player.PlayerId;
            }

            return PartialView(game.GetMessages(playerId));
        }

        public ActionResult PartialIndex()
        {
            return PartialView(db.Games.OrderByDescending(g => g.GameId));
        }

        public ActionResult PlayersList(int id)
        {
            var game = new GameModel(id);

            return PartialView(game.Players);
        }

        public ActionResult PlayersForm(int id)
        {
            var game = new GameModel(id);
            var player = game.Players.Find(p => p.Player.PlayerName == User.Identity.Name);

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

            if (message.PlayerName != User.Identity.Name)
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

        //
        // GET: /Game/TestEntry

        public ActionResult TestEntry(int gameId)
        {
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName");
            ViewBag.GameId = gameId;
            return PartialView();
        }

        //
        // POST: /Game/TestEntry

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestEntry(Entry entry)
        {
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName");
            ViewBag.GameId = entry.GameId;

            if (db.Entries.Find(entry.UserId, entry.GameId) != null)
            {
                return PartialView();
            }

            var player = new PlayerModel(db.UserProfiles.Find(entry.UserId).UserName);
            var game = new GameModel(entry.GameId);

            if (ModelState.IsValid && game.AddPlayer(player))
            {
                entry.PlayerId = player.Player.PlayerId;
                //db.Entries.Add(entry);
                db.SaveChanges();

                return PartialView();
            }

            return PartialView();
        }



        //
        // GET: /Game/TestExit

        public ActionResult TestExit(int gameId)
        {
            var entries = db.Entries.Where(e => e.GameId == gameId);
            var users = db.UserProfiles.Where(u => entries.Any(e => e.UserId == u.UserId));
            ViewBag.UserId = new SelectList(users, "UserId", "UserName");
            ViewBag.GameId = gameId;
            return PartialView();
        }


        //
        // POST: /Game/TestExit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestExit(Entry entry)
        {
            var entries = db.Entries.Where(e => e.GameId == entry.GameId);
            var users = db.UserProfiles.Where(u => entries.Any(e => e.UserId == u.UserId));
            ViewBag.UserId = new SelectList(users, "UserId", "UserName");
            ViewBag.GameId = entry.GameId;

            var tempEntry = db.Entries.Find(entry.UserId, entry.GameId);
            if (tempEntry == null)
            {
                return PartialView();
            }
            else
            {
                entry = tempEntry;
            }

            if (ModelState.IsValid)
            {
                var game = new GameModel(entry.GameId);
                game.RemovePlayer(entry.PlayerId);

                //db.Entries.Remove(entry);
                db.SaveChanges();

                return PartialView();
            }

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Entry(int gameId)
        {
            var game = new GameModel(gameId);
            var player = new PlayerModel(User.Identity.Name);
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
            return RedirectToAction("Night", new { id = gameId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Auto(int PlayerNum)
        {
            var game = new GameModel("AutoTestGame");
            for (int i = 0; i < PlayerNum; i++)
            {
                var player = new PlayerModel("User" + i);
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
                return RedirectToAction("Night", new { id = game.GameId });
            }

            return View(game);
        }

        [HttpPost]
        public ActionResult TestCreate()
        {
            Game game = new Game();
            game.GameName = "TestGame" + (db.Games.Count() + 1);
            db.Games.Add(game);
            db.SaveChanges();

            return RedirectToAction("Index");
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
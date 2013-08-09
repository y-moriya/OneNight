using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OneNightWerewolf.Models;

namespace OneNightWerewolf.Controllers
{
    public class EntryController : Controller
    {
        private GamesContext db = new GamesContext();

        //
        // GET: /Entry/

        public ActionResult Index()
        {
            var entries = db.Entries.Include(e => e.UserProfile).Include(e => e.Game).Include(e => e.Player);
            return View(entries.ToList());
        }

        //
        // GET: /Entry/Details/5

        public ActionResult Details(int id = 0)
        {
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        //
        // GET: /Entry/Create

        public ActionResult Create(int gameId)
        {
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName");
            ViewBag.GameId = gameId;
            return PartialView();
        }

        //
        // POST: /Entry/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Entry entry)
        {
            if (ModelState.IsValid)
            {
                db.Entries.Add(entry);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", entry.UserId);
            ViewBag.GameId = new SelectList(db.Games, "GameId", "GameName", entry.GameId);
            ViewBag.PlayerId = new SelectList(db.Players, "PlayerId", "PlayerName", entry.PlayerId);
            return View(entry);
        }

        //
        // GET: /Entry/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", entry.UserId);
            ViewBag.GameId = new SelectList(db.Games, "GameId", "GameName", entry.GameId);
            ViewBag.PlayerId = new SelectList(db.Players, "PlayerId", "PlayerName", entry.PlayerId);
            return View(entry);
        }

        //
        // POST: /Entry/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Entry entry)
        {
            if (ModelState.IsValid)
            {
                db.Entry(entry).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", entry.UserId);
            ViewBag.GameId = new SelectList(db.Games, "GameId", "GameName", entry.GameId);
            ViewBag.PlayerId = new SelectList(db.Players, "PlayerId", "PlayerName", entry.PlayerId);
            return View(entry);
        }

        //
        // GET: /Entry/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Entry entry = db.Entries.Find(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        //
        // POST: /Entry/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Entry entry = db.Entries.Find(id);
            db.Entries.Remove(entry);
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
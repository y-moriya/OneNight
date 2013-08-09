using OneNightWerewolf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace OneNightWerewolf.Filters
{
    // Filter じゃなくていい説
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GamesFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now.ToUniversalTime(), "Tokyo Standard Time");
            var db = new GamesContext();
            var target = db.Games
                .Where(g => (g.NextUpdate.HasValue && g.NextUpdate.Value < now));
            foreach (Game g in target)
            {
                GameModel model = new GameModel(g.GameId);
                model.UpdatePhase();
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
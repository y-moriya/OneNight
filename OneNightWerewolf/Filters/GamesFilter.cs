using OneNightWerewolf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace OneNightWerewolf.Filters
{
    // DB移行時のみ有効にするフィルタ
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GamesFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                using (var db = new GamesContext())
                {
                    if (!db.UserProfiles.Any(u => u.UserName == filterContext.HttpContext.User.Identity.Name))
                    {
                        throw new AuthenticationException("ユーザーが存在しません。ログインし直してください。");
                    }
                }
            }
        }
    }
}
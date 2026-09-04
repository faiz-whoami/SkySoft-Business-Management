using System;
using System.Web;
using System.Web.Helpers;

namespace business_managment_system.Helpers
{
    public static class AntiForgeryCookie
    {
        public static void Expire(HttpContextBase context)
        {
            if (context == null)
            {
                return;
            }

            var cookie = new HttpCookie(AntiForgeryConfig.CookieName)
            {
                Expires = DateTime.UtcNow.AddYears(-1),
                HttpOnly = true,
                Path = "/",
                Secure = context.Request.IsSecureConnection
            };
            context.Response.Cookies.Set(cookie);
        }
    }
}

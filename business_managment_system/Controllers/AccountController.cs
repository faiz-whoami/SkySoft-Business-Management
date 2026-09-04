using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using business_managment_system.Helpers;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AuthService _auth;

        public AccountController()
            : this(new AuthService())
        {
        }

        public AccountController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = _auth.AttemptLogin(model.Username, model.Password);
            if (result.IsInactive)
            {
                ModelState.AddModelError(string.Empty, "This account has been deactivated. Contact your administrator.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            SignIn(result.User);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        private void SignIn(EndUser user)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                user.Username,
                DateTime.Now,
                DateTime.Now.AddMinutes(20),
                false,
                user.RoleName);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection
            };
            Response.Cookies.Add(cookie);

            Session[SessionKeys.EndUserId] = user.EndUserId;
            Session[SessionKeys.FullName] = user.FullName;
            Session[SessionKeys.RoleName] = user.RoleName;
        }
    }
}

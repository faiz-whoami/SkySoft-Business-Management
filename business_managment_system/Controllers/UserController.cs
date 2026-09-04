using System;
using System.Web.Mvc;
using business_managment_system.Filters;
using business_managment_system.Helpers;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    [AppAuthorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly EndUserService _users;

        public UserController()
            : this(new EndUserService())
        {
        }

        public UserController(EndUserService users)
        {
            _users = users;
        }

        public ActionResult Index(string search, string status)
        {
            ViewBag.Title = "End Users";
            var model = _users.GetList(search, status);
            model.CurrentUserId = CurrentUserId;
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _users.GetDetails(id, CurrentUserId);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = model.User.FullName;
            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "New user";
            var model = new EndUserFormViewModel { IsNew = true };
            model.Roles = _users.GetRoleOptions(model.RoleId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EndUserFormViewModel model)
        {
            ViewBag.Title = "New user";
            model.IsNew = true;
            if (!ModelState.IsValid)
            {
                model.Roles = _users.GetRoleOptions(model.RoleId);
                return View(model);
            }

            try
            {
                _users.Create(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Username", ex.Message);
                model.Roles = _users.GetRoleOptions(model.RoleId);
                return View(model);
            }

            TempData["Success"] = "User was created.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var user = _users.Get(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit user";
            var model = EndUserService.ToForm(user, user.EndUserId == CurrentUserId);
            model.Roles = _users.GetRoleOptions(model.RoleId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EndUserFormViewModel model)
        {
            ViewBag.Title = "Edit user";
            model.IsNew = false;
            model.IsSelf = model.EndUserId == CurrentUserId;
            if (!ModelState.IsValid)
            {
                model.Roles = _users.GetRoleOptions(model.RoleId);
                return View(model);
            }

            try
            {
                _users.Update(model, CurrentUserId);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "User was not found.")
                {
                    return HttpNotFound();
                }

                if (ex.Message == EndUserService.DuplicateUsernameMessage)
                {
                    ModelState.AddModelError("Username", ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                model.Roles = _users.GetRoleOptions(model.RoleId);
                return View(model);
            }

            TempData["Success"] = "User was updated.";
            return RedirectToAction("Details", new { id = model.EndUserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, string search, string status)
        {
            var user = _users.Get(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var nowActive = !user.IsActive;
            try
            {
                _users.SetActive(id, nowActive, CurrentUserId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", new { search, status });
            }

            if (nowActive)
            {
                TempData["Success"] = "User was reactivated and can sign in again.";
                return RedirectToAction("Index", new { search, status = "active" });
            }

            TempData["Success"] = "User was deactivated. Open Status → Inactive to reactivate them later.";
            return RedirectToAction("Index", new { search, status = "inactive" });
        }

        private int CurrentUserId
        {
            get
            {
                var id = Session[SessionKeys.EndUserId];
                return id is int value ? value : 0;
            }
        }
    }
}

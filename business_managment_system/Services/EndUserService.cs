using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using business_managment_system.Data;
using business_managment_system.Helpers;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class EndUserService
    {
        public const string DuplicateUsernameMessage = "This username is already in use.";
        public const string LastAdminMessage = "At least one active administrator is required.";
        public const string CannotDeactivateSelfMessage = "You cannot deactivate your own account.";

        private readonly EndUserRepository _users;

        public EndUserService()
            : this(new EndUserRepository())
        {
        }

        public EndUserService(EndUserRepository users)
        {
            _users = users;
        }

        public EndUserListViewModel GetList(string search, string status)
        {
            var filter = (status ?? "active").Trim().ToLowerInvariant();
            bool? isActive = null;
            if (filter == "active")
            {
                isActive = true;
            }
            else if (filter == "inactive")
            {
                isActive = false;
            }
            else
            {
                filter = "all";
            }

            return new EndUserListViewModel
            {
                Search = search,
                Status = filter,
                Items = _users.Search(search, isActive)
            };
        }

        public EndUser Get(int id)
        {
            return _users.GetById(id);
        }

        public EndUserDetailsViewModel GetDetails(int id, int currentUserId)
        {
            var user = _users.GetById(id);
            if (user == null)
            {
                return null;
            }

            return new EndUserDetailsViewModel
            {
                User = user,
                IsSelf = user.EndUserId == currentUserId,
                Transactions = _users.GetTransactions(id)
            };
        }

        public IList<SelectListItem> GetRoleOptions(int selectedRoleId)
        {
            return _users.GetRoles()
                .Select(role => new SelectListItem
                {
                    Value = role.RoleId.ToString(),
                    Text = role.RoleName,
                    Selected = role.RoleId == selectedRoleId
                })
                .ToList();
        }

        public int Create(EndUserFormViewModel model)
        {
            EnsureUsernameAvailable(model.Username, null);
            var entity = ToEntity(model);
            entity.PasswordHash = PasswordHasher.Hash(model.Password);
            return _users.Create(entity);
        }

        public void Update(EndUserFormViewModel model, int currentUserId)
        {
            var existing = _users.GetById(model.EndUserId);
            if (existing == null)
            {
                throw new InvalidOperationException("User was not found.");
            }

            EnsureUsernameAvailable(model.Username, model.EndUserId);

            var newRoleName = RoleNameFor(model.RoleId);
            if (IsLastActiveAdmin(existing) && !string.Equals(newRoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(LastAdminMessage);
            }

            if (existing.EndUserId == currentUserId)
            {
                model.RoleId = existing.RoleId;
            }

            var entity = ToEntity(model);
            entity.EndUserId = model.EndUserId;
            var changePassword = !string.IsNullOrWhiteSpace(model.Password);
            if (changePassword)
            {
                entity.PasswordHash = PasswordHasher.Hash(model.Password);
            }

            _users.Update(entity, changePassword);
        }

        public void SetActive(int id, bool isActive, int currentUserId)
        {
            var existing = _users.GetById(id);
            if (existing == null)
            {
                throw new InvalidOperationException("User was not found.");
            }

            if (id == currentUserId && !isActive)
            {
                throw new InvalidOperationException(CannotDeactivateSelfMessage);
            }

            if (!isActive && IsLastActiveAdmin(existing))
            {
                throw new InvalidOperationException(LastAdminMessage);
            }

            _users.SetActive(id, isActive);
        }

        public static EndUserFormViewModel ToForm(EndUser user, bool isSelf)
        {
            return new EndUserFormViewModel
            {
                EndUserId = user.EndUserId,
                Username = user.Username,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                IsNew = false,
                IsSelf = isSelf
            };
        }

        private void EnsureUsernameAvailable(string username, int? excludeUserId)
        {
            if (_users.UsernameExists(username, excludeUserId))
            {
                throw new InvalidOperationException(DuplicateUsernameMessage);
            }
        }

        private bool IsLastActiveAdmin(EndUser user)
        {
            return user.IsActive
                && string.Equals(user.RoleName, "Admin", StringComparison.OrdinalIgnoreCase)
                && _users.CountActiveAdmins() <= 1;
        }

        private string RoleNameFor(int roleId)
        {
            var role = _users.GetRoles().FirstOrDefault(item => item.RoleId == roleId);
            return role == null ? string.Empty : role.RoleName;
        }

        private static EndUser ToEntity(EndUserFormViewModel model)
        {
            return new EndUser
            {
                Username = (model.Username ?? string.Empty).Trim(),
                FullName = (model.FullName ?? string.Empty).Trim(),
                RoleId = model.RoleId
            };
        }
    }
}

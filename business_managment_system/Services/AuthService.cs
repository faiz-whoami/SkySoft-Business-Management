using business_managment_system.Data;
using business_managment_system.Helpers;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class AuthService
    {
        private readonly EndUserRepository _users;

        public AuthService()
            : this(new EndUserRepository())
        {
        }

        public AuthService(EndUserRepository users)
        {
            _users = users;
        }

        public LoginAttemptResult AttemptLogin(string username, string password)
        {
            var user = _users.GetByUsername((username ?? string.Empty).Trim());
            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                return LoginAttemptResult.InvalidCredentials();
            }

            if (!user.IsActive)
            {
                return LoginAttemptResult.Inactive();
            }

            return LoginAttemptResult.Success(user);
        }
    }

    public class LoginAttemptResult
    {
        public bool Succeeded { get; private set; }
        public bool IsInactive { get; private set; }
        public EndUser User { get; private set; }

        public static LoginAttemptResult Success(EndUser user)
        {
            return new LoginAttemptResult { Succeeded = true, User = user };
        }

        public static LoginAttemptResult InvalidCredentials()
        {
            return new LoginAttemptResult();
        }

        public static LoginAttemptResult Inactive()
        {
            return new LoginAttemptResult { IsInactive = true };
        }
    }
}

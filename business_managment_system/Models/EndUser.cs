namespace business_managment_system.Models
{
    public class EndUser
    {
        public int EndUserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}

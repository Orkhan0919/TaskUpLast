namespace TaskUp.ViewModels.Admin
{
    public class UserWithRoleVM
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }
        public string JoinedAt { get; set; }
        public bool IsAdmin => Roles != null && Roles.Contains("Admin");
    }
}
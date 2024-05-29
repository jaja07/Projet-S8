namespace TestS8.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public ICollection<string> Roles { get; set; }
    }
}

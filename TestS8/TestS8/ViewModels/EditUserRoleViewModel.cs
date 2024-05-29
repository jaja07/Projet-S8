using Microsoft.AspNetCore.Identity;

namespace TestS8.ViewModels
{
    public class EditUserRoleViewModel
    {
        public EditUserRoleViewModel()
        {
            Roles = new List<string>();
            AllRoles = new List<IdentityRole>();
        }
        public string UserId { get; set; }
        public string Email { get; set; }
        public ICollection<string> Roles { get; set; }
        public ICollection<IdentityRole> AllRoles { get; internal set; }
    }
}

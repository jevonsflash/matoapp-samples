using Abp.Authorization;
using MatoAppSample.Authorization.Roles;
using MatoAppSample.Authorization.Users;

namespace MatoAppSample.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}

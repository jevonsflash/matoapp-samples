using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq;
using Abp.Organizations;
using MatoAppSample.Authorization.Roles;
using System.Threading.Tasks;

namespace MatoAppSample.Authorization.Users
{
    public class UserStore : AbpUserStore<Role, User>
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UserStore(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, long> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<UserLogin, long> userLoginRepository,
            IRepository<UserClaim, long> userClaimRepository,
            IRepository<UserPermissionSetting, long> userPermissionSettingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository)
            : base(unitOfWorkManager,
                  userRepository,
                  roleRepository,
                  userRoleRepository,
                  userLoginRepository,
                  userClaimRepository,
                  userPermissionSettingRepository,
                  userOrganizationUnitRepository,
                  organizationUnitRoleRepository
                  )
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        //override
        public async Task<User> FindByNameOrPhoneNumberAsync(string userNameOrPhoneNumber)
        {

            return await UserRepository.FirstOrDefaultAsync(
                user => user.NormalizedUserName == userNameOrPhoneNumber || user.PhoneNumber == userNameOrPhoneNumber
            );
        }

        [UnitOfWork]
        public async Task<User> FindByNameOrPhoneNumberAsync(int? tenantId, string userNameOrPhoneNumber)
        {
            using (_unitOfWorkManager.Current.SetTenantId(tenantId))
            {
                return await FindByNameOrPhoneNumberAsync(userNameOrPhoneNumber);
            }
        }

    }
}

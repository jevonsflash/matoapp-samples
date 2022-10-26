using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using System.Threading.Tasks;
using Abp.UI;
using System.Linq;
using MatoAppSample.Authorization.Roles;


namespace MatoAppSample.Authorization.Users
{
    public class UserManager : AbpUserManager<Role, User>
    {
        private readonly UserStore _store;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<UserLogin, long> userLoginRepository;

        public UserManager(
            RoleManager roleManager,
            UserStore store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger,
            IPermissionManager permissionManager,
            IUnitOfWorkManager unitOfWorkManager,
            ICacheManager cacheManager,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IOrganizationUnitSettings organizationUnitSettings,
            IRepository<UserLogin, long> userLoginRepository,
            ISettingManager settingManager)
            : base(
                roleManager,
                store,
                optionsAccessor,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services,
                logger,
                permissionManager,
                unitOfWorkManager,
                cacheManager,
                organizationUnitRepository,
                userOrganizationUnitRepository,
                organizationUnitSettings,
                settingManager,
                userLoginRepository)
        {
            _store = store;
            _unitOfWorkManager = unitOfWorkManager;
            this.userLoginRepository=userLoginRepository;
        }
        //override

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            var result = await CheckDuplicateUsernameOrPhoneNumber(user.Id, user.UserName, user.PhoneNumber);
            if (!result.Succeeded)
            {
                return result;
            }


            return await base.CreateAsync(user);
        }

        public override async Task<IdentityResult> UpdateAsync(User user)
        {
            var result = await CheckDuplicateUsernameOrPhoneNumber(user.Id, user.UserName, user.PhoneNumber);
            if (!result.Succeeded)
            {
                return result;
            }

            return await base.UpdateAsync(user);
        }

        public Task<User> FindByNameOrPhoneNumberAsync(int? tenantId, string userNameOrPhoneNumber)
        {
            return _store.FindByNameOrPhoneNumberAsync(tenantId, userNameOrPhoneNumber);
        }

        public override async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(long? expectedUserId, string userName, string emailAddress)
        {
            var user = await FindByNameAsync(userName);
            if (user != null && user.Id != expectedUserId)
            {
                throw new UserFriendlyException(string.Format(L("Identity.DuplicateUserName"), userName));
            }

            if (!string.IsNullOrEmpty(emailAddress))
            {
                user = await FindByEmailAsync(emailAddress);
                if (user != null && user.Id != expectedUserId)
                {
                    throw new UserFriendlyException(string.Format(L("Identity.DuplicateEmail"), emailAddress));
                }
            }
            return IdentityResult.Success;
        }


        public async Task<IdentityResult> CheckDuplicateUsernameOrPhoneNumber(long? expectedUserId, string userName, string phone)
        {
            var user = await FindByNameAsync(userName);
            if (user != null && user.Id != expectedUserId)
            {
                throw new UserFriendlyException(string.Format(L("Identity.DuplicateUserName"), userName));
            }

            user = await FindByNameOrPhoneNumberAsync(GetCurrentTenantId(), phone);
            if (user != null && user.Id != expectedUserId)
            {
                throw new UserFriendlyException("电话号码重复", phone);
            }

            return IdentityResult.Success;
        }


        public string GetOpenIdByUserId(long userId, int? tenantId, string loginProvider)
        {
            var openId = "";

            var query = from userLogin in userLoginRepository.GetAll()
                        join user in Users on userLogin.UserId equals user.Id
                        where user.Id==userId &&
                        userLogin.LoginProvider == loginProvider &&
                        userLogin.TenantId == tenantId
                        select userLogin;

            var currentUserLogin = query.FirstOrDefault();
            if (currentUserLogin!=null)
            {
                openId= currentUserLogin.ProviderKey;
            }
            else
            {
                throw new UserFriendlyException("此账号未绑定第三方账号");

            }
            return openId;
        }


        private int? GetCurrentTenantId()
        {
            if (_unitOfWorkManager.Current != null)
            {
                return _unitOfWorkManager.Current.GetTenantId();
            }

            return AbpSession.TenantId;
        }

    }
}

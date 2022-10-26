using Microsoft.AspNetCore.Identity;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Zero.Configuration;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using MatoAppSample.MultiTenancy;
using MatoAppSample.Authorization.Roles;
using MatoAppSample.Authorization.Users;

namespace MatoAppSample.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        private new readonly UserManager userManager;
        private readonly IPasswordHasher<User> passwordHasher;

        public LogInManager(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            IPasswordHasher<User> passwordHasher,
            RoleManager roleManager,
            UserClaimsPrincipalFactory claimsPrincipalFactory)
            : base(
                  userManager,
                  multiTenancyConfig,
                  tenantRepository,
                  unitOfWorkManager,
                  settingManager,
                  userLoginAttemptRepository,
                  userManagementConfig,
                  iocResolver,
                  passwordHasher,
                  roleManager,
                  claimsPrincipalFactory)
        {
            this.userManager=userManager;
            this.passwordHasher=passwordHasher;
        }
        public override async Task<AbpLoginResult<Tenant, User>> LoginAsync(
    string combinationName,
    string plainPassword,
    string tenancyName = null,
    bool shouldLockout = true)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var result = await LoginAsyncInternal(
                    combinationName,
                    plainPassword,
                    tenancyName,
                    shouldLockout
                );

                await SaveLoginAttemptAsync(result, tenancyName, combinationName);
                return result;
            });
        }

        protected override async Task<AbpLoginResult<Tenant, User>> LoginAsyncInternal(
            string combinationName,
            string plainPassword,
            string tenancyName,
            bool shouldLockout)
        {
            if (combinationName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(combinationName));
            }

            if (plainPassword.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(plainPassword));
            }

            //Get and check tenant
            Tenant tenant = null;
            using (UnitOfWorkManager.Current.SetTenantId(null))
            {
                if (!MultiTenancyConfig.IsEnabled)
                {
                    tenant = await GetDefaultTenantAsync();
                }
                else if (!string.IsNullOrWhiteSpace(tenancyName))
                {
                    tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                    if (tenant == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                    }

                    if (!tenant.IsActive)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                    }
                }
            }

            var tenantId = tenant == null ? (int?)null : tenant.Id;
            using (UnitOfWorkManager.Current.SetTenantId(tenantId))
            {
                await UserManager.InitializeOptionsAsync(tenantId);

                //TryLoginFromExternalAuthenticationSources method may create the user, that's why we are calling it before AbpUserStore.FindByNameOrEmailAsync
                var loggedInFromExternalSource =
                    await TryLoginFromExternalAuthenticationSourcesAsync(combinationName, plainPassword, tenant);

                var user = await UserManager.FindByNameOrEmailAsync(tenantId, combinationName);
                if (user == null)
                {
                    user = await userManager.FindByNameOrPhoneNumberAsync(tenantId, combinationName);
                }
                if (user == null)
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                }

                if (!loggedInFromExternalSource)
                {
                    if (!await UserManager.CheckPasswordAsync(user, plainPassword))
                    {
                        if (shouldLockout)
                        {
                            if (await base.TryLockOutAsync(tenantId, user.Id))
                            {
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                            }
                        }

                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidPassword, tenant, user);
                    }

                    await UserManager.ResetAccessFailedCountAsync(user);
                }

                return await CreateLoginResultAsync(user, tenant);
            }
        }



        protected override async Task<bool> TryLoginFromExternalAuthenticationSourcesAsync(string userNamePhoneNumberOrEmailAddress,
    string plainPassword, Tenant tenant)
        {
            if (!UserManagementConfig.ExternalAuthenticationSources.Any())
            {
                return false;
            }

            foreach (var sourceType in UserManagementConfig.ExternalAuthenticationSources)
            {
                try
                {

             
                using (var source =
                    IocResolver.ResolveAsDisposable<IExternalAuthenticationSource<Tenant, User>>(sourceType))
                {
                    if (await source.Object.TryAuthenticateAsync(userNamePhoneNumberOrEmailAddress, plainPassword, tenant))
                    {
                        var tenantId = tenant == null ? (int?)null : tenant.Id;
                        using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                        {
                            var user = await userManager.FindByNameOrEmailAsync(tenantId, userNamePhoneNumberOrEmailAddress);

                            if (user == null)
                            {
                                user = await userManager.FindByNameOrPhoneNumberAsync(tenantId, userNamePhoneNumberOrEmailAddress);
                            }
                            if (user == null)
                            {
                                user = await source.Object.CreateUserAsync(userNamePhoneNumberOrEmailAddress, tenant);

                                user.TenantId = tenantId;
                                user.AuthenticationSource = source.Object.Name;
                                user.Password =
                                    passwordHasher.HashPassword(user,
                                        Guid.NewGuid().ToString("N")
                                            .Left(16)); //Setting a random password since it will not be used
                                user.SetNormalizedNames();

                                if (user.Roles == null)
                                {
                                    user.Roles = new List<UserRole>();
                                    foreach (var defaultRole in RoleManager.Roles
                                        .Where(r => r.TenantId == tenantId && r.IsDefault).ToList())
                                    {
                                        user.Roles.Add(new UserRole(tenantId, user.Id, defaultRole.Id));
                                    }
                                }

                                await UnitOfWorkManager.Current.SaveChangesAsync();


                                await userManager.CreateAsync(user);
                            }
                            else
                            {
                                await source.Object.UpdateUserAsync(user, tenant);

                                user.AuthenticationSource = source.Object.Name;

                                await userManager.UpdateAsync(user);
                            }

                            await UnitOfWorkManager.Current.SaveChangesAsync();

                            return true;
                        }
                    }
                }
             }
                catch (Exception ex)
                {

                    throw;
                }  }

            return false;
        }



    }
}

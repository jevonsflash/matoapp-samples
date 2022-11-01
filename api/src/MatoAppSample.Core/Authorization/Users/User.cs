using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.Extensions;

namespace MatoAppSample.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        //override
        public new const int MaxPhoneNumberLength = 11;

        [Required]
        [StringLength(MaxPhoneNumberLength)]
        public override string PhoneNumber { get; set; }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>(),
                PhoneNumber = ""
            };

            user.SetNormalizedNames();

            return user;
        }
    }
}

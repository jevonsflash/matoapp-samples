using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.MultiTenancy;
using MatoAppSample.Captcha;
using MatoAppSample.Captcha.Consts;
using MatoAppSample.Captcha.Sms;
using MatoAppSample.Helper;
using MatoAppSample.MultiTenancy;

namespace MatoAppSample.Authorization.Users
{

    public class PhoneNumberExternalAuthenticationSource : DefaultExternalAuthenticationSource<Tenant, User>, ITransientDependency
    {
        private readonly SmsCaptchaManager captchaManager;

        public PhoneNumberExternalAuthenticationSource(SmsCaptchaManager captchaManager)
        {
            this.captchaManager=captchaManager;
        }
        /// <inheritdoc/>
        public override string Name { get; } = "SMS验证码登录";

        /// <inheritdoc/>
        public override async Task<bool> TryAuthenticateAsync(string phoneNumber, string token, Tenant tenant)
        {
            //for test
            //return true;
            var currentItem = await captchaManager.GetToken(token);
            if (currentItem==null || currentItem.PhoneNumber!=phoneNumber || currentItem.Purpose!=CaptchaPurpose.LOGIN)
            {
                return false;
            }
            await captchaManager.RemoveToken(token);
            return true;
        }

        /// <inheritdoc/>
        public override Task<User> CreateUserAsync(string userNameOrEmailAddress, Tenant tenant)
        {
            var seed = Guid.NewGuid().ToString("N").Substring(0, 7);
            var surname = "手";
            var name = "机用户"+seed;
            var userName = PinyinUtil.PinYin(surname+name);

             var result = new User()
             {
                 Surname = surname,
                 Name = name,
                 UserName =  userName,
                 IsPhoneNumberConfirmed = true,
                 IsActive=true,
                 TenantId = tenant?.Id,
                 PhoneNumber = userNameOrEmailAddress,
                 Settings = null,
                 IsEmailConfirmed = true,
                 EmailAddress=$"{userName}@abc.com"
             };
            return Task.FromResult(result);

        }

        /// <inheritdoc/>
        public override Task UpdateUserAsync(User user, Tenant tenant)
        {
            return Task.FromResult(0);
        }

    }
}
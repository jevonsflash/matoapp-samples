using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Organizations;
using Abp.UI;
using Aliyun.Sms.Models;
using MatoAppSample.Authorization.Users;
using MatoAppSample.Captcha;
using MatoAppSample.Captcha.Consts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Email
{
    public class EmailCaptchaManager : DomainService, ICaptchaManager
    {
        private readonly EmailCaptchaTokenCache captchaTokenCache;


        public static TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(5);

        public EmailCaptchaManager(EmailCaptchaTokenCache captchaTokenCache)
        {
            this.captchaTokenCache=captchaTokenCache;

        }

        public async Task SendCaptchaAsync(long? userId, string phoneNumber, string purpose)
        {
            throw new NotImplementedException();
        }



        public async Task<bool> VerifyCaptchaAsync(string token, string purpose = CaptchaPurpose.IDENTITY_VERIFICATION)
        {

            throw new NotImplementedException();
        }




        public async Task BindAsync(string token)
        {
            throw new NotImplementedException();
        }


        public async Task UnbindAsync(string token)
        {
            throw new NotImplementedException();

        }

    }
}

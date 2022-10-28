using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Organizations;
using Abp.UI;
using Aliyun.Sms.Models;
using Aliyun.Sms.Services;
using MatoAppSample.Authorization.Users;
using MatoAppSample.Captcha.Consts;
using MatoAppSample.Helper;
using Newtonsoft.Json;
using Sms;
using Sms.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Sms
{
    public class SmsCaptchaManager : DomainService, ICaptchaManager
    {
        private readonly ISmsService SmsService;
        private readonly UserManager _userManager;
        private readonly SmsCaptchaTokenCache captchaTokenCache;


        public static TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(5);

        public SmsCaptchaManager(ISmsService SmsService,
            UserManager userManager,
            SmsCaptchaTokenCache captchaTokenCache
            )
        {
            this.SmsService=SmsService;
            _userManager=userManager;
            this.captchaTokenCache=captchaTokenCache;

        }

        public async Task SendCaptchaAsync(long userId, string phoneNumber, string purpose)
        {
            var captcha = CommonHelper.GetRandomCaptchaNumber();
            var model = new SendSmsRequest();
            model.PhoneNumbers= phoneNumber;
            model.SignName="MatoApp";
            //model.SignName="番茄系列网";
            model.TemplateCode= purpose switch
            {
                CaptchaPurpose.BIND_PHONENUMBER => "SMS_255330989",
                //CaptchaPurpose.BIND_PHONENUMBER => "1587660",
                CaptchaPurpose.UNBIND_PHONENUMBER => "SMS_255330989",
                CaptchaPurpose.LOGIN => "SMS_255330989",
                CaptchaPurpose.IDENTITY_VERIFICATION => "SMS_255330989"
            };
            //for aliyun
            model.TemplateParam= JsonConvert.SerializeObject(new { code = captcha });

            //for tencent-cloud
            //model.TemplateParam= JsonConvert.SerializeObject(new string[] {captcha});


            var result = await SmsService.SendSmsAsync(model);

            if (string.IsNullOrEmpty(result.BizId) && result.Code!="OK")
            {
                throw new UserFriendlyException("验证码发送失败，错误信息:"+result.Message);
            }

            await captchaTokenCache.SetAsync(captcha, new SmsCaptchaTokenCacheItem()
            {
                PhoneNumber=phoneNumber,
                UserId=userId,
                Purpose=purpose
            }, absoluteExpireTime: DateTimeOffset.Now.Add(TokenCacheDuration));
        }




        public async Task<bool> VerifyCaptchaAsync(string token, string purpose = CaptchaPurpose.IDENTITY_VERIFICATION)
        {
            SmsCaptchaTokenCacheItem currentItem = await GetToken(token);
            if (currentItem==null || currentItem.Purpose!=purpose)
            {
                return false;
            }
            await RemoveToken(token);
            return true;
        }




        public async Task BindAsync(string token)
        {
            SmsCaptchaTokenCacheItem currentItem = await GetToken(token);
            if (currentItem==null || currentItem.Purpose!=CaptchaPurpose.BIND_PHONENUMBER)
            {
                throw new UserFriendlyException("验证码不正确或已过期");
            }

            var user = await _userManager.GetUserByIdAsync(currentItem.UserId);
            if (user.IsPhoneNumberConfirmed)
            {
                throw new UserFriendlyException("已绑定手机，请先解绑后再绑定");
            }
            user.PhoneNumber=currentItem.PhoneNumber;
            user.IsPhoneNumberConfirmed=true;
            await _userManager.UpdateAsync(user);
            await RemoveToken(token);
        }


        public async Task UnbindAsync(string token)
        {
            SmsCaptchaTokenCacheItem currentItem = await GetToken(token);
            if (currentItem==null|| currentItem.Purpose!=CaptchaPurpose.UNBIND_PHONENUMBER)
            {
                throw new UserFriendlyException("验证码不正确或已过期");
            }

            var user = await _userManager.GetUserByIdAsync(currentItem.UserId);
            user.IsPhoneNumberConfirmed=false;
            await _userManager.UpdateAsync(user);
            await RemoveToken(token);

        }

        public Task RemoveToken(string token)
        {
            return captchaTokenCache.RemoveAsync(token);
        }

        public async Task<SmsCaptchaTokenCacheItem> GetToken(string token)
        {
            return await captchaTokenCache.GetAsync(token, null);
        }


    }
}

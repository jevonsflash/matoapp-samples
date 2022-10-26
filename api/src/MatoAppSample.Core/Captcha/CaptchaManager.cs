using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Organizations;
using Abp.UI;
using Aliyun.Sms.Models;
using Aliyun.Sms.Services;
using MatoAppSample.Authorization.Users;
using MatoAppSample.Captcha.Cache;
using MatoAppSample.Captcha.Consts;
using MatoAppSample.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha
{
    public class CaptchaManager : DomainService
    {
        private readonly IAliyunSmsService aliyunSmsService;
        private readonly UserManager _userManager;
        private readonly CaptchaTokenCache captchaTokenCache;


        public static TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(5);

        public CaptchaManager(IAliyunSmsService aliyunSmsService,
            UserManager userManager,
            CaptchaTokenCache captchaTokenCache
            )
        {
            this.aliyunSmsService=aliyunSmsService;
            _userManager=userManager;
            this.captchaTokenCache=captchaTokenCache;

        }

        public async Task<SendSmsResponse> SendCaptchaAsync(long userId, string phoneNumber, string purpose)
        {
            var captcha = CommonHelper.GetRandomCaptchaNumber();
            var model = new SendSmsRequest();
            model.PhoneNumbers= phoneNumber;
            model.SignName="MatoApp";
            model.TemplateCode= purpose switch
            {
                CaptchaPurpose.BIND_PHONENUMBER => "SMS_255330989",
                CaptchaPurpose.UNBIND_PHONENUMBER => "SMS_255330989",
                CaptchaPurpose.LOGIN => "SMS_255330989",
                CaptchaPurpose.IDENTITY_VERIFICATION => "SMS_255330989"
            };
            model.TemplateParam= JsonConvert.SerializeObject(new { code = captcha });

            var result = await aliyunSmsService.SendSmsAsync(model);

            await captchaTokenCache.SetAsync(captcha, new CaptchaTokenCacheItem()
            {
                PhoneNumber=phoneNumber,
                UserId=userId,
                Purpose=purpose
            }, absoluteExpireTime: DateTimeOffset.Now.Add(TokenCacheDuration));
            return result;

        }

        public async Task<QuerySendDetailResponse> GetSendDetail(string phoneNumber, DateTime sendDate)
        {
            var model = new QuerySendDetailRequest();
            model.PhoneNumbers= phoneNumber;
            model.SendDate=sendDate;
            var result = await aliyunSmsService.QuerySendDetailsAsync(model);
            return result;
        }


        public async Task<bool> VerifyCaptchaAsync(string token, string purpose = CaptchaPurpose.IDENTITY_VERIFICATION)
        {
            CaptchaTokenCacheItem currentItem = await GetToken(token);
            if (currentItem==null || currentItem.Purpose!=purpose)
            {
                return false;
            }
            await RemoveToken(token);
            return true;
        }




        public async Task BindPhoneNumberAsync(string token)
        {
            CaptchaTokenCacheItem currentItem = await GetToken(token);
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


        public async Task UnbindPhoneNumberAsync(string token)
        {
            CaptchaTokenCacheItem currentItem = await GetToken(token);
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

        public async Task<CaptchaTokenCacheItem> GetToken(string token)
        {
            return await captchaTokenCache.GetAsync(token, null);
        }


    }
}

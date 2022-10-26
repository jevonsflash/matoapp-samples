using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Aliyun.Sms.Models;
using MatoAppSample.Authorization.Users;
using MatoAppSample.Captcha.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MatoAppSample.Captcha
{
    public class CaptchaAppService : ApplicationService
    {
        private readonly CaptchaManager captchaManager;

        public CaptchaAppService(CaptchaManager captchaManager,
            UserManager userManager
            )

        {
            this.captchaManager=captchaManager;
        }

        public async Task<QuerySendDetailResponse> GetSendDetail(string phoneNumber)
        {
            return await captchaManager.GetSendDetail(phoneNumber, DateTime.Now);
        }

        [HttpPost]
        public async Task<SendSmsResponse> SendAsync(SendCaptchaInput input)
        {
            return await captchaManager.SendCaptchaAsync(input.UserId, input.PhoneNumber, input.Type);
        }


        [HttpPost]
        public async Task VerifyAsync(VerifyCaptchaInput input)
        {
            await captchaManager.VerifyCaptchaAsync(input.Token);
        }

        [HttpPost]
        public async Task UnbindAsync(VerifyCaptchaInput input)
        {
            await captchaManager.UnbindPhoneNumberAsync(input.Token);

        }

        [HttpPost]
        public async Task BindAsync(VerifyCaptchaInput input)
        {
            await captchaManager.BindPhoneNumberAsync(input.Token);

        }
    }
}


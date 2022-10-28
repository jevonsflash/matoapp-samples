using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using MatoAppSample.Authorization.Users;
using MatoAppSample.Captcha.Dto;
using MatoAppSample.Captcha.Sms;
using Microsoft.AspNetCore.Mvc;
using Sms.Interfaces;

namespace MatoAppSample.Captcha
{
    public class CaptchaAppService : ApplicationService
    {
        private readonly SmsCaptchaManager captchaManager;

        public CaptchaAppService(SmsCaptchaManager captchaManager)
        {
            this.captchaManager=captchaManager;
        }


        [HttpPost]
        public async Task SendAsync(SendCaptchaInput input)
        {
            await captchaManager.SendCaptchaAsync(input.UserId, input.PhoneNumber, input.Type);
        }


        [HttpPost]
        public async Task VerifyAsync(VerifyCaptchaInput input)
        {
            await captchaManager.VerifyCaptchaAsync(input.Token);
        }

        [HttpPost]
        public async Task UnbindAsync(VerifyCaptchaInput input)
        {
            await captchaManager.UnbindAsync(input.Token);

        }

        [HttpPost]
        public async Task BindAsync(VerifyCaptchaInput input)
        {
            await captchaManager.BindAsync(input.Token);

        }
    }
}


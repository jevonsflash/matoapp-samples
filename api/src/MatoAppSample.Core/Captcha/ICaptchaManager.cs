using Aliyun.Sms.Models;
using Sms.Interfaces;
using System;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha
{
    public interface ICaptchaManager
    {
        Task BindAsync(string token);
        Task UnbindAsync(string token);
        Task SendCaptchaAsync(long? userId, string phoneNumber, string purpose);
        Task<bool> VerifyCaptchaAsync(string token, string purpose = "IDENTITY_VERIFICATION");
    }
}
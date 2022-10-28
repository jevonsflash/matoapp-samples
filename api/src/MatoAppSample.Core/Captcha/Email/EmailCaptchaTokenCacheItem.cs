using Abp.Dependency;
using MatoAppSample.Captcha.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Email
{

    public class EmailCaptchaTokenCacheItem : CaptchaTokenCacheItem
    {
        public string EmailAddress { get; set; }
    }

}

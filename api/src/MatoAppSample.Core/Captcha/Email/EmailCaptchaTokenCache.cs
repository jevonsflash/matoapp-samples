using Abp.Dependency;
using MatoAppSample.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Email
{

    public class EmailCaptchaTokenCache : MemoryCacheBase<EmailCaptchaTokenCacheItem>, ISingletonDependency
    {
        public EmailCaptchaTokenCache() : base(nameof(EmailCaptchaTokenCache))
        {

        }
    }

}

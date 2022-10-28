using Abp.Dependency;
using MatoAppSample.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Sms
{

    public class SmsCaptchaTokenCache : MemoryCacheBase<SmsCaptchaTokenCacheItem>, ISingletonDependency
    {
        public SmsCaptchaTokenCache() : base(nameof(SmsCaptchaTokenCache))
        {

        }
    }

}

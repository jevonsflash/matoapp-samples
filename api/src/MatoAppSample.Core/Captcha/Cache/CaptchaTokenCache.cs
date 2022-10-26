using Abp.Dependency;
using MatoAppSample.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Cache
{

    public class CaptchaTokenCache : MemoryCacheBase<CaptchaTokenCacheItem>, ISingletonDependency
    {
        public CaptchaTokenCache() : base(nameof(CaptchaTokenCache))
        {

        }
    }

}

using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoAppSample.Captcha.Cache
{

    public class CaptchaTokenCacheItem
    {
        public long UserId { get; set; }

        public string Purpose { get; set; }

    }

}

using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace MatoAppSample.Controllers
{
    public abstract class MatoAppSampleControllerBase: AbpController
    {
        protected MatoAppSampleControllerBase()
        {
            LocalizationSourceName = MatoAppSampleConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}

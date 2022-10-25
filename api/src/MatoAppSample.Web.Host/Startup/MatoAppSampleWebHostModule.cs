using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoAppSample.Configuration;

namespace MatoAppSample.Web.Host.Startup
{
    [DependsOn(
       typeof(MatoAppSampleWebCoreModule))]
    public class MatoAppSampleWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public MatoAppSampleWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoAppSampleWebHostModule).GetAssembly());
        }
    }
}

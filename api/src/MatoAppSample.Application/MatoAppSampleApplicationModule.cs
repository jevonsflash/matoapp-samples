using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoAppSample.Authorization;

namespace MatoAppSample
{
    [DependsOn(
        typeof(MatoAppSampleCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class MatoAppSampleApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<MatoAppSampleAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(MatoAppSampleApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}

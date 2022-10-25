using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using MatoAppSample.EntityFrameworkCore;
using MatoAppSample.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace MatoAppSample.Web.Tests
{
    [DependsOn(
        typeof(MatoAppSampleWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class MatoAppSampleWebTestModule : AbpModule
    {
        public MatoAppSampleWebTestModule(MatoAppSampleEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(MatoAppSampleWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(MatoAppSampleWebMvcModule).Assembly);
        }
    }
}
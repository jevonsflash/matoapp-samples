using Abp.Application.Services;
using MatoAppSample.MultiTenancy.Dto;

namespace MatoAppSample.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}


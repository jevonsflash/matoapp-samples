using System.Threading.Tasks;
using Abp.Application.Services;
using MatoAppSample.Authorization.Accounts.Dto;

namespace MatoAppSample.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}

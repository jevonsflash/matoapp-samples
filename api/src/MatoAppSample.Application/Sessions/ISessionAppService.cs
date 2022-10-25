using System.Threading.Tasks;
using Abp.Application.Services;
using MatoAppSample.Sessions.Dto;

namespace MatoAppSample.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}

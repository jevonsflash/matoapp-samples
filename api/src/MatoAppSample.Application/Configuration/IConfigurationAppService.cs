using System.Threading.Tasks;
using MatoAppSample.Configuration.Dto;

namespace MatoAppSample.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}

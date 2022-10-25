using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using MatoAppSample.Configuration.Dto;

namespace MatoAppSample.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : MatoAppSampleAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}

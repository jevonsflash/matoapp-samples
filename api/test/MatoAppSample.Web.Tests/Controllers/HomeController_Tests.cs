using System.Threading.Tasks;
using MatoAppSample.Models.TokenAuth;
using MatoAppSample.Web.Controllers;
using Shouldly;
using Xunit;

namespace MatoAppSample.Web.Tests.Controllers
{
    public class HomeController_Tests: MatoAppSampleWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}
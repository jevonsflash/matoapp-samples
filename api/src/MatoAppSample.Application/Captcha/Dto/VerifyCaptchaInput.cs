using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Roles;

namespace MatoAppSample.Captcha.Dto
{
    public class VerifyCaptchaInput
    {

        [Required]
        public string Token { get; set; }


    }
}

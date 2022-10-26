using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Roles;

namespace MatoAppSample.Captcha.Dto
{
    public class SendCaptchaInput
    {
        public long UserId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }


        [Required]
        public string Type { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace MatoAppSample.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}
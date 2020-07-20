using System.ComponentModel.DataAnnotations;

namespace ResorgApi.Models.Accounts
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}

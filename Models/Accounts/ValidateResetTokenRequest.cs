using System.ComponentModel.DataAnnotations;

namespace ResorgApi.Models.Accounts
{
    public class ValidateResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ResorgApi.Models.Accounts
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

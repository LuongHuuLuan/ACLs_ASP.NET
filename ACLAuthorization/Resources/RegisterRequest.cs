using System.ComponentModel.DataAnnotations;

namespace ACLAuthorization.Resources
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        [Required(ErrorMessage = "Password field is Require.")]

        public string Password { get; set; }

    }
}

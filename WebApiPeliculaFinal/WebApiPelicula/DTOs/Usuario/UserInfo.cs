using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Usuario
{
    public class UserInfo
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

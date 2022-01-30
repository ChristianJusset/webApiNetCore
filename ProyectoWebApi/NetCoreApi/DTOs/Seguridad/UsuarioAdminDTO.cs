using System.ComponentModel.DataAnnotations;

namespace NetCoreApi.DTOs.Seguridad
{
    public class UsuarioAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

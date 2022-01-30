using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Suscripcion.Restriccion
{
    public class CrearRestriccionIPDTO
    {
        public int LlaveId { get; set; }
        [Required]
        public string IP { get; set; }
    }
}

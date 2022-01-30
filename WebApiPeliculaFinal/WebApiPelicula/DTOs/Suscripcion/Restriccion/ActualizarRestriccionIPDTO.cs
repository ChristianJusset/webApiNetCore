using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Suscripcion.Restriccion
{
    public class ActualizarRestriccionIPDTO
    {
        [Required]
        public string IP { get; set; }
    }
}

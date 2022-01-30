using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Suscripcion.Restriccion
{
    public class ActualizarRestriccionDominioDTO
    {
        [Required]
        public string Dominio { get; set; }
    }
}

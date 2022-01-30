using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Suscripcion.Restriccion
{
    public class CrearRestriccionesDominioDTO
    {
        public int LlaveId { get; set; }
        [Required]
        public string Dominio { get; set; }
    }
}

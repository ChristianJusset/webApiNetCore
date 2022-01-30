using System.ComponentModel.DataAnnotations;

namespace WebApiPelicula.DTOs.Genero
{
    public class GeneroCreacionDTO
    {
        [Required]
        [StringLength(40)]
        public string Nombre { get; set; }
    }
}

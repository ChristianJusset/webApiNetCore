using NetCoreApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NetCoreApi.DTOs.Libro
{
    public class LibroCreacionDTO
    {
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250)]
        [Required]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }

        public List<int> AutoresIds
        {
            get; set;
        }
    }
}

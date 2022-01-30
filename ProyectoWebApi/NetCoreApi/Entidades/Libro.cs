using NetCoreApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NetCoreApi.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [Required]
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }
        public DateTime? FechaPublicacion { get; set; }

        // propiedad de nevagacion para traer la lista de comentarios del libro
        public List<Comentario> Comentarios { get; set; }

        public List<AutorLibro> AutoresLibros { get; set; }

    }
}

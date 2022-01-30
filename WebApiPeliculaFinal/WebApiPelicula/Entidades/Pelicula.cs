using System.ComponentModel.DataAnnotations;
using WebApiPelicula.Entidades.Interface;

namespace WebApiPelicula.Entidades
{
    public class Pelicula: IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(300)]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstreno { get; set; }
        public string Poster { get; set; }

        // Propiedades de navegacion: no es necesario mandarlo a la BD
        public List<PeliculaActor> PeliculasActores { get; set; }
        public List<PeliculaGenero> PeliculasGeneros { get; set; }
    }
}

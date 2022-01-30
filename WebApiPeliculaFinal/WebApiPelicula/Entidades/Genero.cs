using System.ComponentModel.DataAnnotations;
using WebApiPelicula.Entidades.Interface;

namespace WebApiPelicula.Entidades
{
    public class Genero: IId
    {
        public int Id { get; set; }
        //Required: en la BD se mostrará como not null
        [Required]
        [StringLength(40)]
        public string Nombre { get; set; }

        // Propiedades de navegacion: no es necesario mandarlo a la BD
        public List<PeliculaGenero> PeliculasGeneros { get; set; }
    }
}

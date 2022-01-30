using System.ComponentModel.DataAnnotations;
using WebApiPelicula.Entidades.Interface;

namespace WebApiPelicula.Entidades
{
    public class Actor: IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Foto { get; set; }

        // Propiedades de navegacion: no es necesario mandarlo a la BD
        public List<PeliculaActor> PeliculasActores { get; set; }
    }
}

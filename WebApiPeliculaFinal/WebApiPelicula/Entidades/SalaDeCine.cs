using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using WebApiPelicula.Entidades.Interface;

namespace WebApiPelicula.Entidades
{
    public class SalaDeCine: IId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }
        public Point Ubicacion { get; set; }
        public List<PeliculaSalaDeCine> PeliculasSalasDeCines { get; set; }
    }
}

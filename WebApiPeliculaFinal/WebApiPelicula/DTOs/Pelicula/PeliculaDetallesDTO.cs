using WebApiPelicula.DTOs.Actor;
using WebApiPelicula.DTOs.Genero;

namespace WebApiPelicula.DTOs.Pelicula
{
    public class PeliculaDetallesDTO: PeliculaDTO
    {

        public List<GeneroDTO> Generos { get; set; }
        public List<ActorPeliculaDetalleDTO> Actores { get; set; }
    }
}

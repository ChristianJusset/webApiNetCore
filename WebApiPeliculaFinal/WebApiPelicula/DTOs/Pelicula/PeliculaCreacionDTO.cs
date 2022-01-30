using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApiPelicula.Helpers.Binder;
using WebApiPelicula.Validaciones;

namespace WebApiPelicula.DTOs.Pelicula
{
    public class PeliculaCreacionDTO
    {
        [Required]
        [StringLength(300)]
        public string Titulo { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechaEstreno { get; set; }

        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }

        // utilizando binderType para mappear la lista de entero
        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenerosIds { get; set; }

        // utilizando binderType para mappear la lista de entero
        [ModelBinder(BinderType = typeof(TypeBinder<List<PeliculaActorCreacionDTO>>))]
        public List<PeliculaActorCreacionDTO> Actores { get; set; }

    }
}

using NetCoreApi.DTOs.Comentario;

namespace NetCoreApi.DTOs.Libro
{
    public class LibroComentarioDTO: LibroDTO
    {
        public List<ComentarioDTO> Comentarios { get; set; }
    }
}

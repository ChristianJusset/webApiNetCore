using NetCoreApi.DTOs.Autor;

namespace NetCoreApi.DTOs.Libro
{
    public class LibroAutorDTO: LibroDTO
    {
        public List<AutorDTO> Autores { get; set; }
    }
}

using NetCoreApi.DTOs.Libro;

namespace NetCoreApi.DTOs.Autor
{
    public class AutorLibroDTO: AutorDTO
    {
        public List<LibroDTO> Libros { get; set; }
    }
}

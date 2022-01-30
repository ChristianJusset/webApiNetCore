using NetCoreApi.DTOs.Hateoas;

namespace NetCoreApi.DTOs.Autor
{
    //RecursoHateoas: para mandar las url en el DTO
    public class AutorDTO: RecursoHateoas
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}

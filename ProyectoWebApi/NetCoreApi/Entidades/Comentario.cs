using Microsoft.AspNetCore.Identity;

namespace NetCoreApi.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Contenido { get; set; }

        // para hacer referencia al libro(foreign key)
        public int LibroId { get; set; }

        // propiedad de navegacion para a qué libro corresponde el comentario
        public Libro Libro { get; set; }

        // asigna qué usuario realiza el comentario
        // IdentityUser: es obtenido de Identity 
        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }


    }
}

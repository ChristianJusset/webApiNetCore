using Microsoft.AspNetCore.Identity;

namespace WebApiPelicula.Entidades
{
    public class Usuario: IdentityUser
    {
        public bool MalaPaga { get; set; }
    }
}

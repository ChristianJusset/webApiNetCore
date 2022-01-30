using Microsoft.AspNetCore.Identity;

namespace WebApiPelicula.Entidades.Suscripciones
{
    public class Usuario: IdentityUser
    {
        public bool MalaPaga { get; set; }
    }
}

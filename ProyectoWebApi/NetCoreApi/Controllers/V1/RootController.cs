using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreApi.DTOs.Hateoas;

namespace NetCoreApi.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    // para obtener los datos del usuario en el caso se encuentre logueado
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService authorizationService;
        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        /*
         visualizar los recursos vía un controller
         */


        [HttpGet(Name = "ObtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatoHATEOAS>>> Get()
        {
            var datosHateoas = new List<DatoHATEOAS>();

            //User: controlBase
            var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");



            //self: mismo lugar donde se encuetra el usuario
            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("ObtenerRoot", new { }),
               descripcion: "self", metodo: "GET"));


            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }), descripcion: "autores",
                metodo: "GET"));

            // hay que loguearse como admin
            if (esAdmin.Succeeded)
            {
                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }), descripcion: "autor-crear",
               metodo: "POST"));

                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearLibro", new { }), descripcion: "libro-crear",
                    metodo: "POST"));
            }


            return datosHateoas;

        }

    }
}

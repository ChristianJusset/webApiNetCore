using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NetCoreApi.DTOs.Autor;
using NetCoreApi.DTOs.Hateoas;

namespace NetCoreApi.Servicios
{
    public class GeneradorEnlaces
    {
        //IAuthorizationService: validar si el usuario es administrador
        private readonly IAuthorizationService authorizationService;
        //obtener el usuario que se ha logueado(no hay controllBase): accede al HttpContext desde cualquier clase
        private readonly IHttpContextAccessor httpContextAccessor;
        //asignar la url(no hay controllBase)
        private readonly IActionContextAccessor actionContextAccessor;

        public GeneradorEnlaces(IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
            this.actionContextAccessor = actionContextAccessor;
        }

        private IUrlHelper ConstruirURLHelper()
        {
            var factoria = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(actionContextAccessor.ActionContext);
        }



        private async Task<bool> EsAdmin()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var resultado = await authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;

        }
        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = ConstruirURLHelper();
            autorDTO.Enlaces.Add(new DatoHATEOAS(
                enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                descripcion: "self",
                metodo: "GET"));


            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(
               enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
               descripcion: "autor-actualizar",
               metodo: "PUT"));

                autorDTO.Enlaces.Add(new DatoHATEOAS(
                    enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                    descripcion: "self",
                    metodo: "DELETE"));
            }
        }

    }
}

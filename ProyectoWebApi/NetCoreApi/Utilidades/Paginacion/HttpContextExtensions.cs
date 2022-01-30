using Microsoft.EntityFrameworkCore;

namespace NetCoreApi.Utilidades.Paginacion
{
    //HttpContextExtensions: en la cabecera de las respuestas se va a enviar la cantidad de registros
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }
            double cantidad = await queryable.CountAsync();
            httpContext.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());



        }
    }
}

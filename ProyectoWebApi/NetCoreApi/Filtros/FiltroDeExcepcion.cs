using Microsoft.AspNetCore.Mvc.Filters;

namespace NetCoreApi.Filtros
{
    // filtro global para obtener la excepcion no controlado, se va a ejecutar cuando ocurra alguna excepcion en cualqui lado
    public class FiltroDeExcepcion: ExceptionFilterAttribute
    {
        private readonly ILogger<FiltroDeExcepcion> logger;

        public FiltroDeExcepcion(ILogger<FiltroDeExcepcion> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);

            base.OnException(context);
        }

    }
}

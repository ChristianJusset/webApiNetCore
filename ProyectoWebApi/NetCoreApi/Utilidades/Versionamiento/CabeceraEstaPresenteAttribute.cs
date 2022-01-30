using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace NetCoreApi.Utilidades.Versionamiento
{
    // para el versionamiento en las cabeceras
    //IActionConstraint: validar si el http tiene unas caracteristicas, entonces va a utilizar algunos métodos
    public class CabeceraEstaPresenteAttribute : Attribute, IActionConstraint
    {
        private readonly string cabecera;
        private readonly string valor;

        public CabeceraEstaPresenteAttribute(string cabecera, string valor)
        {
            this.cabecera = cabecera;
            this.valor = valor;
        }

        public int Order => 0;


        public bool Accept(ActionConstraintContext context)
        {
            var cabeceras = context.RouteContext.HttpContext.Request.Headers;

            if (!cabeceras.ContainsKey(cabecera))
            {
                return false;
            }

            return string.Equals(cabeceras[cabecera], valor, StringComparison.OrdinalIgnoreCase);
        }
    }
}

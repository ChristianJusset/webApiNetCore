using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs.Suscripcion.Restriccion;
using WebApiPelicula.Entidades.Suscripciones;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/restriccionesdominio")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesDominioController: ControllerBase
    {
        private readonly ApplicationDbContext context;

        public RestriccionesDominioController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionesDominioDTO crearRestriccionesDominioDTO)
        {
            var llaveDB = await context.LlaveAPI.FirstOrDefaultAsync(x => x.Id == crearRestriccionesDominioDTO.LlaveId);

            if (llaveDB == null)
            {
                return NotFound();
            }

            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;


            if (llaveDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }
            var restriccionDominio = new RestriccionDominio()
            {
                LlaveId = crearRestriccionesDominioDTO.LlaveId,
                Dominio = crearRestriccionesDominioDTO.Dominio
            };

            context.Add(restriccionDominio);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionDominio.Include(x => x.Llave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB == null)
            {
                return NotFound();
            }

            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;

            if (usuarioId != restriccionDB.Llave.UsuarioId)
            {
                return Forbid();
            }

            context.Remove(restriccionDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionDominioDTO actualizarRestriccionDominio)
        {
            var restriccionDB = await context.RestriccionDominio.Include(x => x.Llave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB == null)
            {
                return NotFound();
            }

            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;

            if (restriccionDB.Llave.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            restriccionDB.Dominio = actualizarRestriccionDominio.Dominio;

            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}

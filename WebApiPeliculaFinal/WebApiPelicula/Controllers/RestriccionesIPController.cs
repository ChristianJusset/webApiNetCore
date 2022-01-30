using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs.Suscripcion.Restriccion;
using WebApiPelicula.Entidades.Suscripciones;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/restriccionesip")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesIPController: ControllerBase
    {
        private readonly ApplicationDbContext context;

        public RestriccionesIPController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionIPDTO crearRestriccion)
        {
            var llaveDB = await context.LlaveAPI.FirstOrDefaultAsync(x => x.Id == crearRestriccion.LlaveId);

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

            var restriccionIP = new RestriccionIP
            {
                LlaveId = llaveDB.Id,
                IP = crearRestriccion.IP
            };

            context.Add(restriccionIP);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionIPDTO actualizarRestriccion)
        {
            var restriccionDB = await context.RestriccionIP.Include(x => x.Llave)
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

            restriccionDB.IP = actualizarRestriccion.IP;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {

            var restriccionDB = await context.RestriccionIP.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

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


    }
}

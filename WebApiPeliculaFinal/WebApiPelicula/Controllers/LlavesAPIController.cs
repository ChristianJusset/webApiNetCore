using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs.Suscripcion.Llave;
using WebApiPelicula.Servicios;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/llavesapi")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LlavesAPIController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ServicioLlaves servicioLlaves;

        public LlavesAPIController(ApplicationDbContext context,
            IMapper mapper, ServicioLlaves servicioLlaves)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioLlaves = servicioLlaves;
        }

        [HttpGet]
        public async Task<List<SuscripcionLlaveDTO>> MisLlaves()
        {
            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;
        
            var llaves = await context.LlaveAPI
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .Where(x => x.UsuarioId == usuarioId).ToListAsync();
            return mapper.Map<List<SuscripcionLlaveDTO>>(llaves);
        }

        [HttpPost]
        public async Task<ActionResult> CrearLlave(CrearSuscripcionLlaveDTO crearLlaveDTO)
        {
            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;

            if (crearLlaveDTO.TipoLlave == TipoLlave.Gratuita)
            {
                var elUsuarioYaTieneUnaLlaveGratuita = await context.LlaveAPI
               .AnyAsync(x => x.UsuarioId == usuarioId && x.TipoLlave == TipoLlave.Gratuita);

                if (elUsuarioYaTieneUnaLlaveGratuita)
                {
                    return BadRequest("El usuario ya tiene una llave gratuita");
                }
            }

            await servicioLlaves.CrearLlave(usuarioId, crearLlaveDTO.TipoLlave);
            return NoContent();

        }

        [HttpPut]
        public async Task<ActionResult> ActualizarLlave(ActualizarSuscripcionLlaveDTO actualizarLlaveDTO)
        {
            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;

            var llaveDB = await context.LlaveAPI.FirstOrDefaultAsync(x => x.Id == actualizarLlaveDTO.LlaveId);

            if (llaveDB == null)
            {
                return NotFound();
            }

            if (usuarioId != llaveDB.UsuarioId)
            {
                return Forbid();
            }

            if (actualizarLlaveDTO.ActualizarLlave)
            {
                llaveDB.Llave = servicioLlaves.GenerarLlave();
            }

            llaveDB.Activa = actualizarLlaveDTO.Activa;
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}

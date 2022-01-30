using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs;
using WebApiPelicula.DTOs.Actor;
using WebApiPelicula.Entidades;
using WebApiPelicula.Helpers.Paginacion;
using WebApiPelicula.Servicios;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : ControllerBase
    {
        private readonly string contenedor = "actores";

        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;


        public ActoresController(ApplicationDbContext context, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get()
        {
            var entidades = await context.Actor.ToListAsync();
            var dtos = mapper.Map<List<ActorDTO>>(entidades);
            return dtos;
        }

        [HttpGet("Paginacion")]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Actor.AsQueryable();
            
            // Pasar la cantidad de página en la cabecera de la respuesta
            await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.CantidadRegistrosPorPagina);

            // realizar la configuración de la cantidad de registros por cada AsQueryable
            var entidades = await queryable.Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<ActorDTO>>(entidades);

        }

   

        [HttpGet("{id}", Name = "obtenerActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var entidad = await context.Actor.FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)  return NotFound();
            
            return mapper.Map<ActorDTO>(entidad);
        }

        // version inicial con FromBody
        //[HttpPost]
        //public async Task<ActionResult> Post([FromBody] ActorCreacionDTO actorCreacionDTO)
        //{
        //    var entidad = mapper.Map<Actor>(actorCreacionDTO);
        //    context.Add(entidad);
        //    await context.SaveChangesAsync();
        //    var dto = mapper.Map<ActorDTO>(entidad);
        //    // en el header envía la ruta del obtenerGenero con el id 
        //    return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id }, dto);
        //}

        // segunda version con el FromForm
        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var entidad = mapper.Map<Actor>(actorCreacionDTO);

            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();

                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    entidad.Foto = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor,
                        actorCreacionDTO.Foto.ContentType);
                }
            }
            
            context.Add(entidad);
            await context.SaveChangesAsync();
            var dto = mapper.Map<ActorDTO>(entidad);
            return new CreatedAtRouteResult("obtenerActor", new { id = entidad.Id }, dto);
        }

        // version inicial donde actualiza todos los campos
        //[HttpPut("{id}")]
        //public async Task<ActionResult> Put(int id, [FromBody] ActorActualizacionDTO actorActualizacionDTO) 
        //{
        //    var actorDB = mapper.Map<Actor>(actorActualizacionDTO);
        //    actorDB.Id = id;
        //    context.Entry(actorDB).State = EntityState.Modified;
        //    await context.SaveChangesAsync();
        //    return NoContent();
        //}


        // segunda versión donde actualiza solo algunos campos
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorActualizacionDTO actorCreacionDTO)
        {
            var actorDB = await context.Actor.FirstOrDefaultAsync(x => x.Id == id);

            if (actorDB == null) { return NotFound(); }

            // hace un mapper para obtener solo los campos que se ha realizados cambios
            actorDB = mapper.Map(actorCreacionDTO, actorDB);


            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    actorDB.Foto = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor,
                        actorDB.Foto,
                        actorCreacionDTO.Foto.ContentType);
                }
            }

            await context.SaveChangesAsync();
            return NoContent();

        }


        //ActorPatchDTO: con el patch no se puede actualizar la foto
        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidadDB = await context.Actor.FirstOrDefaultAsync(x => x.Id == id);
            if (entidadDB == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<ActorPatchDTO>(entidadDB);
            patchDocument.ApplyTo(dto, ModelState);

            var isValid = TryValidateModel(dto);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(dto, entidadDB);

            await context.SaveChangesAsync();

            return NoContent();

        }



        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Actor.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Actor() { Id = id });
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreApi.DTOs.Comentario;
using NetCoreApi.DTOs.Paginacion;
using NetCoreApi.DTOs.Seguridad;
using NetCoreApi.Entidades;
using NetCoreApi.Utilidades.Paginacion;

namespace NetCoreApi.Controllers.V1
{
    [ApiController]
    // los comentarios dependen de la existencia del libro
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    public class ComentariosController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context,
            IMapper mapper,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }
        /*
            libroId: se obtiene el parametro del controlador
         
         */

        [HttpGet(Name = "obtenerComentariosLibro")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libro.AnyAsync(libroDB => libroDB.Id == libroId);
            
            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarios = await context.Comentario
               .Where(comentarioDB => comentarioDB.LibroId == libroId).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet(Name = "obtenerComentariosLibroPaginar")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId, [FromQuery] PaginacionDTO paginacionDTO)
        {
            var existeLibro = await context.Libro.AnyAsync(libroDB => libroDB.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var queryable = context.Comentario.Where(comentarioDB => comentarioDB.LibroId == libroId).AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var comentarios = await queryable.OrderBy(comentario => comentario.Id)
                .Paginar(paginacionDTO).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet("{id:int}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await context.Comentario.FirstOrDefaultAsync(comentarioDB => comentarioDB.Id == id);

            if (comentario == null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }


        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            // leyendo el claim del token
            //HttpContext: va a obtener los datos del claims debido a que tiene el authorize, el authorize puede estar a nivel de método o de controller
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            // busca el usuario por su email
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;


            var existeLibro = await context.Libro.AnyAsync(libroDB => libroDB.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            // usuario que realiza el comentario
            comentario.UsuarioId = usuarioId;
            context.Add(comentario);
            await context.SaveChangesAsync();

            // por buenas practicas solo retorna el comentario creado
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);

            // se esta pasando ambos parametros, uno para el controller y el otro para el parametro del metodo: new { id = comentario.Id, libroId = libroId }
            return CreatedAtRoute("ObtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDTO);

        }




        [HttpPut("{id:int}", Name = "actualizarComentario")]
        //libroId: id del controlador
        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libro.AnyAsync(libroDB => libroDB.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var existeComentario = await context.Comentario.AnyAsync(comentarioDB => comentarioDB.Id == id);
            
            if (!existeComentario)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();

        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreApi.DTOs.Autor;
using NetCoreApi.DTOs.Paginacion;
using NetCoreApi.Entidades;
using NetCoreApi.Filtros;
using NetCoreApi.Utilidades;
using NetCoreApi.Utilidades.Paginacion;

namespace NetCoreApi.Controllers.V1
{
    // ApiController: validacion del model binding
    [ApiController]
    [Route("api/v1/autores")]
    //[Route("api/[controller]")]
    //[Authorize]: filtro 
    //tienen que estar autenticado y tener el rol de Admin para realizar las acciones de Autores, la configuracion de la politica tiene que estar en el startup
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class AutoresController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutoresController> _logger;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;


        public AutoresController(ApplicationDbContext context, 
                                ILogger<AutoresController> logger,
                                 IMapper mapper,
                                 IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        /*
            Task<ActionResult<Autor>>: retorna un objeto  o valores que heredan de ActionResult(bad request)
            HttpGet("parametro") : para cambiar el nombre de la rua 
            HttpGet({"parametro"}) : Variables de ruta o parámetros de ruta
            HttpGet({"parametro"}) : por default se tiene marcado el [FromRoute]
            ModelBinding: mapear los valores de parámetro de ruta o del cuerpo de la petición a los parámetro de las acciones
            [FromHeader]: parametro del header
            [FromRoute] : parametro por la ruta
            [FromBody] : parametro en el cuerpo
            [FromQuery] : enviar valores como llave => api/autores/primero?nombre=parametroNombre
            ResponseCache: es un filtro que viene por default en VS
            Proveedores de configuracion: variables de configuración(orden 4), variables de ambiente(orden 2), user secret(orden 3), linea de comando(orden 1)
            name: se puede usar cuando se crea un nuevo recurso y se envía la ruta donde se encuentra la búsqueda del recurso registrado, además se puede utilizar parar obtener los enlaces para los Hateoas
         */

        [HttpGet("configuraciones")]
        public ActionResult<string> ObtenerConfiguracion()
        {
            var connection =  configuration["connectionStrings:defaultConnection"];

            // la configuracion se encuentra en appsetting
            // las variables tambien son unas variable de configuracion
            var variableAmbiente = configuration["apellidoVariableAmbiente"];

            // La configuracion user secret
            var apellidoVariableSecret = configuration["apellidoVariableSecret"];

            // La configuracion se encuentra en Properties
            var variableSetting = configuration["apellidoAppSetting"];


            return configuration["apellidoAppSetting"];
        }



        // va a retornar lo mismo por 10s
        [HttpGet("GUID")]
        // filtro del sistema
        [ResponseCache(Duration = 10)]
        public Guid ObtenerGuids()
        {
            Guid Guid = Guid.NewGuid();
            
            return Guid;
        }

        // url: api/autores
        [HttpGet(Name ="obtenerAutores")]
        // url: api/autores/listado
        [HttpGet("listado")]
        // url: listado
        [HttpGet("/listado")]
        // filtro personalizado
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        // no es protegida con el bearer
        [AllowAnonymous]
        
        public async Task<ActionResult<List<AutorDTO>>> Get()
        {
            _logger.LogInformation("Estamos obteniendo los autores");
            _logger.LogWarning("Este es un mensaje de prueba");

            var autores = await _context.Autor.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);

        }


        // url: api/autores
        //[HttpGet(Name = "obtenerAutoresPaginas")]
        [HttpGet("GetPaginas")]
        [AllowAnonymous]
        //FromQuery: se esta obteniendo un objeto para consulta
        public async Task<ActionResult<List<AutorDTO>>> GetPaginas([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _context.Autor.AsQueryable();
            //InsertarParametrosPaginacionEnCabecera: Pasar el HttpContext para utilizar el método
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);

            //Paginar: pasar el queryble para usar el el Paginar
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();

           
            return mapper.Map<List<AutorDTO>>(autores);

        }



        [HttpGet("GetHateoas")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        //incluirHateoas: tiene que ir con valor Y para ser leido en la configuracion del filtro
        public async Task<ActionResult<List<AutorDTO>>> GetHateoas()
        {
            _logger.LogInformation("Estamos obteniendo los autores");
            _logger.LogWarning("Este es un mensaje de prueba");

            var autores = await _context.Autor.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);

        }

        // url: api/autores/primero?nombre=parametroNombre
        [HttpGet("primero")] 
        public async Task<ActionResult<Autor>> PrimerAutor([FromQuery] string nombre)
        {
            // realizar otras cosas mientras se obtiene a informacion de la BD
            return await _context.Autor.FirstOrDefaultAsync();
        }

        // url: api/autores/2/aa
        // valor por defecto: persona
        [HttpGet("{id:int}/{param2=persona}")]
        public async Task<ActionResult<Autor>> Get(int id, string param2)
        {
            var autor = await _context.Autor.FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                // retorna 404
                return NotFound();
            }

            return autor;
        }

        // api/autores/nombre
        [HttpGet("{nombre}", Name ="obtenerAutorPorNombre")]
        //filtro para asignar las url(Hateoas) al DTO
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        [AllowAnonymous]
        //incluirHateoas: tiene que ir con valor Y para ser leido en la configuracion del filtro
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            // el contains puede encontrar varios valores
            var autores = await _context.Autor.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            if (autores == null)
            {
                return NotFound();
            }

            return mapper.Map<List<AutorDTO>>(autores);
            
        }

        [HttpGet("{id:int}", Name = "obtenerAutor")]
        public async Task<ActionResult<AutorDTO>> Get(int id)
        {
            var autor = await _context.Autor
                .FirstOrDefaultAsync(autorBD => autorBD.Id == id);

            if (autor == null)
            {
                return NotFound();
            }
            return mapper.Map<AutorDTO>(autor);
        }


        // autor con sus libros
        [HttpGet("AutorLibros/{id:int}")]
        public async Task<ActionResult<AutorLibroDTO>> GetAutorLibros(int id)   
        {
            var autor = await _context.Autor
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(autorBD => autorBD.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            autor.AutoresLibros = autor.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<AutorLibroDTO>(autor);

        }

        // url: api/autores
        // Task para que otros métodos asincronos necesitan invocar el método
        [HttpPost(Name = "crearAutor")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {

            //AnyAsync: retorna un booleano
            var existeAutorConElMismoNombre = await _context.Autor.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);

            _context.Add(autor);
            await _context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);
            // retonar el objeto creado: autorDTO
            return CreatedAtRoute("obtenerAutor", new { id = autor.Id }, autorDTO);
        }

       

        // url: api/autores/1 
        [HttpPut("{id:int}", Name ="actualizarAutor")] 
        public async Task<ActionResult> Put([FromBody] AutorCreacionDTO autorActualizacionDTO, int id)  
        {
            // valida si existe el registro para actualizar
            var existe = await _context.Autor.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorActualizacionDTO);
            autor.Id = id;

            _context.Update(autor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // url: api/autores/2


        /// <summary>
        /// Borra un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar</param>
        /// <returns></returns>
        /// 

        [HttpDelete("{id:int}", Name ="eliminarAutor")] 
        public async Task<ActionResult> Delete(int id)
        {
            // AnyAsync: si existe alguno
            var existe = await _context.Autor.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Autor() { Id = id });
            await _context.SaveChangesAsync();
            return Ok();

        }


    }
}

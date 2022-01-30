using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreApi.DTOs.Libro;
using NetCoreApi.Entidades;

namespace NetCoreApi.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context,
                                IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            // trae el libro con la referencia del autor
            var libro = await _context.Libro
                .FirstOrDefaultAsync(x=>x.Id == id);
                

            if (libro == null)
            {
                return NotFound();
            }

            return mapper.Map<LibroDTO>(libro);
        }


        [HttpGet("LibroComentarios/{id:int}")]
        public async Task<ActionResult<LibroComentarioDTO>> GetLibroComentarios(int id)
        {
            // trae el libro con la referencia del autor
            var libro = await _context.Libro
                .Include(libroDB => libroDB.Comentarios)
                .FirstOrDefaultAsync(x => x.Id == id);


            if (libro == null)
            {
                return NotFound();
            }

            return mapper.Map<LibroComentarioDTO>(libro);
        }

        [HttpGet("LibroAutores/{id:int}")]
        public async Task<ActionResult<LibroAutorDTO>> GetLibroAutores(int id)
        {
            // trae el libro con la referencia del autor
            var libro = await _context.Libro
                .Include(libroDB => libroDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            // la entidad de libro tiene un objeto intermedio que no el automapper no puede reconocer para eso 
            // hay que obtenerlo en al configuracion del automapperprofile

            if (libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroAutorDTO>(libro);
        }


        [HttpPost(Name = "crearLibro")]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            // validar si los ids son nulos, ya que los libros tienen que tener una autor
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }


            // validar si los autores existe en la base de datos
            var autoresIds = await _context.Autor
               .Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.Id)).Select(x => x.Id).ToListAsync();



            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
         
            AsignarOrdenAutores(libro);
            _context.Add(libro);
            await _context.SaveChangesAsync();

            // pasar el libro que se ha creado
            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);

        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroActualizacionDTO libroCreacionDTO)
        {
            var libroDB = await _context.Libro
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }


            // EF: mantiene la referencia de los valores de la BD, por ende se puede actualizar los campos
            libroDB = mapper.Map(libroCreacionDTO, libroDB);

            AsignarOrdenAutores(libroDB);

            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        // necesario para la actualizacion AddNewtonsoftJson
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var libroDB = await _context.Libro.FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound();
            }

            // identifica si hay cambios en los campos del request con la BD y le pone un estado
            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);
            patchDocument.ApplyTo(libroDTO, ModelState);

            var esValido = TryValidateModel(libroDTO);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO, libroDB);

            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}", Name = "eliminarLibro")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Libro.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Remove(new Libro() { Id = id });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

    }
}

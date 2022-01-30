using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApiPelicula.DTOs.Genero;
using WebApiPelicula.Entidades;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/generosCustom")]
    public class GenerosCustomController: CustomBaseController
    {
        public GenerosCustomController(ApplicationDbContext context,
           IMapper mapper)
           : base(context, mapper)
        {
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get()
        {
            return await Get<Genero, GeneroDTO>();
        }

        [HttpGet("{id:int}", Name = "obtenerGeneroCustom")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            return await Get<Genero, GeneroDTO>(id);
        }


        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            return await Post<GeneroCreacionDTO, Genero, GeneroDTO>(generoCreacionDTO, "obtenerGenero");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] GenerActualizacionDTO generoCreacionDTO)
        {
            return await Put<GenerActualizacionDTO, Genero>(id, generoCreacionDTO);
        }

        [HttpDelete("{id}")]
        
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Genero>(id);
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApiPelicula.DTOs;
using WebApiPelicula.DTOs.Actor;
using WebApiPelicula.Entidades;
using WebApiPelicula.Servicios;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/actoresCustom")]
    public class ActoresCustomController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "actores";

        public ActoresCustomController(ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos
            )
            : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet("Paginacion")]
        public async Task<ActionResult<List<ActorDTO>>> GetPaginacionCustom([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await Get<Actor, ActorDTO>(paginacionDTO);
        }
    }
}

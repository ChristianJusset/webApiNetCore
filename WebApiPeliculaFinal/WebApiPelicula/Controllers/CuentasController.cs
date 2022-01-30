using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiPelicula.DTOs;
using WebApiPelicula.DTOs.Usuario;
using WebApiPelicula.Servicios;

namespace WebApiPelicula.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : CustomBaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext context;
        private readonly ServicioLlaves servicioLlaves; 

        public CuentasController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IMapper mapper,
             ServicioLlaves servicioLlaves)
            : base(context, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            this.servicioLlaves = servicioLlaves;
            this.context = context;
        }

        [HttpPost("CrearUsuario")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // cuando se crea el usuario se va a crear automaticamente una llave gratuita
                await servicioLlaves.CrearLlave(user.Id, TipoLlave.Gratuita);
                return await ConstruirToken(model,user.Id);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }


        [HttpPost("CrearRol")]
        public async Task<ActionResult> CrearRol()
        {
            context.Roles.Add(new IdentityRole() { Name = "Admin", NormalizedName = "Admin" });
            context.SaveChanges();
            return NoContent();
        }


        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            var resultado = await _signInManager.PasswordSignInAsync(model.Email,
                model.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var usuario = await _userManager.FindByEmailAsync(model.Email);
                return await ConstruirToken(model, usuario.Id);
            }
            else
            {
                return BadRequest("Invalid login attempt");
            }
        }

        [HttpPost("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> Renovar()
        {
            var userInfo = new UserInfo
            {
                Email = HttpContext.User.Identity.Name
            };

            var idClaim = HttpContext.User.Claims.Where(claim => claim.Type == "id").FirstOrDefault();
            var usuarioId = idClaim.Value;

            return await ConstruirToken(userInfo, usuarioId);
        }

        [HttpGet("Usuarios")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<UsuarioDTO>>> Get([FromQuery] PaginacionDTO paginationDTO)
        {
            var queryable = context.Users.AsQueryable();
            queryable = queryable.OrderBy(x => x.Email);
            return await Get<IdentityUser, UsuarioDTO>(paginationDTO);
        }


        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarRolDTO editarAdminDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(editarAdminDTO.Email);
            //var usuario = await _userManager.FindByEmailAsync(editarAdminDTO.UsuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, "Admin"));
            return NoContent();
        }


        [HttpGet("Roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<List<string>>> GetRoles()
        {
            return await context.Roles.Select(x => x.Name).ToListAsync();
        }

        [HttpPost("AsignarRol")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> AsignarRol(EditarRolDTO editarRolDTO)
        {
            //var usuario = await _userManager.FindByIdAsync(editarRolDTO.UsuarioId);
            var usuario = await _userManager.FindByEmailAsync(editarRolDTO.Email);
            if (usuario == null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, editarRolDTO.NombreRol));
            return NoContent();
        }

        [HttpPost("RemoveRol")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> RemoverRol(EditarRolDTO editarRolDTO)
        {
            //var usuario = await _userManager.FindByIdAsync(editarRolDTO.UsuarioId);
            var usuario = await _userManager.FindByEmailAsync(editarRolDTO.Email);
            if (usuario == null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(usuario, new Claim(ClaimTypes.Role, editarRolDTO.NombreRol));
            return NoContent();
        }

        private async Task<UserToken> ConstruirToken(UserInfo userInfo, string usuarioId)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(ClaimTypes.Email, userInfo.Email),
                new Claim("id", usuarioId)
            };

            var identityUser = await _userManager.FindByEmailAsync(userInfo.Email);

            claims.Add(new Claim(ClaimTypes.NameIdentifier, identityUser.Id));

            var claimsDB = await _userManager.GetClaimsAsync(identityUser);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds);

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = expiracion
            };

        }

    }
}

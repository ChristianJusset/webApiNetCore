using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NetCoreApi.DTOs.Seguridad;
using NetCoreApi.Servicios;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NetCoreApi.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController: ControllerBase
    {
        // UserManager: nos permite registrar un usuario, encontrar un usuario
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        // signInManager: necesario para el logeo
        private readonly SignInManager<IdentityUser> signInManager;

       
        private readonly IDataProtector dataProtector;

        private readonly HashService hashService;


        public CuentasController(UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        SignInManager<IdentityUser> signInManager,
        IDataProtectionProvider dataProtectionProvider,
        HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");
        }

        /*
         logout: solo hay que eliminar el token del cliente
         encriptacion: convertir un texto plano en un texto cifrado, para esto se va a utilizar una llave única
         IDataProtector:  servicio para proteger datos, se puede recuperar la información utilizando una llave
         hash: no se puede recuperar la información, un ejemplo es cuando se guarda un password, donde se va a guardar de manera aleatoria y 
               con el mismo algoritmo se puede recuperar el valor de password. Para realizar la validación se utiliza la sal, para ello hay que
               guardar la sal en algún lugar seguro.


         */

        [HttpGet("hash/{textoPlano}")]
        public ActionResult RealizarHash(string textoPlano)
        {
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(new
            {
                textoPlano = textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2
            });
        }


        [HttpGet("encriptar")]
        public ActionResult Encriptar()
        {
            

            var textoPlano = "aaaa";
            var textoCifrado = dataProtector.Protect(textoPlano);
            var textoDesencriptado = dataProtector.Unprotect(textoCifrado);
            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });

        }

        [HttpGet("encriptarPorTiempo")]
        public ActionResult EncriptarPorTiempo()
        {
            var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();

            var textoPlano = "aaaa";
            var textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(6000);
            // ya no se puede desencriptar luego de 6s
            var textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }


        [HttpPost("registrar", Name = "registrarUsuario")] // api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                // retornar un JWT(formato del JSON)
                // el JWT sera validado si es correcto con las variables(nombre, direccion) y una clave unica 
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            // el usuario es bloqueado si las credenciales no son sastifactorio
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
               credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Login incorrecto");
            }

        }


        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(UsuarioAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();

        }

        [HttpPost("RemoverAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(UsuarioAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }


        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            // leyendo el claim del token
            //HttpContext: va a obtener los datos del claims debido a que tiene el authorize, el authorize puede estar a nivel de método o de controller
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();

            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        


        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // informacion que podemos confiar
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
                new Claim("lo que yo quiera", "cualquier otro valor")
            };

            // utiliza el userManager para encontrar un usuario
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            // obtenemos del usuario todos sus claims(uno de ellos puede ser esAdmin)
            var claimsDB = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsDB);


            //obtenemos la llave unica para armar el algoritmo
            // cada vez que el usuario envia el token podemos leer el token y ver los claims que tiene el token
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);


            // se construy el JWT
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
              expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };

        }
    }

}

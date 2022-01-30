using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetCoreApi.Filtros;
using NetCoreApi.Middlewares;
using NetCoreApi.Servicios;
using NetCoreApi.Utilidades;
using NetCoreApi.Utilidades.Hateoas;
using NetCoreApi.Utilidades.Swagger;
using NetCoreApi.Utilidades.Versionamiento;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;


[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace NetCoreApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //limpiar el mapeo automatico de ASPNET CORE: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /*
         ID: acoplamiento alto
         inversion de control: la clase va a depender de la interfaz, se va ingresar cualquier clase que implemente la interfaz
         la ID: nos permite solo declararlo una sola vez : addTransient<IA, A> para resolver la dependencia de las dependencias

         AddTransient: retonar una nueva instancia
         AddScoped: dentro del mismo contexto cambia pero cambia por cliente(cuando se loguea otro usuario o se recarga la página), cambia de estado durante el proceso
         AddSingleton: siempre se tiene la misma instancia para todos los usuarios, cuando se guarda valores de cache donde todos los usuarios puedes acceder a la misma informacion

         Middleware: cuando se solicita un recueso antes pasa por unas tuberias de peticiones   
         Middleware: ruteo, authorizacion
         Middleware: hacen un recorrido de ida y vuelta, el request pasa por el Middleware1, Middleware2, Middleware3, luego hace el response Middleware3, Middleware2. Middleware1 
         Middleware: son los que usan el "use"
         Filtros: los filtros se van ejecutar antes y despues de una accion o varias acciones, ejemplos=> filtro de Autorizacion, filtro de recurso(se ejecutan luego del filtro de autorizacion), filtro de excepcion
         
         CORS: politica de seguridad del mismo origen, politca solo de navegadores
         ApiConventionType: muestra las posibles respuestas de las acciones(ejemplos: GET, POST, PUT, DELETE)
         */
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(opciones =>
            {
                // filtro de manera global
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
                // convención de manera global para el verionamiento
                opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
            });

            // se le pasa la cadena de conexión al ApplicationDbContext
            // cualquier clase que dependa de la ApplicationDbContext y los va a resolver con inyeccion de dependencia
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            // configuracion parar el object cycle
            services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson(); ;


            // filtro de cache
            services.AddResponseCaching();

            

            // filtro de accion: se va a ejecutar antes y despues de ejecutarse una accion
            // va transient porque no va a manejar un estado
            services.AddTransient<MiFiltroDeAccion>();

            // filtro para el tiempo de vida del webapi
            services.AddHostedService<EscribirEnArchivo>();

            // configuracion de automapper
            services.AddAutoMapper(typeof(Startup));


            // filtro para autorizacion, cada recurso que tengo el Bearer valida la autenticidad de la llave
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            // servicio para trabajar con la politca Admin en el Claim
            //EsAdmin: tiene que ser igual a [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            });


            // configurar EF identity de seguridad para trabajar con usuarios y roles
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();



            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebAPIAutores",
                    Version = "v1",
                    Description = "Este es un web api para trabajar con autores y libros",
                    Contact = new OpenApiContact
                    {
                        Email = "ejemplo@hotmail.com",
                        Name = "name ejemplo",
                        Url = new Uri("https://gavilan.blog")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });
                c.SwaggerDoc("v3", new OpenApiInfo { Title = "WebAPIAutores", Version = "v3" });
                // agrega un header(solo para swagger)
                c.OperationFilter<AgregarParametroHATEOAS>();
                // agrega un header para el versionamientto(solo para swagger)
                //c.OperationFilter<AgregarParametroXVersion>();
                // para pasar el token por el swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                // para pasar el token por el swagger
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);

            });

            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    //AllowAnyMethod: cualquier metodo
                    //AllowAnyHeader: cualquier header
                    // si quieres exponer header hay que configurar aca: WithExposedHeaders
                    //https://www.apirequest.io: la url va a poder consumir las url del back
                    builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader()
                    // otorgando permiso para mostrar la cantidad de registros en la cabecera de resultados
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros" }); 
                });
            });

            // activa los servicios para proteger los datos
            services.AddDataProtection();

            // ID: el servicio no va a guardar un estado por esa razón se esta utilizando el transient
            services.AddTransient<HashService>();

            //Servicio para enviar las rutas de los recursos, este no tiene enlace por lo tanto solo inyectamos como una clase
            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();

            // ID: para construir la url IActionContextAccessor
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //Middleware: para response final ya que viene de regreso y va funcionar para cualquier controller
            app.UseLoguearRespuestaHTTP();


            // crear un Middleware y  a la vez no seguir con el proceso(debido al RUN), esto solo se va a ejecutar cuando se ingresa a la ruta "ruta1"
            app.Map("/ruta1", app =>
            {
                app.Run(async contexto =>
                {
                    await contexto.Response.WriteAsync("Estoy interceptando la tubería");
                });
            });
            


            if (env.IsDevelopment())
            {
                app.UseSwagger();
                // necesario para las versiones en swagger
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");
                    c.SwaggerEndpoint("/swagger/v3/swagger.json", "WebAPIAutores v3");
                });
            }

            // Middleware: para redireccionar las peticiones HTTP a HTTPS
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            // filtro de cache
            app.UseResponseCaching();

            // filtro de autorizacion
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}

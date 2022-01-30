using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Text;
using WebApiPelicula.Helpers.AttributeResource;
using WebApiPelicula.Helpers.Automapper;
using WebApiPelicula.Middlewares;
using WebApiPelicula.Servicios;

namespace WebApiPelicula
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                // necesario cuando se utiliza el patch
                    .AddNewtonsoftJson()
                    // para trabajar con xml
                    .AddXmlDataContractSerializerFormatters();
            

            // configuración del dbContext para usuarlo con ID
            services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
              sqlServerOptions => sqlServerOptions.UseNetTopologySuite()
              ));

            // configuración del swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo());

            });

            // configuración de automapper como un servicio para utilizarlo como ED en cualquier lado
            services.AddSingleton(provider =>

                new MapperConfiguration(config =>
                {
                    var geometryFactory = provider.GetRequiredService<GeometryFactory>();
                    config.AddProfile(new AutoMapperProfiles(geometryFactory));
                }).CreateMapper()
            );
            // servicio de topología
            services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

            // configuración de una clase para que pueda ser instanciado en cualquier lugar
            //services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();

            // dependiendo del ambiente se va a ejecutar en azure o local
            services.AddTransient<IAlmacenadorArchivos>((serviceProvider) =>
            {
                var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    return new AlmacenadorArchivosLocal(env, httpContextAccessor);
                }
                else
                {
                    return new AlmacenadorArchivosAzure(Configuration);
                }
            });

            // es necesario para acceder a la ruta al momento de obtener la ruta
            services.AddHttpContextAccessor();

            // servicio de identity para seguridad
            services.AddIdentity<IdentityUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = false,
                      ValidateAudience = false,
                      ValidateLifetime = true,
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                      ClockSkew = TimeSpan.Zero
                  }
              );
            // servicio para trabajar con la politca Admin en el Claim
            //EsAdmin: tiene que ser igual a [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
            //services.AddAuthorization(opciones =>
            //{
            //    opciones.AddPolicy("Admin", politica => politica.RequireClaim("admin"));
            //});

            services.AddScoped<PeliculaExisteAttribute>();

            // agrgamos cors, acepte todas las solicitudes
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            //servicio de llaves
            services.AddScoped<ServicioLlaves>();

            // ejecutacion de facturas
            //services.AddHostedService<FacturasHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                // necesario para las versiones en swagger
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
                });
            }

            // Middleware: para redireccionar las peticiones HTTP a HTTPS
            app.UseHttpsRedirection();

            // para acceder a la url donde se encuentra la imagen: wwwwroot
            app.UseStaticFiles();

      
            app.UseRouting();

            // Habilitacion de cors
            app.UseCors();

            // Habilitacion de middlware: limitar peticiones
            app.UseLimitarPeticiones();


            // filtro de autorizacion
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

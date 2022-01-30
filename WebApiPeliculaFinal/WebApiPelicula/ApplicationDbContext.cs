using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiPelicula.DTOs.Actor;
using WebApiPelicula.DTOs.Pelicula;
using WebApiPelicula.Entidades;
using WebApiPelicula.Entidades.Suscripciones;

namespace WebApiPelicula
{
    //public class ApplicationDbContext: DbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // apifluent para especificar que una tabla tiene 2 llaves
            modelBuilder.Entity<PeliculaActor>()
                .HasKey(x => new { x.ActorId, x.PeliculaId });

            modelBuilder.Entity<PeliculaGenero>()
                .HasKey(x => new { x.GeneroId, x.PeliculaId });

            modelBuilder.Entity<PeliculaSalaDeCine>()
               .HasKey(x => new { x.PeliculaId, x.SalaDeCineId });


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Genero> Genero { get; set; }
        public DbSet<Actor> Actor { get; set; }
        public DbSet<Pelicula> Pelicula { get; set; }
        public DbSet<PeliculaActor> PeliculaActor { get; set; }
        public DbSet<PeliculaGenero> PeliculaGenero { get; set; }
        public DbSet<SalaDeCine> SalasDeCine { get; set; }
        public DbSet<PeliculaSalaDeCine> PeliculaSalaDeCine  { get; set; }

        public DbSet<Review> Review { get; set; }

        public DbSet<LlaveAPI> LlaveAPI { get; set; }
        public DbSet<Peticion> Peticion { get; set; }
        public DbSet<RestriccionDominio> RestriccionDominio { get; set; }
        public DbSet<RestriccionIP> RestriccionIP { get; set; }
        public DbSet<Factura> Factura { get; set; } 
        public DbSet<FacturaEmitida> FacturaEmitida { get; set; } 
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetCoreApi.Entidades;

namespace NetCoreApi
{

    /*
     IdentityDbContext: configura las tablas de seguridad
    IdentityDbContext: agregar el package Microsoft.AspNetCore.Identity.EntityFrameworkCore
     add-migration
     update-database
     */


    // public class ApplicationDbContext : DbContext

    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        // apiFluent
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // agregar para el IdentityDbContext
            base.OnModelCreating(modelBuilder);

            // para configurar una llave compuesta, es necesario
            modelBuilder.Entity<AutorLibro>()
                .HasKey(al => new { al.AutorId, al.LibroId });
        }
        public DbSet<Autor> Autor { get; set; }
        
        // para trabajar especificamente con el libro
        public DbSet<Libro> Libro { get; set; }

        public DbSet<Comentario> Comentario { get; set; }

        public DbSet<AutorLibro> AutorLibro { get; set; }   
    }
}

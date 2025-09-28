using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class ForariaContext : DbContext
    {
        public ForariaContext(DbContextOptions<ForariaContext> options) : base(options) { 

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Rol> Rols { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Rol>().ToTable("rol");

            modelBuilder.Entity<User>()
                .HasOne(u => u.rol)  
                .WithMany(r => r.Users)  
                .HasForeignKey(u => u.Rol_id)  
                .OnDelete(DeleteBehavior.Restrict); 
        }



        }
}

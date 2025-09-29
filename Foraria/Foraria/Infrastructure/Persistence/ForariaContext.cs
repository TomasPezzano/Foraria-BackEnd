using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class ForariaContext : DbContext
    {
        public ForariaContext(DbContextOptions<ForariaContext> options) : base(options) { 

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Rols { get; set; }

        public DbSet<Consortium> Consortium { get; set; }

        public DbSet<Claim> Claims { get; set; }

        public DbSet<ClaimResponse> ClaimsResponse { get; set; }

        public DbSet<ResponsibleSector> ResponsibleSectors { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Residence> Residence { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<Reserve> Reserves { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Role>().ToTable("role");
            modelBuilder.Entity<Consortium>().ToTable("consortium");
            modelBuilder.Entity<Claim>().ToTable("claim");
            modelBuilder.Entity<ClaimResponse>().ToTable("claimResponse");
            modelBuilder.Entity<ResponsibleSector>().ToTable("responsibleSector");
            modelBuilder.Entity<Event>().ToTable("event");
            modelBuilder.Entity<Residence>().ToTable("residence");
            modelBuilder.Entity<Place>().ToTable("place");
            modelBuilder.Entity<Reserve>().ToTable("reserves");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)  
                .WithMany(r => r.Users)  
                .HasForeignKey(u => u.Role_id)  
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Claim>()
                .HasOne(u => u.User)
                .WithMany(r => r.Claims)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()
                .HasOne(u => u.ClaimResponse)
                .WithOne(r => r.Claim)
                .HasForeignKey<Claim>(u => u.ClaimResponse_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClaimResponse>()
                .HasOne(u => u.ResponsibleSector)
                .WithMany(r => r.ClaimsResponse)
                .HasForeignKey(u => u.ResponsibleSector_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserve>()
               .HasOne(u => u.User)
               .WithMany(r => r.Reserves)
               .HasForeignKey(u => u.User_id)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserve>()
              .HasOne(u => u.Place)
              .WithMany(r => r.Reserves)
              .HasForeignKey(u => u.Place_id)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserve>()
                .HasOne(u => u.Residence)
                .WithMany(r => r.Reserves)
                .HasForeignKey(u => u.Residence_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                 .HasMany(u => u.Residence)  
                 .WithMany(r => r.Users)  
                 .UsingEntity<Dictionary<string, object>>(
                     "UserResidence",  
                     j => j.HasOne<Residence>().WithMany().HasForeignKey("ResidenceId"),  
                     j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
                 );

            modelBuilder.Entity<User>()
                  .HasMany(u => u.Events)  
                  .WithMany(e => e.Users)  
                  .UsingEntity<Dictionary<string, object>>(
                      "UserEvent",  
                      j => j.HasOne<Event>().WithMany().HasForeignKey("EventId"), 
                      j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
                  );
        }

       


        }
}

using Foraria.Domain.Model;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using Thread = ForariaDomain.Thread;

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

        public DbSet<Forum> Forums { get; set; }

        public DbSet<Thread> Threads { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Poll> Polls { get; set; }

        public DbSet<PollOption> PollOptions { get; set; }

        public DbSet<Vote> Votes { get; set; }

        public DbSet<ResultPoll> ResultPoll { get; set; }

        public DbSet<CategoryPoll> CategoryPolls { get; set; }

        public DbSet<UserDocument> UserDocuments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        public DbSet<BlockchainProof> BlockchainProofs { get; set; }



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
            modelBuilder.Entity<Forum>().ToTable("forum");
            modelBuilder.Entity<Thread>().ToTable("thread");
            modelBuilder.Entity<Message>().ToTable("message");
            modelBuilder.Entity<Poll>().ToTable("poll");
            modelBuilder.Entity<PollOption>().ToTable("pollOption");
            modelBuilder.Entity<Vote>().ToTable("vote");
            modelBuilder.Entity<ResultPoll>().ToTable("resultPoll");
            modelBuilder.Entity<CategoryPoll>().ToTable("categoryPoll");
            modelBuilder.Entity<UserDocument>().ToTable("userDocument");
            modelBuilder.Entity<Reaction>().ToTable("reaction");
            modelBuilder.Entity<BlockchainProof>().ToTable("blockchainProof");


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

            modelBuilder.Entity<Thread>()
                .HasOne(u => u.Forum)
                .WithMany(r => r.Threads)
                .HasForeignKey(u => u.Forum_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Thread>()
                .HasOne(u => u.User)
                .WithMany(r => r.Threads)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.Thread)
                .WithMany(r => r.Messages)
                .HasForeignKey(u => u.Thread_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.User)
                .WithMany(r => r.Messages)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Poll>()
                .HasOne(u => u.User)
                .WithMany(r => r.Polls)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(u => u.User)
                .WithMany(r => r.Votes)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(u => u.Poll)
                .WithMany(r => r.Votes)
                .HasForeignKey(u => u.Poll_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(u => u.PollOption)
                .WithMany(r => r.Votes)
                .HasForeignKey(u => u.PollOption_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PollOption>()
                .HasOne(u => u.Poll)
                .WithMany(r => r.PollOptions)
                .HasForeignKey(u => u.Poll_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Poll>()
                .HasOne(u => u.CategoryPoll)
                .WithMany(r => r.Polls)
                .HasForeignKey(u => u.CategoryPoll_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Poll>()
                .HasOne(u => u.ResultPoll)
                .WithOne(r => r.Poll)
                .HasForeignKey<Poll>(u => u.ResultPoll_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDocument>()
                .HasOne(u => u.User)
                .WithMany(r => r.UserDocuments)
                .HasForeignKey(u => u.User_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDocument>()
                .HasOne(u => u.Consortium)
                .WithMany(r => r.UserDocuments)
                .HasForeignKey(u => u.Consortium_id)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.User_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.Message_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Thread)
                .WithMany(t => t.Reactions)
                .HasForeignKey(r => r.Thread_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.User_id, r.Message_id, r.Thread_id })
                .IsUnique();

            modelBuilder.Entity<BlockchainProof>()
                .HasOne(bp => bp.Poll)
                .WithOne(p => p.BlockchainProof)
                .HasForeignKey<BlockchainProof>(bp => bp.PollId)
                .OnDelete(DeleteBehavior.Restrict);


        }




    }
}

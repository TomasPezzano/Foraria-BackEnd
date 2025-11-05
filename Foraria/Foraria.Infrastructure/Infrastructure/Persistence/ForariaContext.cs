using Foraria.Domain.Model;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;
using Thread = ForariaDomain.Thread;

namespace Foraria.Infrastructure.Persistence
{
    public class ForariaContext : DbContext
    {
        public ForariaContext(DbContextOptions<ForariaContext> options) : base(options)
        {

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

        public DbSet<Expense> Expenses { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<SupplierContract> SupplierContracts { get; set; }

        public DbSet<BlockchainProof> BlockchainProofs { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        public DbSet<ExpenseDetailByResidence> ExpenseDetailByResidences { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


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
            modelBuilder.Entity<Payment>().ToTable("payment");
            modelBuilder.Entity<PaymentMethod>().ToTable("paymentMethod");
            modelBuilder.Entity<RefreshToken>().ToTable("refreshToken");
            modelBuilder.Entity<Supplier>().ToTable("supplier");
            modelBuilder.Entity<SupplierContract>().ToTable("supplierContract");
            modelBuilder.Entity<Invoice>().ToTable("invoice");
            modelBuilder.Entity<InvoiceItem>().ToTable("invoiceItem");
            modelBuilder.Entity<ExpenseDetailByResidence>().ToTable("ExpenseDetailByResidences");
            modelBuilder.Entity<PasswordResetToken>().ToTable("passwordResetToken");


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

            modelBuilder.Entity<Claim>()
               .HasOne(c => c.Residence)
               .WithMany(r => r.Claims)
               .HasForeignKey(c => c.ResidenceId)
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
                 .HasMany(u => u.Residences)
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
                .HasForeignKey(u => u.ForumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Thread>()
                .HasOne(u => u.User)
                .WithMany(r => r.Threads)
                .HasForeignKey(u => u.UserId)
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
                .HasOne(bp => bp.Poll) //TODO transcripciones/actas,etc
                .WithOne(p => p.BlockchainProof)
                .HasForeignKey<BlockchainProof>(bp => bp.PollId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ExpenseDetailByResidence>()
                .HasOne(e => e.Residence)
                .WithMany(r => r.ExpenseDetailByResidence)
                .HasForeignKey(e => e.ResidenceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExpenseDetailByResidence>()
                .HasOne(e => e.Expense)
                .WithMany(r => r.ExpenseDetailsByResidence)
                .HasForeignKey(e => e.ExpenseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Consortium)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.ConsortiumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expense>()
                .HasMany(e => e.Invoices)
                .WithOne(d => d.Expense)
                .HasForeignKey(d => d.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Residence)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ResidenceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.ExpenseDetailByResidence)
                .WithMany(e => e.Payments)
                .HasForeignKey(p => p.ExpenseDetailByResidenceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            modelBuilder.Entity<Supplier>()
                .HasOne(c => c.Consortium)
                .WithMany(s => s.Suppliers)
                .HasForeignKey(c => c.ConsortiumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplierContract>()
                .HasOne(sc => sc.Supplier)
                .WithMany(s => s.Contracts)
                .HasForeignKey(sc => sc.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Rating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Consortium>()
                .HasMany(e => e.Invoices)
                .WithOne(d => d.Consortium)
                .HasForeignKey(d => d.ConsortiumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordResetToken>()
               .HasOne (u => u.User)
               .WithMany(i => i.PasswordResetTokens)
               .HasForeignKey(u => u.UserId)
               .OnDelete(DeleteBehavior.Cascade);



            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                if (fk.DeleteBehavior == DeleteBehavior.Cascade)
                    fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }

}
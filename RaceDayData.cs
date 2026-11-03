using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RaceDayModel.Models;
using System;

namespace RaceDayData.Data
{
    public class RaceDayDataContext : DbContext
    {
        public RaceDayDataContext(DbContextOptions<RaceDayDataContext> options) : base(options) { }
        public DbSet<Venue> Venue => Set<Venue>();
        public DbSet<Users> Users => Set<Users>();
        public DbSet<Events> Events => Set<Events>();
        public DbSet<Categories> Categories => Set<Categories>();
        public DbSet<Enrolments> Enrolments => Set<Enrolments>();
        public DbSet<RaceDayModel.Models.Results> Results => Set<RaceDayModel.Models.Results>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Venue>(V =>
            {
                V.HasKey(V => V.VenueID);
            });

            modelBuilder.Entity<Users>(U =>
            {
                U.HasKey(U => U.UserID);


            });

            modelBuilder.Entity<Categories>(C =>
            {
                C.HasKey(C => C.CategoryID);
                C.Property(c => c.EntryFee).HasPrecision(18, 2);

                C.HasOne<Events>()
                 .WithMany()
                 .HasForeignKey(C => C.EventID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RaceDayModel.Models.Results>(R =>
            {
                R.HasKey(R => R.ResultsID);

                // A result belongs to an enrolment, not directly to a user.
                R.HasOne<Enrolments>()
                 .WithMany()
                 .HasForeignKey(R => R.EnrolmentID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Events>(E =>
            {
                E.HasKey(E => E.EventID);
                E.Property(e => e.DistanceKm).HasPrecision(10, 3);

                E.HasOne<Venue>()
                 .WithMany()
                 .HasForeignKey(e => e.VenueID)
                 .OnDelete(DeleteBehavior.Restrict);

                E.HasOne<Users>()
                 .WithMany()
                 .HasForeignKey(e => e.OrganiserID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Enrolments>(En =>
            {
                En.HasKey(En => En.EnrolmentID);

               En.HasOne<Users>()
                 .WithMany()
                 .HasForeignKey(En => En.ParticipantID)
                 .OnDelete(DeleteBehavior.Restrict);

               En.HasOne<Categories>()
                 .WithMany()
                 .HasForeignKey(En => En.CategoryID)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

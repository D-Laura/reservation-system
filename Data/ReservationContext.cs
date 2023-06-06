using ReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ReservationSystem.Data
{
    public class ReservationContext : IdentityDbContext<User>
    {
        public ReservationContext(DbContextOptions<ReservationContext> options) : base(options) { }

        public DbSet<Person> People { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<UnregisteredNumber> UnregisteredNumbers { get; set; }

        public DbSet<WorkingHours> WorkingHours { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(user => user.PersonNumber).IsRequired(false);

            modelBuilder.Entity<Person>()
                .HasOne(person => person.Doctor)
                .WithMany(person => person.Patients)
                .HasForeignKey(person => person.DoctorNumber)
                .IsRequired(false);

            modelBuilder.Entity<Person>()
                .HasOne(person => person.WorkingHours)
                .WithOne(workingHours => workingHours.Doctor)
                .HasForeignKey<Person>(person => person.WorkingHoursId)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasOne(user => user.Person)
                .WithOne(person => person.Identity)
                .HasForeignKey<User>(user => user.PersonNumber)
                .IsRequired();

            modelBuilder.Entity<Reservation>()
                .HasOne(reservation => reservation.Doctor)
                .WithMany(person => person.DoctorReservations)
                .HasForeignKey(reservation => reservation.DoctorNumber)
                .IsRequired();

            modelBuilder.Entity<Reservation>()
                .HasOne(reservation => reservation.Patient)
                .WithMany(person => person.PatientReservations)
                .HasForeignKey(reservation => reservation.PatientNumber)
                .IsRequired(false);

            modelBuilder.Entity<UnregisteredNumber>()
                .HasOne(unregisteredNumber => unregisteredNumber.Doctor)
                .WithMany(person => person.UnregisteredPatients)
                .HasForeignKey(unregisteredNumber => unregisteredNumber.DoctorNumber)
                .IsRequired();
        }
    }
}
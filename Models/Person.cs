using Microsoft.IdentityModel.Tokens;
using ReservationSystem.Identity.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationSystem.Models
{
    public class Person
    {
        [Key, Range(0001010000, 9967319999)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BirthNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Range(0001010000, 9967319999)]
        public long? DoctorNumber { get; set; }

        public int? WorkingHoursId { get; set; }

        public User Identity { get; set; }

        public ICollection<Person> Patients { get; set; }

        public Person Doctor { get; set; }

        public WorkingHours WorkingHours { get; set; }

        public ICollection<Reservation> PatientReservations { get; set; }

        public ICollection<Reservation> DoctorReservations { get; set; }

        public ICollection<UnregisteredNumber> UnregisteredPatients { get; set; }
    }
}

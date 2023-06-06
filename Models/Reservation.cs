using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Length { get; set; }

        [Required, DisplayName("Zadajte dôvod návštevy lekára:")]
        public string Reason { get; set; }

        [Required]
        public long DoctorNumber { get; set; }

        public long PatientNumber { get; set; }

        public Person Doctor { get; set; }

        public Person Patient { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationSystem.Models
{
    public class UnregisteredNumber
    {
        const int BIRTH_NUMBER_LENGTH = 10;

        [Key, Range(0001010000, 9967319999)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BirthNumber { get; set; }

        [Required, Range(0001010000, 9967319999)]
        public long DoctorNumber { get; set; }

        public Person Doctor { get; set; }
    }
}

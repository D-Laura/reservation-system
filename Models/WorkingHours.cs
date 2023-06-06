using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Models
{
    public class WorkingHours
    {
        [Key]
        public int Id { get; set; }

        [Required, DisplayName("Pracovná doba od:")]
        public TimeSpan WorkFrom { get; set; }

        [Required, DisplayName("Pracovná doba do:")]
        public TimeSpan WorkTo { get; set; }

        [Required, DisplayName("Prestávka od:")]
        public TimeSpan BreakFrom { get; set; }

        [Required, DisplayName("Prestávka do:")]
        public TimeSpan BreakTo { get; set; }

        [Required, Range(1, 480), DisplayName("Dĺžka návštevy:")]
        public int VisitLength { get; set; }

        public Person Doctor { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ReservationSystem.Models
{
    public class Admin
    {
        [Required, DisplayName("Výber akcie:")]
        public Actions Action { get; set; }

        [Required, DisplayName("Výber role:")]
        public Roles Role { get; set; }

        [Required, Range(0001010000, 9967319999), DisplayName("Zadajte rodné číslo bez / :")]
        public long BirthNumber { get; set; }

        [Required, DisplayName("Zadajte meno:")]
        public string FirstName { get; set; }

        [Required, DisplayName("Zadajte priezvisko:")]
        public string LastName { get; set; }

        [EmailAddress, DisplayName("Zadajte email:")]
        public string Email { get; set; }

        [DisplayName("Zadajte heslo:")]
        public string Password { get; set; }
    }
}

public enum Actions
{
    Add,
    Remove
}

public enum Roles
{
    Doctor,
    Patient
}
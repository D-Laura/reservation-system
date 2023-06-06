using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Models
{
    /// <summary>
    /// 
    /// Register class representing registration form
    /// </summary>
    public class Register
    {
        [Required, DisplayName("Zadajte rodné číslo bez / :")]
        public long BirthNumber { get; set; }

        [Required, DisplayName("Zadajte meno:")]
        public string FirstName { get; set; }

        [Required, DisplayName("Zadajte priezvisko:")]
        public string LastName { get; set; }

        [Required, EmailAddress, DisplayName("Zadajte email:")]
        public string Email { get; set; }

        [Required, DisplayName("Zadajte heslo:")]
        public string Password { get; set; }
    }
}

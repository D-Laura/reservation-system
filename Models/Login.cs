using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Toto pole nesmie byť prázdne"), DisplayName("Zadajte email:")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Toto pole nesmie byť prázdne"), DisplayName("Zadajte heslo:")]
        public string Password { get; set; }
    }
}
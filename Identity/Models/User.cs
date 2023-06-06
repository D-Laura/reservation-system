using Microsoft.AspNetCore.Identity;
using ReservationSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Identity.Models
{
    public class User : IdentityUser
    {
        public long? PersonNumber { get; set; }
        public Person Person { get; set; }
    }
}

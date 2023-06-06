using Microsoft.AspNetCore.Identity;
using ReservationSystem.Controllers;
using ReservationSystem.Identity.Models;
using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    /// <summary>
    /// 
    /// Interface for service used for user administration
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 
        /// Method which checks if currently logged user has admin role.
        /// </summary>
        /// <returns>
        /// Returns true if has admin role else false.
        /// </returns>
        Task<bool> IsAdmin();

        /// <summary>
        /// 
        /// Method which checks if currently logged user has doctor role.
        /// </summary>
        /// <returns>
        /// Returns true if has doctor role else false.
        /// </returns>
        Task<bool> IsDoctor();

        /// <summary>
        /// 
        /// Method which gets currently logged user.
        /// </summary>
        /// <returns>
        /// Returns currently logged user.
        /// </returns>
        User GetCurrentUser();
        
        Task<Tuple<Message, IdentityResult>> Register(Admin form);
        
        Task<Message> Remove(Admin form);

        Task<Message> RemovePatient(Person person);

        Task<Message> RemoveDoctor(Person person);
    }
}

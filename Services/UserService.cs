using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Controllers;
using ReservationSystem.Data;
using ReservationSystem.Identity.Models;
using ReservationSystem.Models;

namespace ReservationSystem.Services
{
    /// <summary>
    /// Class representing UserService, implementing IUserService.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ReservationContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// UserService constructor which initializes dbContext, userManager and httpContextAccessor to given value.
        /// </summary>
        /// <param name="dbContext">database context</param>
        /// <param name="userManager">user manager</param>
        /// <param name="httpContextAccessor">http context accessor</param>
        public UserService(ReservationContext dbContext, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAdmin()
        {
            return (await userManager.GetRolesAsync(GetCurrentUser())).Contains("Admin");
        }

        /// <inheritdoc/>
        public async Task<bool> IsDoctor()
        {
            return (await userManager.GetRolesAsync(GetCurrentUser())).Contains("Doctor");
        }

        /// <inheritdoc/>
        public User GetCurrentUser()
        {
            return dbContext.Users.Where(user => user.UserName == httpContextAccessor.HttpContext.User.Identity.Name)
                .Include(x => x.Person)
                .FirstOrDefault();
        }

        public async Task<Tuple<Message, IdentityResult>> Register(Admin form)
        {
            Person person = null;

            if (dbContext.Users.Where(x => x.UserName == form.Email).Count() > 0)
            {
                return Tuple.Create<Message, IdentityResult>(Message.NameUsed, null);
            }

            Register register = new Register { BirthNumber = form.BirthNumber, FirstName = form.FirstName, LastName = form.LastName, Email = form.Email, Password = form.Password };

            UnregisteredNumber toBeRegistered = await dbContext.UnregisteredNumbers.FindAsync(register.BirthNumber);
            if (form.Role == Roles.Patient && toBeRegistered == null)
            {
                return Tuple.Create<Message, IdentityResult>(Message.InvalidBirthNumber, null);
            }

            var user = new User { UserName = register.Email};

            var result = await userManager.CreateAsync(user, register.Password);

            if (form.Role == Roles.Doctor)
            {
                await userManager.AddToRoleAsync(user, "Doctor");
            }

            if (!result.Succeeded)
            {
                return Tuple.Create(Message.ModelError, result);
            }

            person = new Person { BirthNumber = register.BirthNumber, Name = register.FirstName, Surname = register.LastName, DoctorNumber = (form.Role == Roles.Patient ? toBeRegistered.DoctorNumber : null), Identity = user };

            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();

            user.PersonNumber = form.BirthNumber;

            if (form.Role == Roles.Patient)
            {
                dbContext.UnregisteredNumbers.Remove(toBeRegistered);
                await dbContext.SaveChangesAsync();
            }
            return Tuple.Create<Message, IdentityResult>(Message.Success, null);
        }

        public async Task<Message> RemovePatient(Person toRemove)
        {
            Person person = dbContext.People
                .Where(x => x.BirthNumber == toRemove.BirthNumber)
                .Include(x => x.Identity)
                .Include(x => x.PatientReservations)
                .FirstOrDefault();

            foreach (Reservation reservation in person.PatientReservations)
            {
                dbContext.Reservations.Remove(reservation);
            }
            await dbContext.SaveChangesAsync();

            User identity = person.Identity;
            if ((await userManager.GetRolesAsync(identity)).Contains("Doctor"))
            {
                person.DoctorNumber = null;
                await dbContext.SaveChangesAsync();
                return Message.Success;
            }
            
            dbContext.People.Remove(person);
            await dbContext.SaveChangesAsync();
            return Message.Success;
        }

        public async Task<Message> RemoveDoctor(Person person)
        {
            foreach (Reservation reservation in person.DoctorReservations)
            {
                dbContext.Reservations.Remove(reservation);
            }
            await dbContext.SaveChangesAsync();

            WorkingHours working = person.WorkingHours;
            if (working != null)
            {
                dbContext.WorkingHours.Remove(working);
                await dbContext.SaveChangesAsync();
            }

            foreach (Person patient in person.Patients)
            {
                await RemovePatient(patient);
            }
            await dbContext.SaveChangesAsync();

            foreach (UnregisteredNumber unregistered in person.UnregisteredPatients)
            {
                dbContext.UnregisteredNumbers.Remove(unregistered);
            }
            await dbContext.SaveChangesAsync();

            User identity = person.Identity;
            await userManager.RemoveFromRoleAsync(identity, "Doctor");

            if ((await userManager.GetRolesAsync(identity)).Contains("Patient"))
            {
                return Message.Success;
            }
            
            dbContext.People.Remove(person);
            await dbContext.SaveChangesAsync();
            return Message.Success;
        }

        public async Task<Message> Remove(Admin form)
        {
            Person person = dbContext.People
                .Where(x => x.BirthNumber == form.BirthNumber)
                .Include(x => x.Identity)
                .Include(x => x.UnregisteredPatients)
                .Include(x => x.Patients)
                .Include(x => x.PatientReservations)
                .Include(x => x.DoctorReservations)
                .FirstOrDefault();
            if (person == null)
            {
                return Message.InvalidBirthNumber;
            }

            if (person.Name != form.FirstName && person.Surname != form.LastName)
            {
                return Message.InvalidPerson;
            }

            if (form.Role == Roles.Patient)
            {
                return await RemovePatient(person);
            }

            return await RemoveDoctor(person);
        }
    }
}

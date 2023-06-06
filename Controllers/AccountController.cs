using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Data;
using ReservationSystem.Identity.Models;
using ReservationSystem.Models;
using ReservationSystem.Services;

namespace ReservationSystem.Controllers
{
    /// <summary>
    /// Class representing account controller.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ReservationContext dbContext;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IUserService userService;

        /// <summary>
        /// Account controller constructor which initializes dbContext, signInManager, userManager and userService to given value.
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="signInManager">Sign in manager</param>
        /// <param name="userManager">User manager</param>
        /// <param name="userService">User service</param>
        public AccountController(ReservationContext dbContext, SignInManager<User> signInManager, UserManager<User> userManager, IUserService userService)
        {
            this.dbContext = dbContext;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userService = userService;
        }

        /// <summary>
        /// Account action for login.
        /// </summary>
        /// <returns>
        /// Login screen view.
        /// </returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Account action for login on post.
        /// </summary>
        /// <param name="login">Login form from user</param>
        /// <returns>
        /// Home screen view for patient, admin or doctor with filled working hours.
        /// Working hours setting view for doctor with no working hours.
        /// On fail login view.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LoginPost([FromForm] Login login)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Login));
            }

            // Verify if given email is in database
            if (await userManager.FindByNameAsync(login.Email) is not User user)
            {
                ViewBag.IsInvalidLogin = true;
                return View(nameof(Login));
            }

            var result = await signInManager.PasswordSignInAsync(user, login.Password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(RedirectionAfterLogin));
            }

            ViewBag.IsInvalidLogin = true;
            return View(nameof(Login));
        }

        /// <summary>
        /// Account action for register.
        /// </summary>
        /// <returns>
        /// Register screen view.
        /// </returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Account action for registration on post.
        /// </summary>
        /// <param name="register">Registration form from user</param>
        /// <returns>
        /// Home screen view for patient and admin.
        /// Working hours setting view for doctor.
        /// On fail registration view.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> RegisterPost([FromForm] Register register)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Register));
            }

            var toBeRegistered = await dbContext.UnregisteredNumbers.FindAsync(register.BirthNumber);

            // Verify given birth number is in database of unregistered users
            if (toBeRegistered == null)
            {
                ViewBag.InvalidBirthNumber = true;
                return View("Register");
            }

            // Verify that given email is not already registered
            if (dbContext.Users.Where(x => x.UserName == register.Email).Count() > 0)
            {
                ViewBag.NameUsed = true;
                return View("Register");
            }

            var user = new User { UserName = register.Email };

            var result = await userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Errors.First().Description);
                return View("Register");
            }

            var person = new Person { BirthNumber = register.BirthNumber, Name = register.FirstName, Surname = register.LastName, DoctorNumber = toBeRegistered.DoctorNumber, Identity = user };

            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();

            dbContext.UnregisteredNumbers.Remove(toBeRegistered);
            await dbContext.SaveChangesAsync();

            return View(nameof(Login));
        }

        /// <summary>
        /// Method which returns view based on role and working hours of successfully logged user.
        /// </summary>
        /// <returns>
        /// View for current logged user.
        /// </returns>
        [Authorize]
        public async Task<IActionResult> RedirectionAfterLogin()
        {
            if (await userService.IsAdmin())
            {
                return RedirectToAction("Admin", "Screen");
            }
            if (await userService.IsDoctor())
            {
                var user = userService.GetCurrentUser();
                if (user.Person.WorkingHoursId == null)
                {
                    return RedirectToAction("WorkingHours", "Settings");
                }
                return RedirectToAction("Doctor", "Screen");
            }
            return RedirectToAction("Patient", "Screen");
        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return View(nameof(Login));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Data;
using ReservationSystem.Models;
using ReservationSystem.Services;

namespace ReservationSystem.Controllers
{

    [Authorize(Roles = "Doctor")]
    public class SettingsController : Controller
    {
        private readonly ReservationContext dbContext;
        private readonly IUserService userService;

        public SettingsController(ReservationContext dbContext, IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        public IActionResult WorkingHours()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WorkingHoursPost([FromForm]WorkingHours workingHours)
        {
            if (!ModelState.IsValid)
            {
                return View("WorkingHours");
            }

            await dbContext.AddAsync(workingHours);
           
            await dbContext.SaveChangesAsync();
            var user = userService.GetCurrentUser();

            user.Person.WorkingHours = workingHours;
                
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Doctor", "Screen");
        }
    }
}

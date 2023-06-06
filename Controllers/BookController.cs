using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Data;
using ReservationSystem.Models;
using ReservationSystem.Services;
using System.Data;

namespace ReservationSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ReservationContext dbContext;
        private readonly IUserService userService;

        public BookController(ReservationContext dbContext, IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        [Route("/Book/Index", Name = "bookindex")]
        public IActionResult Index(Dictionary<string, string> data)
        {
            Person patient = dbContext.People.Where(x => x == userService.GetCurrentUser().Person).Include(x => x.Doctor).FirstOrDefault();
            Person doctor = dbContext.People.Where(x => x.BirthNumber == patient.Doctor.BirthNumber).Include(x => x.WorkingHours).FirstOrDefault();
            DateTime date = DateTime.Parse(data["day"]);
            TimeSpan slot = TimeSpan.Parse(data["slot"]);
            DateTime reservationDate = date.Date.Add(slot);
            int length = doctor.WorkingHours.VisitLength;
            Reservation newReservation = new() { Date = reservationDate, Length = length, DoctorNumber = doctor.BirthNumber, PatientNumber = patient.BirthNumber };
            return View(newReservation);
        }

        [HttpPost]
        public IActionResult BookPost([FromForm] Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Patient", "Screen");
            }

            DateTime date = reservation.Date;
            Reservation existingReservation = dbContext.Reservations.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day && x.Date.Hour == date.Hour && x.Date.Minute == date.Minute).Include(x => x.Doctor).FirstOrDefault();
            if (existingReservation != null)
            {
                return RedirectToAction("Patient", "Screen");
            }

            dbContext.Reservations.Add(reservation);
            dbContext.SaveChanges();
            return RedirectToAction("Patient", "Screen");
        }
    }
}

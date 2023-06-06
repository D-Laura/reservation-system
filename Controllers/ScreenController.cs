using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Data;
using ReservationSystem.Identity.Models;
using ReservationSystem.Models;
using ReservationSystem.Services;
using System;

namespace ReservationSystem.Controllers
{
    /// <summary>
    /// Class representing home screen controller.
    /// </summary>
    public class ScreenController : Controller
    {
        private readonly ReservationContext dbContext;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IUserService userService;
        private readonly RoleManager<IdentityRole> roleManager;

        /// <summary>
        /// Screen controller constructor which initializes dbContext, signInManager, userManager and httpContextAccessor to given value.
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="signInManager">Sign in manager</param>
        /// <param name="userManager">User manager</param>
        /// <param name="userService">User service</param>
        /// <param name="roleManager">Role manager</param>
        public ScreenController(ReservationContext dbContext, SignInManager<User> signInManager, UserManager<User> userManager, IUserService userService, RoleManager<IdentityRole> roleManager)
        {
            this.dbContext = dbContext;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userService = userService;
            this.roleManager = roleManager;
        }

        /// <summary>
        /// Screen action for admin.
        /// </summary>
        /// <returns>
        /// Admin home screen view.
        /// </returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View(new Admin());
        }

        [HttpPost]
        public async Task<IActionResult> AdminPost([FromForm] Admin form)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Admin));
            }

            if (form.Action == Actions.Add)
            {
                Tuple<Message, IdentityResult> resultAdd = await userService.Register(form);
                Message message = resultAdd.Item1;
                IdentityResult identityResult = resultAdd.Item2;
                switch (message)
                {
                    case Message.InvalidBirthNumber:
                        ViewBag.InvalidBirthNumber = true;
                        return View("Admin");
                    case Message.ModelError:
                        ModelState.AddModelError(string.Empty, identityResult.Errors.First().Description);
                        return View("Admin");
                    case Message.Success:
                        ViewBag.AddSuccess = true;
                        return View("Admin");
                    case Message.NameUsed:
                        ViewBag.NameUsed = true;
                        return View("Admin");
                }
                return View("Admin");
            }

            Message resultRemove = await userService.Remove(form);
            switch (resultRemove)
            {
                case Message.InvalidBirthNumber:
                    ViewBag.InvalidBirthNumber = true;
                    return View("Admin");
                case Message.Success:
                    ViewBag.RemoveSuccess = true;
                    return View("Admin");
                case Message.InvalidPerson:
                    ViewBag.InvalidPerson = true;
                    return View("Admin");
            }

            return View("Admin");
        }

        /// <summary>
        /// Screen action for doctor.
        /// </summary>
        /// <returns>
        /// Doctor home screen view.
        /// </returns>
        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public IActionResult Doctor(DateTime date = default)
        {
            DoctorCalendar(date.Equals(default) ? DateTime.Now : date);
            ViewData["date"] = date.Equals(default) ? DateTime.Now : date;
            return View();
        }

        private void DoctorCalendar(DateTime day)
        {
            // next 5 days
            List<DateTime> days = new() { day };
            for (int i = 0; i < 4; i++)
            {
                DateTime nextDay = day.AddDays(i + 1);
                days.Add(nextDay);
            }
            ViewData["Days"] = days;

            // time slots
            Person doctor = dbContext.People.Where(x => x == userService.GetCurrentUser().Person).Include(x => x.WorkingHours).Include(x => x.DoctorReservations).FirstOrDefault();
            WorkingHours workingHours = doctor.WorkingHours;
            TimeSpan timeSlot = workingHours.WorkFrom;
            List<TimeSpan> timeSlots = new() { };
            do
            {
                if (timeSlot >= workingHours.BreakFrom && timeSlot < workingHours.BreakTo)
                {
                    timeSlot = workingHours.BreakTo;
                }
                timeSlots.Add(timeSlot);
                timeSlot = timeSlot.Add(TimeSpan.FromMinutes(workingHours.VisitLength));
            } while (timeSlot < workingHours.WorkTo);
            ViewData["TimeSlots"] = timeSlots;

            // reserverSlots
            List<List<Reservation>> reservationsPerDay = new() { };
            for (int i = 0; i < timeSlots.Count; i++)
                {
                reservationsPerDay.Add(new List<Reservation>() { });
                for (int x = 0; x < 5; x++)
                {
                    DateTime d = days[x];
                    TimeSpan s = timeSlots[i];
                    Reservation timeSlotReservation = doctor.DoctorReservations.Where(x => d.Date.Year == x.Date.Year && d.Date.Month == x.Date.Month && d.Date.Day == x.Date.Day && x.Date.Hour == s.Hours && x.Date.Minute == s.Minutes).FirstOrDefault();
                    timeSlotReservation = dbContext.Reservations.Where(x => x.Equals(timeSlotReservation)).Include(x => x.Patient).Include(x => x.Doctor).FirstOrDefault();
                    if (timeSlotReservation == null)
                    {
                        ((List<Reservation>)reservationsPerDay[i]).Add(null);
                        continue;
                    }
                    ((List<Reservation>)reservationsPerDay[i]).Add(timeSlotReservation);
                }
            }
            ViewData["Reserved"] = reservationsPerDay;
            ViewData["NumOfSlots"] = timeSlots.Count;
            ViewData["Length"] = TimeSpan.FromMinutes(workingHours.VisitLength);
        }

        [Route("/Screen/ReserveDoctor", Name = "screenreservedoctor")]
        public IActionResult ReserveDoctor(Dictionary<string, string> data)
        {
            Person doctor = dbContext.People.Where(x => x.Equals(userService.GetCurrentUser().Person)).Include(x => x.WorkingHours).FirstOrDefault();
            DateTime date = DateTime.Parse(data["day"]);
            TimeSpan slot = TimeSpan.Parse(data["slot"]);

            DateTime reservetionDate = date.Date.Add(slot);
            //reservetionDate.Add(slot);
            int length = doctor.WorkingHours.VisitLength;
            string reason = "";
            long doctorNumber = doctor.BirthNumber;
            Reservation newReservation = new() { Date = reservetionDate, Length = length, Reason = reason, DoctorNumber = doctorNumber, PatientNumber = doctorNumber };

            Reservation reservation = dbContext.Reservations.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day && x.Date.Hour == slot.Hours && x.Date.Minute == slot.Minutes).Include(x => x.Doctor).FirstOrDefault();
            ViewData["date"] = DateTime.Now;
            if (reservation != null)
            {
                DoctorCalendar(DateTime.Now);
                return View(nameof(Doctor));
            }
            dbContext.Reservations.Add(newReservation);
            dbContext.SaveChanges();
            DoctorCalendar(DateTime.Now);
            return View(nameof(Doctor));
        }

        [Route("/Screen/CancelDoctor", Name = "screencanceldoctor")]
        public IActionResult CancelDoctor(Dictionary<string, string> data)
        {
            Person doctor = userService.GetCurrentUser().Person;
            DateTime date = DateTime.Parse(data["day"]);
            TimeSpan slot = TimeSpan.Parse(data["slot"]);
            Reservation reservation = dbContext.Reservations.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day && x.Date.Hour == slot.Hours && x.Date.Minute == slot.Minutes).Include(x => x.Doctor).FirstOrDefault();
            dbContext.Reservations.Remove(reservation);
            dbContext.SaveChanges();
            ViewData["date"] = DateTime.Now;
            DoctorCalendar(DateTime.Now);
            return View(nameof(Doctor));
        }

        /// <summary>
        /// Screen action for patient.
        /// </summary>
        /// <returns>
        /// Patient home screen view.
        /// </returns>
        [Authorize]
        [HttpGet]
        public IActionResult Patient(DateTime date = default)
        {
            PatientCalendar(date.Equals(default) ? DateTime.Now : date);
            ViewData["date"] = date.Equals(default) ? DateTime.Now : date;
            return View();
        }

        private void PatientCalendar(DateTime day)
        {
            // next 5 days
            List<DateTime> days = new() { day };
            for (int i = 0; i < 4; i++)
            {
                DateTime nextDay = day.AddDays(i + 1);
                days.Add(nextDay);
            }
            ViewData["Days"] = days;

            // time slots
            Person patient = dbContext.People.Where(x => x == userService.GetCurrentUser().Person).Include(x => x.Doctor).FirstOrDefault();
            Person doctor = dbContext.People.Where(x => x.Equals(patient.Doctor)).Include(x => x.WorkingHours).Include(x => x.DoctorReservations).FirstOrDefault();
            WorkingHours workingHours = doctor.WorkingHours;
            TimeSpan timeSlot = workingHours.WorkFrom;
            List<TimeSpan> timeSlots = new() { };
            do
            {
                if (timeSlot >= workingHours.BreakFrom && timeSlot < workingHours.BreakTo)
                {
                    timeSlot = workingHours.BreakTo;
                }
                timeSlots.Add(timeSlot);
                timeSlot = timeSlot.Add(TimeSpan.FromMinutes(workingHours.VisitLength));
            } while (timeSlot < workingHours.WorkTo);
            ViewData["TimeSlots"] = timeSlots;

            // reserverSlots
            List<List<Reservation>> reservationsPerDay = new() { };
            for (int i = 0; i < timeSlots.Count; i++)
            {
                reservationsPerDay.Add(new List<Reservation>() { });
                for (int x = 0; x < 5; x++)
                {
                    DateTime d = days[x];
                    TimeSpan s = timeSlots[i];
                    Reservation timeSlotReservation = doctor.DoctorReservations.Where(x => d.Date.Year == x.Date.Year && d.Date.Month == x.Date.Month && d.Date.Day == x.Date.Day && x.Date.Hour == s.Hours && x.Date.Minute == s.Minutes).FirstOrDefault();
                    timeSlotReservation = dbContext.Reservations.Where(x => x.Equals(timeSlotReservation)).Include(x => x.Patient).Include(x => x.Doctor).FirstOrDefault();
                    if (timeSlotReservation == null)
                    {
                        ((List<Reservation>)reservationsPerDay[i]).Add(null);
                        continue;
                    }
                    ((List<Reservation>)reservationsPerDay[i]).Add(timeSlotReservation);
                }
            }
            ViewData["Reserved"] = reservationsPerDay;
            ViewData["NumOfSlots"] = timeSlots.Count;
            ViewData["Length"] = TimeSpan.FromMinutes(workingHours.VisitLength);
        }

        [Route("/Screen/CancelPatient", Name = "screencancelpatient")]
        public IActionResult CancelPatient()
        {
            Person patient = userService.GetCurrentUser().Person;
            Reservation reservation = dbContext.People.Where(p => p.BirthNumber == patient.BirthNumber).Include(x => x.PatientReservations).FirstOrDefault().PatientReservations.OrderBy(x => x.Date).Where(x => x.Date > DateTime.Now).FirstOrDefault();
            dbContext.Reservations.Remove(reservation);
            dbContext.SaveChanges();
            ViewData["date"] = DateTime.Now;
            PatientCalendar(DateTime.Now);
            return RedirectToAction("Patient", "Screen");
        }

    }

    public enum Message
    {
        InvalidBirthNumber,
        InvalidPerson,
        ModelError,
        NameUsed,
        Success
    }
}

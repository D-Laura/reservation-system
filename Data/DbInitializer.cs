using Microsoft.AspNetCore.Identity;
using ReservationSystem.Identity.Models;
using ReservationSystem.Models;

namespace ReservationSystem.Data
{
    public class DbInitializer
    {
        public async static Task Initialize(ReservationContext context, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            context.Database.EnsureCreated();

            // Look for any person.
            if (context.People.Any())
            {
                return;   // DB has been seeded
            }

            var people = new Person[]
            {
                new Person{BirthNumber=0057235078,Name="Laura",Surname="Drblíková"},                            // admin
                new Person{BirthNumber=0257235077,Name="Laura",Surname="Drblíková"},                            // doctor 
                new Person{BirthNumber=0057235077,Name="Linda",Surname="Drblíková", DoctorNumber=0257235077},   // patient
                new Person{BirthNumber=0207235077,Name="Emil",Surname="Drblík", DoctorNumber=0257235077},       // patient
                new Person{BirthNumber=9806235077,Name="Alexander",Surname="Carlson", DoctorNumber=0257235077}, // doctor, patient
                new Person{BirthNumber=9857235077,Name="Klára",Surname="Drblíková", DoctorNumber=9806235077},   // patient
                new Person{BirthNumber=0207235078,Name="Ondrej",Surname="Drblík", DoctorNumber=9806235077},     // patient
            };
            context.People.AddRange(people);
            context.SaveChanges();

            var roleAdmin = new IdentityRole() { Name = "Admin"};
            var roleDoctor = new IdentityRole() { Name = "Doctor"};
            // var rolePatient = new IdentityRole() { Name = "Patient" };
            await roleManager.CreateAsync(roleAdmin);
            await roleManager.CreateAsync(roleDoctor);
            // await roleManager.CreateAsync(rolePatient);
            context.SaveChanges();

            User user = new User { PersonNumber = 0057235078, UserName = "laura.admin@gmail.com" };    // admin
            await userManager.CreateAsync(user, "TheBestAdmin5*");
            await userManager.AddToRoleAsync(user, roleAdmin.Name);

            user = new User { PersonNumber = 0257235077, UserName = "drblikova.laura@gmail.com" };     // doctor
            await userManager.CreateAsync(user, "IAmADoctor0!");
            await userManager.AddToRoleAsync(user, roleDoctor.Name);

            user = new User { PersonNumber = 0057235077, UserName = "drblikova.linda@gmail.com" };     // patient
            await userManager.CreateAsync(user, "*AhojSvet0");
            // await userManager.AddToRoleAsync(user, rolePatient.Name);

            user = new User { PersonNumber = 0207235077, UserName = "drblik.emil@gmail.com" };     // patient
            await userManager.CreateAsync(user, "*AhojSvet1");
            // await userManager.AddToRoleAsync(user, rolePatient.Name);

            user = new User { PersonNumber = 9806235077, UserName = "carlson.alex@gmail.com" };     // doctor, patient
            await userManager.CreateAsync(user, "IAmADoctor1!");
            // await userManager.AddToRolesAsync(user, new List<string>() { roleDoctor.Name, rolePatient.Name });
            await userManager.AddToRoleAsync(user, roleDoctor.Name);

            user = new User { PersonNumber = 9857235077, UserName = "drblikova.klara@gmail.com" };     // patient
            await userManager.CreateAsync(user, "*AhojSvet2");
            // await userManager.AddToRoleAsync(user, rolePatient.Name);

            user = new User { PersonNumber = 0207235078, UserName = "drblik.ondrej@gmail.com" };     // patient
            await userManager.CreateAsync(user, "*AhojSvet3");
            // await userManager.AddToRoleAsync(user, rolePatient.Name);

           context.SaveChanges();

            var unregistered = new UnregisteredNumber[]
            {
                new UnregisteredNumber{BirthNumber=0257235079,DoctorNumber=0257235077},
                new UnregisteredNumber{BirthNumber=0257245079,DoctorNumber=0257235077},
                new UnregisteredNumber{BirthNumber=0257235049,DoctorNumber=0257235077},
                new UnregisteredNumber{BirthNumber=0257235029,DoctorNumber=0257235077},
                new UnregisteredNumber{BirthNumber=0257235099,DoctorNumber=0257235077},
                new UnregisteredNumber{BirthNumber=0209235079,DoctorNumber=9806235077},
                new UnregisteredNumber{BirthNumber=0267235079,DoctorNumber=9806235077},
                new UnregisteredNumber{BirthNumber=0207235049,DoctorNumber=9806235077},
                new UnregisteredNumber{BirthNumber=0258235029,DoctorNumber=9806235077},
                new UnregisteredNumber{BirthNumber=0252235099,DoctorNumber=9806235077}
            };
            context.UnregisteredNumbers.AddRange(unregistered);

            context.SaveChanges();
        }
    }
}

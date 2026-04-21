using Microsoft.AspNetCore.Identity;
using TireInventory.Models;

namespace TireInventory
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    createRolesandUsers(scope.ServiceProvider).Wait();
            //}

        }

        private async Task createRolesandUsers(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                bool x = await _roleManager.RoleExistsAsync("Admin");
                if (!x)
                {
                    // first we create Admin rool    
                    var role = new IdentityRole();
                    role.Name = "Admin";
                    await _roleManager.CreateAsync(role);

                    //Here we create a Admin super user who will maintain the website                   

                    var user = new ApplicationUser();

                    user.UserName = "Admin1";
                    user.Email = "cse.com.bd";

                    string userPWD = "Cse@123";

                    IdentityResult chkUser = await _userManager.CreateAsync(user, userPWD);

                    //Add default User to Role Admin    
                    if (chkUser.Succeeded)
                    {
                        var result1 = await _userManager.AddToRoleAsync(user, "Admin");
                    }
                }

                // creating Creating Manager role     
                x = await _roleManager.RoleExistsAsync("Manager");
                if (!x)
                {
                    var role = new IdentityRole();
                    role.Name = "Manager";
                    await _roleManager.CreateAsync(role);
                }

                // creating Creating Employee role     
                x = await _roleManager.RoleExistsAsync("Employee");
                if (!x)
                {
                    var role = new IdentityRole();
                    role.Name = "Employee";
                    await _roleManager.CreateAsync(role);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using slnTestWeb.DataAccess.Data;
using TestApp.Models;
using TestApp.Utilities;

namespace TestApp.DataAccess.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializer(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // get the dbContext, userManager and roleManager from the DI container
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public void Initialize()
    { 
        // Execute migrations if they are not already applied
        try
        {
            var pendingMigrations = _dbContext.Database.GetPendingMigrations();
            if (pendingMigrations.Any()) // check if there are any pending migrations
            {
                _dbContext.Database.Migrate(); // apply pending migrations
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // Create roles if they don't exist
        if (!_roleManager.RoleExistsAsync(StaticDetails.RoleCustomer).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.RoleAdmin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.RoleCustomer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.RoleEmployee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.RoleCompany)).GetAwaiter().GetResult();

            // Create the first admin account
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "applicationadminuser1@gmail.com",
                Email = "applicationadminuser1@gmail.com",
                Name = "Admin 1",
                PhoneNumber = "0988362549",
                StreetAddress = "123 Admin St",
                State = "AS",
                PostalCode = "12345",
                City = "Admin City"
            }, "Admin111@").GetAwaiter().GetResult();
            ApplicationUser user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "applicationadminuser1@gmail.com");
            _userManager.AddToRoleAsync(user, StaticDetails.RoleAdmin).GetAwaiter().GetResult(); // add the user to the admin role
        }

        return;
    }
}
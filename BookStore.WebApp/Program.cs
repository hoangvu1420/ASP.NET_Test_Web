using BookStore.DataAccess.Data;
using BookStore.DataAccess.DbInitializer;
using BookStore.DataAccess.Repositories;
using BookStore.DataAccess.Repositories.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TestApp.Utilities;

namespace BookStore.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        // The AddControllersWithViews method adds the MVC services to the service container.

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        // The AddDbContext method registers the database context class (ApplicationDbContext) with the dependency injection container.
        // this line of code is setting up Entity Framework Core in the application, configuring it to use SQL Server, and specifying the connection string to use when connecting to the database.

        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        // The AddDefaultIdentity method adds the default identity system configuration for the specified User class.
        // The AddEntityFrameworkStores method adds an Entity Framework implementation of the identity system stores.
        // The identity requires that the user has confirmed their account via email confirmation. This is done by setting options => options.SignIn.RequireConfirmedAccount = true
        // When we add identity to the application, it automatically adds the tables required for identity to the database, and it also adds the default UI pages for login, registration, and so on.
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });
        // The ConfigureApplicationCookie method configures the cookie settings for the application.
        // Here, we override the default paths for the login, logout, and access denied pages.
        // This configuration should be done after the AddIdentity method.

        builder.Services.AddAuthentication().AddFacebook(options =>
        {
            options.AppId = "227697123737512";
            options.AppSecret = "701ad2a649431eacc0ae2cf1a38037a7";
        });

        builder.Services
            .AddDistributedMemoryCache(); // sets up the in-memory cache for the application, this is a prerequisite for using session in the application.
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(100); // sets the session timeout to 100 minutes.
            options.Cookie.HttpOnly =
                true; // security setting that prevents client-side scripts from accessing the cookie. Means that the cookie is only accessible by the server side.
            options.Cookie.IsEssential =
                true; // specifies whether the cookie is essential for the application to function correctly.
        }); // the AddSession method adds the session services to the application.

        // builder.Services.AddScoped<IDbInitializer, DbInitializer>(); // registers the DbInitializer class with the dependency injection container.

        builder.Services.AddRazorPages(); // Razor Pages is added to perform identity tasks. Since the application uses Razor Pages for identity, we need to add Razor Pages to the application.

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        // The AddScoped method registers the UnitOfWork class with the dependency injection container.
        // The AddScoped method specifies that a new instance of the UnitOfWork class should be created once per client request (connection).
        // This command basically tells the application to create a new UnitOfWork object whenever it needs to create a new object of type IUnitOfWork.

        builder.Services.AddScoped<IEmailSender, EmailSender>();
        // this line tells the application to create a new EmailSender object whenever it needs to create a new object of type IEmailSender.
        // we created a custom EmailSender class that implements the IEmailSender interface. This class is used to send emails in the application.

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        StripeConfiguration.ApiKey = app.Configuration.GetSection("Stripe")["SecretKey"];

        app.UseRouting();
        // The UseRouting method adds the routing middleware to the request pipeline. The routing middleware determines which controller and action to run based on the HTTP request.

        app.UseAuthentication(); // The UseAuthentication method adds the authentication middleware to the request pipeline.
        app.UseAuthorization();

        app.UseSession();

        // SeedDatabase(); // call the SeedDatabase method to seed the database with initial data.

        app.MapRazorPages();

        app.MapControllerRoute(
            name: "default",
            pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"); // The default area is Customer, the default controller is Home, the default action is Index, and the default id is null.
        // Set the default page to be the Index page of the Home controller. When the app starts, the user will see the Index page of the Home controller.

        app.Run();
        return;

        void SeedDatabase()
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var dbInitializer = services.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize();
        }
    }

    /*
     * In ASP.NET Core, the term �pipeline� refers to the sequence of middleware components that handle an HTTP request and generate an HTTP response.
     * Middleware components are software units that are assembled into an application pipeline to handle requests and responses.
     * Each piece of middleware in the pipeline is responsible for invoking the next component in the pipeline or short-circuiting the pipeline if appropriate.
     * For example, if a middleware component can fully handle a request (like serving a static file), it can generate a response and short-circuit the rest of the pipeline.
     * It�s important to note that some middleware components depend on others. For example, the authorization
     * middleware (UseAuthorization) depends on the authentication middleware (UseAuthentication),
     * so UseAuthentication must be called before UseAuthorization.
     */
}
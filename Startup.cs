using CodeBuggy.Data;
using CodeBuggy.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeBuggy;
public class Startup
{
    public readonly IConfiguration _configuration;
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("CodeBuggyDB") ?? throw new InvalidOperationException("Connection string 'CodeBuggyDB' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddControllersWithViews();

        services.AddTransient<IEmailSender, EmailSender>();

        services.AddMvc()
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.Clear();
                options.RootDirectory = "/Views/Account";
                Console.WriteLine(options);
            });
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }
}

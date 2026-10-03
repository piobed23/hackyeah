using App.Data;
using App.Data.Seed;
using App.Infrastructure;
using App.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace App;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
                          ?? "Data Source=rops.db"));

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(opt =>
        {
            opt.Cookie.Name = ".rops.session";
            opt.Cookie.IsEssential = true;
            opt.IdleTimeout = TimeSpan.FromHours(8);
        });

        builder.Services.AddAuthentication("demo").AddCookie("demo");
        builder.Services.AddAuthorization(opt =>
        {
            opt.AddPolicy("RolaROPS", p => p.RequireRole("PracownikROPS", "Admin"));
            opt.AddPolicy("RolaAutor", p => p.RequireRole("Autor", "Admin"));
            opt.AddPolicy("RolaAdmin", p => p.RequireRole("Admin"));
            opt.AddPolicy("RolaEkspert", p => p.RequireRole("Ekspert", "Admin"));
            opt.AddPolicy("RolaInstytucja", p => p.RequireRole("Instytucja", "Admin"));
        });

        builder.Services.AddRazorPages()
            .AddRazorPagesOptions(o =>
            {
                o.Conventions.AuthorizeFolder("/Rops", "RolaROPS");
                o.Conventions.AuthorizeFolder("/Autor", "RolaAutor");
                o.Conventions.AuthorizeFolder("/Wnioski", "RolaAutor");
                o.Conventions.AuthorizeFolder("/Kreator", "RolaAutor");
                o.Conventions.AuthorizeFolder("/Moje", "RolaAutor");
                o.Conventions.AuthorizeFolder("/Middleman", "RolaInstytucja");
                o.Conventions.AuthorizeFolder("/Admin", "RolaAdmin");
            });

        builder.Services.AddDomainServices();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DemoSeeder.SeedAsync(db);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();
        app.UseMiddleware<DemoRoleMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}

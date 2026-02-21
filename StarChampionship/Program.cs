using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarChampionship.Data;
using System.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; 
using Pomelo.EntityFrameworkCore.MySql;
using StarChampionship.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDbContext<StarChampionshipContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("StarChampionshipContext"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("StarChampionshipContext")),
            mySqlOptions => mySqlOptions.MigrationsAssembly("StarChampionship")
        ));


//Addscoped

builder.Services.AddScoped<PlayerService>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var seedingService = services.GetRequiredService<SeedingService>();
        seedingService.Seed();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao executar o seeding");
    }
}

//locale
var enUS = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(enUS),
    SupportedCultures = new List<CultureInfo> { enUS },
    SupportedUICultures = new List<CultureInfo> { enUS }
};

app.UseRequestLocalization(localizationOptions);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

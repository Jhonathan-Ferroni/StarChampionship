using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using StarChampionship.Data;
using StarChampionship.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// CONFIGURAÇÃO DA PORTA (RENDER)
// ===========================
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";

// ===========================
// CONFIGURAÇÃO DE AUTENTICAÇÃO (JWT + Cookies)
// ===========================
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "StarChampionshipApi";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "StarChampionshipUsers";

var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services
    // Usar Cookies como esquema padrão para páginas MVC/Razor (navegador)
    // e manter JWT Bearer disponível para chamadas API (AJAX / clientes externos).
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        // Página de login usada pelo fluxo baseado em Cookie (Razor Pages / MVC)
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
    })
    // JWT Bearer configurado para APIs — continua retornando JSON em respostas 401/403
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Sem tolerância de tempo
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Responder com JSON para chamadas API
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
            }
        };
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// ===========================
// CONNECTION STRING
// ===========================
var connectionString = builder.Configuration
    .GetConnectionString("StarChampionshipContext");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string não configurada.");
}

builder.Services.AddDbContext<StarChampionshipContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.MigrationsAssembly("StarChampionship")
    ));

// ===========================
// SERVICES
// ===========================
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GeneratorService>();
builder.Services.AddScoped<SeedingService>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddControllersWithViews();
// Registrar Razor Pages para suportar Pages/AdminLogin.cshtml
builder.Services.AddRazorPages();

// Adiciona suporte a APIs REST
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===========================
// SEEDING AUTOMÁTICO
// ===========================
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

// ===========================
// LOCALIZAÇÃO
// ===========================
var enUS = new CultureInfo("en-US");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(enUS),
    SupportedCultures = new List<CultureInfo> { enUS },
    SupportedUICultures = new List<CultureInfo> { enUS }
};

app.UseRequestLocalization(localizationOptions);

// ===========================
// PIPELINE HTTP
// ===========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Adiciona CORS
app.UseCors("AllowAll");

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia rotas MVC tradicionais
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapeia as APIs REST
app.MapControllers();

app.Run();
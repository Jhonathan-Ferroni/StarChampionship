using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using StarChampionship.Data;
using StarChampionship.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;


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
    // Usar JWT Bearer como esquema padrão para APIs
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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

// Registrar apenas controllers (API-only)
builder.Services.AddControllers();

// Adiciona suporte a APIs REST

// Configuração do Swagger com suporte a JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "StarChampionship API", Version = "v1" });

    // Configura o botão "Authorize" para inserir o JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ativa o Swagger apenas em ambiente de Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StarChampionship API v1");
    });
}

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
    // Retornar JSON para erros em produção (API)
    app.UseExceptionHandler(a =>
    {
        a.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred." });
        });
    });
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();


// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia as APIs REST
app.MapControllers();

app.Run();
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RandaSports.Infrastructure.Security;
using RandaSports.Api.Handlers;
using RandaSports.Application.DependencyInjection;
using RandaSports.Persistence.Seeders;
using RandaSports.Persistence.ServiceRegistration;
using RandaSports.Infrastructure.ServiceRegistration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services
    .AddControllers()
    .AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions));
builder.Services.ConfigureHttpJsonOptions(options => ConfigureJson(options.SerializerOptions));

// Kimlik doğrulama: hem yönetim hem okuyucu. Rol talepleri jetonun içinde geliyor.
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key tanımlı değil ya da 32 karakterden kısa. Yönetim uçları imzalanamaz; "
        + "appsettings.Development.json içine uzun ve rastgele bir anahtar ekleyin.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Varsayılan davranış claim adlarını eski SOAP şemalarına çeviriyor
        // ("email" → "http://schemas.xmlsoap.org/.../emailaddress"). Kapatıyoruz ki
        // jetona yazdığımız adlar okurken de aynı kalsın.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

            // Varsayılan 5 dakikalık tolerans, süresi dolmuş jetonu bir süre daha
            // geçerli sayıyor; yönetim panelinde buna gerek yok.
            ClockSkew = TimeSpan.Zero,

            // MapInboundClaims kapalıyken hangi talebin rol, hangisinin ad sayılacağını
            // kendimiz söylüyoruz; yoksa [Authorize(Roles = ...)] boşa düşüyor.
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);

    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().RunAsync(autoMigrate);
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Geliştirmede kapalı: frontend http://localhost:5122'ye bağlanıyor ve
// yönlendirme sonrası kendi imzalı sertifika Node tarafında reddediliyor.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ConfigureJson(JsonSerializerOptions options)
{
    options.Converters.Add(new JsonStringEnumConverter());

    // Boş alanlar JSON'dan atılmıyor: istemci "yok" ile "null" ayrımını
    // yapabilsin ve alanlar undefined'a düşüp patlamasın.
    options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
}

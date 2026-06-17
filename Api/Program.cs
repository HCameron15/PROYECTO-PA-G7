using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text;

// Aquí creamos el constructor principal de la aplicación web.
// Este objeto "builder" se usa para registrar servicios y configuraciones.
var builder = WebApplication.CreateBuilder(args);

// Registramos soporte para controladores (los endpoints de tipo API tradicional).
builder.Services.AddControllers();

// Registramos servicios para generar documentación Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {su_token_jwt}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registramos localización para usar archivos de recursos .resx.
// La ruta de recursos se lee desde appsettings.json -> Localization:ResourcesPath.
var resourcesPath = builder.Configuration["Localization:ResourcesPath"] ?? "Resources";
builder.Services.AddLocalization(options => options.ResourcesPath = resourcesPath);

// Registramos el DbContext (conexión a base de datos SQL Server).
// La cadena de conexión se lee desde appsettings.json -> ConnectionStrings:DefaultConnection.
builder.Services.AddDbContext<Uam.AdvancedProgramming.Api.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registramos Unit of Work para inyección de dependencias.
// Esto permite que el controlador use una sola puerta de acceso a repositorios.
builder.Services.AddScoped<Uam.AdvancedProgramming.Api.Interfaces.IUnitOfWork, Uam.AdvancedProgramming.Api.Repositories.UnitOfWork>();

// Configuramos autenticación JWT nativa.
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Falta Jwt:SecretKey en appsettings.json");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Construimos la aplicación con todo lo registrado anteriormente.
var app = builder.Build();

// Culturas soportadas y cultura por defecto leídas desde appsettings.json -> Localization.
var defaultCulture = app.Configuration["Localization:DefaultCulture"] ?? "es";
var supportedCultureCodes = app.Configuration.GetSection("Localization:SupportedCultures").Get<string[]>() ?? ["es", "en"];
var supportedCultures = supportedCultureCodes.Select(c => new CultureInfo(c)).ToArray();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Middleware global para capturar excepciones y devolver respuesta JSON uniforme.
app.UseMiddleware<Uam.AdvancedProgramming.Api.Middlewares.ExceptionMiddleware>();

// Activamos generación del JSON OpenAPI.
app.UseSwagger();

// Activamos interfaz web de Swagger para probar endpoints.
app.UseSwaggerUI();

// Redirecciona HTTP a HTTPS para mayor seguridad.
app.UseHttpsRedirection();

// Activa autenticación JWT.
app.UseAuthentication();

// Habilita middleware de autorización (si en el futuro agrega [Authorize], ya está la tubería lista).
app.UseAuthorization();

// Mapea los controladores para que sus rutas queden activas.
app.MapControllers();

// Inicia la aplicación y la deja escuchando solicitudes.
app.Run();


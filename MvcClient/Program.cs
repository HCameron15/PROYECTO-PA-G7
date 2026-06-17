// Builder principal de la aplicación MVC.
var builder = WebApplication.CreateBuilder(args);

// Registramos soporte MVC con controladores y vistas.
builder.Services.AddControllersWithViews();

// Registramos HttpClient para consumir el API desde el servidor MVC.
builder.Services.AddHttpClient();

// Construimos la aplicación con todos los servicios ya registrados.
var app = builder.Build();

// Si NO estamos en desarrollo, activamos manejo de errores y HSTS.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Fuerza redirección a HTTPS.
app.UseHttpsRedirection();

// Activa enrutamiento.
app.UseRouting();

// Activa middleware de autorización (queda listo por si se agrega seguridad del lado MVC).
app.UseAuthorization();

// Mapea archivos estáticos (css/js/img) para .NET 10.
app.MapStaticAssets();

// Ruta por defecto de la app MVC: Maintenance/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

// Inicia la aplicación y la deja escuchando peticiones HTTP.
app.Run();

using System.Globalization;
using Expocentro.Web.Data;
using Expocentro.Web.Seguridad;
using Expocentro.Web.Services;
using Expocentro.Web.Services.Notificaciones;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// ============================
// BASE DE DATOS Y SERVICIOS
// ============================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UsuarioSincronizacionService>();
builder.Services.AddScoped<DisponibilidadService>();
builder.Services.AddScoped<SolicitudEstadoService>();

// ============================
// NOTIFICACIONES POR CORREO
// ============================
// Con Notificaciones:Habilitado=true, Remitente y AzureAd:ClientSecret se envía
// por Microsoft Graph; si no, los correos solo se registran en el log.

builder.Services.Configure<NotificacionesOptions>(
    builder.Configuration.GetSection("Notificaciones"));

builder.Services.AddHttpClient("graph");
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ColaCorreos>();
builder.Services.AddHostedService<CorreoBackgroundService>();
builder.Services.AddScoped<NotificacionService>();

var notificaciones = builder.Configuration
    .GetSection("Notificaciones").Get<NotificacionesOptions>() ?? new();

var graphConfigurado =
    notificaciones.Habilitado
    && !string.IsNullOrWhiteSpace(notificaciones.Remitente)
    && !string.IsNullOrWhiteSpace(builder.Configuration["AzureAd:ClientSecret"]);

if (graphConfigurado)
    builder.Services.AddSingleton<ICorreoEmisor, GraphCorreoEmisor>();
else
    builder.Services.AddSingleton<ICorreoEmisor, LogCorreoEmisor>();

// ============================
// AUTENTICACIÓN
// ============================
// - Si existe la sección AzureAd:ClientId => Microsoft Entra ID.
// - Si no, en Development (o con Autenticacion:PermitirAccesoDesarrollo=true)
//   se habilita un acceso de prueba para elegir un usuario demo.

var entraConfigurado =
    !string.IsNullOrWhiteSpace(builder.Configuration["AzureAd:ClientId"]);

var permitirDesarrollo =
    builder.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Autenticacion:PermitirAccesoDesarrollo");

if (entraConfigurado)
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    // Al validar el token: registrar/actualizar el usuario y agregar roles
    builder.Services.Configure<OpenIdConnectOptions>(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            var anterior = options.Events.OnTokenValidated;

            options.Events.OnTokenValidated = async context =>
            {
                await anterior(context);

                var servicio = context.HttpContext.RequestServices
                    .GetRequiredService<UsuarioSincronizacionService>();

                var identidad = await servicio
                    .SincronizarEntraAsync(context.Principal!);

                if (identidad == null)
                {
                    context.Fail("El usuario está desactivado en Expocentro.");
                    return;
                }

                context.Principal!.AddIdentity(identidad);
            };

            // Errores al volver de Microsoft (usuario desactivado, consentimiento
            // cancelado, secreto vencido...): mostrar una pantalla amigable.
            options.Events.OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Expocentro.EntraId");

                logger.LogWarning(context.Failure,
                    "Falló el inicio de sesión con Entra ID");

                var desactivado = context.Failure?.Message
                    .Contains("desactivado", StringComparison.OrdinalIgnoreCase) == true;

                context.Response.Redirect(
                    "/Cuenta/ErrorInicio" + (desactivado ? "?motivo=desactivado" : ""));
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });

    builder.Services.Configure<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options => options.AccessDeniedPath = "/Cuenta/AccesoDenegado");

    builder.Services.AddSingleton(new ModoAutenticacion(TipoAutenticacion.EntraId));
}
else if (permitirDesarrollo)
{
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Cuenta/AccesoDesarrollo";
            options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
            options.Cookie.Name = "Expocentro.Dev";
        });

    builder.Services.AddSingleton(new ModoAutenticacion(TipoAutenticacion.Desarrollo));
}
else
{
    throw new InvalidOperationException(
        "Configure la sección 'AzureAd' (TenantId, ClientId, ClientSecret) " +
        "o habilite 'Autenticacion:PermitirAccesoDesarrollo' solo en ambientes de prueba.");
}

// ============================
// AUTORIZACIÓN
// ============================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.Coordinacion, p =>
        p.RequireRole(Roles.Coordinador, Roles.Administrador));

    options.AddPolicy(Politicas.Administracion, p =>
        p.RequireRole(Roles.Administrador));
});

// Todas las páginas requieren inicio de sesión salvo [AllowAnonymous]
builder.Services.AddControllersWithViews(options =>
{
    var politica = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(politica));
});

// ============================
// APLICACIÓN
// ============================

var app = builder.Build();

if (notificaciones.Habilitado && !graphConfigurado)
{
    app.Logger.LogWarning(
        "Notificaciones:Habilitado=true pero falta Notificaciones:Remitente o AzureAd:ClientSecret. " +
        "Los correos solo se registrarán en el log.");
}

var cultura = new CultureInfo("es-HN");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

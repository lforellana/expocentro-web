using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Channels;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace Expocentro.Web.Services.Notificaciones;

/// <summary>Sección "Notificaciones" de la configuración.</summary>
public class NotificacionesOptions
{
    /// <summary>Si es false, los correos solo se escriben en el log.</summary>
    public bool Habilitado { get; set; }

    /// <summary>Buzón desde el que se envía (ej. eventos@ccichonduras.org).</summary>
    public string Remitente { get; set; } = string.Empty;

    /// <summary>Correos que reciben los avisos de Coordinación (además de los usuarios con rol Coordinador).</summary>
    public string[] CorreosCoordinacion { get; set; } = Array.Empty<string>();

    /// <summary>URL pública de la app para los enlaces; si está vacía se usa la de la petición.</summary>
    public string? UrlBase { get; set; }

    /// <summary>Solo desarrollo: carpeta donde se guardan los correos como .html en vez de enviarlos.</summary>
    public string? CarpetaPrueba { get; set; }
}

public record CorreoMensaje(IReadOnlyList<string> Para, string Asunto, string Html);

public interface ICorreoEmisor
{
    Task EnviarAsync(CorreoMensaje mensaje, CancellationToken cancellationToken);
}

/// <summary>Cola en memoria: las peticiones encolan y el servicio de fondo envía.</summary>
public class ColaCorreos
{
    private readonly Channel<CorreoMensaje> _canal =
        Channel.CreateBounded<CorreoMensaje>(new BoundedChannelOptions(500)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

    public void Encolar(CorreoMensaje mensaje) => _canal.Writer.TryWrite(mensaje);

    public IAsyncEnumerable<CorreoMensaje> LeerTodoAsync(CancellationToken ct) =>
        _canal.Reader.ReadAllAsync(ct);
}

public class CorreoBackgroundService : BackgroundService
{
    private readonly ColaCorreos _cola;
    private readonly ICorreoEmisor _emisor;
    private readonly ILogger<CorreoBackgroundService> _logger;

    public CorreoBackgroundService(
        ColaCorreos cola,
        ICorreoEmisor emisor,
        ILogger<CorreoBackgroundService> logger)
    {
        _cola = cola;
        _emisor = emisor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var mensaje in _cola.LeerTodoAsync(stoppingToken))
        {
            for (var intento = 1; intento <= 3; intento++)
            {
                try
                {
                    await _emisor.EnviarAsync(mensaje, stoppingToken);
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex,
                        "No se pudo enviar el correo \"{Asunto}\" (intento {Intento}/3)",
                        mensaje.Asunto, intento);

                    if (intento < 3)
                        await Task.Delay(TimeSpan.FromSeconds(10 * intento), stoppingToken);
                }
            }
        }
    }
}

/// <summary>Desarrollo o notificaciones deshabilitadas: solo registra el correo.</summary>
public class LogCorreoEmisor : ICorreoEmisor
{
    private readonly ILogger<LogCorreoEmisor> _logger;
    private readonly string? _carpeta;

    public LogCorreoEmisor(ILogger<LogCorreoEmisor> logger, IOptions<NotificacionesOptions> opciones)
    {
        _logger = logger;
        _carpeta = opciones.Value.CarpetaPrueba;
    }

    public async Task EnviarAsync(CorreoMensaje mensaje, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Correo no enviado - notificaciones deshabilitadas] Para: {Para} | Asunto: {Asunto}",
            string.Join(", ", mensaje.Para), mensaje.Asunto);

        if (string.IsNullOrWhiteSpace(_carpeta))
            return;

        Directory.CreateDirectory(_carpeta);
        var archivo = Path.Combine(_carpeta, $"{DateTime.Now:yyyyMMdd-HHmmss-fff}.html");
        var encabezado = $"<!-- Para: {string.Join(", ", mensaje.Para)} | Asunto: {mensaje.Asunto} -->\n" +
                         "<meta charset=\"utf-8\">\n";
        await File.WriteAllTextAsync(archivo, encabezado + mensaje.Html, cancellationToken);
    }
}

/// <summary>
/// Envío con Microsoft Graph (permiso de aplicación Mail.Send) usando el
/// registro de Entra ID de la app (TenantId, ClientId y ClientSecret).
/// </summary>
public class GraphCorreoEmisor : ICorreoEmisor
{
    private static readonly string[] Alcance = { "https://graph.microsoft.com/.default" };

    private readonly IHttpClientFactory _http;
    private readonly TokenCredential _credencial;
    private readonly NotificacionesOptions _opciones;
    private readonly ILogger<GraphCorreoEmisor> _logger;

    public GraphCorreoEmisor(
        IHttpClientFactory http,
        IConfiguration configuration,
        IOptions<NotificacionesOptions> opciones,
        ILogger<GraphCorreoEmisor> logger)
    {
        _http = http;
        _opciones = opciones.Value;
        _logger = logger;
        _credencial = new ClientSecretCredential(
            configuration["AzureAd:TenantId"],
            configuration["AzureAd:ClientId"],
            configuration["AzureAd:ClientSecret"]);
    }

    public async Task EnviarAsync(CorreoMensaje mensaje, CancellationToken cancellationToken)
    {
        var token = await _credencial.GetTokenAsync(
            new TokenRequestContext(Alcance), cancellationToken);

        var cuerpo = new
        {
            message = new
            {
                subject = mensaje.Asunto,
                body = new { contentType = "HTML", content = mensaje.Html },
                toRecipients = mensaje.Para
                    .Select(x => new { emailAddress = new { address = x } })
            },
            saveToSentItems = false
        };

        var cliente = _http.CreateClient("graph");
        using var peticion = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(_opciones.Remitente)}/sendMail")
        {
            Content = JsonContent.Create(cuerpo)
        };
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        using var respuesta = await cliente.SendAsync(peticion, cancellationToken);

        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Graph respondió {(int)respuesta.StatusCode}: {detalle}");
        }

        _logger.LogInformation("Correo enviado: \"{Asunto}\" a {Cantidad} destinatario(s)",
            mensaje.Asunto, mensaje.Para.Count);
    }
}

namespace Expocentro.Web.Services;

public static class Fechas
{
    // Honduras: UTC-6 sin horario de verano
    private static readonly TimeSpan OffsetHonduras = TimeSpan.FromHours(-6);

    public static DateTimeOffset ALocal(DateTimeOffset fecha)
        => fecha.ToOffset(OffsetHonduras);

    public static DateTime Ahora()
        => DateTimeOffset.UtcNow.ToOffset(OffsetHonduras).DateTime;

    public static string Rango(DateTime inicio, DateTime fin)
    {
        if (inicio.Date == fin.Date)
        {
            return $"{inicio:ddd dd/MM/yyyy} · {inicio:hh:mm tt} – {fin:hh:mm tt}";
        }

        return $"{inicio:dd/MM/yyyy hh:mm tt} – {fin:dd/MM/yyyy hh:mm tt}";
    }
}

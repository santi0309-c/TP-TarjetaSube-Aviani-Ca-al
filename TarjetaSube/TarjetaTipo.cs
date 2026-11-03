namespace TarjetaSube;

public class TarjetaTipo
{
    public const string NombreNormal = "Normal";
    public const string NombreMedioBoleto = "Medio Boleto Estudiantil";
    public const string NombreFranquiciaCompleta = "Franquicia Completa";
    public const string NombreBoletoGratuito = "Boleto Gratuito Estudiantil";

    public static TarjetaTipo Normal => new(NombreNormal, 1.0m);
    public static TarjetaTipo MedioBoleto => new(NombreMedioBoleto, 0.5m);
    public static TarjetaTipo FranquiciaCompleta => new(NombreFranquiciaCompleta, 0.0m);
    public static TarjetaTipo BoletoGratuito => new(NombreBoletoGratuito, 0.0m);

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Multiplicador { get; set; } = 1m;

    public bool EsMedioBoleto =>
        !string.IsNullOrEmpty(Nombre) && Nombre.Contains("Medio", StringComparison.OrdinalIgnoreCase);

    public bool EsFranquiciaCompleta =>
        !string.IsNullOrEmpty(Nombre) && (
            Nombre.Contains("Franquicia Completa", StringComparison.OrdinalIgnoreCase) ||
            Nombre.Contains("Gratuito", StringComparison.OrdinalIgnoreCase) ||
            Nombre.Contains("Educativo", StringComparison.OrdinalIgnoreCase)
        );

    public bool EsNormal =>
        string.IsNullOrEmpty(Nombre) ||
        Nombre.Equals(NombreNormal, StringComparison.OrdinalIgnoreCase);

    public bool EsFranquicia => !EsNormal;

    public TarjetaTipo() { }

    public TarjetaTipo(string nombre, decimal multiplicador)
    {
        Nombre = nombre;
        Multiplicador = multiplicador;
    }

    public decimal CalcularPrecio(decimal tarifaBase)
    {
        return tarifaBase * Multiplicador;
    }

    public decimal CalcularTarifa(decimal tarifaBase, int viajesHoy = 0, int viajesMes = 0)
    {
        if (EsNormal)
        {
            if (viajesMes < 29)
            {
                return tarifaBase; // Viajes 1 al 29: Tarifa normal
            }
            if (viajesMes < 79)
            {
                return tarifaBase * 0.80m; // Viajes 30 al 79: 20% de descuento
            }
            return tarifaBase * 0.75m; // Viajes 80 en adelante: 25% de descuento
        }

        if (EsMedioBoleto)
        {
            return viajesHoy >= 2 ? tarifaBase : tarifaBase * Multiplicador;
        }

        if (EsFranquiciaCompleta)
        {
            return viajesHoy >= 2 ? tarifaBase : 0m;
        }

        return tarifaBase * Multiplicador;
    }

    public bool CumpleFranjaHoraria(DateTime fecha)
    {
        if (!EsFranquicia)
        {
            return true;
        }

        bool esDiaHabil = fecha.DayOfWeek >= DayOfWeek.Monday && fecha.DayOfWeek <= DayOfWeek.Friday;
        if (!esDiaHabil)
        {
            return false;
        }

        TimeSpan hora = fecha.TimeOfDay;
        return hora >= TimeSpan.FromHours(6) && hora <= TimeSpan.FromHours(22);
    }
}

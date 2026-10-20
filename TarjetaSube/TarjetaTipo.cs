namespace TarjetaSube;

public class TarjetaTipo
{
    public const string NombreNormal = "Normal";
    public const string NombreMedioBoleto = "Medio Boleto Estudiantil";
    public const string NombreFranquiciaCompleta = "Franquicia Completa";
    public const string NombreBoletoGratuito = "Boleto Gratuito Estudiantil";

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
}
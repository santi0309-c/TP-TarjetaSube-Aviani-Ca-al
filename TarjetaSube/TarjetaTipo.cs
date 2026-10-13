namespace TarjetaSube;

public class TarjetaTipo
{
    public int Id {get; set; }
    public string Nombre {get; set; } = string.Empty;
    public decimal Multiplicador {get; set; } = 1m;

    public TarjetaTipo(){}

    public TarjetaTipo (string nombre, decimal multiplicador)
    {
        Nombre = nombre;
        Multiplicador = multiplicador;
    }

    public decimal CalcularPrecio(decimal tarifaBase)
    {
        return tarifaBase * Multiplicador;
    }

}
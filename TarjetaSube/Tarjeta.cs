namespace TarjetaSube;

public class Tarjeta
{
    public const decimal LimiteSaldo = 40000m;
    public static readonly IReadOnlySet<decimal>
 CargasAceptadas = new HashSet<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };

    
}
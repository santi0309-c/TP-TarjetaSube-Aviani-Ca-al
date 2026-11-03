namespace TarjetaSube;

public class ColectivoInterurbano : Colectivo
{
    public const decimal TarifaInterurbanaFija = 3000m;

    public ColectivoInterurbano() : base(string.Empty, esInterurbano: true, timeProvider: null)
    {
    }

    public ColectivoInterurbano(string linea) : base(linea, esInterurbano: true, timeProvider: null)
    {
    }

    public ColectivoInterurbano(string linea, ITimeProvider? timeProvider) : base(linea, esInterurbano: true, timeProvider)
    {
    }
}

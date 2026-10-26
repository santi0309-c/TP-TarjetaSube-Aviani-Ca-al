namespace TarjetaSube;

public class ColectivoInterurbano : Colectivo
{
    public const decimal TarifaInterurbanaFija = 3000m;

    public ColectivoInterurbano() : base()
    {
        EsInterurbano = true;
    }

    public ColectivoInterurbano(string linea) : base(linea, esInterurbano: true)
    {
    }
}

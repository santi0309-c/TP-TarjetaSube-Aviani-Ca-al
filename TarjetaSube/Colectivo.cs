namespace TarjetaSube;

public class Colectivo
{
    public const decimal TarifaBasica = 1580m;

    public int Id { get; set; }
    public string Linea { get; set; } = string.Empty;
    public decimal Tarifa => TarifaBasica;

    public Colectivo()
    {
    }

    public Colectivo(string linea)
    {
        Linea = linea;
    }

    public Boleto? PagarCon(Tarjeta tarjeta)
    {
        if (tarjeta == null)
        {
            return null;
        }

        decimal tarifa = tarjeta.CalcularPrecio(TarifaBasica);

        if (!tarjeta.DescontarSaldo(tarifa))
        {
            return null;
        }

        return new Boleto(Linea, tarifa, tarjeta.Saldo, tarjeta.Id);
    }

    public Boleto? pagarCon(Tarjeta tarjeta) => PagarCon(tarjeta);
}

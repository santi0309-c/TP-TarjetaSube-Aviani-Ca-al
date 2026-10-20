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

    public Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaViaje = null)
    {
        if (tarjeta == null)
        {
            return null;
        }

        DateTime fecha = fechaViaje ?? DateTime.Now;

        if (!tarjeta.PuedeViajar(fecha))
        {
            return null;
        }

        decimal tarifa = tarjeta.CalcularPrecio(TarifaBasica, fecha);
        decimal saldoPrevio = tarjeta.Saldo;

        if (!tarjeta.DescontarSaldo(tarifa))
        {
            return null;
        }

        tarjeta.RegistrarViaje(fecha);

        decimal totalAbonado = tarifa;
        if (saldoPrevio < 0)
        {
            totalAbonado += Math.Abs(saldoPrevio);
        }

        string tipoNombre = tarjeta.TarjetaTipo?.Nombre ?? TarjetaTipo.NombreNormal;
        return new Boleto(Linea, totalAbonado, tarjeta.Saldo, tarjeta.Id, fecha, tipoNombre);
    }

    public Boleto? pagarCon(Tarjeta tarjeta) => PagarCon(tarjeta);
    public Boleto? pagarCon(Tarjeta tarjeta, DateTime fecha) => PagarCon(tarjeta, fecha);
}

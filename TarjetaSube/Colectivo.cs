namespace TarjetaSube;

public class Colectivo
{
    public const decimal TarifaBasica = 2150m;
    public const decimal TarifaInterurbana = 3000m;

    public int Id { get; set; }
    public string Linea { get; set; } = string.Empty;
    public bool EsInterurbano { get; set; } = false;
    public virtual decimal Tarifa => EsInterurbano ? TarifaInterurbana : TarifaBasica;

    public Colectivo()
    {
    }

    public Colectivo(string linea)
    {
        Linea = linea;
    }

    public Colectivo(string linea, bool esInterurbano)
    {
        Linea = linea;
        EsInterurbano = esInterurbano;
    }

    public Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaViaje = null)
    {
        if (tarjeta == null)
        {
            return null;
        }

        DateTime fecha = fechaViaje ?? DateTime.Now;

        if (!tarjeta.PuedeViajar(fecha, Linea))
        {
            return null;
        }

        bool esTrasbordo = tarjeta.UltimoBoleto != null && tarjeta.UltimoBoleto.EsTrasbordoValido(Linea, fecha);

        decimal tarifa;
        decimal saldoPrevio = tarjeta.Saldo;

        if (esTrasbordo)
        {
            tarifa = 0m;
        }
        else
        {
            tarifa = tarjeta.CalcularPrecio(Tarifa, fecha);
            if (!tarjeta.DescontarSaldo(tarifa))
            {
                return null;
            }
        }

        tarjeta.RegistrarViaje(fecha, esTrasbordo);

        decimal totalAbonado = tarifa;
        if (!esTrasbordo && saldoPrevio < 0)
        {
            totalAbonado += Math.Abs(saldoPrevio);
        }

        string tipoNombre = tarjeta.TarjetaTipo?.Nombre ?? TarjetaTipo.NombreNormal;
        var boleto = new Boleto(Linea, totalAbonado, tarjeta.Saldo, tarjeta.Id, fecha, tipoNombre, esTrasbordo, tarjeta.UltimoBoleto);
        tarjeta.UltimoBoleto = boleto;

        return boleto;
    }

    public Boleto? pagarCon(Tarjeta tarjeta) => PagarCon(tarjeta);
    public Boleto? pagarCon(Tarjeta tarjeta, DateTime fecha) => PagarCon(tarjeta, fecha);
}

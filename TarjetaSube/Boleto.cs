namespace TarjetaSube;

public class Boleto
{
    public int Id { get; set; }
    public string Linea { get; private set; } = string.Empty;
    public decimal MontoPagado { get; private set; }
    public decimal TotalAbonado => MontoPagado;
    public decimal SaldoRestante { get; private set; }
    public decimal Saldo => SaldoRestante;
    public int TarjetaId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string TipoTarjeta { get; private set; } = string.Empty;

    public Boleto()
    {
        Linea = string.Empty;
    }

    public Boleto(string linea, decimal montoPagado, decimal saldoRestante, int tarjetaId,
                  DateTime? fecha = null, string tipoTarjeta = "")
    {
        Linea = linea;
        MontoPagado = montoPagado;
        SaldoRestante = saldoRestante;
        TarjetaId = tarjetaId;
        Fecha = fecha ?? DateTime.Now;
        TipoTarjeta = tipoTarjeta;
    }

    public DateTime ObtenerFecha() => Fecha;
    public string ObtenerTipoTarjeta() => TipoTarjeta;
    public string ObtenerLinea() => Linea;
    public decimal ObtenerTotalAbonado() => TotalAbonado;
    public decimal ObtenerSaldo() => Saldo;
    public int ObtenerTarjetaId() => TarjetaId;
}

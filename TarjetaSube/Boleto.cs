namespace TarjetaSube;

public class Boleto
{
    public int Id {get; set;}
    public string Linea { get; private set; } = string.Empty;
    public decimal MontoPagado {get; private set;}
    public decimal SaldoRestante {get; private set;}
    public int TarjetaId {get; private set;}
    public DateTime Fecha {get; private set;}

    public Boleto()
    {
        Linea = string.Empty;
    }

    public Boleto (string linea, decimal montoPagado, decimal saldoRestante, int tarjetaId, DateTime? fecha = null)
    {
        Linea = linea;
        MontoPagado = montoPagado;
        SaldoRestante = saldoRestante;
        TarjetaId = tarjetaId;
        Fecha = fecha ?? DateTime.Now;
    }
}



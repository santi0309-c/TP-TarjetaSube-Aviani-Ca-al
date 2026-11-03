namespace TarjetaSube;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Representa el comprobante de viaje emitido tras un pago en colectivo.
/// </summary>
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
    public bool EsTrasbordo { get; private set; }
    public bool EsTransbordo => EsTrasbordo;

    [NotMapped]
    public Boleto? BoletoAnterior { get; private set; }

    public Boleto()
    {
        Linea = string.Empty;
    }

    public Boleto(string linea, decimal montoPagado, decimal saldoRestante, int tarjetaId,
                  DateTime? fecha = null, string tipoTarjeta = "", bool esTrasbordo = false,
                  Boleto? boletoAnterior = null)
    {
        Linea = linea;
        MontoPagado = montoPagado;
        SaldoRestante = saldoRestante;
        TarjetaId = tarjetaId;
        Fecha = fecha ?? DateTime.Now;
        TipoTarjeta = tipoTarjeta;
        EsTrasbordo = esTrasbordo;
        BoletoAnterior = boletoAnterior;
    }

    // Métodos para cumplir con el requerimiento explícito de la Iteración 3
    public DateTime ObtenerFecha() => Fecha;
    public string ObtenerTipoTarjeta() => TipoTarjeta;
    public string ObtenerLinea() => Linea;
    public decimal ObtenerTotalAbonado() => TotalAbonado;
    public decimal ObtenerSaldo() => Saldo;
    public int ObtenerTarjetaId() => TarjetaId;
    public bool ObtenerEsTrasbordo() => EsTrasbordo;

    /// <summary>
    /// Recorre la cadena de boletos anteriores para encontrar el primer boleto que originó la ventana de trasbordo.
    /// </summary>
    public Boleto ObtenerPrimerBoletoVentana()
    {
        Boleto actual = this;
        while (actual.BoletoAnterior != null && actual.EsTrasbordo)
        {
            actual = actual.BoletoAnterior;
        }
        return actual;
    }

    /// <summary>
    /// Verifica si una línea determinada ya fue utilizada dentro de la ventana de trasbordo activa.
    /// </summary>
    public bool ContieneLineaEnVentana(string linea)
    {
        Boleto? actual = this;
        while (actual != null)
        {
            if (string.Equals(actual.Linea, linea, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (!actual.EsTrasbordo)
            {
                break;
            }
            actual = actual.BoletoAnterior;
        }
        return false;
    }

    /// <summary>
    /// Determina si un nuevo viaje califica como trasbordo gratuito según las reglas de negocio:
    /// - Línea distinta a la actual y no repetida en la ventana.
    /// - Lunes a sábado entre 07:00 y 22:00.
    /// - Ventana máxima de 60 minutos desde el primer boleto pagado.
    /// </summary>
    public bool EsTrasbordoValido(string nuevaLinea, DateTime fechaNueva)
    {
        if (string.Equals(Linea, nuevaLinea, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (ContieneLineaEnVentana(nuevaLinea))
        {
            return false;
        }

        if (fechaNueva.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        TimeSpan hora = fechaNueva.TimeOfDay;
        if (hora < TimeSpan.FromHours(7) || hora > TimeSpan.FromHours(22))
        {
            return false;
        }

        DateTime fechaPrimerBoleto = ObtenerPrimerBoletoVentana().Fecha;
        TimeSpan diferencia = fechaNueva - fechaPrimerBoleto;
        if (diferencia < TimeSpan.Zero || diferencia > TimeSpan.FromHours(1))
        {
            return false;
        }

        return true;
    }
}

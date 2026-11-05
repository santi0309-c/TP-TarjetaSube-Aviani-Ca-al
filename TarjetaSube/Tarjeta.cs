namespace TarjetaSube;

using System.ComponentModel.DataAnnotations.Schema;

public class Tarjeta : ICobrable
{
    public const decimal LimiteSaldo = 56000m;
    public const decimal LimiteNegativo = -2000m;
    public static readonly IReadOnlySet<decimal> CargasAceptadas = new HashSet<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };

    public int Id { get; set; }

    public decimal Saldo { get; private set; }
    public decimal SaldoPendiente { get; private set; } = 0m;
    public decimal MultasPendientes { get; private set; } = 0m;

    [NotMapped]
    public decimal MultasBiciPendientes
    {
        get => MultasPendientes;
        set => MultasPendientes = value;
    }

    public int ViajesPlus { get; private set; }
    public int ContadorViajesPlus => ViajesPlus;

    public int TarjetaTipoId { get; set; }

    public TarjetaTipo? TarjetaTipo { get; set; }

    public bool EsNormal => TarjetaTipo == null || TarjetaTipo.EsNormal;
    public bool EsFranquicia => TarjetaTipo != null && TarjetaTipo.EsFranquicia;

    private readonly List<DateTime> _viajes = new();
    private readonly List<DateTime> _trasbordos = new();
    private readonly HashSet<DateTime> _pagosBiciDiarios = new();
    private DateTime? _ultimoViaje;

    [NotMapped]
    public Boleto? UltimoBoleto { get; set; }

    [NotMapped]
    public DateTime? UltimoViaje => _ultimoViaje ?? (_viajes.Count > 0 ? _viajes[^1] : null);

    [NotMapped]
    public IReadOnlyList<DateTime> HistorialViajes => _viajes.AsReadOnly();

    public Tarjeta() : this(0, null)
    {
    }

    public Tarjeta(decimal saldoInicial, TarjetaTipo? tarjetaTipo = null)
    {
        if (saldoInicial < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(saldoInicial), "El saldo inicial no puede ser negativo.");
        }
        if (saldoInicial > LimiteSaldo)
        {
            throw new ArgumentOutOfRangeException(nameof(saldoInicial), $"El saldo inicial supera el límite máximo de ${LimiteSaldo} pesos.");
        }

        Saldo = saldoInicial;
        TarjetaTipo = tarjetaTipo;
        if (tarjetaTipo != null)
        {
            TarjetaTipoId = tarjetaTipo.Id;
        }
    }

    public Tarjeta(TarjetaTipo tarjetaTipo) : this(0, tarjetaTipo)
    {
    }

    public bool PuedeViajar(DateTime fecha, string? linea = null)
    {
        var tipo = TarjetaTipo ?? TarjetaTipo.Normal;
        if (!tipo.CumpleFranjaHoraria(fecha))
        {
            return false;
        }

        if (TarjetaTipo != null && TarjetaTipo.EsMedioBoleto && UltimoViaje.HasValue)
        {
            bool esTrasbordoValido = UltimoBoleto != null && linea != null && UltimoBoleto.EsTrasbordoValido(linea, fecha);
            if (!esTrasbordoValido && (fecha - UltimoViaje.Value < TimeSpan.FromMinutes(5)))
            {
                return false;
            }
        }
        return true;
    }

    public bool PuedeViajar(DateTime fecha) => PuedeViajar(fecha, null);
    public bool PuedeViajar() => PuedeViajar(DateTime.Now, null);

    public int ContarViajesEnFecha(DateTime fecha)
    {
        return _viajes.Count(v => v.Date == fecha.Date);
    }

    public int ContarViajesEnMes(DateTime fecha)
    {
        return _viajes.Count(v => v.Year == fecha.Year && v.Month == fecha.Month);
    }

    public void RegistrarViaje(DateTime fecha, bool esTrasbordo = false)
    {
        _ultimoViaje = fecha;
        if (!esTrasbordo)
        {
            _viajes.Add(fecha);
        }
        else
        {
            _trasbordos.Add(fecha);
        }
    }

    public void RegistrarViaje(DateTime fecha) => RegistrarViaje(fecha, false);
    public void RegistrarViaje() => RegistrarViaje(DateTime.Now, false);

    public bool TieneTarifaBiciPagada(DateTime fecha)
    {
        return _pagosBiciDiarios.Contains(fecha.Date);
    }

    public void RegistrarPagoTarifaBici(DateTime fecha)
    {
        _pagosBiciDiarios.Add(fecha.Date);
    }

    public decimal CalcularPrecio(decimal tarifaBase, DateTime fecha)
    {
        var tipo = TarjetaTipo ?? TarjetaTipo.Normal;
        return tipo.CalcularTarifa(tarifaBase, ContarViajesEnFecha(fecha), ContarViajesEnMes(fecha));
    }

    public decimal CalcularPrecio(decimal tarifaBase) => CalcularPrecio(tarifaBase, DateTime.Now);

    public decimal AcreditarCarga()
    {
        if (SaldoPendiente <= 0 || Saldo >= LimiteSaldo)
        {
            return 0m;
        }

        decimal espacioDisponible = LimiteSaldo - Saldo;
        decimal montoAAcreditar = Math.Min(SaldoPendiente, espacioDisponible);
        Saldo += montoAAcreditar;
        SaldoPendiente -= montoAAcreditar;
        return montoAAcreditar;
    }

    public bool AcreditarCarga(decimal monto)
    {
        if (monto <= 0)
        {
            return false;
        }

        SaldoPendiente += monto;
        AcreditarCarga();
        return true;
    }

    public decimal acreditarCarga() => AcreditarCarga();
    public bool acreditarCarga(decimal monto) => AcreditarCarga(monto);

    public bool CargarSaldo(decimal monto)
    {
        if (!CargasAceptadas.Contains(monto))
        {
            return false;
        }

        return AcreditarCarga(monto);
    }

    public bool Cargar(decimal monto) => CargarSaldo(monto);
    public bool cargar(decimal monto) => CargarSaldo(monto);
    public bool cargarSaldo(decimal monto) => CargarSaldo(monto);

    public bool DescontarSaldo(decimal monto)
    {
        if (monto < 0)
        {
            return false;
        }

        if (monto == 0)
        {
            return true;
        }

        if (Saldo - monto < LimiteNegativo)
        {
            return false;
        }

        Saldo -= monto;
        if (Saldo < 0)
        {
            ViajesPlus++;
        }

        if (SaldoPendiente > 0)
        {
            AcreditarCarga();
        }

        return true;
    }

    public bool Descontar(decimal monto) => DescontarSaldo(monto);
    public bool descontar(decimal monto) => DescontarSaldo(monto);
    public bool descontarSaldo(decimal monto) => DescontarSaldo(monto);

    public decimal ObtenerSaldo() => Saldo;
    public decimal obtenerSaldo() => Saldo;
    public decimal ObtenerSaldoPendiente() => SaldoPendiente;
    public decimal obtenerSaldoPendiente() => SaldoPendiente;

    public void AgregarMulta(decimal monto)
    {
        if (monto > 0)
        {
            MultasPendientes += monto;
        }
    }

    public void LimpiarMultas()
    {
        MultasPendientes = 0m;
    }
}

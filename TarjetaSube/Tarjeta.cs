namespace TarjetaSube;

using System.ComponentModel.DataAnnotations.Schema;

public class Tarjeta
{
    public const decimal LimiteSaldo = 56000m;
    public const decimal LimiteNegativo = -2000m;
    public static readonly IReadOnlySet<decimal>
     CargasAceptadas = new HashSet<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };
    
    public int Id { get; set;}

    public decimal Saldo { get; private set; }
    public decimal SaldoPendiente { get; private set; } = 0m;
    public decimal MultasPendientes { get; private set; } = 0m;
    public int ViajesPlus { get; private set; }

    public int TarjetaTipoId { get; set; }

    public TarjetaTipo? TarjetaTipo { get; set; }

    public bool EsNormal => TarjetaTipo == null || TarjetaTipo.EsNormal;
    public bool EsFranquicia => TarjetaTipo != null && TarjetaTipo.EsFranquicia;

    private readonly List<DateTime> _viajes = new();
    private readonly List<DateTime> _trasbordos = new();
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
        if (EsFranquicia)
        {
            bool esDiaHabil = fecha.DayOfWeek >= DayOfWeek.Monday && fecha.DayOfWeek <= DayOfWeek.Friday;
            if (!esDiaHabil)
            {
                return false;
            }

            TimeSpan hora = fecha.TimeOfDay;
            if (hora < TimeSpan.FromHours(6) || hora > TimeSpan.FromHours(22))
            {
                return false;
            }
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

    public decimal CalcularPrecio(decimal tarifaBase, DateTime fecha)
    {
        if (EsNormal)
        {
            int viajesMes = ContarViajesEnMes(fecha);
            if (viajesMes < 29)
            {
                return tarifaBase; // Viajes 1 al 29: Tarifa normal
            }
            if (viajesMes < 79)
            {
                return tarifaBase * 0.80m; // Viajes 30 al 79: 20% de descuento
            }
            return tarifaBase * 0.75m; // Viajes 80 en adelante: 25% de descuento
        }

        if (TarjetaTipo != null && TarjetaTipo.EsMedioBoleto)
        {
            int viajesHoy = ContarViajesEnFecha(fecha);
            if (viajesHoy >= 2)
            {
                return tarifaBase;
            }
            return TarjetaTipo.CalcularPrecio(tarifaBase);
        }

        if (TarjetaTipo != null && TarjetaTipo.EsFranquiciaCompleta)
        {
            int viajesHoy = ContarViajesEnFecha(fecha);
            if (viajesHoy >= 2)
            {
                return tarifaBase;
            }
            return 0m;
        }

        return TarjetaTipo != null ? TarjetaTipo.CalcularPrecio(tarifaBase) : tarifaBase;
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

    public bool CargarSaldo(decimal monto)
    {
        if (!CargasAceptadas.Contains(monto))
        {
            return false;
        }

        return AcreditarCarga(monto);
    }

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
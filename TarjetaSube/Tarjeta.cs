namespace TarjetaSube;

using System.ComponentModel.DataAnnotations.Schema;

public class Tarjeta
{
    public const decimal LimiteSaldo = 40000m;
    public const decimal LimiteNegativo = -2000m;
    public static readonly IReadOnlySet<decimal>
     CargasAceptadas = new HashSet<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };
    
    public int Id { get; set;}

    public decimal Saldo { get; private set; }
    public int ViajesPlus { get; private set; }

    public int TarjetaTipoId { get; set; }

    public TarjetaTipo? TarjetaTipo { get; set; }

    private readonly List<DateTime> _viajes = new();

    [NotMapped]
    public DateTime? UltimoViaje => _viajes.Count > 0 ? _viajes[^1] : null;

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
            throw new ArgumentOutOfRangeException(nameof(saldoInicial), "El saldo inicial supera el límite máximo de $40.000 pesos.");
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

    public bool PuedeViajar(DateTime fecha)
    {
        if (TarjetaTipo != null && TarjetaTipo.EsMedioBoleto && UltimoViaje.HasValue)
        {
            if (fecha - UltimoViaje.Value < TimeSpan.FromMinutes(5))
            {
                return false;
            }
        }
        return true;
    }

    public bool PuedeViajar() => PuedeViajar(DateTime.Now);

    public int ContarViajesEnFecha(DateTime fecha)
    {
        return _viajes.Count(v => v.Date == fecha.Date);
    }

    public void RegistrarViaje(DateTime fecha)
    {
        _viajes.Add(fecha);
    }

    public void RegistrarViaje() => RegistrarViaje(DateTime.Now);

    public decimal CalcularPrecio(decimal tarifaBase, DateTime fecha)
    {
        if (TarjetaTipo == null)
        {
            return tarifaBase;
        }

        if (TarjetaTipo.EsMedioBoleto)
        {
            int viajesHoy = ContarViajesEnFecha(fecha);
            if (viajesHoy >= 2)
            {
                return tarifaBase;
            }
            return TarjetaTipo.CalcularPrecio(tarifaBase);
        }

        if (TarjetaTipo.EsFranquiciaCompleta)
        {
            int viajesHoy = ContarViajesEnFecha(fecha);
            if (viajesHoy >= 2)
            {
                return tarifaBase;
            }
            return 0m;
        }

        return TarjetaTipo.CalcularPrecio(tarifaBase);
    }

    public decimal CalcularPrecio(decimal tarifaBase) => CalcularPrecio(tarifaBase, DateTime.Now);

    public bool CargarSaldo(decimal monto)
    {
        if (!CargasAceptadas.Contains(monto))
        {
            return false;
        }

        if (Saldo + monto > LimiteSaldo)
        {
            return false;
        }
        Saldo += monto;
        return true;
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
        return true;
    }
}
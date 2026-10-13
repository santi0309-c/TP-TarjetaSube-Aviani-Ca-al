namespace TarjetaSube;

public class Tarjeta
{
    public const decimal LimiteSaldo = 40000m;
    public static readonly IReadOnlySet<decimal>
 CargasAceptadas = new HashSet<decimal>
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };
    
    public int Id { get; set;}

    public decimal Saldo {get; private set; }

    public int TarjetaTipoId { get; set; }

    public TarjetaTipo? TarjetaTipo { get; set; }

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

    public decimal CalcularPrecio(decimal tarifaBase)
    {
        return TarjetaTipo != null ? TarjetaTipo.CalcularPrecio(tarifaBase) : tarifaBase;
    }

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

        if (Saldo < monto)
        {
            return false;
        }

        Saldo -= monto;
        return true;
    }
}
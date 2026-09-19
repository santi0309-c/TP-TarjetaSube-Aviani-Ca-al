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

    public Tarjeta(decimal saldoInicial = 0)
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

    public bool DescontarSaldo (decimal monto)
    {
        if (monto <= 0 || Saldo < monto)
        {
            return false;
        }

        Saldo -= monto;
        return true;
    }
}
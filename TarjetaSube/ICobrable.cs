namespace TarjetaSube;

/// <summary>
/// Define las operaciones esenciales de cobro y consulta de saldo sobre una tarjeta (ISP).
/// </summary>
public interface ICobrable
{
    decimal Saldo { get; }
    bool DescontarSaldo(decimal monto);
}


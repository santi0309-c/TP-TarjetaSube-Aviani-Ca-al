namespace TarjetaSube;

public interface ICobrable
{
    decimal Saldo { get; }
    bool DescontarSaldo(decimal monto);
}

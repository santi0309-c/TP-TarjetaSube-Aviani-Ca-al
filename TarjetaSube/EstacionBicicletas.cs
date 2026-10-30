namespace TarjetaSube;

public class EstacionBicicletas
{
    public const decimal TarifaRetiro = 2418.75m;
    public const decimal MultaExceso = 1250m;
    public static readonly TimeSpan TiempoMaximoUso = TimeSpan.FromHours(1);

    public int Id { get; set; }

    private DateTime? _fechaRetiro;

    public bool EstaEnUso => _fechaRetiro.HasValue;
    public DateTime? FechaRetiro => _fechaRetiro;

    public bool RetirarBicicleta(Tarjeta tarjeta, DateTime fechaRetiro)
    {
        decimal totalACobrar = TarifaRetiro + tarjeta.MultasPendientes;

        if (tarjeta.Saldo < totalACobrar)
        {
            return false;
        }

        tarjeta.DescontarSaldo(totalACobrar);
        tarjeta.LimpiarMultas();
        _fechaRetiro = fechaRetiro;
        return true;
    }

    public bool RetirarBicicleta(Tarjeta tarjeta) => RetirarBicicleta(tarjeta, DateTime.Now);

    public bool DevolverBicicleta(Tarjeta tarjeta, DateTime fechaDevolucion)
    {
        if (!_fechaRetiro.HasValue)
        {
            return false;
        }

        TimeSpan duracion = fechaDevolucion - _fechaRetiro.Value;

        if (duracion > TiempoMaximoUso)
        {
            tarjeta.AgregarMulta(MultaExceso);
        }

        _fechaRetiro = null;
        return true;
    }

    public bool DevolverBicicleta(Tarjeta tarjeta) => DevolverBicicleta(tarjeta, DateTime.Now);
}

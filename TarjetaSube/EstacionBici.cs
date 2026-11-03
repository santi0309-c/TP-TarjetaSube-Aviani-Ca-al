namespace TarjetaSube;

public class EstacionBici : IServicioBici
{
    public const decimal TarifaServicio = 2418.75m;
    public const decimal TarifaDiaria = 2418.75m;
    public const decimal TarifaRetiro = 2418.75m;
    public const decimal TarifaMulta = 1250m;
    public const decimal MultaExceso = 1250m;
    public static readonly TimeSpan TiempoMaximoUso = TimeSpan.FromHours(1);

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    private DateTime? _fechaRetiro;

    public bool EstaEnUso => _fechaRetiro.HasValue;
    public DateTime? FechaRetiro => _fechaRetiro;

    public EstacionBici()
    {
    }

    public EstacionBici(string nombre)
    {
        Nombre = nombre;
    }

    public bool YaPagoTarifaDiaria(Tarjeta tarjeta, DateTime fecha)
    {
        if (tarjeta == null)
        {
            return false;
        }

        return tarjeta.TieneTarifaBiciPagada(fecha);
    }

    public decimal ObtenerMultasPendientes(Tarjeta tarjeta)
    {
        return tarjeta?.MultasPendientes ?? 0m;
    }

    public bool RetirarBicicleta(Tarjeta tarjeta, DateTime fechaRetiro)
    {
        if (tarjeta == null)
        {
            return false;
        }

        bool cobrarTarifaDiaria = !tarjeta.TieneTarifaBiciPagada(fechaRetiro);
        decimal multas = tarjeta.MultasPendientes;
        decimal montoTotal = (cobrarTarifaDiaria ? TarifaServicio : 0m) + multas;

        if (montoTotal == 0m)
        {
            _fechaRetiro = fechaRetiro;
            return true;
        }

        if (!tarjeta.DescontarSaldo(montoTotal))
        {
            return false;
        }

        if (cobrarTarifaDiaria)
        {
            tarjeta.RegistrarPagoTarifaBici(fechaRetiro);
        }

        if (multas > 0)
        {
            tarjeta.LimpiarMultas();
        }

        _fechaRetiro = fechaRetiro;
        return true;
    }

    public bool RetirarBicicleta(Tarjeta tarjeta) => RetirarBicicleta(tarjeta, DateTime.Now);

    // Alias para compatibilidad con tests existentes
    public bool RetirarBici(Tarjeta tarjeta, DateTime? fechaRetiro = null) => RetirarBicicleta(tarjeta, fechaRetiro ?? DateTime.Now);

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

namespace TarjetaSube;

public class EstacionBici
{
    public const decimal TarifaServicio = 2418.75m;
    public const decimal TarifaDiaria = 2418.75m;
    public const decimal TarifaMulta = 1250m;

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

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

    public bool TieneTarifaDiariaPagada(Tarjeta tarjeta, DateTime fecha) => YaPagoTarifaDiaria(tarjeta, fecha);

    public decimal ObtenerMultasPendientes(Tarjeta tarjeta)
    {
        return tarjeta?.MultasBiciPendientes ?? 0m;
    }

    public void AgregarMulta(Tarjeta tarjeta, decimal monto = TarifaMulta)
    {
        if (tarjeta != null && monto > 0)
        {
            tarjeta.MultasBiciPendientes += monto;
        }
    }

    public decimal CalcularMontoACobrar(Tarjeta tarjeta, DateTime fecha)
    {
        if (tarjeta == null)
        {
            return 0m;
        }

        decimal total = 0m;
        if (!tarjeta.TieneTarifaBiciPagada(fecha))
        {
            total += TarifaServicio;
        }

        total += tarjeta.MultasBiciPendientes;
        return total;
    }

    public bool RetirarBici(Tarjeta tarjeta, DateTime? fechaRetiro = null)
    {
        if (tarjeta == null)
        {
            return false;
        }

        DateTime fecha = fechaRetiro ?? DateTime.Now;
        bool cobrarTarifaDiaria = !tarjeta.TieneTarifaBiciPagada(fecha);
        decimal multas = tarjeta.MultasBiciPendientes;
        decimal montoTotal = (cobrarTarifaDiaria ? TarifaServicio : 0m) + multas;

        if (montoTotal == 0m)
        {
            return true;
        }

        if (!tarjeta.DescontarSaldo(montoTotal))
        {
            return false;
        }

        if (cobrarTarifaDiaria)
        {
            tarjeta.RegistrarPagoTarifaBici(fecha);
        }

        if (multas > 0)
        {
            tarjeta.MultasBiciPendientes = 0m;
        }

        return true;
    }

    public bool RetirarBici(Tarjeta tarjeta) => RetirarBici(tarjeta, null);
    public bool retirarBici(Tarjeta tarjeta) => RetirarBici(tarjeta, null);
    public bool retirarBici(Tarjeta tarjeta, DateTime fecha) => RetirarBici(tarjeta, fecha);

    public bool RetirarBicicleta(Tarjeta tarjeta, DateTime? fecha = null) => RetirarBici(tarjeta, fecha);
    public bool retirarBicicleta(Tarjeta tarjeta, DateTime? fecha = null) => RetirarBici(tarjeta, fecha);
}

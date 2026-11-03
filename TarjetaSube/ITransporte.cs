namespace TarjetaSube;

/// <summary>
/// Define el contrato de un servicio de transporte público que cobra pasajes (ISP).
/// </summary>
public interface ITransporte
{
    string Linea { get; }
    decimal Tarifa { get; }
    Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaViaje = null);
}


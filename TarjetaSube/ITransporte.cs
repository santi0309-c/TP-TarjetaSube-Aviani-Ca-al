namespace TarjetaSube;

public interface ITransporte
{
    string Linea { get; }
    decimal Tarifa { get; }
    Boleto? PagarCon(Tarjeta tarjeta, DateTime? fechaViaje = null);
}

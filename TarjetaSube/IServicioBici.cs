namespace TarjetaSube;

/// <summary>
/// Define las operaciones específicas del servicio de préstamo de bicicletas públicas (ISP).
/// </summary>
public interface IServicioBici
{
    string Nombre { get; }
    bool EstaEnUso { get; }
    bool RetirarBicicleta(Tarjeta tarjeta, DateTime fechaRetiro);
    bool DevolverBicicleta(Tarjeta tarjeta, DateTime fechaDevolucion);
}


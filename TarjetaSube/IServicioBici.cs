namespace TarjetaSube;

public interface IServicioBici
{
    string Nombre { get; }
    bool EstaEnUso { get; }
    bool RetirarBicicleta(Tarjeta tarjeta, DateTime fechaRetiro);
    bool DevolverBicicleta(Tarjeta tarjeta, DateTime fechaDevolucion);
}

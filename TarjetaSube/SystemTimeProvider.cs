namespace TarjetaSube;

/// <summary>
/// Proveedor de tiempo predeterminado basado en el reloj del sistema.
/// </summary>
public class SystemTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.Now;
}

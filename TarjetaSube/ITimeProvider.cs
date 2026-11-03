namespace TarjetaSube;

/// <summary>
/// Abstracción para el acceso a la fecha y hora del sistema (DIP).
/// Permite desacoplar el tiempo y sustituirlo en pruebas unitarias determinísticas.
/// </summary>
public interface ITimeProvider
{
    DateTime Now { get; }
}

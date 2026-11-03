namespace TarjetaSubeTest;

using TarjetaSube;

/// <summary>
/// Doble de prueba (Test Double / Fake) que implementa ITimeProvider para simular el paso del tiempo en tests unitarios (DIP).
/// </summary>
public class FakeTimeProvider : ITimeProvider
{
    public DateTime Now { get; set; }

    public FakeTimeProvider(DateTime fechaInicial)
    {
        Now = fechaInicial;
    }

    public void AvanzarMinutos(double minutos)
    {
        Now = Now.AddMinutes(minutos);
    }

    public void AvanzarHoras(double horas)
    {
        Now = Now.AddHours(horas);
    }

    public void AvanzarDias(double dias)
    {
        Now = Now.AddDays(dias);
    }
}

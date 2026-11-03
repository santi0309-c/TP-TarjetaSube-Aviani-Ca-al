namespace TarjetaSube;

public class MiBiciTuBici : EstacionBici
{
    public MiBiciTuBici() : base(string.Empty, null)
    {
    }

    public MiBiciTuBici(string nombre) : base(nombre, null)
    {
    }

    public MiBiciTuBici(string nombre, ITimeProvider? timeProvider) : base(nombre, timeProvider)
    {
    }
}

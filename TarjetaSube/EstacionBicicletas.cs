namespace TarjetaSube;

public class EstacionBicicletas : EstacionBici
{
    public EstacionBicicletas() : base(string.Empty, null)
    {
    }

    public EstacionBicicletas(string nombre) : base(nombre, null)
    {
    }

    public EstacionBicicletas(string nombre, ITimeProvider? timeProvider) : base(nombre, timeProvider)
    {
    }
}


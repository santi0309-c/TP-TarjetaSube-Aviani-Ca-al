namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class Iteracion5Tarea2Test
{
    private static readonly DateTime FechaBase = new(2026, 10, 30, 10, 0, 0);

    [Test]
    public void Devolucion_DentroDeUnaHora_NoGeneraMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        var estacion = new EstacionBicicletas();

        estacion.RetirarBicicleta(tarjeta, FechaBase);
        estacion.DevolverBicicleta(tarjeta, FechaBase.AddMinutes(45));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void Devolucion_EnLimiteExactoDeUnaHora_NoGeneraMulta()
    {
        var tarjeta = new Tarjeta(5000m);
        var estacion = new EstacionBicicletas();

        estacion.RetirarBicicleta(tarjeta, FechaBase);
        estacion.DevolverBicicleta(tarjeta, FechaBase.AddHours(1));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
    }

    [Test]
    public void Devolucion_ExcediendoUnaHora_AplicaMultaDe1250()
    {
        var tarjeta = new Tarjeta(10000m);
        var estacion = new EstacionBicicletas();

        estacion.RetirarBicicleta(tarjeta, FechaBase);
        estacion.DevolverBicicleta(tarjeta, FechaBase.AddMinutes(75));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
    }

    [Test]
    public void Devolucion_ExcediendoUnaHora_MultasSeAcumulanEnElMismoDia()
    {
        var tarjeta = new Tarjeta(15000m);

        var estacion1 = new EstacionBicicletas();
        estacion1.RetirarBicicleta(tarjeta, FechaBase);
        estacion1.DevolverBicicleta(tarjeta, FechaBase.AddMinutes(90));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));

        var estacion2 = new EstacionBicicletas();
        estacion2.RetirarBicicleta(tarjeta, FechaBase.AddHours(2));
        estacion2.DevolverBicicleta(tarjeta, FechaBase.AddHours(2).AddMinutes(80));

        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
    }

    [Test]
    public void Retiro_ConMultasAcumuladas_PagaMultasYRetiraExitosamente()
    {
        var tarjeta = new Tarjeta(10000m);
        tarjeta.AgregarMulta(EstacionBicicletas.MultaExceso);
        tarjeta.AgregarMulta(EstacionBicicletas.MultaExceso);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(2500m));

        decimal saldoAntes = tarjeta.Saldo;
        var estacion = new EstacionBicicletas();

        bool resultado = estacion.RetirarBicicleta(tarjeta, FechaBase);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntes - EstacionBicicletas.TarifaRetiro - 2500m));
    }

    [Test]
    public void Retiro_SaldoInsuficienteParaTarifaMasMultas_EsRechazado()
    {
        var tarjeta = new Tarjeta(1000m);
        tarjeta.AgregarMulta(EstacionBicicletas.MultaExceso);

        var estacion = new EstacionBicicletas();
        bool resultado = estacion.RetirarBicicleta(tarjeta, FechaBase);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.MultasPendientes, Is.EqualTo(1250m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void Retiro_SaldoInsuficienteParaTarifaSola_EsRechazado()
    {
        var tarjeta = new Tarjeta(300m);

        var estacion = new EstacionBicicletas();
        bool resultado = estacion.RetirarBicicleta(tarjeta, FechaBase);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(300m));
    }
}

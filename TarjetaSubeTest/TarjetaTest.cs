namespace TarjetaSubeTest;

using NUnit.Framework;
using TarjetaSube;

[TestFixture]
public class TarjetaTest
{
    [TestCase(2000)]
    [TestCase(3000)]
    [TestCase(4000)]
    [TestCase(5000)]
    [TestCase(8000)]
    [TestCase(10000)]
    [TestCase(15000)]
    [TestCase(20000)]
    [TestCase(25000)]
    [TestCase(30000)]
    public void CargarSaldo_ConMontosAceptados_AcreditaSaldoCorrectamente(decimal monto)
    {
        var tarjeta = new Tarjeta();

        bool resultado = tarjeta.CargarSaldo(monto);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(monto));
    }
    [TestCase(1000)]
    [TestCase(6000)]
    [TestCase(50000)]
    [TestCase(-2000)]
    public void CargarSaldo_ConMontosNoAceptados_RetornaFalseYNoModificaSaldo(decimal monto)
    {
        var tarjeta = new Tarjeta();

        bool resultado = tarjeta.CargarSaldo(monto);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0));
    }

    [Test]
    public void CargarSaldo_SuperandoLimiteMaximo_RetornaFalse()
    {
        var tarjeta = new Tarjeta(30000m);

        bool resultado = tarjeta.CargarSaldo(15000m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(30000m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoSuficiente_DescuentaYRetornaTrue()
    {
        var tarjeta = new Tarjeta(5000m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoInsuficiente_RetornaFalseYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void Constructor_ConSaldoNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tarjeta(-500m));
    }
}

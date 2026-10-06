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
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m); // Queda en -1580m

        // Intentar otro descuento superaría el límite negativo (-1580m - 1580m = -3160m < -2000m)
        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoInsuficiente_PermiteViajePlusYSaldoQuedaNegativo()
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-580m));
        Assert.That(tarjeta.Saldo, Is.GreaterThanOrEqualTo(Tarjeta.LimiteNegativo));
    }

    [Test]
    public void DescontarSaldo_SuperandoLimiteNegativo_RetornaFalseYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m); // Saldo en -1580m

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void CargarSaldo_ConDeudaNegativa_DescuentaDeudaYActualizaSaldoCorrectamente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m); // Saldo = -1580m

        bool resultado = tarjeta.CargarSaldo(2000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));
    }

    [Test]
    public void DescontarSaldo_ViajesPlus_SeIncrementaConCadaViajePlusOtorgado()
    {
        var tarjeta = new Tarjeta(1000m);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(0));

        bool primerViaje = tarjeta.DescontarSaldo(1580m);
        Assert.That(primerViaje, Is.True);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(1));

        bool segundoViaje = tarjeta.DescontarSaldo(500m);
        Assert.That(segundoViaje, Is.True);
        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(2));
    }

    [Test]
    public void DescontarSaldo_ConSaldoSuficiente_NoIncrementaViajesPlus()
    {
        var tarjeta = new Tarjeta(5000m);

        tarjeta.DescontarSaldo(1580m);

        Assert.That(tarjeta.ViajesPlus, Is.EqualTo(0));
    }

    [Test]
    public void DescontarSaldo_ConSaldoExacto_DescuentaYRetornaTrueYSaldoQuedaEnCero()
    {
        var tarjeta = new Tarjeta(1580m);

        bool resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [TestCase(0)]
    [TestCase(-100)]
    public void DescontarSaldo_ConMontoCeroONegativo_RetornaFalseYNoModificaSaldo(decimal monto)
    {
        var tarjeta = new Tarjeta(1000m);

        bool resultado = tarjeta.DescontarSaldo(monto);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1000m));
    }

    [Test]
    public void Constructor_ConSaldoNegativo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tarjeta(-500m));
    }
}

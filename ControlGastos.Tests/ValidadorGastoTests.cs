using ControlGastos.Helpers;
using ControlGastos.Models;

namespace ControlGastos.Tests;

public class ValidadorGastoTests
{
    private static readonly DateTime Hoy = new(2025, 3, 15);

    private static Gasto GastoValido() => new()
    {
        Descripcion = "Tacos",
        Monto = 85.50m,
        Categoria = CategoriaGasto.Alimentos,
        Fecha = Hoy
    };

    [Fact]
    public void GastoValido_NoTieneErrores()
        => Assert.Empty(ValidadorGasto.Validar(GastoValido(), Hoy));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    public void DescripcionVaciaOCorta_EsInvalida(string descripcion)
    {
        var gasto = GastoValido();
        gasto.Descripcion = descripcion;

        Assert.NotEmpty(ValidadorGasto.Validar(gasto, Hoy));
    }

    [Fact]
    public void DescripcionMuyLarga_EsInvalida()
    {
        var gasto = GastoValido();
        gasto.Descripcion = new string('a', ValidadorGasto.LongitudMaximaDeDescripcion + 1);

        Assert.Single(ValidadorGasto.Validar(gasto, Hoy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1_000_001)]
    public void MontoFueraDeRango_EsInvalido(double monto)
    {
        var gasto = GastoValido();
        gasto.Monto = (decimal)monto;

        Assert.Single(ValidadorGasto.Validar(gasto, Hoy));
    }

    [Fact]
    public void FechaFutura_EsInvalida()
    {
        var gasto = GastoValido();
        gasto.Fecha = Hoy.AddDays(1);

        Assert.Single(ValidadorGasto.Validar(gasto, Hoy));
    }

    [Fact]
    public void CategoriaInexistente_EsInvalida()
    {
        var gasto = GastoValido();
        gasto.Categoria = (CategoriaGasto)99;

        Assert.Single(ValidadorGasto.Validar(gasto, Hoy));
    }

    [Fact]
    public void VariosErrores_SeReportanJuntos()
    {
        var gasto = new Gasto { Descripcion = "", Monto = 0, Fecha = Hoy.AddDays(5) };

        Assert.Equal(3, ValidadorGasto.Validar(gasto, Hoy).Count);
    }

    [Theory]
    [InlineData("12.50", true, 12.50)]
    [InlineData("  80 ", true, 80)]
    [InlineData("abc", false, 0)]
    [InlineData("", false, 0)]
    [InlineData(null, false, 0)]
    public void ConversorMonto_InterpretaElTextoEscrito(string? texto, bool esperadoValido, double esperadoMonto)
    {
        var resultado = ConversorMonto.TryConvertir(texto, out var monto);

        Assert.Equal(esperadoValido, resultado);
        Assert.Equal((decimal)esperadoMonto, monto);
    }
}

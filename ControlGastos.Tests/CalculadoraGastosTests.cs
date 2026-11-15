using ControlGastos.Helpers;
using ControlGastos.Models;

namespace ControlGastos.Tests;

public class CalculadoraGastosTests
{
    private static Gasto CrearGasto(decimal monto, CategoriaGasto categoria, int anio = 2025, int mes = 3, int dia = 10)
        => new() { Descripcion = "Prueba", Monto = monto, Categoria = categoria, Fecha = new DateTime(anio, mes, dia) };

    [Fact]
    public void CalcularTotal_SumaTodosLosMontos()
    {
        var gastos = new[] { CrearGasto(100, CategoriaGasto.Alimentos), CrearGasto(50.5m, CategoriaGasto.Salud) };

        Assert.Equal(150.5m, CalculadoraGastos.CalcularTotal(gastos));
    }

    [Fact]
    public void CalcularTotal_SinGastos_EsCero()
        => Assert.Equal(0m, CalculadoraGastos.CalcularTotal(Array.Empty<Gasto>()));

    [Fact]
    public void FiltrarPorMes_DevuelveSoloElMesIndicado()
    {
        var gastos = new[]
        {
            CrearGasto(10, CategoriaGasto.Otros, 2025, 3),
            CrearGasto(20, CategoriaGasto.Otros, 2025, 4),
            CrearGasto(30, CategoriaGasto.Otros, 2024, 3)
        };

        var resultado = CalculadoraGastos.FiltrarPorMes(gastos, 2025, 3).ToList();

        Assert.Single(resultado);
        Assert.Equal(10, resultado[0].Monto);
    }

    [Theory]
    [InlineData(25, 100, 25.0)]
    [InlineData(1, 3, 33.3)]
    [InlineData(0, 100, 0.0)]
    [InlineData(10, 0, 0.0)]
    public void CalcularPorcentaje_RedondeaAUnDecimalYEvitaDivisionEntreCero(double parte, double total, double esperado)
        => Assert.Equal((decimal)esperado, CalculadoraGastos.CalcularPorcentaje((decimal)parte, (decimal)total));

    [Fact]
    public void AgruparPorCategoria_OrdenaDeMayorAMenorYCalculaPorcentajes()
    {
        var gastos = new[]
        {
            CrearGasto(300, CategoriaGasto.Alimentos),
            CrearGasto(100, CategoriaGasto.Transporte),
            CrearGasto(100, CategoriaGasto.Alimentos)
        };

        var resumen = CalculadoraGastos.AgruparPorCategoria(gastos);

        Assert.Equal(2, resumen.Count);
        Assert.Equal(CategoriaGasto.Alimentos, resumen[0].Categoria);
        Assert.Equal(400m, resumen[0].Total);
        Assert.Equal(2, resumen[0].CantidadDeGastos);
        Assert.Equal(80m, resumen[0].Porcentaje);
        Assert.Equal(20m, resumen[1].Porcentaje);
    }

    [Fact]
    public void ObtenerCategoriaConMayorGasto_SinGastos_DevuelveNulo()
        => Assert.Null(CalculadoraGastos.ObtenerCategoriaConMayorGasto(Array.Empty<Gasto>()));

    [Fact]
    public void ObtenerCategoriaConMayorGasto_DevuelveLaDeMayorTotal()
    {
        var gastos = new[] { CrearGasto(10, CategoriaGasto.Salud), CrearGasto(90, CategoriaGasto.Servicios) };

        Assert.Equal(CategoriaGasto.Servicios, CalculadoraGastos.ObtenerCategoriaConMayorGasto(gastos));
    }

    [Fact]
    public void ObtenerDiasTranscurridos_MesActual_UsaElDiaDeHoy()
    {
        var hoy = new DateTime(2025, 3, 12);

        Assert.Equal(12, CalculadoraGastos.ObtenerDiasTranscurridos(new DateTime(2025, 3, 1), hoy));
    }

    [Fact]
    public void ObtenerDiasTranscurridos_MesPasado_UsaTodosLosDiasDelMes()
    {
        var hoy = new DateTime(2025, 3, 12);

        Assert.Equal(28, CalculadoraGastos.ObtenerDiasTranscurridos(new DateTime(2025, 2, 1), hoy));
    }

    [Theory]
    [InlineData(300, 10, 30)]
    [InlineData(100, 3, 33.33)]
    [InlineData(100, 0, 0)]
    public void CalcularPromedioDiario_DivideEntreLosDias(double total, int dias, double esperado)
        => Assert.Equal((decimal)esperado, CalculadoraGastos.CalcularPromedioDiario((decimal)total, dias));

    [Fact]
    public void CalcularRestante_PuedeSerNegativoCuandoSeExcedeElPresupuesto()
        => Assert.Equal(-50m, CalculadoraGastos.CalcularRestante(1000, 1050));

    [Theory]
    [InlineData(1000, 500, 0.5)]
    [InlineData(1000, 2000, 1.0)]
    [InlineData(0, 500, 0.0)]
    public void CalcularProgreso_QuedaEntreCeroYUno(double presupuesto, double gastado, double esperado)
        => Assert.Equal(esperado, CalculadoraGastos.CalcularProgreso((decimal)presupuesto, (decimal)gastado), 3);

    [Theory]
    [InlineData(0, 100, EstadoPresupuesto.SinPresupuesto)]
    [InlineData(1000, 500, EstadoPresupuesto.DentroDelLimite)]
    [InlineData(1000, 800, EstadoPresupuesto.CercaDelLimite)]
    [InlineData(1000, 1000, EstadoPresupuesto.CercaDelLimite)]
    [InlineData(1000, 1001, EstadoPresupuesto.LimiteExcedido)]
    public void ObtenerEstadoPresupuesto_ClasificaElAvance(double presupuesto, double gastado, EstadoPresupuesto esperado)
        => Assert.Equal(esperado, CalculadoraGastos.ObtenerEstadoPresupuesto((decimal)presupuesto, (decimal)gastado));
}

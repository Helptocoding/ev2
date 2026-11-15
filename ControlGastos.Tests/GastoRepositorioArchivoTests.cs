using ControlGastos.Models;
using ControlGastos.Services;

namespace ControlGastos.Tests;

public class GastoRepositorioArchivoTests : IDisposable
{
    private readonly string carpetaTemporal = Path.Combine(Path.GetTempPath(), "gastos-pruebas-" + Guid.NewGuid());

    private string RutaArchivo => Path.Combine(carpetaTemporal, "gastos.json");

    public void Dispose()
    {
        if (Directory.Exists(carpetaTemporal))
            Directory.Delete(carpetaTemporal, true);
    }

    [Fact]
    public async Task GuardarYLeer_ConservaLosDatosEntreInstancias()
    {
        var gasto = new Gasto { Descripcion = "Gasolina", Monto = 500, Categoria = CategoriaGasto.Transporte, Fecha = new DateTime(2025, 3, 4) };
        var primero = new GastoRepositorioArchivo(RutaArchivo);

        await primero.GuardarAsync(gasto);
        await primero.GuardarPresupuestoAsync(3000);

        var segundo = new GastoRepositorioArchivo(RutaArchivo);
        var leidos = await segundo.ObtenerTodosAsync();

        Assert.Single(leidos);
        Assert.Equal("Gasolina", leidos[0].Descripcion);
        Assert.Equal(CategoriaGasto.Transporte, leidos[0].Categoria);
        Assert.Equal(3000m, await segundo.ObtenerPresupuestoAsync());
    }

    [Fact]
    public async Task GuardarConElMismoId_ActualizaEnLugarDeDuplicar()
    {
        var repositorio = new GastoRepositorioArchivo(RutaArchivo);
        var gasto = new Gasto { Descripcion = "Cine", Monto = 100 };
        await repositorio.GuardarAsync(gasto);

        gasto.Monto = 150;
        await repositorio.GuardarAsync(gasto);

        var leidos = await repositorio.ObtenerTodosAsync();
        Assert.Single(leidos);
        Assert.Equal(150m, leidos[0].Monto);
    }

    [Fact]
    public async Task Eliminar_QuitaElGastoYDevuelveSiExistia()
    {
        var repositorio = new GastoRepositorioArchivo(RutaArchivo);
        var gasto = new Gasto { Descripcion = "Renta", Monto = 4000 };
        await repositorio.GuardarAsync(gasto);

        Assert.True(await repositorio.EliminarAsync(gasto.Id));
        Assert.False(await repositorio.EliminarAsync(gasto.Id));
        Assert.Empty(await repositorio.ObtenerTodosAsync());
    }

    [Fact]
    public async Task ArchivoCorrupto_ArrancaVacioEnLugarDeFallar()
    {
        Directory.CreateDirectory(carpetaTemporal);
        await File.WriteAllTextAsync(RutaArchivo, "{ esto no es json");

        var repositorio = new GastoRepositorioArchivo(RutaArchivo);

        Assert.Empty(await repositorio.ObtenerTodosAsync());
    }

    [Fact]
    public async Task ObtenerTodos_DevuelveCopias_QueNoAfectanLosDatosGuardados()
    {
        var repositorio = new GastoRepositorioArchivo(RutaArchivo);
        await repositorio.GuardarAsync(new Gasto { Descripcion = "Agua", Monto = 20 });

        var copia = (await repositorio.ObtenerTodosAsync())[0];
        copia.Monto = 999;

        Assert.Equal(20m, (await repositorio.ObtenerTodosAsync())[0].Monto);
    }
}

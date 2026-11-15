using ControlGastos.Models;
using ControlGastos.Services;
using ControlGastos.ViewModels;

namespace ControlGastos.Tests;

public class ViewModelsTests
{
    private readonly RepositorioEnMemoria repositorio = new();
    private readonly DispositivoFalso dispositivo = new();
    private readonly NavegacionFalsa navegacion = new();
    private readonly ContextoMes contextoMes = new();

    private VMGastos CrearVmGastos() => new(repositorio, dispositivo, navegacion, contextoMes);
    private VMNuevoGasto CrearVmNuevoGasto() => new(repositorio, dispositivo, navegacion);
    private VMResumen CrearVmResumen() => new(repositorio, dispositivo, contextoMes);

    private static Gasto GastoDeEsteMes(string descripcion, decimal monto, CategoriaGasto categoria = CategoriaGasto.Alimentos)
        => new() { Descripcion = descripcion, Monto = monto, Categoria = categoria, Fecha = DateTime.Today };

    [Fact]
    public void BaseViewModel_NotificaSoloCuandoCambiaElValor()
    {
        var vm = CrearVmNuevoGasto();
        var notificaciones = new List<string?>();
        vm.PropertyChanged += (_, e) => notificaciones.Add(e.PropertyName);

        vm.Descripcion = "Café";
        vm.Descripcion = "Café";

        Assert.Equal(new[] { nameof(VMNuevoGasto.Descripcion) }, notificaciones);
    }

    [Fact]
    public async Task VMGastos_Cargar_CalculaTotalesDelMes()
    {
        await repositorio.GuardarAsync(GastoDeEsteMes("Super", 300));
        await repositorio.GuardarAsync(GastoDeEsteMes("Taxi", 100, CategoriaGasto.Transporte));
        await repositorio.GuardarAsync(new Gasto { Descripcion = "Viejo", Monto = 999, Fecha = DateTime.Today.AddYears(-1) });
        await repositorio.GuardarPresupuestoAsync(1000);
        var vm = CrearVmGastos();

        await vm.CargarAsync();

        Assert.Equal(2, vm.GastosDelMes.Count);
        Assert.Equal(400m, vm.TotalGastado);
        Assert.Equal(600m, vm.Restante);
        Assert.Equal(0.4, vm.ProgresoPresupuesto, 3);
        Assert.False(vm.NoHayGastos);
        Assert.False(vm.EstaOcupado);
    }

    [Fact]
    public async Task VMGastos_SinGastos_MarcaListaVacia()
    {
        var vm = CrearVmGastos();

        await vm.CargarAsync();

        Assert.True(vm.NoHayGastos);
        Assert.Equal(0m, vm.TotalGastado);
    }

    [Fact]
    public async Task VMGastos_CambiarMes_RecargaConElMesAnterior()
    {
        await repositorio.GuardarAsync(GastoDeEsteMes("Este mes", 100));
        var vm = CrearVmGastos();
        await vm.CargarAsync();

        await vm.MesAnteriorCommand.EjecutarAsync();

        Assert.Empty(vm.GastosDelMes);
        Assert.Equal(contextoMes.Titulo, vm.MesTitulo);
    }

    [Fact]
    public async Task VMGastos_Eliminar_PideConfirmacionYQuitaElGasto()
    {
        var gasto = GastoDeEsteMes("Cine", 120);
        await repositorio.GuardarAsync(gasto);
        var vm = CrearVmGastos();
        await vm.CargarAsync();

        dispositivo.RespuestaDeConfirmacion = false;
        await vm.EliminarGastoCommand.EjecutarAsync(gasto);
        Assert.Single(vm.GastosDelMes);

        dispositivo.RespuestaDeConfirmacion = true;
        await vm.EliminarGastoCommand.EjecutarAsync(gasto);
        Assert.Empty(vm.GastosDelMes);
    }

    [Fact]
    public async Task VMGastos_Editar_EnviaElModeloALaNavegacion()
    {
        var gasto = GastoDeEsteMes("Luz", 450, CategoriaGasto.Servicios);
        var vm = CrearVmGastos();

        await vm.EditarGastoCommand.EjecutarAsync(gasto);

        Assert.Same(gasto, navegacion.UltimoGastoEnviado);
    }

    [Fact]
    public async Task VMGastos_NuevoGasto_NavegaSinModelo()
    {
        var vm = CrearVmGastos();

        await vm.NuevoGastoCommand.EjecutarAsync();

        Assert.Equal(1, navegacion.VecesQueIrAlFormulario);
        Assert.Null(navegacion.UltimoGastoEnviado);
    }

    [Fact]
    public async Task VMGastos_EditarPresupuesto_GuardaElNumeroEscrito()
    {
        dispositivo.RespuestaDeNumero = "2500";
        var vm = CrearVmGastos();

        await vm.EditarPresupuestoCommand.EjecutarAsync();

        Assert.Equal(2500m, vm.PresupuestoMensual);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-10")]
    public async Task VMGastos_EditarPresupuesto_RechazaValoresInvalidos(string respuesta)
    {
        dispositivo.RespuestaDeNumero = respuesta;
        var vm = CrearVmGastos();

        await vm.EditarPresupuestoCommand.EjecutarAsync();

        Assert.Equal(0m, vm.PresupuestoMensual);
        Assert.Single(dispositivo.Mensajes);
    }

    [Fact]
    public async Task VMNuevoGasto_Guardar_PersisteVibraYRegresa()
    {
        var vm = CrearVmNuevoGasto();
        vm.Descripcion = "Pizza";
        vm.MontoTexto = "150.75";
        vm.CategoriaSeleccionada = CategoriaGasto.Alimentos;

        await vm.GuardarCommand.EjecutarAsync();

        var guardados = await repositorio.ObtenerTodosAsync();
        Assert.Single(guardados);
        Assert.Equal(150.75m, guardados[0].Monto);
        Assert.Equal(1, dispositivo.Vibraciones);
        Assert.Equal(1, navegacion.VecesQueVolvio);
        Assert.False(vm.HayErrores);
    }

    [Fact]
    public async Task VMNuevoGasto_ConDatosInvalidos_MuestraErroresYNoGuarda()
    {
        var vm = CrearVmNuevoGasto();
        vm.Descripcion = "";
        vm.MontoTexto = "-3";

        await vm.GuardarCommand.EjecutarAsync();

        Assert.True(vm.HayErrores);
        Assert.Empty(await repositorio.ObtenerTodosAsync());
        Assert.Equal(0, navegacion.VecesQueVolvio);
    }

    [Fact]
    public async Task VMNuevoGasto_MontoNoNumerico_MuestraError()
    {
        var vm = CrearVmNuevoGasto();
        vm.Descripcion = "Algo";
        vm.MontoTexto = "mucho";

        await vm.GuardarCommand.EjecutarAsync();

        Assert.Contains("número", vm.MensajeDeErrores);
    }

    [Fact]
    public async Task VMNuevoGasto_Cargar_RellenaElFormularioYActualizaEnVezDeDuplicar()
    {
        var original = GastoDeEsteMes("Taxi", 80, CategoriaGasto.Transporte);
        await repositorio.GuardarAsync(original);
        var vm = CrearVmNuevoGasto();

        vm.Cargar(original);
        Assert.True(vm.EsEdicion);
        Assert.Equal("Editar gasto", vm.Titulo);
        Assert.Equal("Taxi", vm.Descripcion);

        vm.MontoTexto = "95";
        await vm.GuardarCommand.EjecutarAsync();

        var guardados = await repositorio.ObtenerTodosAsync();
        Assert.Single(guardados);
        Assert.Equal(95m, guardados[0].Monto);
    }

    [Fact]
    public async Task VMResumen_Cargar_AgrupaPorCategoria()
    {
        await repositorio.GuardarAsync(GastoDeEsteMes("Super", 300, CategoriaGasto.Alimentos));
        await repositorio.GuardarAsync(GastoDeEsteMes("Taxi", 100, CategoriaGasto.Transporte));
        var vm = CrearVmResumen();

        await vm.CargarAsync();

        Assert.Equal(2, vm.Categorias.Count);
        Assert.Equal(400m, vm.TotalGastado);
        Assert.Equal("Alimentos", vm.CategoriaConMayorGasto);
        Assert.Equal(75m, vm.Categorias[0].Porcentaje);
    }

    [Fact]
    public async Task VMResumen_SinGastos_NoMarcaCategoriaMayor()
    {
        var vm = CrearVmResumen();

        await vm.CargarAsync();

        Assert.True(vm.NoHayGastos);
        Assert.Equal("Sin gastos", vm.CategoriaConMayorGasto);
    }
}

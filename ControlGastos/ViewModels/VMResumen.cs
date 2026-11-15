using System.Collections.ObjectModel;
using ControlGastos.Helpers;
using ControlGastos.Models;
using ControlGastos.Services;
using ControlGastos.ViewModels.Comandos;

namespace ControlGastos.ViewModels;

public class VMResumen : BaseViewModel
{
    private readonly IGastoRepositorio repositorio;
    private readonly IServicioDispositivo dispositivo;
    private readonly ContextoMes contextoMes;

    private decimal totalGastado;
    private decimal promedioDiario;
    private string categoriaConMayorGasto = "Sin gastos";
    private string mesTitulo = string.Empty;
    private bool noHayGastos;

    public VMResumen(
        IGastoRepositorio repositorio,
        IServicioDispositivo dispositivo,
        ContextoMes contextoMes)
    {
        this.repositorio = repositorio;
        this.dispositivo = dispositivo;
        this.contextoMes = contextoMes;

        Titulo = "Resumen";
        Categorias = new ObservableCollection<ResumenCategoria>();

        CargarCommand = new AsyncRelayCommand(CargarAsync);
        MesAnteriorCommand = new AsyncRelayCommand(() => CambiarMesAsync(contextoMes.IrAlMesAnterior));
        MesSiguienteCommand = new AsyncRelayCommand(() => CambiarMesAsync(contextoMes.IrAlMesSiguiente));
    }

    public ObservableCollection<ResumenCategoria> Categorias { get; }

    public AsyncRelayCommand CargarCommand { get; }
    public AsyncRelayCommand MesAnteriorCommand { get; }
    public AsyncRelayCommand MesSiguienteCommand { get; }

    public decimal TotalGastado
    {
        get => totalGastado;
        private set => AsignarPropiedad(ref totalGastado, value);
    }

    public decimal PromedioDiario
    {
        get => promedioDiario;
        private set => AsignarPropiedad(ref promedioDiario, value);
    }

    public string CategoriaConMayorGasto
    {
        get => categoriaConMayorGasto;
        private set => AsignarPropiedad(ref categoriaConMayorGasto, value);
    }

    public string MesTitulo
    {
        get => mesTitulo;
        private set => AsignarPropiedad(ref mesTitulo, value);
    }

    public bool NoHayGastos
    {
        get => noHayGastos;
        private set => AsignarPropiedad(ref noHayGastos, value);
    }

    public Task CargarAsync() => EjecutarOcupadoAsync(async () =>
    {
        var todosLosGastos = await repositorio.ObtenerTodosAsync();
        var gastosDelMes = CalculadoraGastos
            .FiltrarPorMes(todosLosGastos, contextoMes.Mes.Year, contextoMes.Mes.Month)
            .ToList();

        Categorias.Clear();
        foreach (var resumen in CalculadoraGastos.AgruparPorCategoria(gastosDelMes))
            Categorias.Add(resumen);

        TotalGastado = CalculadoraGastos.CalcularTotal(gastosDelMes);

        var diasTranscurridos = CalculadoraGastos.ObtenerDiasTranscurridos(contextoMes.Mes, DateTime.Today);
        PromedioDiario = CalculadoraGastos.CalcularPromedioDiario(TotalGastado, diasTranscurridos);

        var categoriaMayor = CalculadoraGastos.ObtenerCategoriaConMayorGasto(gastosDelMes);
        CategoriaConMayorGasto = categoriaMayor?.ToString() ?? "Sin gastos";

        MesTitulo = contextoMes.Titulo;
        NoHayGastos = Categorias.Count == 0;
    });

    private async Task CambiarMesAsync(Action cambiarMes)
    {
        cambiarMes();
        await CargarAsync();
    }

    protected override Task AlOcurrirErrorAsync(Exception excepcion)
        => dispositivo.MostrarMensajeAsync("Ocurrió un error", excepcion.Message);
}

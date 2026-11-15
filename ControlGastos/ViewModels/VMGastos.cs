using System.Collections.ObjectModel;
using ControlGastos.Helpers;
using ControlGastos.Models;
using ControlGastos.Services;
using ControlGastos.ViewModels.Comandos;

namespace ControlGastos.ViewModels;

public class VMGastos : BaseViewModel
{
    private readonly IGastoRepositorio repositorio;
    private readonly IServicioDispositivo dispositivo;
    private readonly IServicioNavegacion navegacion;
    private readonly ContextoMes contextoMes;

    private decimal totalGastado;
    private decimal presupuestoMensual;
    private decimal restante;
    private decimal promedioDiario;
    private double progresoPresupuesto;
    private string textoEstado = string.Empty;
    private string mesTitulo = string.Empty;
    private bool noHayGastos;
    private bool estaRefrescando;

    public VMGastos(
        IGastoRepositorio repositorio,
        IServicioDispositivo dispositivo,
        IServicioNavegacion navegacion,
        ContextoMes contextoMes)
    {
        this.repositorio = repositorio;
        this.dispositivo = dispositivo;
        this.navegacion = navegacion;
        this.contextoMes = contextoMes;

        Titulo = "Gastos";
        GastosDelMes = new ObservableCollection<Gasto>();

        CargarCommand = new AsyncRelayCommand(CargarAsync);
        NuevoGastoCommand = new AsyncRelayCommand(() => navegacion.IrAFormularioDeGastoAsync(null));
        EditarGastoCommand = new AsyncRelayCommand<Gasto>(gasto => navegacion.IrAFormularioDeGastoAsync(gasto));
        EliminarGastoCommand = new AsyncRelayCommand<Gasto>(EliminarGastoAsync);
        EditarPresupuestoCommand = new AsyncRelayCommand(EditarPresupuestoAsync);
        MesAnteriorCommand = new AsyncRelayCommand(() => CambiarMesAsync(contextoMes.IrAlMesAnterior));
        MesSiguienteCommand = new AsyncRelayCommand(() => CambiarMesAsync(contextoMes.IrAlMesSiguiente));
    }

    public ObservableCollection<Gasto> GastosDelMes { get; }

    public AsyncRelayCommand CargarCommand { get; }
    public AsyncRelayCommand NuevoGastoCommand { get; }
    public AsyncRelayCommand<Gasto> EditarGastoCommand { get; }
    public AsyncRelayCommand<Gasto> EliminarGastoCommand { get; }
    public AsyncRelayCommand EditarPresupuestoCommand { get; }
    public AsyncRelayCommand MesAnteriorCommand { get; }
    public AsyncRelayCommand MesSiguienteCommand { get; }

    public decimal TotalGastado
    {
        get => totalGastado;
        private set => AsignarPropiedad(ref totalGastado, value);
    }

    public decimal PresupuestoMensual
    {
        get => presupuestoMensual;
        private set => AsignarPropiedad(ref presupuestoMensual, value);
    }

    public decimal Restante
    {
        get => restante;
        private set => AsignarPropiedad(ref restante, value);
    }

    public decimal PromedioDiario
    {
        get => promedioDiario;
        private set => AsignarPropiedad(ref promedioDiario, value);
    }

    public double ProgresoPresupuesto
    {
        get => progresoPresupuesto;
        private set => AsignarPropiedad(ref progresoPresupuesto, value);
    }

    public string TextoEstado
    {
        get => textoEstado;
        private set => AsignarPropiedad(ref textoEstado, value);
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

    public bool EstaRefrescando
    {
        get => estaRefrescando;
        set => AsignarPropiedad(ref estaRefrescando, value);
    }

    public async Task CargarAsync()
    {
        await EjecutarOcupadoAsync(CargarDatosAsync);
        EstaRefrescando = false;
    }

    private async Task CargarDatosAsync()
    {
        var todosLosGastos = await repositorio.ObtenerTodosAsync();
        var gastosDelMes = CalculadoraGastos
            .FiltrarPorMes(todosLosGastos, contextoMes.Mes.Year, contextoMes.Mes.Month)
            .OrderByDescending(gasto => gasto.Fecha)
            .ToList();

        GastosDelMes.Clear();
        foreach (var gasto in gastosDelMes)
            GastosDelMes.Add(gasto);

        PresupuestoMensual = await repositorio.ObtenerPresupuestoAsync();
        TotalGastado = CalculadoraGastos.CalcularTotal(gastosDelMes);
        Restante = CalculadoraGastos.CalcularRestante(PresupuestoMensual, TotalGastado);
        ProgresoPresupuesto = CalculadoraGastos.CalcularProgreso(PresupuestoMensual, TotalGastado);

        var diasTranscurridos = CalculadoraGastos.ObtenerDiasTranscurridos(contextoMes.Mes, DateTime.Today);
        PromedioDiario = CalculadoraGastos.CalcularPromedioDiario(TotalGastado, diasTranscurridos);

        TextoEstado = DescribirEstado(CalculadoraGastos.ObtenerEstadoPresupuesto(PresupuestoMensual, TotalGastado));
        MesTitulo = contextoMes.Titulo;
        NoHayGastos = GastosDelMes.Count == 0;
    }

    private async Task CambiarMesAsync(Action cambiarMes)
    {
        cambiarMes();
        await CargarAsync();
    }

    private async Task EliminarGastoAsync(Gasto gasto)
    {
        var confirmado = await dispositivo.ConfirmarAsync(
            "Eliminar gasto",
            $"¿Eliminar \"{gasto.Descripcion}\" por {gasto.Monto:C}?");

        if (!confirmado)
            return;

        await repositorio.EliminarAsync(gasto.Id);
        await CargarAsync();
    }

    private async Task EditarPresupuestoAsync()
    {
        var respuesta = await dispositivo.PedirNumeroAsync(
            "Presupuesto mensual",
            "Escribe el monto máximo que quieres gastar al mes.",
            PresupuestoMensual.ToString("0.##"));

        if (respuesta is null)
            return;

        if (!ConversorMonto.TryConvertir(respuesta, out var nuevoPresupuesto) || nuevoPresupuesto < 0)
        {
            await dispositivo.MostrarMensajeAsync("Monto no válido", "Escribe un número igual o mayor que cero.");
            return;
        }

        await repositorio.GuardarPresupuestoAsync(nuevoPresupuesto);
        await CargarAsync();
    }

    protected override Task AlOcurrirErrorAsync(Exception excepcion)
        => dispositivo.MostrarMensajeAsync("Ocurrió un error", excepcion.Message);

    private static string DescribirEstado(EstadoPresupuesto estado) => estado switch
    {
        EstadoPresupuesto.SinPresupuesto => "Define un presupuesto para ver tu avance.",
        EstadoPresupuesto.DentroDelLimite => "Vas bien: estás dentro de tu presupuesto.",
        EstadoPresupuesto.CercaDelLimite => "Atención: ya usaste más del 80% de tu presupuesto.",
        EstadoPresupuesto.LimiteExcedido => "Te pasaste del presupuesto de este mes.",
        _ => string.Empty
    };
}

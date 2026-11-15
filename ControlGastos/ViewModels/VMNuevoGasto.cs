using ControlGastos.Helpers;
using ControlGastos.Models;
using ControlGastos.Services;
using ControlGastos.ViewModels.Comandos;

namespace ControlGastos.ViewModels;

public class VMNuevoGasto : BaseViewModel
{
    private readonly IGastoRepositorio repositorio;
    private readonly IServicioDispositivo dispositivo;
    private readonly IServicioNavegacion navegacion;

    private Guid idDelGasto = Guid.NewGuid();
    private string descripcion = string.Empty;
    private string montoTexto = string.Empty;
    private CategoriaGasto categoriaSeleccionada = CategoriaGasto.Alimentos;
    private DateTime fecha = DateTime.Today;
    private string mensajeDeErrores = string.Empty;
    private bool esEdicion;

    public VMNuevoGasto(
        IGastoRepositorio repositorio,
        IServicioDispositivo dispositivo,
        IServicioNavegacion navegacion)
    {
        this.repositorio = repositorio;
        this.dispositivo = dispositivo;
        this.navegacion = navegacion;

        Titulo = "Nuevo gasto";
        Categorias = Enum.GetValues<CategoriaGasto>().ToList();

        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        CancelarCommand = new AsyncRelayCommand(navegacion.VolverAsync);
    }

    public IReadOnlyList<CategoriaGasto> Categorias { get; }

    public AsyncRelayCommand GuardarCommand { get; }
    public AsyncRelayCommand CancelarCommand { get; }

    public string Descripcion
    {
        get => descripcion;
        set => AsignarPropiedad(ref descripcion, value);
    }

    public string MontoTexto
    {
        get => montoTexto;
        set => AsignarPropiedad(ref montoTexto, value);
    }

    public CategoriaGasto CategoriaSeleccionada
    {
        get => categoriaSeleccionada;
        set => AsignarPropiedad(ref categoriaSeleccionada, value);
    }

    public DateTime Fecha
    {
        get => fecha;
        set => AsignarPropiedad(ref fecha, value);
    }

    public string MensajeDeErrores
    {
        get => mensajeDeErrores;
        private set
        {
            if (AsignarPropiedad(ref mensajeDeErrores, value))
                NotificarCambio(nameof(HayErrores));
        }
    }

    public bool HayErrores => !string.IsNullOrEmpty(MensajeDeErrores);

    public bool EsEdicion
    {
        get => esEdicion;
        private set => AsignarPropiedad(ref esEdicion, value);
    }

    public void Cargar(Gasto? gastoAEditar)
    {
        MensajeDeErrores = string.Empty;

        if (gastoAEditar is null)
        {
            EsEdicion = false;
            Titulo = "Nuevo gasto";
            return;
        }

        EsEdicion = true;
        Titulo = "Editar gasto";
        idDelGasto = gastoAEditar.Id;
        Descripcion = gastoAEditar.Descripcion;
        MontoTexto = gastoAEditar.Monto.ToString("0.##");
        CategoriaSeleccionada = gastoAEditar.Categoria;
        Fecha = gastoAEditar.Fecha;
    }

    private Task GuardarAsync() => EjecutarOcupadoAsync(async () =>
    {
        if (!ConversorMonto.TryConvertir(MontoTexto, out var monto))
        {
            MensajeDeErrores = "El monto debe ser un número válido.";
            return;
        }

        var gasto = new Gasto
        {
            Id = idDelGasto,
            Descripcion = Descripcion.Trim(),
            Monto = monto,
            Categoria = CategoriaSeleccionada,
            Fecha = Fecha.Date
        };

        var errores = ValidadorGasto.Validar(gasto, DateTime.Today);

        if (errores.Count > 0)
        {
            MensajeDeErrores = string.Join(Environment.NewLine, errores);
            return;
        }

        MensajeDeErrores = string.Empty;
        await repositorio.GuardarAsync(gasto);
        dispositivo.VibrarSuavemente();
        await navegacion.VolverAsync();
    });

    protected override Task AlOcurrirErrorAsync(Exception excepcion)
        => dispositivo.MostrarMensajeAsync("No se pudo guardar", excepcion.Message);
}

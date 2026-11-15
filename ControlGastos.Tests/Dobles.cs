using ControlGastos.Models;
using ControlGastos.Services;

namespace ControlGastos.Tests;

public class RepositorioEnMemoria : IGastoRepositorio
{
    private readonly List<Gasto> gastos = new();
    private decimal presupuesto;

    public Task<IReadOnlyList<Gasto>> ObtenerTodosAsync()
        => Task.FromResult<IReadOnlyList<Gasto>>(gastos.Select(g => g.Clonar()).ToList());

    public Task GuardarAsync(Gasto gasto)
    {
        gastos.RemoveAll(existente => existente.Id == gasto.Id);
        gastos.Add(gasto.Clonar());
        return Task.CompletedTask;
    }

    public Task<bool> EliminarAsync(Guid id)
        => Task.FromResult(gastos.RemoveAll(gasto => gasto.Id == id) > 0);

    public Task<decimal> ObtenerPresupuestoAsync() => Task.FromResult(presupuesto);

    public Task GuardarPresupuestoAsync(decimal nuevoPresupuesto)
    {
        presupuesto = nuevoPresupuesto;
        return Task.CompletedTask;
    }
}

public class DispositivoFalso : IServicioDispositivo
{
    public bool RespuestaDeConfirmacion { get; set; } = true;
    public string? RespuestaDeNumero { get; set; }
    public List<string> Mensajes { get; } = new();
    public int Vibraciones { get; private set; }

    public Task MostrarMensajeAsync(string titulo, string mensaje)
    {
        Mensajes.Add($"{titulo}: {mensaje}");
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmarAsync(string titulo, string mensaje) => Task.FromResult(RespuestaDeConfirmacion);

    public Task<string?> PedirNumeroAsync(string titulo, string mensaje, string valorInicial)
        => Task.FromResult(RespuestaDeNumero);

    public void VibrarSuavemente() => Vibraciones++;
}

public class NavegacionFalsa : IServicioNavegacion
{
    public Gasto? UltimoGastoEnviado { get; private set; }
    public int VecesQueIrAlFormulario { get; private set; }
    public int VecesQueVolvio { get; private set; }

    public Task IrAFormularioDeGastoAsync(Gasto? gastoAEditar)
    {
        UltimoGastoEnviado = gastoAEditar;
        VecesQueIrAlFormulario++;
        return Task.CompletedTask;
    }

    public Task VolverAsync()
    {
        VecesQueVolvio++;
        return Task.CompletedTask;
    }
}

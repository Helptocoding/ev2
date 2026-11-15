using System.Windows.Input;

namespace ControlGastos.ViewModels.Comandos;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> accion;
    private readonly Func<bool>? puedeEjecutar;
    private bool enEjecucion;

    public AsyncRelayCommand(Func<Task> accion, Func<bool>? puedeEjecutar = null)
    {
        this.accion = accion;
        this.puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => !enEjecucion && (puedeEjecutar?.Invoke() ?? true);

    public async void Execute(object? parameter) => await EjecutarAsync();

    public async Task EjecutarAsync()
    {
        if (!CanExecute(null))
            return;

        try
        {
            enEjecucion = true;
            NotificarPuedeEjecutar();
            await accion();
        }
        finally
        {
            enEjecucion = false;
            NotificarPuedeEjecutar();
        }
    }

    public void NotificarPuedeEjecutar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

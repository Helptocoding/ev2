using System.Windows.Input;

namespace ControlGastos.ViewModels.Comandos;

public class RelayCommand : ICommand
{
    private readonly Action accion;
    private readonly Func<bool>? puedeEjecutar;

    public RelayCommand(Action accion, Func<bool>? puedeEjecutar = null)
    {
        this.accion = accion;
        this.puedeEjecutar = puedeEjecutar;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => puedeEjecutar?.Invoke() ?? true;

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
            accion();
    }

    public void NotificarPuedeEjecutar() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

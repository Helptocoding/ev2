using System.Windows.Input;

namespace ControlGastos.ViewModels.Comandos;

public class AsyncRelayCommand<TParametro> : ICommand
{
    private readonly Func<TParametro, Task> accion;

    public AsyncRelayCommand(Func<TParametro, Task> accion)
    {
        this.accion = accion;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => parameter is TParametro;

    public async void Execute(object? parameter)
    {
        if (parameter is TParametro valor)
            await accion(valor);
    }

    public Task EjecutarAsync(TParametro parametro) => accion(parametro);
}

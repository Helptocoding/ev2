using ControlGastos.Services;

namespace ControlGastos.Plataforma;

public class ServicioDispositivo : IServicioDispositivo
{
    public Task MostrarMensajeAsync(string titulo, string mensaje)
        => Shell.Current.DisplayAlert(titulo, mensaje, "Aceptar");

    public Task<bool> ConfirmarAsync(string titulo, string mensaje)
        => Shell.Current.DisplayAlert(titulo, mensaje, "Sí", "No");

    public Task<string?> PedirNumeroAsync(string titulo, string mensaje, string valorInicial)
        => Shell.Current.DisplayPromptAsync(
            titulo,
            mensaje,
            accept: "Guardar",
            cancel: "Cancelar",
            placeholder: "0.00",
            maxLength: 12,
            keyboard: Keyboard.Numeric,
            initialValue: valorInicial);

    public void VibrarSuavemente()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
        }
    }
}

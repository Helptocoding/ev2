using System.Globalization;

namespace ControlGastos.Helpers;

public static class ConversorMonto
{
    public static bool TryConvertir(string? texto, out decimal monto)
    {
        monto = 0;

        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var textoLimpio = texto.Trim();

        return decimal.TryParse(textoLimpio, NumberStyles.Number, CultureInfo.CurrentCulture, out monto)
            || decimal.TryParse(textoLimpio, NumberStyles.Number, CultureInfo.InvariantCulture, out monto);
    }
}

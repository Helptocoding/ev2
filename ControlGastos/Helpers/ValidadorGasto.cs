using ControlGastos.Models;

namespace ControlGastos.Helpers;

public static class ValidadorGasto
{
    public const int LongitudMinimaDeDescripcion = 3;
    public const int LongitudMaximaDeDescripcion = 60;
    public const decimal MontoMaximoPermitido = 1_000_000m;

    public static IReadOnlyList<string> Validar(Gasto gasto, DateTime hoy)
    {
        var errores = new List<string>();
        var descripcion = gasto.Descripcion?.Trim() ?? string.Empty;

        if (descripcion.Length == 0)
            errores.Add("La descripción es obligatoria.");
        else if (descripcion.Length < LongitudMinimaDeDescripcion)
            errores.Add($"La descripción debe tener al menos {LongitudMinimaDeDescripcion} caracteres.");
        else if (descripcion.Length > LongitudMaximaDeDescripcion)
            errores.Add($"La descripción no puede pasar de {LongitudMaximaDeDescripcion} caracteres.");

        if (gasto.Monto <= 0)
            errores.Add("El monto debe ser mayor que cero.");
        else if (gasto.Monto > MontoMaximoPermitido)
            errores.Add($"El monto no puede pasar de {MontoMaximoPermitido:N0}.");

        if (gasto.Fecha.Date > hoy.Date)
            errores.Add("La fecha no puede ser futura.");

        if (!Enum.IsDefined(gasto.Categoria))
            errores.Add("Selecciona una categoría válida.");

        return errores;
    }
}

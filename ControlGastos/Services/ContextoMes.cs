using System.Globalization;

namespace ControlGastos.Services;

public class ContextoMes
{
    public DateTime Mes { get; private set; } = PrimerDiaDelMes(DateTime.Today);

    public string Titulo
    {
        get
        {
            var texto = Mes.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
            return char.ToUpper(texto[0], CultureInfo.CurrentCulture) + texto[1..];
        }
    }

    public void IrAlMesAnterior() => Mes = Mes.AddMonths(-1);

    public void IrAlMesSiguiente() => Mes = Mes.AddMonths(1);

    private static DateTime PrimerDiaDelMes(DateTime fecha) => new(fecha.Year, fecha.Month, 1);
}

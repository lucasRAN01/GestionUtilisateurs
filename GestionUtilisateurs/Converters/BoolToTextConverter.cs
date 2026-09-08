using System.Globalization;

namespace GestionUtilisateurs.Converters;

/// <summary>Convertisseur bool -> libellé "Oui"/"Non".</summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Oui" : "Non";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

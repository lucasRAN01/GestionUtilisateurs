using System.Globalization;

namespace GestionUtilisateurs.Converters;

/// <summary>Convertisseur DateTime? -> texte "jamais" si null sinon date/heure.</summary>
public class NullableDateTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt.ToString("dd/MM/yyyy HH:mm");
        return "Jamais";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

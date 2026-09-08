using System.Globalization;

namespace GestionUtilisateurs.Converters;

/// <summary>Convertisseur enum UserStatus -> libellé français.</summary>
public class UserStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DataAccess.Models.UserStatus status)
            return status == DataAccess.Models.UserStatus.Actif ? "Actif" : "Inactif";
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

using System.Globalization;

namespace GestionUtilisateurs.Converters;

/// <summary>Convertisseur bool (statut actif) -> libellé du bouton d'activation.</summary>
public class ToggleStatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DataAccess.Models.UserStatus s && s == DataAccess.Models.UserStatus.Actif
            ? "Désactiver"
            : "Activer";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

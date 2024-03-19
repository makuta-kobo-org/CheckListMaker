using System.Globalization;

namespace CheckListMaker.Converters;

/// <summary> Drag and dropで使用するconverter </summary>
public class BoolToColorConverter : IValueConverter
{
    /// <summary> bool to color convert を行う </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isBeingDragged = (bool?)value;

        var result = (isBeingDragged ?? false) ? Color.FromArgb("#919191") : Color.FromArgb("#99B080");
        return result;
    }

    /// <summary> リバースする </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

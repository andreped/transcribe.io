using System.Globalization;
using Whisper.net.Ggml;

namespace transcribe.io.Converters;

public class GgmlTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is GgmlType type)
        {
            return type.ToString();
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

namespace Neu;

public static partial class DateTimeFunctions {

    public static String ToFriendlyLocalTimestamp(
        this DateTime now) {

        return $"{now.ToLocalTime().ToString("HH:mm 'on' dd/MM/yyyy")}";
    }
}
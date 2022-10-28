
namespace Neu;

public static partial class DateTimeFunctions {

    public static String ToFriendlyLocalTimestamp(
        this DateTime now) {

        return $"{now.ToLocalTime().ToString("HH:mm 'on' dd/MM/yyyy")}";
    }
}

public static partial class StringFunctions {

    public static String ReplacingExtension(
        this String sample,
        String ext,
        String newExt) {
        
        if (!sample.EndsWith(ext)) {

            throw new Exception();
        }

        return $"{sample.Substring(0, sample.Length - ext.Length)}.{newExt}";
    }
}
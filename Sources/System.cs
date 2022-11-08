
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

    public static String SetExtension(
        this String sample,
        String newExt) {

        var ext = Path.GetExtension(sample);

        return sample.ReplacingExtension(ext, newExt);
    }

    // public static bool IsInstanceNamespace(
    //     this List<String> ns) {

    //     if (ns.Count == 0) {

    //         return false;
    //     }

    //     return ns.All(x => x.StartsWithLowerCase());
    // }

    public static bool IsStaticNamespace(
        this List<String> ns) {

        if (ns.LastOrDefault() is String l && l.Length > 0) {

            return Char.IsUpper(l[0]);
        }

        return false;
    }

    // public static bool IsIndexedStructExpression(
    //     this List<String> ns) {

    //     if (ns.Count != 2) {

    //         return false;
    //     }

    //     if (ns[0] != "this") {

    //         return false;
    //     }

    //     return ns[1].StartsWithLowerCase();
    // }

    public static bool StartsWithLowerCase(
        this String str) {

        if (str.Length == 0) {

            return false;
        }

        return Char.IsLower(str[0]);
    }
}
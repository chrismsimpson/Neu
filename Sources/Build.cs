
namespace Neu;

public partial class Compiler {

    public static (String StandardOutput, String StandardError, bool ExitedSuccessfully) Build(
        String compilerPath,
        String runtimePath,
        String inputCpp,
        bool verbose) {

        var ext = Path.GetExtension(inputCpp);

        var exeExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ".exe"
            : ".out";

        var exeName = $"{inputCpp.Substring(0, inputCpp.Length - ext.Length)}{exeExt}";       

        return Build(compilerPath, runtimePath, inputCpp, exeName, verbose);
    }

    public static (String StandardOutput, String StandardError, bool ExitedSuccessfully) Build(
        String compilerPath,
        String runtimePath,
        String inputCpp,
        String exeName,
        bool verbose) {

        var o = (String data) => {

            WriteLine(data);
        };

        var e = (String err) => {
            
            var og = Console.ForegroundColor;

            Console.ForegroundColor = 
                err.Contains("error")
                    ? ConsoleColor.Red
                    : ConsoleColor.Yellow;

            WriteLine(err);

            Console.ForegroundColor = og;
        };

        var args = new String[] {
            verbose
                ? "-fcolor-diagnostics"
                : "",
            "-std=c++20",
            // Don't complain about unsupported -W flags below.
            "-Wno-unknown-warning-option",
            "-Wno-unqualified-std-cast-call",
            "-Wno-user-defined-literals",
            "-Wno-deprecated-declarations",
            "-DNEU_CONTINUE_ON_PANIC",
            $"-I",
            // Environment.CurrentDirectory,
            runtimePath,
            inputCpp,            
            $"-o",
            exeName
        };

        return Process.Run(
            name: compilerPath,
            // arguments: $"-I{Environment.CurrentDirectory} -IRuntime {runtimePath} -o {exeName} -std=c++20 -Wno-user-defined-literals -DNEU_CONTINUE_ON_PANIC",
            arguments: Join(" ", args),
            dataReceived: verbose ? o : null,
            errorReceived: verbose ? e : null);
    }
}

namespace Neu;

public partial class Compiler {

    public static (String StandardOutput, String StandardError, bool ExitedSuccessfully) Build(
        String path,
        bool verbose) {

        var ext = System.IO.Path.GetExtension(path);

        var exeName = path.Substring(0, path.Length - ext.Length);

        return Build(path, exeName, verbose);
    }

    public static (String StandardOutput, String StandardError, bool ExitedSuccessfully) Build(
        String path,
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

        return Process.Run(
            name: "clang++",
            arguments: $"-I{Environment.CurrentDirectory} -IRuntime {path} -o {exeName} -std=c++20 -Wno-user-defined-literals -DNEU_CONTINUE_ON_PANIC",
            dataReceived: verbose ? o : null,
            errorReceived: verbose ? e : null);
    }
}
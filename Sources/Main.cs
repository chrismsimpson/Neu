
namespace Neu;

public static partial class Program {

    public static int Main(String[] args) {

        switch (true) {

            case var _ when 
                args.Length > 1
                && args[0] == "build"
                && File.Exists(args[1]): {

                Compiler.Build(
                    path: args[1], 
                    verbose: !args.Contains("--silent"));

                return 0;
            }

            case var _ when
                args.Length > 0
                && args[0] == "clean": {

                Compiler.Clean();

                WriteLine(CLEAN_MESSAGE);

                return 0;
            }

            case var _ when 
                args.Length == 1 
                && File.Exists(args[0]): {

                throw new Exception();
            }

            ///

            case var _ when 
                args.Length > 0
                && args[0] == "build": {

                Console.Error.WriteLine(BUILD_USAGE);

                return 1;
            }

            default: {

                Console.Error.WriteLine(USAGE);

                return 1;
            }
        }
    }

    public static readonly String USAGE = 
@"usage: neu [subtask] [-h]

Subtasks:
  build                     Build specified file
  clean                     Clean build directory
  transpile (default)       Transpile specified file to C++

Options:
  -h                        Subtask specific help
";

    public static readonly String BUILD_USAGE = 
@"usage: neu build [-h] [OPTIONS] [FILES...]

Flags:
  -h, --help                Print this help and exit
  
Options:
  -o,--binary-dir PATH      Output directory for compiled FILES
                            Defaults to $PWD/Build

Arguments:
  FILES...                  List of files to compile. The outputs
                            `<inputFilename>.cpp` in the binary directory.
";

    public static readonly String CLEAN_MESSAGE = 
@"Clean successful
";

    // public static void Foo() {

    //     // jakt file.jakt
    //     // clang++ -std=c++20 -I. -Iruntime -Wno-user-defined-literals ./build/file.cpp

    //     var n = "Apps/foo";

    //     var (stdOutput, stdError, exitSuccess) = Process.Run(
    //         name: "clang++",
    //         arguments: $"-std=c++20 -I. -IRuntime -Wno-user-defined-literals ./build/{n}.cpp -o ./build/{n}.out",
    //         dataReceived: data => {

    //             WriteLine(data);
    //         },
    //         errorReceived: err => {

    //             var og = Console.ForegroundColor;

    //             Console.ForegroundColor = 
    //                 err.Contains("error")
    //                     ? ConsoleColor.Red
    //                     : ConsoleColor.Yellow;

    //             WriteLine(err);

    //             Console.ForegroundColor = og;
    //         });

    //     // WriteLine($"success: {exitSuccess}");
    // }
}

public enum BuildMode {

    Clang,
    CMakeLists
}
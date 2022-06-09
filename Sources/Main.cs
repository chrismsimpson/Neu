
namespace Neu;

public enum CompilerMode {

    Transpile,
    CMakeGenerate,
    CMakeBuild,
    All
}

public static partial class Program {

    public static void Main(
        String[] args) {

        var parser = new Compiler();

        ///

        var mode = CompilerMode.Transpile;

        IEnumerable<String> _args = args;

        var firstArg = args.FirstOrDefault();

        switch (firstArg?.ToLower()) {

            case "cmake-generate":

                mode = CompilerMode.CMakeGenerate;

                _args = args.Skip(1);

                break;

            ///

            case "cmake-build":

                mode = CompilerMode.CMakeBuild;

                _args = args.Skip(1);

                break;

            ///

            case "all":

                mode = CompilerMode.All;

                _args = args.Skip(1);

                break;

            ///

            case null:

                throw new Exception();

            ///

            default:

                break;
        }

        ///

        switch (mode) {

            case CompilerMode.Transpile:

                foreach (var arg in _args) {

                    var compiledOrError = parser.Compile(arg);

                    switch (compiledOrError) {

                        case var _ when compiledOrError.Error is ParserError pe:

                            DisplayError(parser, pe.Content, pe.Span);

                            break;

                        case var _ when compiledOrError.Error is TypeCheckError te:

                            DisplayError(parser, te.Content, te.Span);

                            break;

                        case var _ when compiledOrError.Error is ValidationError ve:

                            DisplayError(parser, ve.Content, ve.Span);

                            break;

                        case var _ when compiledOrError.Error is Error e:

                            WriteLine($"Error: {compiledOrError.Error}");

                            break;

                        default:

                            WriteLine("Success!");

                            break;
                    }           
                }

                break;

            ///
            
            case CompilerMode.CMakeGenerate:

                throw new NotImplementedException();

                // break;

            ///

            case CompilerMode.CMakeBuild:

                throw new NotImplementedException();

                // break;

            ///

            case CompilerMode.All:

                foreach (var arg in _args) {

                    var og = Console.ForegroundColor;
                    
                    Write($"{arg} ");

                    // Transpile

                    var cppStringOrError = parser.ConvertToCPP(arg);

                    if (cppStringOrError.Error != null) {

                        throw new Exception();
                    }

                    var cppString = cppStringOrError.Value ?? throw new Exception();

                    ///

                    var id = Path.GetFileNameWithoutExtension(arg);

                    ///

                    var buildDir = $"./Build";
                    var projBuildDir = $"{buildDir}/{id}";
                    var genDir = $"./Generated";
                    var projGenDir = $"{genDir}/{id}";

                    ///

                    parser.Generate(
                        buildDir,
                        projBuildDir,
                        genDir,
                        projGenDir,
                        $"{id}", 
                        cppString ?? throw new Exception());

                    ///

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = parser
                        .Process(
                            name: "cmake",
                            arguments: $"{projGenDir} -B {projBuildDir} -G Ninja",
                            printProgress: true);

                    if (cmakeGenerateBuildErr) {

                        // if (!IsNullOrWhiteSpace(cmakeGenerateBuildOutput)) {

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed to generated build\n");

                            Console.ForegroundColor = og;
                        // }

                        continue;
                    }

                    ///

                    var (cmakeBuildOutput, cmakeBuildErr) = parser
                        .Process(
                            name: "cmake",
                            arguments: $"--build {projBuildDir}",
                            printProgress: true);
                        
                    if (cmakeBuildErr) {

                        // if (!IsNullOrWhiteSpace(cmakeBuildOutput)) {

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed to build\n");

                            Console.ForegroundColor = og;
                        // }

                        continue;
                    }

                    ///

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Built\n");

                    Console.ForegroundColor = og;
                }

                break;

            ///
            
            default:

                throw new Exception("Unexpected");
        }

        ///

        // foreach (var arg in args) {

        //     var compiledOrError = parser.Compile(arg);

        //     switch (compiledOrError) {

        //         case var _ when compiledOrError.Error is ParserError pe:

        //             DisplayError(parser, pe.Content, pe.Span);

        //             break;

        //         case var _ when compiledOrError.Error is TypeCheckError te:

        //             DisplayError(parser, te.Content, te.Span);

        //             break;

        //         case var _ when compiledOrError.Error is ValidationError ve:

        //             DisplayError(parser, ve.Content, ve.Span);

        //             break;

        //         case var _ when compiledOrError.Error is Error e:

        //             WriteLine($"Error: {compiledOrError.Error}");

        //             break;

        //         default:

        //             WriteLine("Success!");

        //             break;
        //     }           
        // }
    }

    public static void DisplayError(Compiler parser, String? msg, Span span) {
        
        if (msg is String m) {

            WriteLine($"Error: {m}");
        }
        else {
            
            WriteLine("Error");
        }
    
        WriteLine("-----");

        var fileContents = parser.GetFileContents(span.FileId);

        var index = 0;

        while (index <= fileContents.Length) {

            var c = ' ';

            if (index < fileContents.Length) {

                c = ToChar(fileContents[index]);
            }
            else if (span.Start == span.End && index == span.Start) {

                c = '_';
            }

            if ((index >= span.Start && index < span.End)
                || (span.Start == span.End && index == span.Start)) {

                // In the error span

                var ogColor = System.Console.ForegroundColor;

                System.Console.ForegroundColor = ConsoleColor.Red;

                Write(c);

                System.Console.ForegroundColor = ogColor;
            }
            else {

                Write(c);
            }

            index += 1;
        }

        WriteLine("");
        
        WriteLine("-----");
    }
}
namespace Neu;

public enum CompilerMode {

    Build,
    Clean,
    CleanTests,
    CMakeGenerate,
    CMakeBuild,
    Transpile
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

            case "build":

                mode = CompilerMode.Build;

                _args = args.Skip(1);

                break;

            ///

            case "clean":

                mode = CompilerMode.Clean;

                _args = args.Skip(1);

                break;

            ///

            case "clean-tests":

                mode = CompilerMode.CleanTests;

                _args = args.Skip(1);

                break;

            ///

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

            case "transpile":

                // already in correct mode

                _args = args.Skip(1);

                break;

            ///

            case null: // maybe you want help?

                throw new Exception();

            ///

            default:

                break;
        }

        ///

        switch (mode) {

            case CompilerMode.Build: {

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
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
                            printProgress: true);

                    if (cmakeGenerateBuildErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to generate build\n");

                        Console.ForegroundColor = og;

                        if (!IsNullOrWhiteSpace(cmakeGenerateBuildOutput)) {

                            WriteLine($"Generate CMake error:\n\n{cmakeGenerateBuildOutput}");
                        }

                        continue;
                    }

                    ///

                    var (cmakeBuildOutput, cmakeBuildErr) = parser
                        .BuildWithCMake(
                            projBuildDir, 
                            printProgress: true);
                        
                    if (cmakeBuildErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to build\n");

                        Console.ForegroundColor = og;

                        ///

                        if (!IsNullOrWhiteSpace(cmakeBuildOutput)) {

                            WriteLine($"Build CMake error:\n\n{cmakeBuildOutput}");
                        }

                        continue;
                    }

                    ///

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Built\n");

                    Console.ForegroundColor = og;
                }

                break;
            }

            ///
            
            case CompilerMode.Clean: {

                var og = Console.ForegroundColor;
            
                Write($"Cleaning ");

                ///

                Compiler.Clean();

                ///

                Console.ForegroundColor = ConsoleColor.Green;

                Write($" Cleaned\n");

                Console.ForegroundColor = og;

                break;
            }

            ///
            
            case CompilerMode.CleanTests: {

                var og = Console.ForegroundColor;
            
                Write($"Cleaning tests ");

                ///

                Compiler.CleanTests();

                ///

                Console.ForegroundColor = ConsoleColor.Green;

                Write($" Cleaned\n");

                Console.ForegroundColor = og;

                break;
            }

            ///
            
            case CompilerMode.CMakeGenerate: {

                foreach (var arg in _args) {

                    var og = Console.ForegroundColor;
                    
                    Write($"Generating CMake build setup for {arg} ");

                    ///

                    var id = Path.GetFileNameWithoutExtension(arg);

                    ///

                    var buildDir = $"./Build";
                    var projBuildDir = $"{buildDir}/{id}";
                    var genDir = $"./Generated";
                    var projGenDir = $"{genDir}/{id}";

                    ///

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = parser
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
                            printProgress: true);

                    if (cmakeGenerateBuildErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to generate build\n");

                        Console.ForegroundColor = og;

                        continue;
                    }

                    ///

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Generated\n");

                    Console.ForegroundColor = og;
                }

                break;
            }

            ///

            case CompilerMode.CMakeBuild: {

                foreach (var arg in _args) {

                    var og = Console.ForegroundColor;
                    
                    Write($"Building via CMake for {arg} ");

                    ///

                    var id = Path.GetFileNameWithoutExtension(arg);

                    ///

                    var buildDir = $"./Build";
                    var projBuildDir = $"{buildDir}/{id}";
                    
                    ///

                    var (cmakeBuildOutput, cmakeBuildErr) = parser
                        .BuildWithCMake(
                            projBuildDir,
                            printProgress: true);

                    if (cmakeBuildErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to build\n");

                        Console.ForegroundColor = og;

                        continue;
                    }

                    ///

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Built\n");

                    Console.ForegroundColor = og;
                }

                break;
            }

            ///

            case CompilerMode.Transpile: {

                // Default

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
            }

            ///

            default: {

                throw new Exception("Unexpected");
            }
        }
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
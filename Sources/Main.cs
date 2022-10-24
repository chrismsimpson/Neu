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

        Error? firstError = null;

        ///

        var mode = CompilerMode.Transpile;

        ///

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

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr, cmakeExitedSuccessfully) = parser
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
                            printProgress: true);

                    if (cmakeExitedSuccessfully) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to generate build\n");

                        Console.ForegroundColor = og;

                        if (!IsNullOrWhiteSpace(cmakeGenerateBuildOutput)) {

                            WriteLine($"Generate CMake error:\n\n{cmakeGenerateBuildOutput}");
                        }

                        continue;
                    }

                    ///

                    var (cmakeBuildOutput, _, cmakeBuildExitedSuccessfully) = parser
                        .BuildWithCMake(
                            projBuildDir, 
                            printProgress: true);
                        
                    if (cmakeBuildExitedSuccessfully) {

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

                    // cmakeGenerateBuildErr
                    var (cmakeGenerateBuildOutput, _, cmakeGenerateBuildExitedSuccessfully) = parser
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
                            printProgress: true);

                    if (cmakeGenerateBuildExitedSuccessfully) {

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

                    var (cmakeBuildOutput, _, cmakeBuildExitedSuccessfully) = parser
                        .BuildWithCMake(
                            projBuildDir,
                            printProgress: true);

                    if (cmakeBuildExitedSuccessfully) {

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

                    ///

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

                    ///        

                    if (compiledOrError.Error is Error transpileErr) {

                        firstError = firstError ?? transpileErr;
                    }
                }

                break;
            }

            ///

            default: {

                throw new Exception("Unexpected");
            }
        }
    
        ///

        if (firstError != null) {

            throw new Exception($"{firstError}");
        }
    }

    public static void DisplayError(Compiler parser, String? msg, Span span) {

        var ogColor = Console.ForegroundColor;
        
        if (msg is String m) {

            WriteLine($"Error: {m}");
        }
        else {
            
            WriteLine("Error");
        }

        var fileContents = parser.GetFileContents(span.FileId);

        var lineSpans = LineSpans(fileContents);

        var lineIndex = 0;

        var largestLineNumber = lineSpans.Count;

        var width = $"{largestLineNumber}".Length;

        WriteLine($"{new String('-', width + 3)}");

        while (lineIndex < lineSpans.Count) {

            if (span.Start >= lineSpans[lineIndex].Item1 && span.Start < lineSpans[lineIndex].Item2) {

                if (lineIndex > 0) {

                    PrintSourceLine(
                        fileContents, 
                        lineSpans[lineIndex - 1], 
                        span, 
                        lineIndex - 1, 
                        largestLineNumber);
                }

                PrintSourceLine(
                    fileContents, 
                    lineSpans[lineIndex], 
                    span, 
                    lineIndex, 
                    largestLineNumber);

                Write($"{new String(' ', lineSpans[lineIndex].Item2 - lineSpans[lineIndex].Item1 + width + 2)}");

                Console.ForegroundColor = ConsoleColor.Red;

                WriteLine($"^- {msg}");

                Console.ForegroundColor = ogColor;

                while (lineIndex < lineSpans.Count && span.End > lineSpans[lineIndex].Item1) {

                    lineIndex += 1;

                    if (lineIndex >= lineSpans.Count) {

                        break;
                    }

                    PrintSourceLine(
                        fileContents, 
                        lineSpans[lineIndex], 
                        span, 
                        lineIndex, 
                        largestLineNumber);
                }

                break;
            }
            else {

                lineIndex += 1;
            }
        }

        if (span.Start == span.End && span.Start == fileContents.Length && lineIndex > 0) {

            PrintSourceLine(
                fileContents,
                lineSpans[lineIndex - 1],
                span,
                lineIndex - 1,
                largestLineNumber);
        }

        Console.ForegroundColor = ogColor;

        WriteLine($"{new String('-', width + 3)}");

        // WriteLine("-----");

        // var fileContents = parser.GetFileContents(span.FileId);

        // var index = 0;

        // while (index <= fileContents.Length) {

        //     var c = ' ';

        //     if (index < fileContents.Length) {

        //         c = ToChar(fileContents[index]);
        //     }
        //     else if (span.Start == span.End && index == span.Start) {

        //         c = '_';
        //     }

        //     if ((index >= span.Start && index < span.End)
        //         || (span.Start == span.End && index == span.Start)) {

        //         // In the error span

        //         var ogColor = System.Console.ForegroundColor;

        //         System.Console.ForegroundColor = ConsoleColor.Red;

        //         Write(c);

        //         System.Console.ForegroundColor = ogColor;
        //     }
        //     else {

        //         Write(c);
        //     }

        //     index += 1;
        // }

        // WriteLine("");
        
        // WriteLine("-----");
    }

    public static void PrintSourceLine(
        byte[] fileContents,
        (Int32, Int32) fileSpan,
        Span errorSpan,
        Int32 lineNumber,
        Int32 largestLineNumber) {

        var ogColor = Console.ForegroundColor;

        var index = fileSpan.Item1;

        var width = $"{largestLineNumber}".Length;

        Write($" {lineNumber} | ");

        while (index <= fileSpan.Item2) {

            char? c = null;

            if (index < fileSpan.Item2) {

                c = ToChar(fileContents[index]);
            }
            else if (errorSpan.Start == errorSpan.End && index == errorSpan.Start) {

                c = '_';
            }
            else {

                c = ' ';
            }

            if ((index >= errorSpan.Start && index < errorSpan.End)
                || (errorSpan.Start == errorSpan.End && index == errorSpan.Start)) {

                // In the error span

                Console.ForegroundColor = ConsoleColor.Red;

                Write(c);
            }
            else {

                Console.ForegroundColor = ogColor;

                Write(c);
            }

            index += 1;
        }

        WriteLine();
    }

    public static List<(Int32, Int32)> LineSpans(
        byte[] contents) {

        var idx = 0;

        var output = new List<(Int32, Int32)>();

        var start = idx;

        while (idx < contents.Length) {

            if (ToChar(contents[idx]) == '\n') {

                output.Add((start, idx));

                start = idx + 1;
            }

            idx += 1;
        }

        if (start < idx) {

            output.Add((start, idx));
        }

        return output;
    }
}
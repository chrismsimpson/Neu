
namespace Neu;

public static partial class ArgumentFunctions {

    public static String? ValueFromKeys(
        this String[] args,
        params String[] keys) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (keys.Contains(arg)
                && (i + 1) < args.Length) {

                return args[i + 1];
            }
        }

        return null;
    }

    public static bool ContainsAny(
        this String[] args,
        params String[] keys) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (keys.Contains(arg)) {

                return true;
            }
        }

        return false;
    }

    public static List<String> ValuesFromKeys(
        this String[] args,
        params String[] keys) {

        var values = new List<String>();

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (keys.Contains(arg)) {
            
                if ((i + 1) < args.Length) {

                    i++;

                    values.Add(args[i]);
                }
                else {

                    throw new Exception();
                }
            }
        }

        return values;
    }
}

public static partial class Program {

    public static int Main(String[] args) {

        var arguments = args.ParseArguments();

        String binDirectory;

        switch (arguments.BinaryDirectory) {

            case String dir: {

                binDirectory = dir;

                break;
            }

            default: {

                var dir = Directory.GetCurrentDirectory();

                binDirectory = Path.Combine(dir, "Build");

                break;
            }
        }

        Directory.CreateDirectory(binDirectory);

        switch (true) {

            case var _ when
                args.Length > 0
                && args[0] == "clean": {

                Compiler.Clean();

                WriteLine(CLEAN_MESSAGE);

                return 0;
            }

            case var _ when
                args.Length > 0
                && args[0] == "dummy": {

                var compiler = new Compiler(arguments.IncludePaths);

                var filename = "./Build/foo.cpp";

                var contents = UTF8.GetBytes("#include <lib.h>\n\nErrorOr<int> NeuInternal::main(const Array<String> args)\n{\n    return (static_cast<Int64>(0LL));\n}");

                File.WriteAllBytes(filename, contents);

                var runtimePath = arguments.RuntimePath ?? "Runtime";

                var defaultCxxCompilerPath = "clang++";

                var cxxCompilerPath = arguments.CXXCompilerPath ?? defaultCxxCompilerPath;

                var (stdOut, stdErr, success) = Compiler.Build(
                    cxxCompilerPath,
                    runtimePath,
                    filename,
                    verbose: !args.Contains("--silent"));

                return success ? 0 : 1;
            }

            case var _ when
                arguments.InputFiles.Any(): {

                var compiler = new Compiler(arguments.IncludePaths);

                Error? firstError = null;

                foreach (var filename in args.Where(x => File.Exists(x))) {

                    var cppOrErr = compiler.ConvertToCPP(filename);

                    switch (true) {

                        case var _ when 
                            cppOrErr.Error == null
                            && !IsNullOrWhiteSpace(cppOrErr.Value): {

                            var outFilePath = Path.Combine(
                                binDirectory,
                                filename.StartsWith("./")
                                    ? filename.Substring(2, filename.Length - 2)
                                    : filename)
                                .ReplacingExtension(".neu", "cpp");

                            var outPath = outFilePath
                                .Substring(0, outFilePath.Length - Path.GetFileName(outFilePath).Length);

                            Directory.CreateDirectory(outPath);

                            File.WriteAllText(outFilePath, cppOrErr.Value);

                            if (arguments.PrettifyCppSource) {

                                var defaultClangFormatPath = "clang-format";

                                var clangFormatPath = arguments.ClangFormatPath ?? defaultClangFormatPath;

                                _ = Process.Run(
                                    clangFormatPath,
                                    arguments: new [] {
                                        "-i",
                                        outFilePath,
                                        "--style=WebKit"
                                    });
                            }

                            if (!arguments.EmitSourceOnly) {

                                var runtimePath = arguments.RuntimePath ?? "Runtime";

                                var defaultCxxCompilerPath = "clang++";

                                var cxxCompilerPath = arguments.CXXCompilerPath ?? defaultCxxCompilerPath;

                                Compiler.Build(
                                    cxxCompilerPath,
                                    runtimePath,
                                    outFilePath,
                                    verbose: !args.Contains("--silent"));
                            }

                            break;
                        }

                        case var _ when
                            cppOrErr.Error is Error e: {

                            switch (e) {

                                case ParserError pe: {

                                    DisplayError(compiler, pe.Content, pe.Span);

                                    break;
                                }

                                case ParserErrorWithHint peh: {

                                    DisplayError(compiler, peh.Content, peh.Span);

                                    DisplayHint(compiler, peh.HintString, peh.HintSpan);

                                    break;
                                }

                                case TypeCheckError te: {

                                    DisplayError(compiler, te.Content, te.Span);

                                    break;
                                }

                                case ValidationError ve: {

                                    DisplayError(compiler, ve.Content, ve.Span);

                                    break;
                                }

                                case TypecheckErrorWithHint teh: {

                                    DisplayError(compiler, teh.Content, teh.Span);

                                    DisplayHint(compiler, teh.HintString, teh.HintSpan);

                                    break;
                                }

                                default: {

                                    throw new Exception();
                                }
                            }

                            firstError = firstError ?? e;

                            break;
                        }

                        default: {

                            throw new Exception();
                        }
                    }
                }

                if (firstError != null) {

                    return 1;
                }
                else {

                    return 0;
                }
            }

            ///

            default: {

                Console.Error.WriteLine(USAGE);

                return 1;
            }
        }
    }

    public static readonly String USAGE = 
@"usage: neu [subtask] [-h]

Subtasks:
  build (default)               Transpile then build specified file
  clean                         Clean build directory

Flags:
  -h,--help                     Print this help and exit.
  -p,--prettify-cpp-source      Run emitted C++ source through clang-format.
  -S,--emit-cpp-source-only     Only emit the generated C++ source, do not compile.

Options:
  -o,--binary-dir PATH          Output directory for compiled files.
                                Defaults to $PWD/build.
  -C,--cxx-compiler-path PATH   Path of the C++ compiler to use when compiling the generated sources.
                                Defaults to clang++.
  -F,--clang-format-path PATH   Path to clang-format executable.
                                Defaults to clang-format.
  -R,--runtime-path PATH        Path of the Neu runtime headers.
                                Defaults to $PWD/runtime.
  -I,--include-path PATH        Add an include path for imported Neu files.
                                Can be specified multiple times.

Arguments:
  FILES...                      List of files to compile. The outputs are
                                `<input-filename>.cpp` in the binary directory.
";

    public static readonly String CLEAN_MESSAGE = 
@"
Clean successful
";
}

public partial class NeuArguments {

    public String? BinaryDirectory { get; set; }

    public List<String> InputFiles { get; init; }

    public List<String> IncludePaths { get; init; }

    public bool EmitSourceOnly { get; init; }

    public String? CXXCompilerPath { get; set; }

    public bool PrettifyCppSource { get; init; }

    public String? ClangFormatPath { get; set; }

    public String? RuntimePath { get; set; }

    ///

    public NeuArguments(       
        String? binaryDirectory,
        List<String> inputFiles,
        List<String> includePaths,
        bool emitSourceOnly,
        String? cxxCompilerPath,
        bool prettifyCppSource,
        String? clangFormatPath,
        String? runtimePath) {

        this.BinaryDirectory = binaryDirectory;
        this.InputFiles = inputFiles;
        this.IncludePaths = includePaths;
        this.EmitSourceOnly = emitSourceOnly;
        this.CXXCompilerPath = cxxCompilerPath;
        this.PrettifyCppSource = prettifyCppSource;
        this.ClangFormatPath = clangFormatPath;
        this.RuntimePath = runtimePath;
    }
}

public static partial class Program {

    public static NeuArguments ParseArguments(
        this String[] args) {

        var emitSourceOnly = args.ContainsAny("-S", "--emit-cpp-source-only");

        var prettifyCppSource = args.ContainsAny("-p", "--prettify-cpp-source");

        var includePaths = args.ValuesFromKeys("-I", "--include-path");

        var arguments = new NeuArguments(
            binaryDirectory: null,
            inputFiles: new List<string>(),
            includePaths,
            emitSourceOnly,
            cxxCompilerPath: null,
            prettifyCppSource,
            clangFormatPath: null,
            runtimePath: null);

        if (args.ValueFromKeys("-o", "--binary-dir") is String binDirectory) {

            arguments.BinaryDirectory = binDirectory;
        }
       
        if (args.ValueFromKeys("-C", "--cxx-compiler-path") is String cxxCompilerPath) {
        
            arguments.CXXCompilerPath = cxxCompilerPath;
        }

        if (args.ValueFromKeys("-F", "--clang-format-path") is String clangFormatPath) {
        
            arguments.ClangFormatPath = clangFormatPath;
        }

        if (args.ValueFromKeys("-R", "--runtime-path") is String runtimePath) {
        
            arguments.RuntimePath = runtimePath;
        }

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg.StartsWith('-')) {

                // if a flag

                continue;
            }

            if (!arg.EndsWith(".neu")) {

                // if not a neu file (i.e. a path or bin)

                continue;
            }

            if (!File.Exists(arg)) {

                throw new Exception($"neu: error: file '{arg}' not found or inaccessible");
            }

            arguments.InputFiles.Add(arg);
        }

        return arguments;
    }

}

public enum MessageSeverity {

    Hint,
    Error
}

public static partial class MessageSeverityFunctions {

    public static String GetName(
        this MessageSeverity severity) {

        switch (severity) {

            case MessageSeverity.Hint:
                return "Hint";

            case MessageSeverity.Error:
                return "Error";

            default: 
                throw new Exception();
        }
    }

    public static ConsoleColor ToConsoleColor(
        this MessageSeverity severity) {

        switch (severity) {

            case MessageSeverity.Hint:
                return ConsoleColor.Blue;

            case MessageSeverity.Error:
                return ConsoleColor.Red;    

            default:
                throw new Exception();
        }
    }
}

public static partial class Program {

    public static void DisplayMessageWithSpan(
        MessageSeverity severity,
        Compiler parser, 
        String? msg, 
        Span span) {

        var ogColor = Console.ForegroundColor;
        
        if (msg is String m) {

            WriteLine($"{severity.GetName()}: {m}");
        }
        else {
            
            WriteLine($"{severity.GetName()}");
        }

        var fileContents = parser.GetFileContents(span.FileId);

        var fileName = parser.GetFileName(span.FileId);

        var lineSpans = LineSpans(fileContents);

        var lineIndex = 0;

        var largestLineNumber = lineSpans.Count;

        var width = $"{largestLineNumber}".Length;

        while (lineIndex < lineSpans.Count) {

            if (span.Start >= lineSpans[lineIndex].Item1 && span.Start <= lineSpans[lineIndex].Item2) {

                var columnIndex = span.Start - lineSpans[lineIndex].Item1;

                Console.ForegroundColor = ogColor;

                Write($"{new String('-', width + 3)} ");

                Console.ForegroundColor = ConsoleColor.Yellow;

                Write($"{fileName}:{lineIndex + 1}:{columnIndex + 1}\n");
                
                Console.ForegroundColor = ogColor;

                if (lineIndex > 0) {

                    PrintSourceLine(
                        severity,
                        fileContents, 
                        lineSpans[lineIndex - 1], 
                        span, 
                        lineIndex - 1, 
                        largestLineNumber);
                }

                PrintSourceLine(
                    severity,
                    fileContents, 
                    lineSpans[lineIndex], 
                    span, 
                    lineIndex, 
                    largestLineNumber);

                Write($"{new String(' ', span.Start - lineSpans[lineIndex].Item1 + width + 4)}");

                Console.ForegroundColor = severity.ToConsoleColor();

                WriteLine($"^- {msg}");

                Console.ForegroundColor = ogColor;

                while (lineIndex < lineSpans.Count && span.End > lineSpans[lineIndex].Item1) {

                    lineIndex += 1;

                    if (lineIndex >= lineSpans.Count) {

                        break;
                    }

                    PrintSourceLine(
                        severity,
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

        Console.ForegroundColor = ogColor;

        WriteLine($"{new String('-', width + 3)}");
    }

    public static void DisplayError(Compiler compiler, String? msg, Span span) {

        DisplayMessageWithSpan(MessageSeverity.Error, compiler, msg, span);
    }

    public static void DisplayHint(Compiler compiler, String? msg, Span span) {

        DisplayMessageWithSpan(MessageSeverity.Hint, compiler, msg, span);
    }

    public static void PrintSourceLine(
        MessageSeverity severity,
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

                Console.ForegroundColor = severity.ToConsoleColor();

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

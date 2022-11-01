
namespace Neu;

public static partial class ArgumentFunctions {

    public static String? GetBinDirectory(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-o" || arg == "--binary-dir"
                && (i + 1) < args.Length) {

                return args[i + 1];
            }
        }

        return null;
    }

    public static String? GetRuntimeDirectory(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-R" || arg == "--runtime-path"
                && (i + 1) < args.Length) {

                return args[i + 1];
            }
        }

        return null;
    }

    public static String? GetCPPCompilerPath(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-C" || arg == "--cxx-compiler-path"
                && (i + 1) < args.Length) {

                return args[i + 1];
            }
        }

        return null;
    }

    public static String? GetClangFormatPath(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-F" || arg == "--clang-format-path"
                && (i + 1) < args.Length) {

                return args[i + 1];
            }
        }

        return null;
    }

    public static bool PrettifyCPPSource(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-p" || arg == "--prettify-cpp-source") {

                return true;
            }
        }

        return false;;
    }

    public static bool EmitCPPSourceOnly(
        this String[] args) {

        for (var i = 0; i < args.Length; i++) {

            var arg = args[i];

            if (arg == "-S" || arg == "--emit-cpp-source-only") {

                return true;
            }
        }

        return false;;
    }
}

public static partial class Program {

    public static int Main(String[] args) {

        var binDirectory = args.GetBinDirectory() ?? "./Build";

        var cppCompilerPath = args.GetCPPCompilerPath() ?? "clang++";

        var runtimePath = args.GetRuntimeDirectory() ?? "./Runtime";

        var clangFormatPath = args.GetClangFormatPath() ?? "clang-format";

        switch (true) {

            case var _ when
                args.Length > 0
                && args[0] == "clean": {

                Compiler.Clean();

                WriteLine(CLEAN_MESSAGE);

                return 0;
            }

            case var _ when
                args.Where(x => File.Exists(x)).Any(): {

                var compiler = new Compiler();

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

                            if (args.PrettifyCPPSource()) {

                                _ = Process.Run(
                                    clangFormatPath,
                                    arguments: new [] {
                                        "-i",
                                        outFilePath,
                                        "--style=WebKit"
                                    });
                            }

                            if (!args.EmitCPPSourceOnly()) {

                                Compiler.Build(
                                    cppCompilerPath,
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
  build (default)           Transpile then build specified file
  clean                     Clean build directory

Options:
  -h                        Subtask specific help
";

    public static readonly String CLEAN_MESSAGE = 
@"
Clean successful
";
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

        var lineSpans = LineSpans(fileContents);

        var lineIndex = 0;

        var largestLineNumber = lineSpans.Count;

        var width = $"{largestLineNumber}".Length;

        WriteLine($"{new String('-', width + 3)}");

        while (lineIndex < lineSpans.Count) {

            if (span.Start >= lineSpans[lineIndex].Item1 && span.Start <= lineSpans[lineIndex].Item2) {

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

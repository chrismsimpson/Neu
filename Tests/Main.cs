
namespace NeuTests;

public static partial class Program {

    public static List<int> MatchIndices(this String str, char value) {
    
        var indices = new List<int>();

        for (var i = 0; i < str.Length; i++) {

            if (str[i] == value) {

                indices.Add(i);
            }
        }
        
        return indices;
    }

    public static (String, String)? SplitOnce(this String str, char split) {

        var i = str.IndexOf(split);

        if (i >= 0 && (i + 2 < str.Length)) {

            var a = str[0..i];

            var b = str[(i + 1)..str.Length];

            return (a, b);
        }

        return (str, String.Empty);
    }

    public static String TrimStartMatching(
        this String str,
        String match) {

        var trimmed = str.TrimStart();

        if (trimmed.StartsWith(match)) {

            return trimmed.Substring(match.Length, trimmed.Length - match.Length);
        }

        return trimmed;
    }

    public static readonly String[] CommonArgs = new [] {
        "-I",
        ".",
        "-I",
        "Runtime",
        "-std=c++20",
        "-Wno-user-defined-literals",
        "-DNEU_CONTINUE_ON_PANIC"
    };

    public static Lazy<String> PchFilename => new Lazy<String>(() => { 

        var pchFilename = Path.Combine(
            Path.GetTempPath(), 
            $"Neu-{Guid.NewGuid()}lib.h.pch");

        var (_, _, success) = 
            Neu.Process.Run(
                "clang++",
                new [] {
                    "-x",
                    "c++-header",
                    "Runtime/lib.h",
                    "-o",
                    pchFilename,
                    "-fpch-instantiate-templates"
                },
                CommonArgs);

        if (!success) {

            throw new Exception("Failed to launch compiler for PCH generation.");
        }

        return pchFilename;
    });

    public static ErrorOr<String> ParseQuotedString(
        String s) {

        var _s = s.Trim();

        if (!_s.StartsWith('"') || !_s.EndsWith('"')) {

            return new ErrorOr<String>(new StringError("Expected quoted string"));
        }

        var data = _s[1..(_s.Length - 1)];

        var result = new StringBuilder();

        var lastIndex = 0;

        foreach (var i in data.MatchIndices('\\')) {

            result.Append(data[lastIndex..i]);

            var escapedChar = data[(i + 1)..(i + 2)];

            switch (escapedChar) {

                case "\"": {

                    result.Append('"');
                    
                    break;
                }

                case "\\": {

                    result.Append('\\');

                    break;
                }

                case "a": {

                    result.Append('\x07');

                    break;
                }

                case "b": {

                    result.Append('\x07');

                    break;
                }

                case "f": {

                    result.Append('\x08');

                    break;
                }

                case "n": {

                    result.Append('\n');

                    break;
                }

                case "r": {

                    result.Append('\r');

                    break;
                }

                case "t": {

                    result.Append('\t');

                    break;
                }

                case "v": {

                    result.Append('\x0b');

                    break;
                }

                case var c: {

                    return new ErrorOr<string>(new StringError($"Unknown escape sequence: \\{c}"));
                }
            }

            lastIndex = i + 2;
        }

        result.Append(data[lastIndex..]);

        return new ErrorOr<String>(result.ToString());
    }

    public static ErrorOr<(String?, String?, String?)> FindResults(
        String path) {

        String? output = null;
        String? error = null;
        String? stdErr = null;

        if (File.ReadAllText(path) is String contents) {

            contents = contents.Replace("\r\n", "\n");

            var lines = contents.Split('\n');

            var expectLines = lines.Where(line => line.StartsWith("/// ")).ToList();

            Int32 SEEN_MARKER = 1;
            Int32 SEEN_OUTPUT = 2;
            Int32 SEEN_ERROR = 4;
            Int32 SEEN_STDERR = 8;

            var state = 0;

            foreach (var line in expectLines) {

                if ((state & SEEN_MARKER) == 0) {

                    if (line.StartsWith("/// Expect:")) {

                        state |= SEEN_MARKER;

                        if ((line
                            .SplitOnce(':') ?? throw new Exception())
                            .Item2
                            .TrimStart()
                            .StartsWith("Skip")) {

                            // Not a test

                            return new ErrorOr<(String?, String?, String?)>((null, null, null));
                        }

                        continue;
                    }
                    else {

                        break;
                    }
                }

                if ((state & SEEN_OUTPUT) == 0 && line.StartsWith("/// - output: ")) {

                    output = ParseQuotedString(line.TrimStartMatching("/// - output: ")).Value;

                    state |= SEEN_OUTPUT;

                    continue;
                }

                if ((state & SEEN_ERROR) == 0 && line.StartsWith("/// - error: ")) {

                    error = ParseQuotedString(line.TrimStartMatching("/// - error: ")).Value;

                    state |= SEEN_ERROR;

                    continue;
                }

                if ((state & SEEN_STDERR) == 0 && line.StartsWith("/// - stderr: ")) {

                    stdErr = ParseQuotedString(line.TrimStartMatching("/// - stderr: ")).Value;

                    state |= SEEN_STDERR;

                    continue;
                }

                break;
            }
        }

        if (output is null && error is null && stdErr is null) {

            var outputPath = path.SetExtension("out");

            var errorOutputPath = path.SetExtension("error");

            var stdErrOutputPath = path.SetExtension("errorOut");

            if (File.Exists(outputPath)) {

                output = File.ReadAllText(outputPath).Replace("\r\n", "\n");
            }

            if (File.Exists(errorOutputPath)) {

                error = File.ReadAllText(errorOutputPath).Replace("\r\n", "\n");
            }

            if (File.Exists(stdErrOutputPath)) {

                stdErr = File.ReadAllText(stdErrOutputPath).Replace("\r\n", "\n");
            }
        }

        return new ErrorOr<(String?, String?, String?)>((output, error, stdErr));
    }

    public static ErrorOrVoid TestSamples(
        String path) {

        if (!Directory.Exists(path)) {

            return new ErrorOrVoid();
        }

        var files = Directory.GetFiles(path);

        if (!files.Any()) {

            return new ErrorOrVoid();
        }

        foreach (var sample in files.OrderBy(x => x)) {

            var ext = Path.GetExtension(sample);

            if (ext == ".neu") {

                // Great, we found test file

                var (expectedOutput, expectedError, expectedStdErr) = FindResults(sample).Value;

                if (expectedOutput is null
                    && expectedError is null
                    && expectedStdErr is null) {

                    // No expectations, skip it

                    continue;
                }

                var og = Console.ForegroundColor;

                Write($"Test: {sample}");

                var filename = Path.GetFileName(sample);

                var name = filename.Substring(0, filename.Length - ext.Length);

                // We have an output to compare to, let's do it.

                var compiler = new Compiler(new List<String>());

                var cppStringOrError = compiler.ConvertToCPP(sample);

                String cppString;

                switch (true) {

                    case var _ when 
                        cppStringOrError.Error is null 
                        && !IsNullOrWhiteSpace(cppStringOrError.Value): {

                        if (!IsNullOrWhiteSpace(expectedError)) {

                            var expectedErrorMsg = expectedError
                                .Replace("\r", "")
                                .Replace("\n", "");

                            throw new Exception($"\r\nTest: {sample}\r\nExpected error not created: {expectedErrorMsg}");
                        }

                        cppString = cppStringOrError.Value;

                        break;
                    }

                    case var _ when
                        cppStringOrError.Error is Error err: {

                        if (!IsNullOrWhiteSpace(expectedError)) {

                            var expectedErrorMsg = expectedError
                                .Replace("\r", "")
                                .Replace("\n", "");

                            var returnedError = err.Content; 

                            if (returnedError?.Contains(expectedErrorMsg) != true) {

                                throw new Exception($"\r\nTest: {sample}\r\nReturned error: {returnedError}\r\nExpected error: {expectedErrorMsg}");
                            }

                            Write($" ......");

                            Console.ForegroundColor = ConsoleColor.Green;

                            Write($" Verified (parse/type check)\n");

                            Console.ForegroundColor = og;

                            ///

                            SuccesfulTests += 1;

                            continue;
                        }
                        else {

                            Write($" ......");

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed\n");

                            Console.ForegroundColor = og;

                            Neu.Program.DisplayError(compiler, err.Content, err.GetSpan());                               

                            return new ErrorOrVoid(err);
                        }
                    }

                    default: {

                        throw new Exception();
                    }
                }

                ///

                var sampleDir = sample.Substring(2, sample.Length - filename.Length - 2);

                var cppDir = $"./Build/{sampleDir}";

                Directory.CreateDirectory(cppDir);

                var cppFilename = $"{cppDir}{name}.cpp";

                File.WriteAllText(cppFilename, cppString);

                var exeExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? ".exe"
                    : ".out";

                var exeName = $"{cppFilename.Substring(0, cppFilename.Length - ext.Length)}{exeExt}";

                var (stdOut, stdErr, success) =
                    Neu.Process.Run(
                        "clang++", 
                        new [] {
                            cppFilename,
                            "-o",
                            exeName,
                            "-include-pch",
                            PchFilename.Value
                        },
                        CommonArgs);

                if (!success) {

                    WriteLine(stdOut);
                    WriteLine(stdErr);

                    throw new Exception();
                }

                var (binStdOut, binStdErr, binSuccess) = 
                    Neu.Process
                        .Run(name: exeName);

                // var baselineText = File.Exists(outputPath)
                //     ? File.ReadAllText(outputPath)
                //     : null;

                // var baselineStdErrText = File.Exists(stdErrOutputPath)
                //     ? File.ReadAllText(stdErrOutputPath)
                //     : null;
                
                var stdErrChecked = false;

                if (!IsNullOrWhiteSpace(expectedStdErr)) {

                    if (!binStdErr.Contains(expectedStdErr)) {

                        throw new Exception(
                            $"\r\n Test (stderr): {path}");
                    }

                    stdErrChecked = true;

                    Write($" ......");

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Verified (std err)\n");

                    Console.ForegroundColor = og;

                    ///

                    SuccesfulTests += 1;
                }

                if (!stdErrChecked || !IsNullOrWhiteSpace(expectedOutput)) {

                    if (!Equals(binStdOut, expectedOutput)) {

                        throw new Exception($"\r\nTests: {path}");
                    }
                    
                    Write($" ......");

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Success\n");

                    Console.ForegroundColor = og;

                    ///

                    SuccesfulTests += 1;
                }
            }
        }

        return new ErrorOrVoid();
    }

    public static Int32 SuccesfulTests = 0;

    public static void Main(String[] args) {

        Console.Clear();

        Compiler.Clean();

        ///

        SuccesfulTests = 0;

        ///

        var start = System.Environment.TickCount;

        switch (true) {

            case var _ when
                args.Length > 0
                && args[0] == "apps": {

                TestApps();

                break;
            }

            case var _ when
                args.Length > 0
                && args[0] == "scratchpad": {

                TestScratchpad();

                break;
            }
            
            case var _ when
                args.Length > 0
                && args[0] == "selfhost": {

                TestSelfHost();

                break;
            }

            default: {

                TestBasics();
                TestControlFlow();
                TestFunctions();
                TestMath();
                TestVariables();
                TestStrings();
                TestArrays();
                TestOptional();
                TestTuples();
                TestStructs();
                TestPointers();
                TestClasses();
                TestBoolean();
                TestRanges();
                TestDictionaries();
                TestGenerics();
                TestEnums();
                TestInlineCPP();
                TestParser();
                TestTypeChecker();
                TestSets();
                TestNamespaces();
                TestWeak();
                TestCodeGen();
                TestWhen();
                TestImports();
                TestSelfHost();

                break;
            }
        }

        WriteLine($"\nCompleted {SuccesfulTests} test{(SuccesfulTests == 1 ? "" : "s")} in {start.Elapsed()} at {DateTime.UtcNow.ToFriendlyLocalTimestamp()}");

        ///

        SuccesfulTests = 0;
    }
}

public static partial class IntFunctions {

    public static double ElapsedSeconds(
        this int start) {

        return ToDouble(Environment.TickCount - start) / 1000.0;
    }

    public static String Elapsed(
        this int start) {

        var seconds = start.ElapsedSeconds();

        if (seconds < 1) {

            return $"{(seconds * 1000.0).ToString("G3")}ms";
        }
        else if (seconds > 60) {

            var r = seconds % 60;

            var m = ToInt32(Math.Floor(seconds / 60));

            return $"{m}m {r.ToString("G3")}s";
        }
        else {

            return $"{seconds.ToString("G3")}s";
        }
    }
}

public static partial class Program {

    public static ErrorOrVoid TestScratchpad() {

        return TestSamples("./Samples/Scratchpad");
    }

    public static ErrorOrVoid TestApps() {

        return TestSamples("./Samples/Apps");
    }

    public static ErrorOrVoid TestBasics() {

        return TestSamples("./Samples/Basics");
    }

    public static ErrorOrVoid TestControlFlow() {

        return TestSamples("./Samples/ControlFlow");
    }

    public static ErrorOrVoid TestFunctions() {

        return TestSamples("./Samples/Functions");
    }

    public static ErrorOrVoid TestMath() {

        return TestSamples("./Samples/Math");
    }

    public static ErrorOrVoid TestVariables() {

        return TestSamples("./Samples/Variables");
    }

    public static ErrorOrVoid TestStrings() {

        return TestSamples("./Samples/Strings");
    }

    public static ErrorOrVoid TestArrays() {

        return TestSamples("./Samples/Arrays");
    }

    public static ErrorOrVoid TestOptional() {

        return TestSamples("./Samples/Optional");
    }

    public static ErrorOrVoid TestTuples() {

        return TestSamples("./Samples/Tuples");
    }

    public static ErrorOrVoid TestStructs() {

        return TestSamples("./Samples/Structs");
    }

    public static ErrorOrVoid TestPointers() {

        return TestSamples("./Samples/Pointers");
    }

    public static ErrorOrVoid TestClasses() {

        return TestSamples("./Samples/Classes");
    }

    public static ErrorOrVoid TestBoolean() {

        return TestSamples("./Samples/Boolean");
    }

    public static ErrorOrVoid TestRanges() {

        return TestSamples("./Samples/Ranges");
    }

    public static ErrorOrVoid TestDictionaries() {

        return TestSamples("./Samples/Dictionaries");
    }

    public static ErrorOrVoid TestGenerics() {

        return TestSamples("./Samples/Generics");
    }

    public static ErrorOrVoid TestEnums() {

        return TestSamples("./Samples/Enums");
    }

    public static ErrorOrVoid TestInlineCPP() {

        return TestSamples("./Samples/InlineCPP");
    }

    public static ErrorOrVoid TestParser() {

        return TestSamples("./Tests/Parser");
    }

    public static ErrorOrVoid TestTypeChecker() {

        return TestSamples("./Tests/TypeChecker");
    }

    public static ErrorOrVoid TestSets() {

        return TestSamples("./Samples/Sets");
    }

    public static ErrorOrVoid TestNamespaces() {

        return TestSamples("./Samples/Namespaces");
    }

    public static ErrorOrVoid TestWeak() {

        return TestSamples("./Samples/Weak");
    }

    public static ErrorOrVoid TestCodeGen() {

        return TestSamples("./Tests/CodeGen");
    }

    public static ErrorOrVoid TestWhen() {

        return TestSamples("./Samples/When");
    }

    public static ErrorOrVoid TestImports() {

        return TestSamples("./Samples/Imports");
    }

    public static ErrorOrVoid TestSelfHost() {

        return TestSamples("./SelfHost");
    }
}
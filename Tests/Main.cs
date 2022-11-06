
namespace NeuTests;

public static partial class Program {

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

                var outputPath = sample.ReplacingExtension(ext, "out");

                var errorOutputPath = sample.ReplacingExtension(ext, "error");

                var stdErrOutputPath = sample.ReplacingExtension(ext, "errorOut");

                if (File.Exists(outputPath) || File.Exists(errorOutputPath) || File.Exists(stdErrOutputPath)) {

                    var og = Console.ForegroundColor;

                    Write($"Test: {sample}");

                    var filename = Path.GetFileName(sample);

                    var name = filename.Substring(0, filename.Length - ext.Length);
                    
                    // We have an output to compare to, let's do it.

                    var compiler = new Compiler(new List<String>());

                    var cppStringOrError = compiler.ConvertToCPP(sample);

                    String? cppString = null;

                    switch (true) {

                        case var _ when 
                            cppStringOrError.Error == null 
                            && !IsNullOrWhiteSpace(cppStringOrError.Value): {

                            if (File.Exists(errorOutputPath)) {

                                var expectedErrorMsg = File.ReadAllText(errorOutputPath).Trim();

                                throw new Exception($"Expected error not created: {expectedErrorMsg}");
                            }

                            cppString = cppStringOrError.Value;

                            break;
                        }

                        case var _ when 
                            cppStringOrError.Error is Error err: {

                            if (File.Exists(errorOutputPath)) {

                                var expectedErrorMsg = File.ReadAllText(errorOutputPath).Trim();

                                var returnedError = err.Content?.Trim() ?? String.Empty;

                                if (!returnedError.Contains(expectedErrorMsg)) {

                                    throw new Exception($"\nTest: {path}\nReturned error: {returnedError}\nExpected error: {expectedErrorMsg}");
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

                    var sampleDir = sample.Substring(2, sample.Length - filename.Length - 2);

                    var cppDir = $"./Build/{sampleDir}";

                    Directory.CreateDirectory(cppDir);

                    var cppFilename = $"{cppDir}{name}.cpp";

                    File.WriteAllText(cppFilename, cppString);

                var exeExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? ".exe"
                    : ".out";

                    var exeName = $"{cppFilename.Substring(0, cppFilename.Length - ext.Length)}.out";

                    var (stdOut, stdErr, success) = 
                        Compiler.Build(
                            compilerPath: "clang++",
                            runtimePath: "Runtime",
                            inputCpp: cppFilename,
                            verbose: false);

                    if (!success) {

                        WriteLine(stdOut);
                        WriteLine(stdErr);

                        throw new Exception();
                    }

                    var (binStdOut, binStdErr, binSuccess) = 
                        Neu.Process
                            .Run(name: exeName);

                    var baselineText = File.Exists(outputPath)
                        ? File.ReadAllText(outputPath)
                        : null;

                    var baselineStdErrText = File.Exists(stdErrOutputPath)
                        ? File.ReadAllText(stdErrOutputPath)
                        : null;
                   
                    var stdErrChecked = false;

                    if (!IsNullOrWhiteSpace(baselineStdErrText)) {

                        if (!binStdErr.Contains(baselineStdErrText)) {

                            throw new Exception(
                                $"\n Test (stderr): {path}");
                        }

                        stdErrChecked = true;

                        Write($" ......");

                        Console.ForegroundColor = ConsoleColor.Green;

                        Write($" Verified (std err)\n");

                        Console.ForegroundColor = og;

                        ///

                        SuccesfulTests += 1;
                    }

                    if (!stdErrChecked || !IsNullOrWhiteSpace(baselineText)) {

                        if (!Equals(binStdOut, baselineText)) {

                            throw new Exception($"\nTests: {path}");
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
}
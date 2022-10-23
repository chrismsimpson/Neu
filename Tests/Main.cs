
namespace NeuTests;

public static partial class Program {

    public static Int32 SuccesfulTests = 0;

    public static void Main(String[] args) {

        Compiler.CleanTests();

        ///

        SuccesfulTests = 0;

        ///

        var start = System.Environment.TickCount;

        if (args.Contains("scratchpad")) {

            TestScratchpad();
        }
        else if (args.Contains("apps")) {

            TestApps();
        }
        else {

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
        }

        WriteLine($"\nCompleted {SuccesfulTests + 1} tests in {start.Elapsed()}");

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

                var name = Path.GetFileNameWithoutExtension(sample);

                ///

                var outputFilename = $"{name}.out";

                var outputPath = $"{path}/{outputFilename}";

                ///

                var errorOutputFilename = $"{name}.error";

                var errorOutputPath = $"{path}/{errorOutputFilename}";

                ///

                var stderrOutputFilename = $"{name}.errorOut";
                
                var stderrOutputFilenamePath = $"{path}/{stderrOutputFilename}";

                ///

                if (File.Exists(outputPath) || File.Exists(errorOutputPath) || File.Exists(stderrOutputFilenamePath)) {

                    // We have an output to compare to, let's do it.
                    
                    var og = Console.ForegroundColor;
                    
                    Write($"Test: {sample} ");

                    ///

                    var compiler = new Compiler();

                    var cppStringOrError = compiler.ConvertToCPP(sample);

                    String? cppString = null;

                    if (cppStringOrError.Value is String c && cppStringOrError.Error == null) {

                        if (File.Exists(errorOutputPath)) {

                            var expectedErrorMsg = File.ReadAllText(errorOutputPath).Trim();

                            throw new Exception($"Expected error not created: {expectedErrorMsg}");
                        }

                        cppString = c;
                    }
                    else {

                        if (File.Exists(errorOutputPath)) {

                            var expectedErrorMsg = File.ReadAllText(errorOutputPath).Trim();

                            var returnedError = cppStringOrError.Error?.Content?.Trim() ?? "";

                            if (!Equals(returnedError, expectedErrorMsg)) {

                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                
                                Write($" `{returnedError}`, expected `{expectedErrorMsg}`\n");

                                Console.ForegroundColor = og;
                            }
                            else {

                                Write($"....................");

                                Console.ForegroundColor = ConsoleColor.Green;

                                Write($" Verified (parse/type check)\n");

                                Console.ForegroundColor = og;

                                ///

                                SuccesfulTests += 1;
                            }

                            continue;
                        }
                    }

                    ///

                    var id = Guid.NewGuid();

                    ///

                    var buildDir = $"./Build";
                    var projBuildDir = $"{buildDir}/{id}";
                    var genDir = $"./Generated";
                    var projGenDir = $"{genDir}/{id}";

                    ///

                    if (cppStringOrError.Error is Error e) {

                        var errStr = e.Content ?? e.ErrorType.ToString();

                        WriteLine(errStr);

                        throw new Exception(errStr);
                    }

                    compiler.Generate(
                        buildDir,
                        projBuildDir,
                        genDir,
                        projGenDir,
                        $"{id}", 
                        cppString ?? throw new Exception());
                    
                    ///

                    var (cmakeGenerateBuildOutput, _, cmakeGenerateBuildExitedSuccessfully) = compiler
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
                            printProgress: true);

                    if (!cmakeGenerateBuildExitedSuccessfully) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to generate build\n");

                        Console.ForegroundColor = og;

                        continue;
                    }

                    ///

                    var (cmakeBuildOutput, _, cmakeBuildExitedSuccessfully) = compiler
                        .BuildWithCMake(
                            projBuildDir,
                            printProgress: true);
                        
                    if (!cmakeBuildExitedSuccessfully) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to build\n");

                        Console.ForegroundColor = og;

                        continue;
                    }

                    ///

                    var (builtOutput, builtErr, builtExitedSuccessfully) = compiler
                        .Process(
                            name: $"{projBuildDir}/Output/output-{id}",
                            arguments: null,
                            printProgress: true);

                    switch (true) {

                        case var _ when
                            builtExitedSuccessfully &&
                            File.Exists(outputPath): {

                            var output = ReadAllText(outputPath);

                            var eq = Equals(builtOutput, output);

                            if (eq) {

                                Console.ForegroundColor = ConsoleColor.Green;

                                Write($" Success\n");

                                Console.ForegroundColor = og;

                                ///

                                SuccesfulTests += 1;
                            }
                            else {

                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                
                                Write($" Mismatched output\n");

                                Console.ForegroundColor = og;
                            }

                            continue;
                        }

                        case var _ when
                            File.Exists(stderrOutputFilenamePath): {

                            var stderrOutput = ReadAllText(stderrOutputFilenamePath);

                            var eq = Equals(builtErr, stderrOutput);

                            if (eq) {

                                Console.ForegroundColor = ConsoleColor.Green;

                                Write($" Verified (std err)\n");

                                Console.ForegroundColor = og;

                                ///

                                SuccesfulTests += 1;
                            }
                            else {

                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                
                                Write($" Mismatched output (std err)\n");

                                Console.ForegroundColor = og;
                            }

                            continue;
                        }

                        default: {

                            throw new Exception("something went wrong");
                        }
                    }
                }
            }
        }

        return new ErrorOrVoid();
    }

    ///

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
}
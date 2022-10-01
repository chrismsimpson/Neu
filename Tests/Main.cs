
namespace NeuTests;

public static partial class Program {

    public static void Main() {

        Compiler.CleanTests();

        ///

        TestBasics();
        TestControlFlow();
        TestFunctions();
        TestMath();
        TestVariables();
        TestStrings();
        TestVectors();
        TestOptional();
        TestTuples();
    }

    public static ErrorOrVoid TestSamples(
        String path) {

        foreach (var sample in Directory.GetFiles(path).OrderBy(x => x)) {

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

                if (File.Exists(outputPath) || File.Exists(errorOutputPath)) {

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

                                Console.ForegroundColor = ConsoleColor.Green;

                                Write($" Verified\n");

                                Console.ForegroundColor = og;
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

                    compiler.Generate(
                        buildDir,
                        projBuildDir,
                        genDir,
                        projGenDir,
                        $"{id}", 
                        cppString ?? throw new Exception());
                    
                    ///

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = compiler
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

                    var (cmakeBuildOutput, cmakeBuildErr) = compiler
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

                    var (builtOutput, builtErr) = compiler
                        .Process(
                            name: $"{projBuildDir}/Output/output-{id}",
                            arguments: null,
                            printProgress: true);

                    if (builtErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to run\n");

                        ///

                        if (!IsNullOrWhiteSpace(builtOutput)) {

                            WriteLine(cmakeBuildOutput);
                        }

                        ///

                        Console.ForegroundColor = og;

                        ///

                        continue;
                    }

                    ///

                    var output = ReadAllText(outputPath);

                    var eq = Equals(builtOutput, output);

                    Write($".");

                    if (!eq) {

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        
                        Write($" Mismatched output\n");

                        Console.ForegroundColor = og;

                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;

                    Write($" Success\n");

                    Console.ForegroundColor = og;
                }
            }
        }

        return new ErrorOrVoid();
    }

    ///

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

    public static ErrorOrVoid TestVectors() {

        return TestSamples("./Samples/Vectors");
    }

    public static ErrorOrVoid TestOptional() {

        return TestSamples("./Samples/Optional");
    }

    public static ErrorOrVoid TestTuples() {

        return TestSamples("./Samples/Tuples");
    }
}
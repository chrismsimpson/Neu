
namespace NeuTests;

public static partial class Program {

    public static void Main() {

        DeleteBuildCruft();

        ///

        TestBasics();
        TestControlFlow();
        TestFunctions();
        TestMath();
        TestVariables();
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

                    // var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = compiler
                    //     .Process(
                    //         name: "cmake",
                    //         arguments: $"{projGenDir} -B {projBuildDir} -G Ninja",
                    //         printProgress: true);

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = compiler
                        .GenerateNinjaCMake(
                            projBuildDir, 
                            projGenDir, 
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

                    // var (cmakeBuildOutput, cmakeBuildErr) = compiler
                    //     .Process(
                    //         name: "cmake",
                    //         arguments: $"--build {projBuildDir}",
                    //         printProgress: true);

                    var (cmakeBuildOutput, cmakeBuildErr) = compiler
                        .BuildWithCMake(
                            projBuildDir,
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

                    var (builtOutput, builtErr) = compiler
                        .Process(
                            name: $"{projBuildDir}/Output/output-{id}",
                            arguments: null,
                            printProgress: true);

                    if (builtErr) {

                        Console.ForegroundColor = ConsoleColor.Red;

                        Write($" Failed to run\n");

                        Console.ForegroundColor = og;

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

    public static void DeleteBuildCruft() {
        
        var directories = Directory.GetDirectories("./");

        foreach (var dir in directories) {

            if (dir.StartsWith("./Build") || dir.StartsWith("./Generated")) {

                foreach (var sub in Directory.GetDirectories(dir)) {

                    var s = sub;

                    if (s.StartsWith("./Build/")) {

                        s = s.Substring(8, s.Length - 8);
                    }

                    if (s.StartsWith("./Generated/")) {

                        s = s.Substring(12, s.Length - 12);
                    }

                    Guid g = Guid.Empty;

                    if (Guid.TryParse(s, out g)) {

                        Directory.Delete(sub, true);
                    }
                }
            }
        }
    }

    // public static (String, bool) Process(
    //     String name,
    //     String? arguments,
    //     bool printProgress = false) {

    //     var processStartInfo = new System.Diagnostics.ProcessStartInfo();

    //     processStartInfo.FileName = name;

    //     if (!IsNullOrWhiteSpace(arguments)) {

    //         processStartInfo.Arguments = arguments;
    //     }

    //     processStartInfo.CreateNoWindow = true;

    //     processStartInfo.UseShellExecute = false;
        
    //     processStartInfo.RedirectStandardError = true;
        
    //     processStartInfo.RedirectStandardOutput = true;

    //     ///

    //     var process = System.Diagnostics.Process.Start(processStartInfo);

    //     if (process == null) {

    //         throw new Exception();
    //     }

    //     ///

    //     var error = false;

    //     ///

    //     var output = new StringBuilder();

    //     process.OutputDataReceived += (sender, e) => {

    //         if (e.Data != null) {

    //             output.AppendLine(e.Data);
    //         }

    //         if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

    //             if (!error && printProgress) {

    //                 Write(".");
    //             }
    //         }
    //     };

    //     process.BeginOutputReadLine();

    //     ///
        
    //     var errOutput = new StringBuilder();

    //     process.ErrorDataReceived += (sender, e) => {

    //         if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

    //             error = true;

    //             ///

    //             errOutput.AppendLine(l);
    //         }
    //     };

    //     process.BeginErrorReadLine();

    //     ///

    //     process.WaitForExit();

    //     process.Close();

    //     ///

    //     return (
    //         error 
    //             ? errOutput.ToString() 
    //             : output.ToString(), 
    //         error);
    // }

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
}
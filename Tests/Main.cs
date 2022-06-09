
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

                    var id = Guid.NewGuid();

                    ///

                    var buildAutoDir = $"./Build";

                    if (!Directory.Exists(buildAutoDir)) {

                        Directory.CreateDirectory(buildAutoDir);
                    }

                    ///

                    var buildDir = $"{buildAutoDir}/{id}";
                    
                    ///

                    var generatedAutoDir = $"./Generated";

                    if (!Directory.Exists(generatedAutoDir)) {

                        Directory.CreateDirectory(generatedAutoDir);
                    }

                    ///

                    var genDir = $"{generatedAutoDir}/{id}";

                    if (!Directory.Exists(genDir)) {

                        Directory.CreateDirectory(genDir);
                    }

                    ///

                    var cmakeProjectFile = $"{genDir}/CMakeLists.txt";

                    if (!File.Exists(cmakeProjectFile)) {

                        File.WriteAllText(cmakeProjectFile, $"project(Generated-{id})\n\ncmake_minimum_required(VERSION 3.21)\n\nset(CMAKE_CXX_STANDARD 20)\n\nadd_subdirectory(Runtime)\nadd_subdirectory(Output)");
                    }

                    ///

                    var runtimeDir = $"{genDir}/Runtime";

                    if (!Directory.Exists(runtimeDir)) {

                        Directory.CreateDirectory(runtimeDir);
                    }

                    ///

                    var runtimeCmakeFile = $"{runtimeDir}/CMakeLists.txt";

                    if (!File.Exists(runtimeCmakeFile)) {

                        File.WriteAllText(runtimeCmakeFile, $"project (Runtime-{id})\n\nadd_library(runtime-{id} runtime.cpp)");
                    }

                    ///

                    var runtimeCppFile = $"{runtimeDir}/runtime.cpp";

                    if (!File.Exists(runtimeCppFile)) {

                        File.WriteAllText(runtimeCppFile, $"#include \"../../../Runtime/lib.h\"\n\nvoid foo() {{\n\n}}");
                    }

                    ///

                    var outputDir = $"{genDir}/Output";

                    if (!Directory.Exists(outputDir)) {

                        Directory.CreateDirectory(outputDir);
                    }

                    ///

                    var outputCmakeFile = $"{outputDir}/CMakeLists.txt";

                    if (!File.Exists(outputCmakeFile)) {

                        File.WriteAllText(outputCmakeFile, $"project (Output-{id})\n\nadd_executable(output-{id} output.cpp)\n\ntarget_link_libraries(output-{id} runtime-{id})");
                    }

                    ///

                    var cppFilename = $"{outputDir}/output.cpp";

                    File.WriteAllText(cppFilename, cppString);

                    ///

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = Process(
                        name: "cmake",
                        arguments: $"{genDir} -B {buildDir} -G Ninja",
                        printProgress: true);

                    if (cmakeGenerateBuildErr) {

                        if (!IsNullOrWhiteSpace(cmakeGenerateBuildOutput)) {

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed to generated build\n");

                            Console.ForegroundColor = og;
                        }

                        continue;
                    }

                    var (cmakeBuildOutput, cmakeBuildErr) = Process(
                        name: "cmake",
                        arguments: $"--build {buildDir}",
                        printProgress: true);
                        
                    if (cmakeBuildErr) {

                        if (!IsNullOrWhiteSpace(cmakeBuildOutput)) {

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed to build\n");

                            Console.ForegroundColor = og;
                        }

                        continue;
                    }

                    ///

                    var (builtOutput, builtErr) = Process(
                        name: $"{buildDir}/Output/output-{id}",
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

            // if (dir.StartsWith("./BuildAuto")
            //     || dir.StartsWith("./Build-")
            //     || dir.StartsWith("./GeneratedAuto")
            //     || dir.StartsWith("./Generated-")) {

            //     Directory.Delete(dir, true);
            // }

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

    public static (String, bool) Process(
        String name,
        String? arguments,
        bool printProgress = false) {

        var processStartInfo = new System.Diagnostics.ProcessStartInfo();

        processStartInfo.FileName = name;

        if (!IsNullOrWhiteSpace(arguments)) {

            processStartInfo.Arguments = arguments;
        }

        processStartInfo.CreateNoWindow = true;

        processStartInfo.UseShellExecute = false;
        
        processStartInfo.RedirectStandardError = true;
        
        processStartInfo.RedirectStandardOutput = true;

        ///

        var process = System.Diagnostics.Process.Start(processStartInfo);

        if (process == null) {

            throw new Exception();
        }

        ///

        var error = false;

        ///

        var output = new StringBuilder();

        process.OutputDataReceived += (sender, e) => {

            if (e.Data != null) {

                output.AppendLine(e.Data);
            }

            if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

                if (!error && printProgress) {

                    Write(".");
                }
            }
        };

        process.BeginOutputReadLine();

        ///
        
        var errOutput = new StringBuilder();

        process.ErrorDataReceived += (sender, e) => {

            if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

                error = true;

                ///

                errOutput.AppendLine(l);
            }
        };

        process.BeginErrorReadLine();

        ///

        process.WaitForExit();

        process.Close();

        ///

        return (
            error 
                ? errOutput.ToString() 
                : output.ToString(), 
            error);
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
}
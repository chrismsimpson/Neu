
namespace NeuTests;

public static partial class Program {

    public static void Main() {

        DeleteBuildCruft();

        ///

        Build();
    
        ///

        DeleteBuildCruft();
    }

    public static void Build() {

        var n = Directory.GetFiles("./Samples");

        foreach (var path in n.OrderBy(x => x)) {

            var ext = Path.GetExtension(path);

            if (ext == ".neu") {

                var name = Path.GetFileNameWithoutExtension(path);

                var outputFilename = $"{name}.out";

                var outputPath = $"./Samples/{outputFilename}";

                if (File.Exists(outputPath)) {

                    var compiler = new Compiler();

                    var cppStringOrError = compiler.ConvertToCPP(path);

                    if (cppStringOrError.Error != null) {

                        throw new Exception();
                    }

                    var cppString = cppStringOrError.Value ?? throw new Exception();

                    var id = Guid.NewGuid();

                    ///

                    var cmakeDir = $"./Generated-{id}";

                    if (!Directory.Exists(cmakeDir)) {

                        Directory.CreateDirectory(cmakeDir);
                    }

                    ///

                    var cmakeProjectFile = $"{cmakeDir}/CMakeLists.txt";

                    if (!File.Exists(cmakeProjectFile)) {

                        File.WriteAllText(cmakeProjectFile, $"project(Generated-{id})\n\ncmake_minimum_required(VERSION 3.21)\n\nset(CMAKE_CXX_STANDARD 20)\n\nadd_subdirectory(Runtime)\nadd_subdirectory(Output)");
                    }

                    ///

                    var runtimeDir = $"{cmakeDir}/Runtime";

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

                        File.WriteAllText(runtimeCppFile, $"#include \"../../Runtime/lib.h\"\n\nvoid foo() {{\n\n}}");
                    }

                    ///

                    var outputDir = $"{cmakeDir}/Output";

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
                    
                    var og = Console.ForegroundColor;
                    
                    Write($"Test: {path} ");

                    ///

                    var (cmakeGenerateBuildOutput, cmakeGenerateBuildErr) = Process(
                        name: "cmake",
                        arguments: $"Generated-{id} -B Build-{id} -G Ninja",
                        printProgress: true);

                    if (cmakeGenerateBuildErr) {

                        if (!IsNullOrWhiteSpace(cmakeGenerateBuildOutput)) {

                            Console.ForegroundColor = ConsoleColor.Red;

                            Write($" Failed to generated build\n");

                            Console.ForegroundColor = og;
                        }

                        continue;
                    }

                    ///

                    var (cmakeBuildOutput, cmakeBuildErr) = Process(
                        name: "cmake",
                        arguments: $"--build Build-{id}",
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
                        name: $"./Build-{id}/Output/output-{id}",
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
    }

    public static void DeleteBuildCruft() {
        
        var directories = Directory.GetDirectories("./");

        foreach (var dir in directories) {

            if (dir.StartsWith("./Build-") || dir.StartsWith("./Generated-")) {

                Directory.Delete(dir, true);
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
}
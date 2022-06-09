
namespace Neu;

public partial class Compiler {

    public void Generate(
        String id,
        String cppString) {

        var buildDir = $"./Build";
        var projBuildDir = $"{buildDir}/{id}";
        var genDir = $"./Generated";
        var projGenDir = $"{genDir}/{id}";

        this.Generate(
            buildDir,
            projBuildDir,
            genDir,
            projGenDir,
            id,
            cppString);
    }

    public void Generate(
        String buildDir,
        String projBuildDir,
        String genDir,
        String projGenDir,
        String id,
        String cppString) {

        if (!Directory.Exists(buildDir)) {

            Directory.CreateDirectory(buildDir);
        }

        ///

        if (!Directory.Exists(genDir)) {

            Directory.CreateDirectory(genDir);
        }

        ///

        if (!Directory.Exists(projGenDir)) {

            Directory.CreateDirectory(projGenDir);
        }

        ///

        var projCMakeFile = $"{projGenDir}/CMakeLists.txt";

        if (!File.Exists(projCMakeFile)) {

            File.WriteAllText(projCMakeFile, $"project(Generated-{id})\n\ncmake_minimum_required(VERSION 3.21)\n\nset(CMAKE_CXX_STANDARD 20)\n\nadd_subdirectory(Runtime)\nadd_subdirectory(Output)");
        }

        ///

        var projRuntimeDir = $"{projGenDir}/Runtime";

        if (!Directory.Exists(projRuntimeDir)) {

            Directory.CreateDirectory(projRuntimeDir);
        }

        ///

        var projRuntimeCMakeFile = $"{projRuntimeDir}/CMakeLists.txt";

        if (!File.Exists(projRuntimeCMakeFile)) {

            File.WriteAllText(projRuntimeCMakeFile, $"project (Runtime-{id})\n\nadd_library(runtime-{id} runtime.cpp)");
        }

        ///

        var projRuntimeCppFile = $"{projRuntimeDir}/runtime.cpp";

        if (!File.Exists(projRuntimeCppFile)) {

            File.WriteAllText(projRuntimeCppFile, $"#include \"../../../Runtime/lib.h\"\n\nvoid foo() {{\n\n}}");
        }

        ///

        var projOutputDir = $"{projGenDir}/Output";

        if (!Directory.Exists(projOutputDir)) {

            Directory.CreateDirectory(projOutputDir);
        }

        ///

        var projOutputCMakeFile = $"{projOutputDir}/CMakeLists.txt";

        if (!File.Exists(projOutputCMakeFile)) {

            File.WriteAllText(projOutputCMakeFile, $"project (Output-{id})\n\nadd_executable(output-{id} output.cpp)\n\ntarget_link_libraries(output-{id} runtime-{id})");
        }

        ///

        var projOutputCppFile = $"{projOutputDir}/output.cpp";

        File.WriteAllText(projOutputCppFile, cppString);
    }

    ///

    public (String, bool) Process(
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
        
        processStartInfo.RedirectStandardOutput = true;
        
        processStartInfo.RedirectStandardError = true;

        ///

        var process = System.Diagnostics.Process.Start(processStartInfo);

        if (process == null) {

            throw new Exception();
        }

        ///

        var output = new StringBuilder();

        process.OutputDataReceived += (sender, e) => {

            if (e.Data != null) {

                output.AppendLine(e.Data);
            }

            if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

                if (printProgress) {

                    Write(".");
                }
            }
        };

        process.BeginOutputReadLine();

        ///
        
        var errOutput = new StringBuilder();

        process.ErrorDataReceived += (sender, e) => {

            if (e.Data?.Trim() is String l && !IsNullOrWhiteSpace(l)) {

                errOutput.AppendLine(l);
            }
        };

        process.BeginErrorReadLine();

        ///

        process.WaitForExit();

        ///

        var error = process.ExitCode != 0; // Cannot get this after .Close()

        ///
        
        process.Close();

        ///

        return (
            error 
                ? errOutput.ToString() 
                : output.ToString(), 
            error);
    }

    ///

    public (String, bool) GenerateNinjaCMake(
        String projBuildDir,
        String projGenDir,
        bool printProgress = false) {

        return this
            .Process(
                name: "cmake",
                arguments: $"{projGenDir} -B {projBuildDir} -G Ninja",
                printProgress: printProgress);
    }

    ///

    public (String, bool) BuildWithCMake(
        String projBuildDir,
        bool printProgress = false) {

        return this
            .Process(
                name: "cmake",
                arguments: $"--build {projBuildDir}",
                printProgress: printProgress);
    }

    ///

    public static void CleanTests() {

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

    public static void Clean() {

        var directories = Directory.GetDirectories("./");

        foreach (var dir in directories) {

            if (dir.StartsWith("./Build") || dir.StartsWith("./Generated")) {

                foreach (var sub in Directory.GetDirectories(dir)) {

                    Directory.Delete(sub, true);
                }
            }
        }
    }
}
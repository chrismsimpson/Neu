
namespace Neu;

public static partial class Process {

    public static (String StandardOutput, String StandardError, bool ExitedSuccessfully) Run(
        String name,
        String? arguments = null,
        Action<String>? dataReceived = null,
        Action<String>? errorReceived = null) {

        var startInfo = new ProcessStartInfo();

        startInfo.FileName = name;

        if (!IsNullOrWhiteSpace(arguments)) {

            startInfo.Arguments = arguments;
        }

        startInfo.CreateNoWindow = true;

        startInfo.UseShellExecute = false;

        startInfo.RedirectStandardOutput = true;

        startInfo.RedirectStandardError = true;

        ///

        var process = System.Diagnostics.Process.Start(startInfo);

        if (process == null) {

            throw new Exception();
        }

        ///

        var standardOutput = new StringBuilder();

        process.OutputDataReceived += (sender, e) => {

            if (e.Data != null)  {

                standardOutput.AppendLine(e.Data);

                dataReceived?.Invoke(e.Data);
            }
        };

        process.BeginOutputReadLine();

        ///

        var standardError = new StringBuilder();

        process.ErrorDataReceived += (sender, e) => {

            if (e.Data != null) {

                standardError.AppendLine(e.Data);

                errorReceived?.Invoke(e.Data);
            }
        };

        process.BeginErrorReadLine();

        ///

        process.WaitForExit();

        ///

        var exitedSuccessfully = process.ExitCode == 0;

        ///

        process.Close();
        
        ///

        return (
            standardOutput.ToString(),
            standardError.ToString(),
            exitedSuccessfully);
    }
}
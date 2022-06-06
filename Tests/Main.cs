
namespace NeuTests;

public static partial class Program {

    public static void Main() {

        var n = Directory.GetFiles("./Samples");

        foreach (var path in n) {

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

                    // var cppFilename = $

                    var cppFilename = $"./Generated/Tests/output{id}.cpp";

                    File.WriteAllText(cppFilename, cppString);

                    WriteLine($"Test: {path}");

                    ///

                    
                }
            }

            // WriteLine($"file: {f}, ext: {ext}");
        }

        // WriteLine($"hello foo");
    }
}

namespace Neu;

public partial class Compiler {

    public static void Clean() {

        var directories = Directory.GetDirectories("./");

        foreach (var dir in directories) {

            if (dir.StartsWith("./Build")) {

                foreach (var sub in Directory.GetDirectories(dir)) {

                    Directory.Delete(sub, true);
                }

                foreach (var file in Directory.GetFiles(dir)) {

                    var ext = Path.GetExtension(file);

                    if (ext == ".cpp" || ext == ".out") {

                        File.Delete(file);
                    }
                }
            }
        }
    }
}
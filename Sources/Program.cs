
namespace Neu;

public static partial class Program {

    public static void Main(
        String[] args) {

        var parser = new Compiler();

        ///

        foreach (var arg in args) {

            var n = parser.Compile(arg);

            
            
        }
    }
}
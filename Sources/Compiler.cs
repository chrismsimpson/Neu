
namespace Neu;

public partial class Compiler {

    public List<(String, byte[])> RawFiles { get; init; }

    ///

    public Compiler()
        : this(new List<(String, byte[])>()) {
    }

    public Compiler(
        List<(String, byte[])> rawFiles) {

        this.RawFiles = rawFiles;
    }
}

///

public partial class Compiler {

    public void Compile(
        String filename) {

        var contents = ReadAllBytes(filename);
        
        this.RawFiles.Add((filename, contents));

        

    }
}
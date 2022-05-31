
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

    public ErrorOrVoid Compile(
        String filename) {

        var contents = ReadAllBytes(filename);
        
        this.RawFiles.Add((filename, contents));

        var lexed = LexerFunctions.Lex(ToUInt64(this.RawFiles.Count - 1), this.RawFiles[this.RawFiles.Count - 1].Item2);

        return new ErrorOrVoid();
    }
}
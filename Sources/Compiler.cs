
namespace Neu;

public partial class Compiler {

    public List<(String, byte[])> RawFiles { get; init; }

    public List<(String, CheckedFile)> CheckedFiles { get; init; }

    ///

    // public Compiler()
    //     : this(
    //         new List<(String, byte[])>(), 
    //         new List<(String, CheckedFile)>()) {
    // }

    public Compiler() {

        var prelude = Compiler.Prelude();

        var rawFiles = new List<(String, byte[])>();

        var checkedFiles = new List<(String, CheckedFile)>();

        // Not sure where to put prelude, but we're hoping its parsing is infallible

        rawFiles.Add(("<prelude>", prelude));

        // Compile the prelude

        var (lexed, _) = LexerFunctions.Lex(rawFiles.Count - 1, rawFiles[rawFiles.Count - 1].Item2);
        var (file, _) = ParserFunctions.ParseFile(lexed);
        var (chkdFile, _) = TypeCheckerFunctions.TypeCheckFile(file, new CheckedFile());

        checkedFiles.Add(("<prelude>", chkdFile));

        this.RawFiles = rawFiles;
        this.CheckedFiles = checkedFiles;
    }

    // public Compiler(
    //     List<(String, byte[])> rawFiles,
    //     List<(String, CheckedFile)> checkedFiles) {

    //     this.RawFiles = rawFiles;
    //     this.CheckedFiles = checkedFiles;
    // }
}

///

public partial class Compiler {

    public ErrorOr<String> ConvertToCPP(String filename) {

        var contents = ReadAllBytes(filename);
        
        this.RawFiles.Add((filename, contents));

        var (lexed, lexErr) = LexerFunctions.Lex(
            this.RawFiles.Count - 1, 
            this.RawFiles[this.RawFiles.Count - 1].Item2);

        switch (lexErr) {

            case Error e: {

                return new ErrorOr<String>(e);
            }

            ///

            default: {

                break;
            }
        }

        var (parsedFile, parseErr) = ParserFunctions.ParseFile(lexed);

        switch (parseErr) {

            case Error e: {

                return new ErrorOr<String>(e);
            }

            ///

            default: {

                break;
            }
        }

        ///

        var (checkedFile, checkErr) = TypeCheckerFunctions.TypeCheckFile(parsedFile, this.CheckedFiles[0].Item2);

        switch (checkErr) {

            case Error e: {

                return new ErrorOr<String>(e);
            }

            default: {

                break;
            }
        }

        // TODO: do something with this

        this.CheckedFiles.Add((filename, checkedFile));

        return new ErrorOr<String>(this.CodeGen(checkedFile));
    }
    
    public ErrorOrVoid Compile(
        String filename) {

        var cppStringOrError = this.ConvertToCPP(filename);

        if (cppStringOrError.Error != null) {

            // throw new Exception();

            return new ErrorOrVoid(cppStringOrError.Error);
        }

        var cppString = cppStringOrError.Value ?? throw new Exception();

        ///

        var id = Path.GetFileNameWithoutExtension(filename);

        ///

        this.Generate(id, cppString);

        ///

        return new ErrorOrVoid();
    }

    ///

    public byte[] GetFileContents(FileId fileId) {

        return this.RawFiles[fileId].Item2;
    }

    ///

    public static byte[] Prelude() {

        return UTF8.GetBytes(@"
extern class String {
    func split(this, anon c: CChar) -> [String] { }
    func characters(this) -> raw CChar { }
    func toLowercase(this) -> String { }
    func toUppercase(this) -> String { }
    func isEmpty(this) -> Bool { }
    func length(this) -> Int64 { }
}

extern class RefVector {
    func size(this) -> Int64 { }
    fun resize(var this, anon size: UInt64) { }
}

");
    }
}
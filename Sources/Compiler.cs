
namespace Neu;

public partial class Compiler {

    public List<(String, byte[])> RawFiles { get; init; }

    public List<(String, CheckedFile)> CheckedFiles { get; init; }

    ///

    public Compiler()
        : this(
            new List<(String, byte[])>(), 
            new List<(String, CheckedFile)>()) {
    }

    public Compiler(
        List<(String, byte[])> rawFiles,
        List<(String, CheckedFile)> checkedFiles) {

        this.RawFiles = rawFiles;
        this.CheckedFiles = checkedFiles;
    }
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

        var (checkedFile, checkErr) = TypeCheckerFunctions.TypeCheckFile(parsedFile);

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

        return new ErrorOr<String>(this.Translate(checkedFile));
    }
    
    public ErrorOrVoid Compile(
        String filename) {

        var cppStringOrError = this.ConvertToCPP(filename);

        if (cppStringOrError.Error != null) {

            throw new Exception();
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
}
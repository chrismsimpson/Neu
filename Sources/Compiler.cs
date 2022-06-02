
namespace Neu;

public partial class Compiler {

    public List<(String, byte[])> RawFiles { get; init; }

    public List<(String, ParsedFile)> ParsedFiles { get; init; }

    ///

    public Compiler()
        : this(new List<(String, byte[])>(), new List<(String, ParsedFile)>()) {
    }

    public Compiler(
        List<(String, byte[])> rawFiles,
        List<(String, ParsedFile)> parsedFiles) {

        this.RawFiles = rawFiles;
        this.ParsedFiles = parsedFiles;
    }
}

///

public partial class Compiler {

    public ErrorOrVoid Compile(
        String filename) {

        var contents = ReadAllBytes(filename);
        
        this.RawFiles.Add((filename, contents));

        var (lexed, lexErr) = LexerFunctions.Lex(
            this.RawFiles.Count - 1, 
            this.RawFiles[this.RawFiles.Count - 1].Item2);

        switch (lexErr) {

            case Error e: {

                return new ErrorOrVoid(e);
            }

            ///

            default: {

                break;
            }
        }

        var (file, parseErr) = ParserFunctions.ParseFile(lexed);

        switch (parseErr) {

            case Error e: {

                return new ErrorOrVoid(e);
            }

            ///

            default: {

                break;
            }
        }

        var cppFile = this.Translate(file);

        WriteAllText("./Generated/Output/output.cpp", cppFile);

        // TODO: do something with this

        this.ParsedFiles.Add((filename, file));

        return new ErrorOrVoid();
    }

    public byte[] GetFileContents(FileId fileId) {

        return this.RawFiles[fileId].Item2;
    }
}
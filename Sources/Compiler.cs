
namespace Neu;

public partial class Compiler {

    public const Int32 UnknownTypeId    = 0;
    public const Int32 VoidTypeId       = 1;
    public const Int32 BoolTypeId       = 2;

    public const Int32 Int8TypeId       = 3;
    public const Int32 Int16TypeId      = 4;
    public const Int32 Int32TypeId      = 5;
    public const Int32 Int64TypeId      = 6;

    public const Int32 UInt8TypeId      = 7;
    public const Int32 UInt16TypeId     = 8;
    public const Int32 UInt32TypeId     = 9;
    public const Int32 UInt64TypeId     = 10;

    public const Int32 FloatTypeId      = 11;
    public const Int32 DoubleTypeId     = 12;

    public const Int32 CCharTypeId      = 13;
    public const Int32 CIntTypeId       = 14;

    // Note: keep StringTypeId last as it is how we know how many slots to pre-fill

    public const Int32 StringTypeId     = 15;

    public List<(String, byte[])> RawFiles { get; init; }

    ///

    public Compiler() {

        this.RawFiles = new List<(String, byte[])>();
    }

    public void IncludePrelude(
        Project project) {

        // First, let's make types for all the builtin types
        // This order *must* match the order of the constants the typechecker expects

        for (var i = 0; i < (Compiler.StringTypeId + 1); i++) {

            project.Types.Add(new Builtin());
        }

        var prelude = Compiler.Prelude();

        // Not sure where to put prelude, but we're hoping its parsing is infallible

        this.RawFiles.Add(("<prelude>", prelude));

        // Compile the prelude

        var (lexed, _) = LexerFunctions.Lex(
            this.RawFiles.Count - 1, 
            this.RawFiles[this.RawFiles.Count - 1].Item2);

        var (file, _) = ParserFunctions.ParseFile(lexed);

        // Scope ID 0 is the global project-level scope that all files can see

        TypeCheckerFunctions.TypeCheckFile(file, 0, project);
    }
}

///

public partial class Compiler {

    public ErrorOr<String> ConvertToCPP(String filename) {

        var project = new Project();

        this.IncludePrelude(project);

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

        var scope = new Scope(0);

        project.Scopes.Add(scope);

        var fileScopeId = project.Scopes.Count - 1;

        var checkErr = TypeCheckerFunctions.TypeCheckFile(parsedFile, fileScopeId, project);

        switch (checkErr) {

            case Error e: {

                return new ErrorOr<String>(e);
            }

            default: {

                break;
            }
        }

        // Hardwire to first file for now

        return new ErrorOr<String>(this.CodeGen(
            project, 
            project.Scopes[fileScopeId]));
    }
    
    public ErrorOrVoid Compile(
        String filename) {

        var cppStringOrError = this.ConvertToCPP(filename);

        if (cppStringOrError.Error != null) {

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

extern class RefVector<T> {
    func size(this) -> Int64 { }
    func resize(var this, anon size: Int64) { }
}

extern class Optional<T> { }

extern class Tuple { }

extern class Range { }

");
    }
}
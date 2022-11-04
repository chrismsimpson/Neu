
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

    public const Int32 UIntTypeId       = 15;
    public const Int32 IntTypeId        = 16;

    // Note: keep StringTypeId last as it is how we know how many slots to pre-fill

    public const Int32 StringTypeId     = 17;

    public List<String> IncludePaths { get; init; }

    public List<(String, byte[])> RawFiles { get; init; }

    ///

    public Compiler() {

        this.IncludePaths = new List<String>();

        this.RawFiles = new List<(String, byte[])>();
    }

    ///

    public (ParsedNamespace, Error?) IncludeFile(
        String path) {

        var contents = File.ReadAllBytes(path);

        return this.IncludeFile(path, contents);
    }

    public (ParsedNamespace, Error?) IncludeFile(
        String path,
        byte[] contents) {

        this.RawFiles.Add((path, contents));

        var (lexed, lexErr) = LexerFunctions.Lex(
            this.RawFiles.Count - 1, 
            this.RawFiles[this.RawFiles.Count - 1].Item2);

        if (lexErr is Error le) {

            return (new ParsedNamespace(), le);
        }

        var index = 0;
        
        return ParserFunctions.ParseNamespace(lexed, ref index, this);
    }

    public (ParsedNamespace, Error?) FindAndIncludeModule(
        String moduleName,
        Span span) {

        foreach (var path in this.IncludePaths) {

            var filename = Path.Combine(path, $"{moduleName}.neu");

            if (File.Exists(filename)) {

                return this.IncludeFile(filename);
            }
        }

        return (
            new ParsedNamespace(),
            new ParserError(
                $"Module '{moduleName}' not found",
                span));
    }

    public Error? IncludePrelude(
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

        var index = 0;

        var (file, _) = ParserFunctions.ParseNamespace(lexed, ref index, this);

        // Scope ID 0 is the global project-level scope that all files can see

        return TypeCheckerFunctions.TypeCheckNamespace(file, 0, project);
    }

    public ErrorOr<String> ConvertToCPP(
        String filename) {

        var project = new Project();

        var err = this.IncludePrelude(project);

        if (err is Error preludeErr) {

            return new ErrorOr<String>(preludeErr);
        }

        Trace($"-----------------------------");

        // TODO: We also want to be able to accept include paths from CLI

        var parentDir = Directory
            .GetParent(filename)?
            .FullName
            ?? throw new Exception("Cannot find parent directory of file");

        this.IncludePaths.Add(parentDir);

        var (parsedFile, parseErr) = this.IncludeFile(filename);

        if (parseErr is Error pe) {

            return new ErrorOr<String>(pe);
        }

        ///

        var scope = new Scope(0);

        project.Scopes.Add(scope);

        var fileScopeId = project.Scopes.Count - 1;

        var _nsErr = TypeCheckerFunctions.TypeCheckNamespace(parsedFile, fileScopeId, project);

        if (_nsErr is Error nsErr) {

            return new ErrorOr<String>(nsErr);
        }

        if (CheckCodeGenPreconditions(project) is Error preCondErr) {

            return new ErrorOr<String>(preCondErr);
        }

        // Hardwire to first file for now

        return new ErrorOr<String>(
            CodeGenFunctions.CodeGen(
                project, 
                project.Scopes[fileScopeId]));
    }

    public byte[] GetFileContents(
        Int32 fileId) {

        return this.RawFiles[fileId].Item2;
    }

    public String GetFileName(
        Int32 fileId) {

        return this.RawFiles[fileId].Item1;
    }

    public static byte[] Prelude() {

        return File.ReadAllBytes("./Runtime/prelude.neu");
    }

    public static Error? CheckCodeGenPreconditions(
        Project project) {

        // Make sure all functions have a known return type

        foreach (var func in project.Functions) {

            if (func.ReturnTypeId == Compiler.UnknownTypeId) {

                return new TypeCheckError(
                    $"Could not infer the return type of function '{func.Name}', please explicitly specify it",
                    (func.Function
                        ?? throw new Exception("Typechecking non-parsed function")).NameSpan);
            }
        }

        return null;
    }
}
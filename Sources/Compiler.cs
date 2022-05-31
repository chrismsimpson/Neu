
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

        var lexed = LexerFunctions.Lex(
            this.RawFiles.Count - 1, 
            this.RawFiles[this.RawFiles.Count - 1].Item2);

        var fileOrError = ParserFunctions.ParseFile(lexed);

        if (fileOrError.Error != null) {

            throw new Exception();
        }

        var file = fileOrError.Value ?? throw new Exception();

        var cppFile = Translate(file);

        WriteAllText("output.cpp", cppFile);

        // TODO: do something with this

        this.ParsedFiles.Add((filename, file));

        return new ErrorOrVoid();
    }

    public String Translate(ParsedFile file) {

        var output = new StringBuilder();

        output.Append("#include <stdio.h>\n");

        foreach (var fun in file.Functions) {

            var funOutput = TranslateFunction(fun);

            output.Append(funOutput);

            output.Append("\n");
        }

        return output.ToString();
    }

    public String TranslateFunction(Function fun) {

        var output = new StringBuilder();

        output.Append("void ");

        output.Append(fun.Name);

        output.Append("()");

        var block = TranslateBlock(fun.Block);

        output.Append(block);

        return output.ToString();
    }

    public String TranslateBlock(Block block) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var expr in block.Statements) {

            var e = TranslateExpr(expr);

            output.Append(e);

            output.Append(";\n");
        }

        output.Append("}\n");

        return output.ToString();
    }

    public String TranslateExpr(Expression expr) {

        var output = new StringBuilder();

        switch (expr) {

            case QuotedStringExpression qs: {
                
                output.Append("\"");
                output.Append(qs.Value);
                output.Append("\"");

                break;
            }

            case CallExpression ce: {

                if (ce.Call.Name == "printLine") {

                    output.Append("printf");
                }
                else {

                    output.Append(ce.Call.Name);
                }

                output.Append("(");

                foreach (var parameter in ce.Call.Parameters) {

                    output.Append(TranslateExpr(parameter.Item2));
                }

                output.Append(")");

                break;
            }

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }

    public byte[] GetFileContents(FileId fileId) {

        return this.RawFiles[fileId].Item2;
    }
}
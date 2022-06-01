
namespace Neu;

public partial class Call {

    public String Name { get; set; }

    public List<(String, Expression)> Args { get; init; }

    ///

    public Call()
        : this(String.Empty, new List<(string, Expression)>()) { }

    public Call(
        String name,
        List<(String, Expression)> args) {

        this.Name = name;
        this.Args = args;
    }
}

public partial class Span {

    public FileId FileId { get; init; }

    public int Start { get; init; }
    
    public int End { get; init; }

    ///

    public Span(
        FileId fileId,
        int start,
        int end) {

        this.FileId = fileId;
        this.Start = start;
        this.End = end;
    }
}

///

public partial class QuotedStringToken: Token {

    public String Value { get; init; }

    ///

    public QuotedStringToken(
        String value,
        Span span)
        : base(span) {

        this.Value = value;
    }
}

public partial class NumberToken: Token {

    public Int64 Value { get; init; }

    ///

    public NumberToken(
        Int64 value,
        Span span)
        : base(span) {

        this.Value = value;
    }
}

public partial class NameToken: Token {

    public String Value { get; init; }

    ///

    public NameToken(
        String value,
        Span span)
        : base(span) {

        this.Value = value;
    }
}

public partial class SemicolonToken: Token {

    public SemicolonToken(Span span)
        : base(span) { }
}

public partial class LParenToken: Token {

    public LParenToken(Span span)
        : base(span) { }
}

public partial class RParenToken: Token {

    public RParenToken(Span span)
        : base(span) { }
}

public partial class LCurlyToken: Token {

    public LCurlyToken(Span span)
        : base(span) { }
}

public partial class RCurlyToken: Token {

    public RCurlyToken(Span span)
        : base(span) { }
}

public partial class CommaToken: Token {

    public CommaToken(Span span)
        : base(span) { }
}

public partial class EolToken: Token {

    public EolToken(Span span)
        : base(span) { }
}

public partial class EofToken: Token {

    public EofToken(Span span)
        : base(span) { }
}

///

public partial class Token {

    public Span Span { get; init; }

    ///

    public Token(
        Span span) {

        this.Span = span;
    }
}

///

public partial class ParsedFile {

    public List<Function> Functions { get; init; }

    ///

    public ParsedFile()
        : this(new List<Function>()) { }

    public ParsedFile(
        List<Function> functions) {

        this.Functions = functions;
    }
}

///

public partial class Function {

    public String Name { get; init; }

    public List<Parameter> Parameters { get; init; }

    public Block Block { get; init; }

    ///

    public Function(
        String name,
        List<Parameter> parameters,
        Block block) {

        this.Name = name;
        this.Parameters = parameters;
        this.Block = block;
    }
}

///

public partial class Parameter {

    public String Name { get; init; }

    ///

    public Parameter(
        String name) {

        this.Name = name;
    }
}

///

public partial class Statement {

    public Statement() { }
}

///

public partial class Block {

    public List<Statement> Statements { get; init; }

    ///

    public Block()
        : this(new List<Statement>()) {}

    public Block(
        List<Statement> statements) {

        this.Statements = statements;
    }
}

///

public partial class Expression: Statement {

    public Expression() : base() { }
}

public partial class CallExpression: Expression {

    public Call Call { get; init; }

    ///

    public CallExpression(
        Call call)
        : base() {

        this.Call = call;
    }
}

public partial class Int64Expression: Expression {

    public Int64 Value { get; init; }

    ///

    public Int64Expression(
        Int64 value)
        : base() {

        this.Value = value;
    }
}

public partial class QuotedStringExpression: Expression {

    public String Value { get; init; }

    ///

    public QuotedStringExpression(
        String value)
        : base() { 
        
        this.Value = value;
    }
}

public partial class DeferStatement: Statement {

    public Block Block { get; init; }

    ///

    public DeferStatement(
        Block block)
        : base() { 

        this.Block = block;
    }
}

///

public static partial class ParserFunctions {

    public static ErrorOr<ParsedFile> ParseFile(
        List<Token> tokens) {

        var parsedFile = new ParsedFile();

        int index = 0;

        var cont = true;

        while (index < tokens.Count && cont) {

            var token = tokens.ElementAt(index);

            switch (token) {

                case NameToken name when name.Value == "func":

                    var funcOrError = ParseFunction(tokens, ref index);

                    if (funcOrError.Error != null) {

                        throw new Exception();
                    }

                    var func = funcOrError.Value ?? throw new Exception();

                    parsedFile.Functions.Add(func);
                    
                    break;

                ///

                case NameToken _:

                    return new ErrorOr<ParsedFile>("unexpected keyword", token.Span);

                ///

                case EolToken _:

                    index += 1;

                    break;

                ///

                case EofToken _:

                    cont = false;

                    break;

                ///

                case Token _:

                    return new ErrorOr<ParsedFile>("unexpected token (expected keyword)", token.Span);
            }
        }

        return new ErrorOr<ParsedFile>(parsedFile);        
    }

    public static ErrorOr<Function> ParseFunction(
        List<Token> tokens,
        ref int index) {

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the function

            switch (tokens.ElementAt(index)) {

                case NameToken name:

                    index += 3;

                    var blockOrError = ParseBlock(tokens, ref index);

                    if (blockOrError.Error != null) {

                        throw new Exception();
                    }

                    var block = blockOrError.Value ?? throw new Exception();

                    return new ErrorOr<Function>(new Function(name.Value, new List<Parameter>(), block));

                ///

                default:    

                    return new ErrorOr<Function>("expected function name", tokens.ElementAt(index).Span);
            }
        }
        else {

            return new ErrorOr<Function>("incomplete function definition", tokens.ElementAt(index).Span);
        }
    }

    public static ErrorOr<Block> ParseBlock(List<Token> tokens, ref int index) {

        var block = new Block();

        index += 1;

        while (index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case RCurlyToken _:

                    index += 1;

                    return new ErrorOr<Block>(block);

                ///

                case SemicolonToken _:  

                    index += 1;

                    break;

                ///

                case EolToken _:  

                    index += 1;

                    break;

                ///

                default:  

                    var statementOrError = ParseStatement(tokens, ref index);

                    if (statementOrError.Error != null) {

                        throw new Exception();
                    }

                    var stmt = statementOrError.Value ?? throw new Exception();

                    block.Statements.Add(stmt);

                    index += 1;

                    break;
            }
        }

        return new ErrorOr<Block>("expected complete block", tokens.ElementAt(index - 1).Span);
    }

    public static ErrorOr<Statement> ParseStatement(List<Token> tokens, ref int index) {

        switch (tokens[index]) {

            case NameToken nt when nt.Value == "defer": {

                index += 1;

                var blockOrError = ParseBlock(tokens, ref index);

                if (blockOrError.Error != null) {

                    throw new Exception();
                }

                var block = blockOrError.Value ?? throw new Exception();

                return new ErrorOr<Statement>(new DeferStatement(block));
            }

            default: {

                var exprOrError = ParseExpression(tokens, ref index);

                if (exprOrError.Error != null) {

                    throw new Exception();
                }

                var expr = exprOrError.Value ?? throw new Exception();

                return new ErrorOr<Statement>(expr);
            }
        }
    }

    public static ErrorOr<Expression> ParseExpression(List<Token> tokens, ref int index) {

        switch (tokens.ElementAt(index)) {

            case NameToken name:

                var callOrError = ParseCall(tokens, ref index);

                if (callOrError.Error != null) {

                    throw new Exception();
                }

                var call = callOrError.Value ?? throw new Exception();
                
                return new ErrorOr<Expression>(new CallExpression(call));

            ///

            case NumberToken number:

                index += 1;

                return new ErrorOr<Expression>(new Int64Expression(number.Value));

            ///

            case QuotedStringToken qs:

                index += 1;

                return new ErrorOr<Expression>(new QuotedStringExpression(qs.Value));

            ///

            case var t:

                WriteLine($"{t}");

                return new ErrorOr<Expression>("unsupported expression", t.Span);
        }
    }

    public static ErrorOr<Call> ParseCall(List<Token> tokens, ref int index) {
        
        var call = new Call();

        ///

        switch (tokens.ElementAt(index)) {

            case NameToken name: {

                call.Name = name.Value;

                ///

                index += 1;

                ///

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case LParenToken _:

                            index += 1;

                            break;

                        ///

                        default:

                            return new ErrorOr<Call>("expected '('", name.Span);
                    }
                }
                else {

                    return new ErrorOr<Call>("incomplete function", tokens[index - 1].Span);
                }

                ///

                var cont = true;

                while (index < tokens.Count && cont) {

                    switch (tokens.ElementAt(index)) {

                        case RParenToken _: {

                            index += 1;

                            cont = false;

                            break;
                        }

                        ///

                        case CommaToken _: {

                            index += 1;

                            break;
                        }

                        ///

                        default: {

                            var exprOrError = ParseExpression(tokens, ref index);

                            if (exprOrError.Error != null) {

                                throw new Exception();
                            }

                            var expr = exprOrError.Value ?? throw new Exception();

                            ///

                            call.Args.Add((String.Empty, expr));

                            break;
                        }
                    }
                }

                ///

                if (index >= tokens.Count) {

                    return new ErrorOr<Call>("incomplete function", tokens.ElementAt(index - 1).Span);
                }

                ///

                return new ErrorOr<Call>(call);
            }

            ///

            default: {

                return new ErrorOr<Call>("expected function call", tokens.ElementAt(index).Span);
            }
        }
    }
}
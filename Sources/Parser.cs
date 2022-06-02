
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

///

public partial class NeuType {

    public NeuType() { }
}

public partial class StringType : NeuType {

    public StringType() 
        : base() { }
}

public partial class Int8Type : NeuType {

    public Int8Type() 
        : base() { }
}

public partial class Int16Type : NeuType {

    public Int16Type() 
        : base() { }
}

public partial class Int32Type : NeuType {

    public Int32Type() 
        : base() { }
}

public partial class Int64Type : NeuType {

    public Int64Type() 
        : base() { }
}

public partial class UInt8Type : NeuType {

    public UInt8Type() 
        : base() { }
}

public partial class UInt16Type : NeuType {

    public UInt16Type() 
        : base() { }
}

public partial class UInt32Type : NeuType {

    public UInt32Type() 
        : base() { }
}

public partial class UInt64Type : NeuType {

    public UInt64Type() 
        : base() { }
}

public partial class FloatType : NeuType {

    public FloatType() 
        : base() { }
}

public partial class DoubleType : NeuType {

    public DoubleType() 
        : base() { }
}

public partial class VoidType : NeuType {

    public VoidType() 
        : base() { }
}

///

public partial class VarDecl {

    public String Name { get; init; }

    public NeuType Type { get; init; }

    public bool Mutable { get; set; }

    ///

    public VarDecl(
        String name,
        NeuType type,
        bool mutable) {

        this.Name = name;
        this.Type = type;
        this.Mutable = mutable;
    }
}

///

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

public partial class ColonToken: Token {

    public ColonToken(Span span)
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

public partial class MinusToken: Token {

    public MinusToken(Span span)
        : base(span) { }
}

public partial class EqualsToken: Token {

    public EqualsToken(Span span) 
        : base(span) { }
}

public partial class GreaterThanToken: Token {

    public GreaterThanToken(Span span)
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

    public List<(String, NeuType)> Parameters { get; init; }

    public Block Block { get; init; }

    public NeuType ReturnType { get; init; }

    ///

    public Function(
        String name,
        List<(String, NeuType)> parameters,
        Block block,
        NeuType returnType) {

        this.Name = name;
        this.Parameters = parameters;
        this.Block = block;
        this.ReturnType = returnType;
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

public partial class VarExpression: Expression {

    public String Value { get; init; }

    ///

    public VarExpression(
        String value) : base() {

        this.Value = value;
    }
}

///

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

public partial class VarDeclStatement: Statement {

    public VarDecl Decl { get; init; }

    public Expression Expr { get; init; }

    ///

    public VarDeclStatement(
        VarDecl decl,
        Expression expr) {

        this.Decl = decl;
        this.Expr = expr;
    }
}

///

public partial class ReturnStatement: Statement {

    public Expression Expr { get; init; }

    ///

    public ReturnStatement(
        Expression expr) {

        this.Expr = expr;
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

                    index += 1;

                    if (index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case LParenToken _: {

                                index += 1;

                                break;
                            }

                            ///

                            default: {

                                return new ErrorOr<Function>(
                                    "expected '('", 
                                    tokens.ElementAt(index).Span);
                            }
                        }
                    }
                    else {

                        return new ErrorOr<Function>(
                            "incomplete function", 
                            tokens.ElementAt(index - 1).Span);
                    }
                    
                    var parameters = new List<(String, NeuType)>();

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

                            case NameToken nt: {

                                var varDeclOrError = ParseVariableDeclaration(tokens, ref index);

                                if (varDeclOrError.Error != null) {

                                    throw new Exception();
                                }

                                var varDecl = varDeclOrError.Value ?? throw new Exception();
                            
                                parameters.Add((varDecl.Name, varDecl.Type));

                                break;
                            }

                            ///

                            case var t: {

                                return new ErrorOr<Function>("expected parameter", t.Span);
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        return new ErrorOr<Function>("incomplete function", tokens.ElementAt(index - 1).Span);
                    }

                    ///

                    NeuType returnType = new VoidType();

                    if ((index + 2) < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case MinusToken _: {

                                index += 1;

                                switch (tokens.ElementAt(index)) {

                                    case GreaterThanToken _: {

                                        index += 1;

                                        var returnTypeOrError = ParseTypeName(tokens, ref index);

                                        if (returnTypeOrError.Error != null) {

                                            throw new Exception();
                                        }

                                        returnType = returnTypeOrError.Value ?? throw new Exception();

                                        index += 1;

                                        break;
                                    }

                                    ///

                                    default: {

                                        return new ErrorOr<Function>(
                                            "expected ->", 
                                            tokens.ElementAt(index - 1).Span);
                                    }
                                }

                                break;
                            }

                            ///

                            default: {

                                break;
                            }
                        }
                    }

                    ///

                    if (index >= tokens.Count) {

                        return new ErrorOr<Function>(
                            "incomplete function", 
                            tokens.ElementAt(index - 1).Span);
                    }

                    ///

                    var blockOrError = ParseBlock(tokens, ref index);

                    if (blockOrError.Error != null) {

                        throw new Exception();
                    }

                    var block = blockOrError.Value ?? throw new Exception();

                    return new ErrorOr<Function>(
                        new Function(
                            name: name.Value, 
                            parameters, 
                            block,
                            returnType));

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

            ///

            case NameToken nt when nt.Value == "return": {

                index += 1;

                var exprOrError = ParseExpression(tokens, ref index);

                if (exprOrError.Error != null) {

                    throw new Exception();
                }

                var expr = exprOrError.Value ?? throw new Exception();

                return new ErrorOr<Statement>(new ReturnStatement(expr));
            }

            ///

            case NameToken nt when nt.Value == "let" || nt.Value == "var": {

                var mutable = nt.Value == "var";

                index += 1;

                var varDeclOrError = ParseVariableDeclaration(tokens, ref index);

                if (varDeclOrError.Error != null) {

                    throw new Exception();
                }

                var varDecl = varDeclOrError.Value ?? throw new Exception();

                varDecl.Mutable = mutable;

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case EqualsToken _: {

                            index += 1;

                            if (index < tokens.Count) {

                                var exprOrError = ParseExpression(tokens, ref index);

                                if (exprOrError.Error != null) {

                                    throw new Exception();
                                }

                                var expr = exprOrError.Value ?? throw new Exception();

                                return new ErrorOr<Statement>(new VarDeclStatement(varDecl, expr));
                            }
                            else {

                                return new ErrorOr<Statement>(
                                    "expected initializer",
                                    tokens.ElementAt(index - 1).Span);

                            }
                        }

                        ///

                        default: {

                            return new ErrorOr<Statement>(
                                "expected initializer", 
                                tokens.ElementAt(index - 1).Span);
                        }
                    }
                }
                else {

                    return new ErrorOr<Statement>(
                        "expected initializer", 
                        tokens.ElementAt(index).Span);
                }
            }

            ///

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

            // case NameToken name:

            //     var callOrError = ParseCall(tokens, ref index);

            //     if (callOrError.Error != null) {

            //         throw new Exception();
            //     }

            //     var call = callOrError.Value ?? throw new Exception();
                
            //     return new ErrorOr<Expression>(new CallExpression(call));

            case NameToken nt: {

                if ((index + 1) < tokens.Count) {

                    switch (tokens.ElementAt(index + 1)) {

                        case LParenToken _: {

                            var callOrError = ParseCall(tokens, ref index);

                            if (callOrError.Error != null) {

                                throw new Exception();
                            }

                            var call = callOrError.Value ?? throw new Exception();

                            ///

                            return new ErrorOr<Expression>(new CallExpression(call));
                        }
                        default: {

                            break;
                        }
                    }
                }

                index += 1;

                return new ErrorOr<Expression>(new VarExpression(nt.Value));
            }

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

    public static ErrorOr<VarDecl> ParseVariableDeclaration(List<Token> tokens, ref int index) {

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                var varName = nt.Value;

                index += 1;

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case ColonToken _ : {

                            index += 1;

                            break;
                        }

                        ///

                        default: {

                            return new ErrorOr<VarDecl>(
                                "expected ':'", 
                                tokens.ElementAt(index).Span);
                        }
                    }
                }

                if (index < tokens.Count) {

                    var varTypeOrError = ParseTypeName(tokens, ref index);

                    if (varTypeOrError.Error != null) {

                        throw new Exception();
                    }

                    var varType = varTypeOrError.Value ?? throw new Exception();

                    var result = new VarDecl(varName, varType, mutable: false);

                    index += 1;

                    return new ErrorOr<VarDecl>(result);
                }
                else {

                    return new ErrorOr<VarDecl>(
                        "expected type", 
                        tokens.ElementAt(index).Span);
                }
            }

            ///

            default: {

                return new ErrorOr<VarDecl>(
                    "expected type", 
                    tokens.ElementAt(index).Span);
            }
        }
    }

    public static ErrorOr<NeuType> ParseTypeName(List<Token> tokens, ref int index) {

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                switch (nt.Value) {

                    case "Int8":

                        return new ErrorOr<NeuType>(new Int8Type());

                    case "Int16":

                        return new ErrorOr<NeuType>(new Int16Type());

                    case "Int32":

                        return new ErrorOr<NeuType>(new Int32Type());

                    case "Int64":

                        return new ErrorOr<NeuType>(new Int64Type());

                    case "UInt8":

                        return new ErrorOr<NeuType>(new UInt8Type());

                    case "UInt16":

                        return new ErrorOr<NeuType>(new UInt16Type());

                    case "UInt32":

                        return new ErrorOr<NeuType>(new UInt32Type());

                    case "UInt64":

                        return new ErrorOr<NeuType>(new UInt64Type());

                    case "Float":

                        return new ErrorOr<NeuType>(new FloatType());

                    case "Double":

                        return new ErrorOr<NeuType>(new DoubleType());

                    case "String":

                        return new ErrorOr<NeuType>(new StringType());

                    default:

                        return new ErrorOr<NeuType>("unknown type", nt.Span);
                }
            }

            ///

            case var t: {

                return new ErrorOr<NeuType>("expected function all", t.Span);
            }
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

                        case var t:

                            return new ErrorOr<Call>("expected '('", t.Span);
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

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

public partial class BoolType : NeuType {

    public BoolType() 
        : base() { }
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

    public VarDecl()
        : this(String.Empty, new VoidType(), false) { }

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

public partial class PlusToken: Token {

    public PlusToken(Span span) 
        : base(span) { }
}

public partial class MinusToken: Token {

    public MinusToken(Span span)
        : base(span) { }
}

public partial class EqualToken: Token {

    public EqualToken(Span span) 
        : base(span) { }
}

public partial class NotEqualToken: Token {
    
    public NotEqualToken(Span span) 
        : base(span) { }
}

public partial class DoubleEqualToken: Token {
    
    public DoubleEqualToken(Span span) 
        : base(span) { }
}

public partial class GreaterThanToken: Token {

    public GreaterThanToken(Span span)
        : base(span) { }
}

public partial class GreaterThanOrEqualToken: Token {
    
    public GreaterThanOrEqualToken(Span span) 
        : base(span) { }
}

public partial class LessThanToken: Token {

    public LessThanToken(Span span)
        : base(span) { }
}

public partial class LessThanOrEqualToken: Token {
    
    public LessThanOrEqualToken(Span span) 
        : base(span) { }
}

public partial class AsteriskToken: Token {

    public AsteriskToken(Span span) 
        : base(span) { }
}

public partial class ForwardSlashToken: Token {

    public ForwardSlashToken(Span span) 
        : base(span) { }
}

public partial class ExclamationToken: Token {

    public ExclamationToken(Span span)
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

public partial class UnknownToken: Token {

    public UnknownToken(Span span) 
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

    public Function()
        : this(
            String.Empty, 
            new List<(string, NeuType)>(), 
            new Block(), 
            new VoidType()) { }

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

public partial class IfStatement: Statement {

    public Expression Expr { get; init; }

    public Block Block { get; init; }

    ///

    public IfStatement(
        Expression expr,
        Block block)
        : base() {

        this.Expr = expr;
        this.Block = block;
    }
}

///

public partial class WhileStatement: Statement {

    public Expression Expr { get; init; }

    public Block Block { get; init; }

    ///

    public WhileStatement(
        Expression expr,
        Block block) 
        : base() {

        this.Expr = expr;
        this.Block = block;
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

public partial class GarbageStatement: Statement {

    public GarbageStatement() { }
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

    // Standalone

    public partial class BooleanExpression: Expression {

        public bool Value { get; init; }

        ///

        public BooleanExpression(
            bool value) {

            this.Value = value;
        }
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

    public partial class BinaryOpExpression: Expression {

        public Expression Lhs { get; init; }
        
        public Expression Op { get; init; }
        
        public Expression Rhs { get; init; }

        ///

        public BinaryOpExpression(
            Expression lhs,
            Expression op,
            Expression rhs) {

            this.Lhs = lhs;
            this.Op = op;
            this.Rhs = rhs;
        }
    }

    public partial class VarExpression: Expression {

        public String Value { get; init; }

        ///

        public VarExpression(
            String value) 
            : base() {

            this.Value = value;
        }
    }

    // Not standalone

    public enum Operator {

        Add,
        Subtract,
        Multiply,
        Divide,
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Assign
    }

    public partial class OperatorExpression: Expression {

        public Operator Operator { get; init; }

        ///

        public OperatorExpression(
            Operator op)
            : base() {

            this.Operator = op;
        }
    }
    
    // Parsing error

    public partial class GarbageExpression: Expression {

        ///

        public GarbageExpression() 
            : base() { }
    }

///

public static partial class ExpressionFunctions {

    public static UInt64 Precendence(
        this Expression expr) {

        switch (expr) {

            case OperatorExpression opExpr when 
                opExpr.Operator == Operator.Multiply || opExpr.Operator == Operator.Divide:

                return 100;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == Operator.Add || opExpr.Operator == Operator.Subtract:

                return 90;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == Operator.LessThan
                || opExpr.Operator == Operator.LessThanOrEqual
                || opExpr.Operator == Operator.GreaterThan
                || opExpr.Operator == Operator.GreaterThanOrEqual
                || opExpr.Operator == Operator.Equal
                || opExpr.Operator == Operator.NotEqual:
                
                return 80;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == Operator.Assign:

                return 50;

            ///

            default:

                return 0;
        }
    }
}

///

public static partial class IListFunctions {

    public static T Pop<T>(
        this IList<T> list) {

        if (list.Count < 1) {

            throw new Exception();
        }

        ///

        var l = list.Last();

        ///

        list.RemoveAt(list.Count - 1);

        ///

        return l;
    }
}

///

public static partial class ParserFunctions {

    public static (ParsedFile, Error?) ParseFile(
        List<Token> tokens) {

        Error? error = null;

        var parsedFile = new ParsedFile();

        var index = 0;

        var cont = true;

        while (index < tokens.Count && cont) {

            var token = tokens.ElementAt(index);

            switch (token) {

                case NameToken nt: {

                    switch (nt.Value) {

                        case "func": {

                            var (fun, err) = ParseFunction(tokens, ref index);

                            error = error ?? err;

                            parsedFile.Functions.Add(fun);

                            break;
                        }

                        ///

                        default: {

                            error = error ?? new ParserError(
                                "unexpected keyword", 
                                nt.Span);

                            break;
                        }
                    }

                    break;
                }

                ///

                case EolToken _: {

                    // ignore

                    index += 1;

                    break;
                }

                ///

                case EofToken _: {

                    cont = false;

                    break;
                }

                ///

                case var t: {

                    error = error ?? new ParserError(
                        "unexpected token (expected keyword)", 
                        t.Span);

                    break;
                }
            }
        }

        return (parsedFile, error);
    }

    public static (Function, Error?) ParseFunction(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the function

            switch (tokens.ElementAt(index)) {

                case NameToken funNameToken: {

                    index += 1;

                    if (index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case LParenToken _: {

                                index += 1;

                                break;
                            }

                            ///

                            default: {

                                error = error ?? new ParserError(
                                    "expected '('", 
                                    tokens.ElementAt(index).Span);

                                break;
                            }
                        }
                    }
                    else {

                        error = error ?? new ParserError(
                            "incomplete function", 
                            tokens.ElementAt(index).Span);
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

                                // Treat comma as whitespace? Might require them in the future

                                index += 1;

                                break;
                            }

                            ///

                            case NameToken _: {

                                // Now lets parse a parameter

                                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index);

                                error = error ?? varDeclErr;

                                parameters.Add((varDecl.Name, varDecl.Type));

                                break;
                            }

                            ///

                            default: {

                                error = error ?? new ParserError(
                                    "expected parameter",
                                    tokens.ElementAt(index).Span);

                                break;
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        error = error ?? new ParserError(
                            "incomplete function",
                            tokens.ElementAt(index).Span);
                    }

                    NeuType returnType = new VoidType();

                    if ((index + 2) < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case MinusToken _: {

                                index += 1;

                                switch (tokens.ElementAt(index)) {

                                    case GreaterThanToken: {

                                        index += 1;

                                        var (retType, retTypeErr) = ParseTypeName(tokens, ref index);

                                        returnType = retType;

                                        error = error ?? retTypeErr;

                                        index += 1;

                                        break;
                                    }

                                    ///

                                    default: {

                                        error = error ?? new ParserError(
                                            "expecrted ->",
                                            tokens.ElementAt(index - 1).Span);

                                        break;
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

                    if (index >= tokens.Count) {

                        error = error ?? new ParserError(
                            "incomplete function", 
                            tokens.ElementAt(index - 1).Span);
                    }

                    var (block, blockErr) = ParseBlock(tokens, ref index);

                    error = error ?? blockErr;

                    return (
                        new Function(
                            name: funNameToken.Value,
                            parameters,
                            block,
                            returnType),
                        error);
                }

                ///

                default: {

                    return (
                        new Function(),
                        new ParserError(
                            "expected function name", 
                            tokens.ElementAt(index).Span));
                }
            }
        }
        else {

            return (
                new Function(),
                new ParserError(
                    "incomplete function definition", 
                    tokens.ElementAt(index).Span));
        }
    }

    ///

    public static (Block, Error?) ParseBlock(List<Token> tokens, ref int index) {

        var block = new Block();

        Error? error = null;

        index += 1;

        while (index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case RCurlyToken _: {

                    index += 1;

                    return (block, error);
                }

                ///

                case SemicolonToken _: {

                    index += 1;

                    break;
                }

                ///

                case EolToken _: {

                    index += 1;

                    break;
                }

                ///

                default: {

                    var (stmt, stmtErr) = ParseStatement(tokens, ref index);

                    error = error ?? stmtErr;

                    block.Statements.Add(stmt);

                    break;
                }
            }
        }

        return (
            new Block(),
            new ParserError(
                "expected complete block", 
                tokens.ElementAt(index - 1).Span));
    }
    
    ///

    public static (Statement, Error?) ParseStatement(List<Token> tokens, ref int index) {

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "defer": {

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new DeferStatement(block), 
                    error);
            }

            ///

            case NameToken nt when nt.Value == "if": {

                index += 1;

                var (condExpr, condExprErr) = ParseExpression(tokens, ref index);

                error = error ?? condExprErr;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new IfStatement(condExpr, block),
                    error);
            }

            ///

            case NameToken nt when nt.Value == "while": {

                index += 1;

                var (condExpr, condExprErr) = ParseExpression(tokens, ref index);

                error = error ?? condExprErr;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new WhileStatement(condExpr, block),
                    error
                );
            }

            ///

            case NameToken nt when nt.Value == "return": {

                index += 1;

                var (expr, exprErr) = ParseExpression(tokens, ref index);

                error = error ?? exprErr;

                return (
                    new ReturnStatement(expr),
                    error
                );
            }

            ///

            case NameToken nt when nt.Value == "let" || nt.Value == "var": {

                var mutable = nt.Value == "var";

                index += 1;

                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index);

                error = error ?? varDeclErr;

                varDecl.Mutable = mutable;

                // Hardwire an initialiser for now, but we may not want this long-term

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case EqualToken _: {

                            index += 1;

                            if (index < tokens.Count) {

                                var (expr, exprErr) = ParseExpression(tokens, ref index);

                                error = error ?? exprErr;

                                return (
                                    new VarDeclStatement(varDecl, expr),
                                    error
                                );
                            }
                            else {

                                return (
                                    new GarbageStatement(),
                                    new ParserError(
                                        "expected initializer", 
                                        tokens.ElementAt(index - 1).Span)
                                );
                            }
                        }

                        ///

                        default: {

                            return (
                                new GarbageStatement(),
                                new ParserError(
                                    "expected initializer", 
                                    tokens.ElementAt(index - 1).Span));
                        }
                    }
                }
                else {

                    return (
                        new GarbageStatement(),
                        new ParserError(
                            "expected initializer", 
                            tokens.ElementAt(index - 1).Span));
                }
            }

            ///

            case var t: {

                var (expr, exprErr) = ParseExpression(tokens, ref index);

                error = error ?? exprErr;

                return (
                    expr,
                    error);
            }
        }
    }

    ///

    public static (Expression, Error?) ParseExpression(List<Token> tokens, ref int index) {

        // As the exprStack grows, we increase the required precedence to grow larger
        // If, at any time, the operator we're looking at is the same or lower precedence
        // of what is in the expression stack, we collapse the expression stack.

        Error? error = null;

        var exprStack = new List<Expression>();

        UInt64 lastPrecedence = 1000000;

        var (lhs, lhsErr) = ParseOperand(tokens, ref index);

        error = error ?? lhsErr;

        exprStack.Add(lhs);

        while (index < tokens.Count) {

            var (op, opErr) = ParseOperator(tokens, ref index);

            if (opErr != null) {

                break;
            }

            var precedence = op.Precendence();

            if (index == tokens.Count) {

                error = error ?? new ParserError(
                    "incomplete math expression",
                    tokens.ElementAt(index - 1).Span);

                exprStack.Add(new GarbageExpression());
                
                exprStack.Add(new GarbageExpression());

                break;
            }

            var (rhs, rhsErr) = ParseOperand(tokens, ref index);

            error = error ?? rhsErr;

            while (precedence <= lastPrecedence && exprStack.Count > 1) {

                var _rhs = exprStack.Pop();

                var _op = exprStack.Pop();

                lastPrecedence = _op.Precendence();

                if (lastPrecedence < precedence) {

                    exprStack.Add(_op);

                    exprStack.Add(_rhs);

                    break;
                }

                var _lhs = exprStack.Pop();

                exprStack.Add(new BinaryOpExpression(_lhs, _op, _rhs));
            }

            exprStack.Add(op);

            exprStack.Add(rhs);

            lastPrecedence = precedence;
        }

        while (exprStack.Count != 1) {

            var _rhs = exprStack.Pop();

            var _op = exprStack.Pop();

            var _lhs = exprStack.Pop();

            exprStack.Add(new BinaryOpExpression(_lhs, _op, _rhs));
        }

        var output = exprStack.Pop();

        return (output, error);
    }

    ///

    public static (Expression, Error?) ParseOperand(List<Token> tokens, ref int index) {

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "true": {

                index += 1;

                return (
                    new BooleanExpression(true),
                    null);
            }

            ///

            case NameToken nt when nt.Value == "false": {

                index += 1;

                return (
                    new BooleanExpression(false),
                    null);
            }

            ///

            case NameToken nt: {

                if ((index + 1) < tokens.Count) {

                    switch (tokens.ElementAt(index + 1)) {

                        case LParenToken _: {

                            var (call, callErr) = ParseCall(tokens, ref index);

                            error = error ?? callErr;

                            return (
                                new CallExpression(call), 
                                error);
                        }

                        ///

                        default: {

                            break;
                        }
                    }
                }

                index += 1;

                return (
                    new VarExpression(nt.Value),
                    error);
            }

            ///

            case LParenToken _: {

                index += 1;

                var (expr, exprErr) = ParseExpression(tokens, ref index);

                error = error ?? exprErr;

                switch (tokens.ElementAt(index)) {

                    case RParenToken _: {

                        index += 1;

                        break;
                    }

                    ///

                    default: {

                        error = error ?? new ParserError(
                            "expected ')'", 
                            tokens.ElementAt(index).Span);

                        break;
                    }
                }

                return (expr, error);
            }

            ///

            case NumberToken numTok: {

                index += 1;

                return (
                    new Int64Expression(numTok.Value),
                    error);
            }

            ///

            case QuotedStringToken qs: {

                index += 1;

                return (
                    new QuotedStringExpression(qs.Value),
                    error);
            }

            ///

            default: {

                return (
                    new GarbageExpression(),
                    new ParserError(
                        "unsupported expression",
                        tokens.ElementAt(index).Span)
                );
            }
        }
    }

    ///

    public static (Expression, Error?) ParseOperator(List<Token> tokens, ref int index) {

        switch (tokens.ElementAt(index)) {

            case PlusToken _: {

                index += 1;

                return (new OperatorExpression(Operator.Add), null);
            }

            case MinusToken _: {

                index += 1;

                return (new OperatorExpression(Operator.Subtract), null);
            }

            case AsteriskToken _: {

                index += 1;

                return (new OperatorExpression(Operator.Multiply), null);
            }

            case ForwardSlashToken _: {

                index += 1;

                return (new OperatorExpression(Operator.Divide), null);
            }

            case EqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.Assign), null);
            }
            
            case DoubleEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.Equal), null);
            }
            
            case NotEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.NotEqual), null);
            }
            
            case LessThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.LessThan), null);
            }
            
            case LessThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.LessThanOrEqual), null);
            }
            
            case GreaterThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.GreaterThan), null);
            }
            
            case GreaterThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(Operator.GreaterThanOrEqual), null);
            }

            ///

            default: {

                return (
                    new GarbageExpression(),
                    new ParserError(
                        "unsupported operator", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    ///

    public static (VarDecl, Error?) ParseVariableDeclaration(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                var varName = nt.Value;

                index += 1;

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case ColonToken _: {

                            index += 1;

                            break;
                        }

                        ///
                        
                        default: {

                            return (
                                new VarDecl(), 
                                new ParserError(
                                    "expected ':'", 
                                    tokens.ElementAt(index).Span));
                        }
                    }
                }
            
                if (index < tokens.Count) {

                    var (varType, typeErr) = ParseTypeName(tokens, ref index);

                    error = error ?? typeErr;

                    var result = new VarDecl(
                        name: varName, 
                        type: varType, 
                        mutable: false);

                    index += 1;

                    return (result, error);
                }
                else {

                    return (
                        new VarDecl(
                            nt.Value, 
                            new VoidType(), 
                            mutable: false), 
                        new ParserError(
                            "expected type", 
                            tokens.ElementAt(index).Span));
                }
            }

            ///

            default: {

                return (
                    new VarDecl(),
                    new ParserError(
                        "expected name", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    ///

    public static (NeuType, Error?) ParseTypeName(
        List<Token> tokens, 
        ref int index) {

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                switch (nt.Value) {

                    case "Int8":

                        return (new Int8Type(), null);

                    case "Int16":

                        return (new Int16Type(), null);

                    case "Int32":

                        return (new Int32Type(), null);

                    case "Int64":

                        return (new Int64Type(), null);

                    case "UInt8":

                        return (new UInt8Type(), null);

                    case "UInt16":

                        return (new UInt16Type(), null);

                    case "UInt32":

                        return (new UInt32Type(), null);

                    case "UInt64":

                        return (new UInt64Type(), null);

                    case "Float":

                        return (new FloatType(), null);

                    case "Double":

                        return (new DoubleType(), null);

                    case "String":

                        return (new StringType(), null);

                    case "Bool":

                        return (new BoolType(), null);

                    default:

                        return (
                            new VoidType(), 
                            new ParserError("unknown type", nt.Span));
                }
            }

            ///

            default: {

                return (
                    new VoidType(), 
                    new ParserError(
                        "expected function all", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    ///

    public static (Call, Error?) ParseCall(List<Token> tokens, ref int index) {

        var call = new Call();

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                call.Name = nt.Value;

                index += 1;

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case LParenToken _: {

                            index += 1;

                            break;
                        }

                        ///

                        case var t: {

                            return (
                                call,
                                new ParserError("expected '('", t.Span));
                        }
                    }
                }
                else {

                    return (
                        call,
                        new ParserError("incomplete function", tokens.ElementAt(index - 1).Span));
                }

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

                            // Treat comma as whitespace? Might require them in the future

                            index += 1;

                            break;
                        }

                        ///

                        default: {

                            var (expr, exprError) = ParseExpression(tokens, ref index);

                            error = error ?? exprError;

                            call.Args.Add((String.Empty, expr));

                            break;
                        }
                    }
                }

                if (index >= tokens.Count) {

                    error = error ?? new ParserError(
                        "incomplete call", 
                        tokens.ElementAt(index - 1).Span);
                }

                break;
            }

            ///

            default: {

                error = error ?? new ParserError(
                    "expected function call", 
                    tokens.ElementAt(index).Span);

                break;
            }
        }

        ///

        return (call, error);
    }
}
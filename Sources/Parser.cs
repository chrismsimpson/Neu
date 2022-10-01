
namespace Neu;

public static partial class TraceFunctions {

    public static void Trace(
        String expr) {

        #if NEU_TRACE

        WriteLine($"{expr}");

        #endif
    }
}

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

public static partial class CallFunctions {

    public static bool Eq(
        Call l, 
        Call r) {

        if (!Equals(l.Name, r.Name)) {

            return false;
        }

        if (l.Args.Count != r.Args.Count) {

            return false;
        }

        for (var i = 0; i < l.Args.Count; ++i) {

            var argL = l.Args[i];

            var argR = r.Args[i];

            if (!Equals(argL.Item1, argR.Item1)) {

                return false;
            }

            if (!ExpressionFunctions.Eq(argL.Item2, argR.Item2)) {

                return false;
            }
        }

        return true;
    }
}

///

public enum ExpressionKind {

    ExpressionWithAssignments,
    ExpressionWithoutAssignment
}

///

public partial class VarDecl {

    public String Name { get; init; }

    public NeuType Type { get; set; }

    public bool Mutable { get; set; }

    public Span Span { get; init; }

    ///

    public VarDecl(Span span)
        : this(String.Empty, new VoidType(), false, span) { }

    public VarDecl(
        String name,
        NeuType type,
        bool mutable,
        Span span) {

        this.Name = name;
        this.Type = type;
        this.Mutable = mutable;
        this.Span = span;
    }
}

public static partial class VarDeclFunctions {

    public static bool Eq(
        VarDecl l,
        VarDecl r) {

        return l.Name == r.Name 
            && NeuTypeFunctions.Eq(l.Type, r.Type) 
            && l.Mutable == r.Mutable;
    }
}

///


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

    public Span NameSpan { get; init; }

    public List<Parameter> Parameters { get; init; }

    public Block Block { get; init; }

    public NeuType ReturnType { get; init; }

    ///

    public Function()
        : this(
            String.Empty,
            new Span(
                fileId: 0, 
                start: 0, 
                end: 0),
            new List<Parameter>(), 
            new Block(), 
            new VoidType()) { }

    public Function(
        String name,
        Span nameSpan,
        List<Parameter> parameters,
        Block block,
        NeuType returnType) {

        this.Name = name;
        this.NameSpan = nameSpan;
        this.Parameters = parameters;
        this.Block = block;
        this.ReturnType = returnType;
    }
}

///

public partial class Variable {

    public String Name { get; init; }

    public NeuType Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public Variable(
        String name,
        NeuType ty,
        bool mutable) {

        this.Name = name;
        this.Type = ty;
        this.Mutable = mutable;
    }
}

///

public partial class Parameter {

    public bool RequiresLabel { get; init; }

    public Variable Variable { get; init; }

    ///

    public Parameter(
        bool requiresLabel,
        Variable variable) {

        this.RequiresLabel = requiresLabel;
        this.Variable = variable;
    }
}

///

public partial class Statement {

    public Statement() { }
}

public static partial class StatementFunctions {

    public static bool Eq(
        Statement? l,
        Statement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ///

        switch (true) {

            case var _ when
                l is DeferStatement deferL
                && r is DeferStatement deferR:

                return DeferStatementFunctions.Eq(deferL, deferR);

            ///

            case var _ when 
                l is VarDeclStatement vdsL
                && r is VarDeclStatement vdsR:

                return VarDeclStatementFunctions.Eq(vdsL, vdsR);

            ///

            case var _ when
                l is IfStatement ifL
                && r is IfStatement ifR:

                return IfStatementFunctions.Eq(ifL, ifR);

            ///

            case var _ when
                l is BlockStatement blockL
                && r is BlockStatement blockR:

                return BlockStatementFunctions.Eq(blockL, blockR);

            ///

            case var _ when
                l is WhileStatement whileL
                && r is WhileStatement whileR:

                return WhileStatementFunctions.Eq(whileL, whileR);

            ///

            case var _ when
                l is ReturnStatement retL
                && r is ReturnStatement retR:

                return ReturnStatementFunctions.Eq(retL, retR);

            ///

            case var _ when
                l is GarbageStatement gL
                && r is GarbageStatement gR:

                return GarbageStatementFunctions.Eq(gL, gR);

            ///

            case var _ when
                l is Expression exprL
                && r is Expression exprR:

                return ExpressionFunctions.Eq(exprL, exprR);

            ///

            default:

                return false;
        }
    }

    public static bool Eq(
        List<Statement> l,
        List<Statement> r) {

        if (l.Count != r.Count) {

            return false;
        }

        for (var i = 0; i < l.Count; ++i) {

            if (!StatementFunctions.Eq(l.ElementAt(i), r.ElementAt(i))) {

                return false;
            }
        }

        return true;
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

public static partial class DeferStatementFunctions {

    public static bool Eq(
        DeferStatement? l,
        DeferStatement? r) {

        return BlockFunctions.Eq(l?.Block, r?.Block);
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

public static partial class VarDeclStatementFunctions {

    public static bool Eq(
        VarDeclStatement? l,
        VarDeclStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        VarDeclStatement _l = l!;
        VarDeclStatement _r = r!;

        ///

        if (!VarDeclFunctions.Eq(_l.Decl, _r.Decl)) {

            return false;
        }

        return ExpressionFunctions.Eq(_l.Expr, _r.Expr);
    }
}

///

public partial class IfStatement: Statement {

    public Expression Expr { get; init; }

    public Block Block { get; init; }

    public Statement? Trailing { get; init; }

    ///

    public IfStatement(
        Expression expr,
        Block block,
        Statement? trailing)
        : base() {

        this.Expr = expr;
        this.Block = block;
        this.Trailing = trailing;
    }
}

public static partial class IfStatementFunctions {

    public static bool Eq(
        IfStatement? l,
        IfStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        IfStatement _l = l!;
        IfStatement _r = r!;

        ///

        return ExpressionFunctions.Eq(_l.Expr, _r.Expr)
            && BlockFunctions.Eq(_l.Block, _r.Block)
            && StatementFunctions.Eq(_l.Trailing, _r.Trailing);
    }
}

///

public partial class BlockStatement: Statement {

    public Block Block { get; init; }

    public BlockStatement(
        Block block) 
        : base() {

        this.Block = block;
    }
}

public static partial class BlockStatementFunctions {

    public static bool Eq(
        BlockStatement? l,
        BlockStatement? r) {

        return BlockFunctions.Eq(l?.Block, r?.Block);
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

public static partial class WhileStatementFunctions {

    public static bool Eq(
        WhileStatement? l,
        WhileStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        WhileStatement _l = l!;
        WhileStatement _r = r!;

        ///

        return ExpressionFunctions.Eq(_l.Expr, _r.Expr) 
            && BlockFunctions.Eq(_l.Block, _r.Block);
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

public static partial class ReturnStatementFunctions {

    public static bool Eq(
        ReturnStatement? l,
        ReturnStatement? r) {

        return ExpressionFunctions.Eq(l?.Expr, r?.Expr);
    }
}

///

public partial class GarbageStatement: Statement {

    public GarbageStatement() { }
}

public static partial class GarbageStatementFunctions {

    public static bool Eq(
        GarbageStatement? l,
        GarbageStatement? r) {
        
        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        return true;
    }
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

public static partial class BlockFunctions {

    public static bool Eq(
        Block? l,
        Block? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        Block _l = l!;
        Block _r = r!;

        return StatementFunctions.Eq(_l.Statements, _r.Statements);
    }
}

///

public partial class Expression: Statement {

    public Expression() : base() { }
}

    // Standalone

    public partial class BooleanExpression: Expression {

        public bool Value { get; init; }

        public Span Span { get; init; }

        ///

        public BooleanExpression(
            bool value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class CallExpression: Expression {

        public Call Call { get; init; }

        public Span Span { get; init; }

        ///

        public CallExpression(
            Call call,
            Span span)
            : base() {

            this.Call = call;
            this.Span = span;
        }
    }

    public partial class Int64Expression: Expression {

        public Int64 Value { get; init; }

        public Span Span { get; init; }

        ///

        public Int64Expression(
            Int64 value,
            Span span)
            : base() {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class QuotedStringExpression: Expression {

        public String Value { get; init; }

        public Span Span { get; init; }

        ///

        public QuotedStringExpression(
            String value,
            Span span)
            : base() { 
            
            this.Value = value;
            this.Span = span;
        }
    }

    public partial class VectorExpression: Expression {

        public List<Expression> Expressions { get; init; }

        public Span Span { get; init; }

        ///

        public VectorExpression(
            List<Expression> expressions,
            Span span) {

            this.Expressions = expressions;
            this.Span = span;
        }
    }

    public partial class IndexedExpression: Expression {

        public Expression Expression { get; init; }

        public Expression Index { get; init; }

        public Span Span { get; init; }

        ///

        public IndexedExpression(
            Expression expression,
            Expression index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class UnaryOpExpression: Expression {

        public Expression Expression { get; init; }
        
        public UnaryOperator Operator { get; init; }
        
        public Span Span { get; init; }

        ///

        public UnaryOpExpression(
            Expression expression,
            UnaryOperator op,
            Span Span) {

            this.Expression = expression;
            this.Operator = op;
            this.Span = Span;
        }
    }

    public partial class BinaryOpExpression: Expression {

        public Expression Lhs { get; init; }
        
        public BinaryOperator Operator { get; init; }
        
        public Expression Rhs { get; init; }

        public Span Span { get; init; }

        ///

        public BinaryOpExpression(
            Expression lhs,
            BinaryOperator op,
            Expression rhs,
            Span span) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Span = span;
        }
    }

    public partial class VarExpression: Expression {

        public String Value { get; init; }
        
        public Span Span { get; init; }

        ///

        public VarExpression(
            String value,
            Span span) 
            : base() {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class ForcedUnwrapExpression: Expression {

        public Expression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public ForcedUnwrapExpression(
            Expression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // FIXME: These should be implemented as `enum` variant values once available.

    public partial class OptionalNoneExpression: Expression {

        public Span Span { get; init; }
        
        ///

        public OptionalNoneExpression(
            Span span) : base() {

            this.Span = span;
        }
    }

    public partial class OptionalSomeExpression: Expression {

        public Expression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public OptionalSomeExpression(
            Expression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // Not standalone

    public enum UnaryOperator {

        PreIncrement,
        PostIncrement,
        PreDecrement,
        PostDecrement
    }

    public enum BinaryOperator {

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
        Assign,
        AddAssign,
        SubtractAssign,
        MultiplyAssign,
        DivideAssign
    }

    public partial class OperatorExpression: Expression {

        public BinaryOperator Operator { get; init; }
        
        public Span Span { get; init; }

        ///

        public OperatorExpression(
            BinaryOperator op,
            Span span)
            : base() {

            this.Operator = op;
            this.Span = span;
        }
    }
    
    // Parsing error

    public partial class GarbageExpression: Expression {

        public Span Span { get; init; }

        ///

        public GarbageExpression(
            Span span) 
            : base() {

            this.Span = span;
        }
    }

///

public static partial class ExpressionFunctions {

    public static Span GetSpan(
        this Expression expr) {

        switch (expr) {

            case BooleanExpression be: {

                return be.Span;
            }

            case CallExpression ce: {

                return ce.Span;
            }

            case Int64Expression ie: {

                return ie.Span;
            }

            case QuotedStringExpression qse: {

                return qse.Span;
            }

            case VectorExpression ve: {

                return ve.Span;
            }

            case IndexedExpression ie: {

                return ie.Span;
            }

            case UnaryOpExpression uoe: {

                return uoe.Span;
            }

            case BinaryOpExpression boe: {

                return boe.Span;
            }

            case VarExpression ve: {

                return ve.Span;
            }

            case OperatorExpression oe: {

                return oe.Span;
            }

            case OptionalNoneExpression optNoneExpr: {

                return optNoneExpr.Span;
            }

            case OptionalSomeExpression optSomeExpr: {

                return optSomeExpr.Span;
            }

            case ForcedUnwrapExpression forceUnwrapExpr: {

                return forceUnwrapExpr.Span;
            }

            case GarbageExpression ge: {

                return ge.Span;
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static bool Eq(
        Expression? l, 
        Expression? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ///

        switch (true) {

            case var _ when
                l is BooleanExpression boolL
                && r is BooleanExpression boolR:

                return boolL.Value == boolR.Value;

            ///

            case var _ when
                l is CallExpression callL
                && r is CallExpression callR:

                return CallFunctions.Eq(callL.Call, callR.Call);

            ///

            case var _ when
                l is Int64Expression intL
                && r is Int64Expression intR:

                return intL.Value == intR.Value;

            ///

            case var _ when
                l is QuotedStringExpression strL
                && r is QuotedStringExpression strR:

                return Equals(strL.Value, strR.Value);
            
            ///

            case var _ when
                l is BinaryOpExpression lbo
                && r is BinaryOpExpression rbo: 

                return ExpressionFunctions.Eq(lbo.Lhs, rbo.Lhs)
                    && lbo.Operator == rbo.Operator
                    && ExpressionFunctions.Eq(lbo.Rhs, rbo.Rhs);

            ///

            case var _ when
                l is VarExpression varL
                && r is VarExpression varR:

                return varL.Value == varR.Value;

            ///

            case var _ when
                l is OperatorExpression opL
                && r is OperatorExpression opR:

                return opL.Operator == opR.Operator;

            ///

            case var _ when
                l is GarbageExpression
                && r is GarbageExpression:

                return true;

            ///

            default: {

                return false;
            }
        }
    }

    public static UInt64 Precendence(
        this Expression expr) {

        switch (expr) {

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Multiply 
                || opExpr.Operator == BinaryOperator.Divide:

                return 100;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Add 
                || opExpr.Operator == BinaryOperator.Subtract:

                return 90;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LessThan
                || opExpr.Operator == BinaryOperator.LessThanOrEqual
                || opExpr.Operator == BinaryOperator.GreaterThan
                || opExpr.Operator == BinaryOperator.GreaterThanOrEqual
                || opExpr.Operator == BinaryOperator.Equal
                || opExpr.Operator == BinaryOperator.NotEqual:
                
                return 80;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Assign
                || opExpr.Operator == BinaryOperator.AddAssign
                || opExpr.Operator == BinaryOperator.SubtractAssign
                || opExpr.Operator == BinaryOperator.MultiplyAssign
                || opExpr.Operator == BinaryOperator.DivideAssign:

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

        Trace($"parse_file");

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

                            Trace("ERROR: unexpected keyword");

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

                    Trace("ERROR: unexpected token (expected keyword)");

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

        Trace($"ParseFunction: {tokens.ElementAt(index)}");

        Error? error = null;

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the function

            switch (tokens.ElementAt(index)) {

                case NameToken funNameToken: {

                    var nameSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    if (index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case LParenToken _: {

                                index += 1;

                                break;
                            }

                            ///

                            default: {

                                Trace("ERROR: expected '('");

                                error = error ?? new ParserError(
                                    "expected '('", 
                                    tokens.ElementAt(index).Span);

                                break;
                            }
                        }
                    }
                    else {

                        Trace("ERROR: incomplete function");

                        error = error ?? new ParserError(
                            "incomplete function", 
                            tokens.ElementAt(index).Span);
                    }

                    var parameters = new List<Parameter>();

                    var currentParamRequiresLabel = true;

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

                                currentParamRequiresLabel = true;

                                break;
                            }

                            ///

                            case NameToken nt when nt.Value == "_": {

                                currentParamRequiresLabel = false;

                                index += 1;

                                break;
                            }

                            ///

                            case NameToken _: {

                                // Now lets parse a parameter

                                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index);

                                error = error ?? varDeclErr;

                                if (varDecl.Type is UnknownType) {

                                    Trace("ERROR: parameter missing type");

                                    error = error ?? new ParserError(
                                        "parameter missing type",
                                        varDecl.Span);
                                }
                                
                                parameters.Add(
                                    new Parameter(
                                        requiresLabel: currentParamRequiresLabel,
                                        variable: new Variable(
                                            varDecl.Name, 
                                            varDecl.Type, 
                                            varDecl.Mutable)));

                                break;
                            }

                            ///

                            default: {

                                Trace("ERROR: expected parameter");

                                error = error ?? new ParserError(
                                    "expected parameter",
                                    tokens.ElementAt(index).Span);

                                break;
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        Trace("ERROR: incomplete function");

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

                                        Trace("ERROR: expected ->");

                                        error = error ?? new ParserError(
                                            "expected ->",
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

                        Trace("ERROR: incomplete function");

                        error = error ?? new ParserError(
                            "incomplete function", 
                            tokens.ElementAt(index - 1).Span);
                    }

                    var (block, blockErr) = ParseBlock(tokens, ref index);

                    error = error ?? blockErr;

                    return (
                        new Function(
                            name: funNameToken.Value,
                            nameSpan,
                            parameters,
                            block,
                            returnType),
                        error);
                }

                ///

                default: {

                    Trace("ERROR: expected function name");

                    return (
                        new Function(),
                        new ParserError(
                            "expected function name", 
                            tokens.ElementAt(index).Span));
                }
            }
        }
        else {

            Trace("ERROR: incomplete function definition");

            return (
                new Function(),
                new ParserError(
                    "incomplete function definition", 
                    tokens.ElementAt(index).Span));
        }
    }

    ///

    public static (Block, Error?) ParseBlock(List<Token> tokens, ref int index) {

        Trace($"ParseBlock: {tokens.ElementAt(index)}");

        var block = new Block();

        Error? error = null;

        var start = tokens.ElementAt(index).Span;

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

        Trace("ERROR: expected complete block");

        return (
            new Block(),
            new ParserError(
                "expected complete block", 
                new Span(
                    fileId: start.FileId,
                    start: start.Start,
                    end: tokens.ElementAt(index - 1).Span.End)));
    }
    
    ///

    public static (Statement, Error?) ParseStatement(List<Token> tokens, ref int index) {

        Trace($"ParseStatement: {tokens.ElementAt(index)}");

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "defer": {

                Trace("parsing defer");

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new DeferStatement(block), 
                    error);
            }

            ///

            case NameToken nt when nt.Value == "if": {

                return ParseIfStatement(tokens, ref index);
            }

            ///

            case NameToken nt when nt.Value == "while": {

                Trace("parsing while");

                index += 1;

                var (condExpr, condExprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

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

                Trace("parsing return");

                index += 1;

                var (expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? exprErr;

                return (
                    new ReturnStatement(expr),
                    error
                );
            }

            ///

            case NameToken nt when nt.Value == "let" || nt.Value == "var": {

                Trace("parsing let/var");

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

                                var (expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

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

            case LCurlyToken _: {

                Trace("parsing block from statement parser");

                var (block, blockErr) = ParseBlock(tokens, ref index);
            
                error = error ?? blockErr;

                return (new BlockStatement(block), error);
            }

            ///

            case var t: {

                Trace("parsing expression from statement parser");

                var (expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithAssignments);

                error = error ?? exprErr;

                // Make sure, if there is an error and we can make progress, that we make progress.
                // This allows the parser to be more forgiving when there are errors
                // and to ensure parsing continues to make progress.

                if (error != null) {

                    if (index < tokens.Count) {

                        index += 1;
                    }
                }

                return (
                    expr,
                    error);
            }
        }
    }

    ///

    public static (Statement, Error?) ParseIfStatement(
        List<Token> tokens,
        ref int index) {

        Trace($"ParseIfStatement: {tokens.ElementAt(index)}");

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "if": {

                // Good, we have our keyword

                break;
            }

            default: {

                return (
                    new GarbageStatement(),
                    new ParserError(
                        "expected if statement",
                        tokens.ElementAt(index).Span));
            }
        }

        var startingSpan = tokens.ElementAt(index).Span;

        index += 1;

        var (cond, condErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

        error = error ?? condErr;

        var (block, blockErr) = ParseBlock(tokens, ref index);

        error = error ?? blockErr;

        Statement? elseStmt = null;

        if (index < tokens.Count) {

            if (tokens.ElementAt(index) is EolToken) {

                index += 1;
            }

            // Check for an 'else'

            switch (tokens.ElementAt(index)) {

                case NameToken n1 when n1.Value == "else": {

                    // Good, we have our else keyword

                    index += 1;

                    if (index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case NameToken n2 when n2.Value == "if": {

                                var (elseIfStmt, elseIfErr) = ParseIfStatement(tokens, ref index);

                                elseStmt = elseIfStmt;

                                error = error ?? elseIfErr;

                                break;
                            }

                            case LCurlyToken _: {

                                var (elseBlock, elseBlockErr) = ParseBlock(tokens, ref index);

                                // Before we continue, quickly lint that the else block and the if block are not
                                // the same. This helps prevent a copy/paste error

                                if (BlockFunctions.Eq(block, elseBlock)) {

                                    error = error ?? new ValidationError(
                                        "if and else have identical blocks",
                                        startingSpan);
                                }

                                elseStmt = new BlockStatement(elseBlock);

                                error = error ?? elseBlockErr;

                                break;
                            }

                            default: {

                                error = error ?? new ParserError(
                                    "else missing if or block",
                                    tokens.ElementAt(index - 1).Span);

                                break;
                            }
                        }

                    }
                    else {

                        error = error ?? new ParserError(
                            "else missing if or block",
                            tokens.ElementAt(index - 1).Span);
                    }

                    break;
                }

                ///

                default: {

                    break;
                }
            }
        
            // try to parse an if statement again if we see an else
        }

        return (new IfStatement(cond, block, elseStmt), error);
    }

    ///

    public static (Expression, Error?) ParseExpression(
        List<Token> tokens, 
        ref int index, 
        ExpressionKind exprKind) {

        Trace($"ParseExpression: {tokens.ElementAt(index)}");

        // As the exprStack grows, we increase the required precedence.
        // If, at any time, the operator we're looking at is the same or lower precedence
        // of what is in the expression stack, we collapse the expression stack.

        Error? error = null;

        var exprStack = new List<Expression>();

        UInt64 lastPrecedence = 1000000;

        var (lhs, lhsErr) = ParseOperand(tokens, ref index);

        error = error ?? lhsErr;

        exprStack.Add(lhs);

        var cont = true;

        while (cont && index < tokens.Count) {

            var (op, opErr) = exprKind switch {
                ExpressionKind.ExpressionWithAssignments => ParseOperatorWithAssignment(tokens, ref index),
                ExpressionKind.ExpressionWithoutAssignment => ParseOperator(tokens, ref index),
                _ => throw new Exception()
            };

            // var (op, opErr) = ParseOperatorForKind(tokens, ref index, exprKind);
            
            if (opErr is Error e) {

                switch (e) {

                    case ValidationError ve:

                        // Because we just saw a validation error, we need to remember it
                        // for later

                        error = error ?? e;

                        break;

                    default:

                        cont = false;

                        break;
                }
            }

            if (!cont) {

                break;
            }

            var precedence = op.Precendence();

            if (index == tokens.Count) {

                Trace("ERROR: incomplete math expression");

                error = error ?? new ParserError(
                    "incomplete math expression",
                    tokens.ElementAt(index - 1).Span);

                exprStack.Add(new GarbageExpression(tokens.ElementAt(index - 1).Span));
                
                exprStack.Add(new GarbageExpression(tokens.ElementAt(index - 1).Span));

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

                switch (_op) {

                    case OperatorExpression oe: {

                        var span = new Span(
                            fileId: _lhs.GetSpan().FileId,
                            start: _lhs.GetSpan().Start,
                            end: _rhs.GetSpan().End);

                        exprStack.Add(new BinaryOpExpression(_lhs, oe.Operator, _rhs, span));

                        break;
                    }

                    ///

                    default: {

                        throw new Exception("internal error: operator is not an operator");
                    }
                }
            }

            exprStack.Add(op);

            exprStack.Add(rhs);

            lastPrecedence = precedence;
        }

        while (exprStack.Count != 1) {

            var _rhs = exprStack.Pop();

            var _op = exprStack.Pop();

            var _lhs = exprStack.Pop();

            // exprStack.Add(new BinaryOpExpression(_lhs, _op, _rhs));

            switch (_op) {

                case OperatorExpression oe: {

                    var span = new Span(
                        fileId: _lhs.GetSpan().FileId,
                        start: _lhs.GetSpan().Start,
                        end: _rhs.GetSpan().End);

                    exprStack.Add(new BinaryOpExpression(_lhs, oe.Operator, _rhs, span));

                    break;
                }

                ///

                default: {

                    throw new Exception("internal error: operator is not an operator");
                }
            }
        }

        var output = exprStack.Pop();

        return (output, error);
    }

    ///

    public static (Expression, Error?) ParseOperand(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperand: {tokens.ElementAt(index)}");

        Error? error = null;

        var span = tokens.ElementAt(index).Span;

        Expression? expr = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "true": {

                index += 1;

                expr = new BooleanExpression(true, span);

                break;
            }

            ///

            case NameToken nt when nt.Value == "false": {

                index += 1;

                expr = new BooleanExpression(false, span);

                break;
            }

            ///

            case NameToken nt: {

                if (index + 1 < tokens.Count) {

                    switch (tokens.ElementAt(index + 1)) {

                        case LParenToken _: {

                            switch (nt.Value) {

                                case "Some": {

                                    index += 1;

                                    var (someExpr, parseSomeExprErr) = ParseExpression(
                                        tokens, 
                                        ref index, 
                                        ExpressionKind.ExpressionWithoutAssignment);

                                    error = error ?? parseSomeExprErr;

                                    expr = new OptionalSomeExpression(someExpr, span);

                                    break;
                                }

                                ///

                                default: {

                                    var (call, parseCallErr) = ParseCall(tokens, ref index);

                                    error = error ?? parseCallErr;

                                    expr = new CallExpression(call, span);

                                    break;
                                }

                            }

                            break;
                        }

                        ///

                        default: {

                            index += 1;

                            switch (nt.Value) {

                                case "None": {

                                    expr = new OptionalNoneExpression(span);

                                    break;
                                }

                                ///

                                default: {

                                    expr = new VarExpression(nt.Value, span);

                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
                else {

                    index += 1;

                    expr = new VarExpression(nt.Value, span);
                }

                break;
            }

            ///

            case LParenToken _: {

                index += 1;

                var (_expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? exprErr;

                switch (tokens.ElementAt(index)) {

                    case RParenToken _: {

                        index += 1;

                        break;
                    }

                    ///

                    default: {

                        Trace("ERROR: expected ')'");

                        error = error ?? new ParserError(
                            "expected ')'", 
                            tokens.ElementAt(index).Span);

                        break;
                    }
                }

                expr = _expr;

                break;
            }

            ///

            case LSquareToken _: {

                var (_expr, exprErr) = ParseVector(tokens, ref index);

                error = error ?? exprErr;

                expr = _expr;

                break;
            }

            ///

            case PlusPlusToken _: {
                
                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, parseExprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? parseExprErr;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, UnaryOperator.PreIncrement, _span);

                break;
            }

            ///

            case MinusMinusToken _: {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, parseExprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? parseExprErr;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, UnaryOperator.PreDecrement, _span);

                break;
            }

            ///

            case NumberToken numTok: {

                index += 1;

                expr = new Int64Expression(numTok.Value, span);

                break;
            }

            ///

            case QuotedStringToken qs: {

                index += 1;

                expr = new QuotedStringExpression(qs.Value, span);

                break;
            }

            ///

            default: {

                Trace("ERROR: unsupported expression");

                error = error ??
                    new ParserError(
                        "unsupported expression",
                        tokens.ElementAt(index).Span);

                expr = new GarbageExpression(span);

                break;
            }
        }

        // Check for postfix operators, while we're at it

        // if (index < tokens.Count) {

        var cont = true;

        while (cont && index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case ExclamationToken _: {

                    index += 1;

                    // Forced Optional unwrap

                    expr = new ForcedUnwrapExpression(expr, span);

                    break;
                }

                ///

                case PlusPlusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new UnaryOpExpression(expr, UnaryOperator.PostIncrement, _span);

                    break;
                }

                ///

                case MinusMinusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new UnaryOpExpression(expr, UnaryOperator.PostDecrement, _span);

                    break;
                }

                ///

                case LSquareToken _: {

                    index += 1;

                    if (index < tokens.Count) {

                        var (idx, parseExprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                        error = error ?? parseExprErr;

                        var end = index;

                        if (index < tokens.Count) {

                            switch (tokens.ElementAt(index)) {

                                case RSquareToken _: {

                                    index += 1;

                                    break;
                                }

                                ///

                                case var t: {

                                    error = error ?? 
                                        new ParserError(
                                            "expected ']'", 
                                            t.Span);

                                    break;
                                }
                            }
                        }
                        else {

                            end -= 1;

                            error = error ?? 
                                new ParserError(
                                    "expected ']'",
                                    tokens.ElementAt(index - 1).Span);
                        }

                        expr = new IndexedExpression(
                            expr, 
                            idx, 
                            new Span(
                                span.FileId, 
                                start: span.Start, 
                                end: end));
                    }

                    break;
                }

                ///

                default: {

                    cont = false;

                    break;
                }
            }
        }

        return (expr, error);
    }

    ///

    // public static (Expression, Error?) ParseOperatorForKind(
    //     List<Token> tokens,
    //     ref int index,
    //     ExpressionKind exprKind) {

    //     switch (exprKind) {

    //         case ExpressionKind.ExpressionWithAssignments: 

    //             return ParseOperatorWithAssignment(tokens, ref index);

    //         case ExpressionKind.ExpressionWithoutAssignment:

    //             return ParseOperator(tokens, ref index);

    //         default:

    //             throw new Exception();
    //     }
    // }

    public static (Expression, Error?) ParseOperator(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperator: {tokens.ElementAt(index)}");

        var span = tokens.ElementAt(index).Span;

        switch (tokens.ElementAt(index)) {

            case PlusToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Add, span), null);
            }

            case MinusToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Subtract, span), null);
            }

            case AsteriskToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Multiply, span), null);
            }

            case ForwardSlashToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Divide, span), null);
            }

            case EqualToken _: {

                Trace("ERROR: assignment not allowed in this position");
                
                index += 1;
                
                return (
                    new OperatorExpression(BinaryOperator.Assign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case PlusEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.AddAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }
            
            case MinusEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.SubtractAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case AsteriskEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.MultiplyAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case ForwardSlashEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.DivideAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case DoubleEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.Equal, span), null);
            }
            
            case NotEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.NotEqual, span), null);
            }
            
            case LessThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.LessThan, span), null);
            }
            
            case LessThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.LessThanOrEqual, span), null);
            }
            
            case GreaterThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.GreaterThan, span), null);
            }
            
            case GreaterThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.GreaterThanOrEqual, span), null);
            }

            ///

            default: {

                Trace("ERROR: unsupported operator (possibly just the end of an expression)");

                return (
                    new GarbageExpression(span),
                    new ParserError(
                        "unsupported operator", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    public static (Expression, Error?) ParseOperatorWithAssignment(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperatorWithAssignment: {tokens.ElementAt(index)}");

        var span = tokens.ElementAt(index).Span;

        switch (tokens.ElementAt(index)) {

            case PlusToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Add, span), null);
            }

            case MinusToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Subtract, span), null);
            }

            case AsteriskToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Multiply, span), null);
            }

            case ForwardSlashToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Divide, span), null);
            }

            case EqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.Assign, span), null);
            }

            case PlusEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.AddAssign, span), null);
            }
            
            case MinusEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.SubtractAssign, span), null);
            }

            case AsteriskEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.MultiplyAssign, span), null);
            }

            case ForwardSlashEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.DivideAssign, span), null);
            }

            case DoubleEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.Equal, span), null);
            }
            
            case NotEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.NotEqual, span), null);
            }
            
            case LessThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.LessThan, span), null);
            }
            
            case LessThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.LessThanOrEqual, span), null);
            }
            
            case GreaterThanToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.GreaterThan, span), null);
            }
            
            case GreaterThanOrEqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.GreaterThanOrEqual, span), null);
            }

            ///

            default: {

                Trace("ERROR: unsupported operator (possibly just the end of an expression)");

                return (
                    new GarbageExpression(span),
                    new ParserError(
                        "unsupported operator", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    ///

    public static (Expression, Error?) ParseVector(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        var output = new List<Expression>();

        var start = index;

        if (index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case LSquareToken _: {

                    index += 1;

                    break;
                }

                ///

                default: {

                    Trace($"Error: expected '[");

                    error = error ?? 
                        new ParserError(
                            "expected '(", 
                            tokens.ElementAt(index).Span);

                    break;
                }
            }
        }
        else {

            start -= 1;

            Trace($"ERROR: incomplete function");

            error = error ?? 
                new ParserError(
                    "incomplete function", 
                    tokens.ElementAt(index - 1).Span);
        }

        ///

        var cont = true;

        while (cont && index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case RSquareToken _: {

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

                    var (expr, err) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                    error = error ?? err;

                    output.Add(expr);

                    break;
                }
            }
        }

        ///

        var end = index;

        if (index >= tokens.Count) {

            Trace("ERROR: incomplete function");

            error = error ?? new ParserError(
                "incomplete function",
                tokens.ElementAt(index - 1).Span);

            end -= 1;
        }

        ///

        return (
            new VectorExpression(
                expressions: output,
                new Span(
                    fileId: tokens.ElementAt(start).Span.FileId,
                    start: tokens.ElementAt(start).Span.Start,
                    end: tokens.ElementAt(end).Span.End)),
            error);
    }

    ///

    public static (VarDecl, Error?) ParseVariableDeclaration(
        List<Token> tokens,
        ref int index) {

        Trace($"ParseVariableDeclaration: {tokens.ElementAt(index)}");

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
                                new VarDecl(
                                    name: nt.Value, 
                                    type: new UnknownType(), 
                                    mutable: false,
                                    span: tokens.ElementAt(index - 1).Span),
                                null);
                        }
                    }
                }
                else {

                    return (
                        new VarDecl(
                            name: nt.Value, 
                            type: new UnknownType(), 
                            mutable: false, 
                            span: tokens.ElementAt(index - 1).Span), 
                        null);
                }
            
                if (index < tokens.Count) {

                    var (varType, typeErr) = ParseTypeName(tokens, ref index);

                    error = error ?? typeErr;

                    var result = new VarDecl(
                        name: varName, 
                        type: varType, 
                        mutable: false,
                        span: tokens.ElementAt(index - 3).Span);

                    index += 1;

                    return (result, error);
                }
                else {

                    Trace("ERROR: expected type");

                    return (
                        new VarDecl(
                            nt.Value, 
                            new UnknownType(), 
                            mutable: false,
                            span: tokens.ElementAt(index - 2).Span), 
                        new ParserError(
                            "expected type", 
                            tokens.ElementAt(index).Span));
                }
            }

            ///

            default: {

                Trace("ERROR: expected name");

                return (
                    new VarDecl(tokens.ElementAt(index).Span),
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

        Trace($"ParseTypeName: {tokens.ElementAt(index)}");

        Error? error = null;

        NeuType baseType = new VoidType();

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                switch (nt.Value) {

                    case "Int8": {

                        baseType = new Int8Type();

                        break;
                    }

                    ///

                    case "Int16": {

                        baseType = new Int16Type();

                        break;
                    }

                    ///

                    case "Int32": {

                        baseType = new Int32Type();

                        break;
                    }

                    ///

                    case "Int64": {

                        baseType = new Int64Type();

                        break;
                    }

                    ///

                    case "UInt8": {

                        baseType = new UInt8Type();

                        break;
                    }

                    ///

                    case "UInt16": {

                        baseType = new UInt16Type();

                        break;
                    }

                    ///

                    case "UInt32": {

                        baseType = new UInt32Type();

                        break;
                    }

                    ///

                    case "UInt64": {

                        baseType = new UInt64Type();

                        break;
                    }

                    ///

                    case "Float": {

                        baseType = new FloatType();

                        break;
                    }

                    ///

                    case "Double": {

                        baseType = new DoubleType();

                        break;
                    }

                    ///

                    case "String": {

                        baseType = new StringType();

                        break;
                    }

                    ///

                    case "Bool": {

                        baseType = new BoolType();

                        break;
                    }

                    ///

                    default: {

                        Trace("ERROR: unknown type");

                        baseType = new VoidType();

                        error = error ?? 
                            new ParserError(
                                "unknown type", 
                                nt.Span);

                        break;
                    }
                }
            
                break;
            }

            ///

            default: {

                Trace("ERROR: expected type name");

                baseType = new VoidType();

                error = error ?? 
                    new ParserError(
                        "expected type name", 
                        tokens.ElementAt(index).Span);

                break;
            }
        }

        if (index + 1 < tokens.Count) {

            if (tokens.ElementAt(index + 1) is QuestionToken) {

                // T? is shorthand for Optional<T>

                index += 1;

                return (
                    new OptionalType(baseType),
                    error);
            }
        }

        return (baseType, error);
    }

    ///

    public static (String, Error?) ParseCallParameterName(List<Token> tokens, ref int index) {

        if (tokens.ElementAt(index) is NameToken nt) {

            if (index + 1 < tokens.Count) {

                if (tokens.ElementAt(index + 1) is ColonToken) {

                    index += 2;

                    return (
                        nt.Value, 
                        null);
                }
            }
        }

        return (
            String.Empty,
            null);
    }

    ///

    public static (Call, Error?) ParseCall(List<Token> tokens, ref int index) {

        Trace($"ParseCall: {tokens.ElementAt(index)}");

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

                            Trace("ERROR: expected '('");

                            return (
                                call,
                                new ParserError("expected '('", t.Span));
                        }
                    }
                }
                else {

                    Trace("ERROR: incomplete function");

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

                            var (paramName, parseCallParamNameErr) = ParseCallParameterName(tokens, ref index);
                            
                            error = error ?? parseCallParamNameErr;

                            var (expr, exprError) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                            error = error ?? exprError;

                            call.Args.Add((paramName, expr));

                            break;
                        }
                    }
                }

                if (index >= tokens.Count) {

                    Trace("ERROR: incomplete call");

                    error = error ?? new ParserError(
                        "incomplete call", 
                        tokens.ElementAt(index - 1).Span);
                }

                break;
            }

            ///

            default: {

                Trace("ERROR: expected function call");

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

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

    public List<String> Namespace { get; set; }

    public String Name { get; set; }

    public List<(String, Expression)> Args { get; init; }

    ///

    public Call()
        : this(new List<String>(), String.Empty, new List<(string, Expression)>()) { }

    public Call(
        List<String> ns,
        String name,
        List<(String, Expression)> args) {

        this.Namespace = ns;
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

public partial class UncheckedType {

    public UncheckedType() { }
}

    public partial class UncheckedNameType: UncheckedType {

        public String Name { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedNameType(
            String name,
            Span span) {

            this.Name = name;
            this.Span = span;
        }
    }

    public partial class UncheckedGenericType: UncheckedType {

        public String Name { get; init; }

        public List<UncheckedType> Types { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedGenericType(
            String name,
            List<UncheckedType> types,
            Span span) { 

            this.Name = name;
            this.Types = types;
            this.Span = span;
        }
    }

    public partial class UncheckedArrayType: UncheckedType {

        public UncheckedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedArrayType(
            UncheckedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class UncheckedOptionalType: UncheckedType {

        public UncheckedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedOptionalType(
            UncheckedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }
    
    public partial class UncheckedRawPointerType: UncheckedType {

        public UncheckedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedRawPointerType(
            UncheckedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class UncheckedEmptyType: UncheckedType {

        public UncheckedEmptyType() { }
    }


public static partial class UncheckedTypeFunctions {

    public static bool Eq(UncheckedType l, UncheckedType r) {

        switch (true) {

            case var _ when
                l is UncheckedNameType ln
                && r is UncheckedNameType rn: {

                return ln.Name == rn.Name
                    && SpanFunctions.Eq(ln.Span, rn.Span);
            }

            case var _ when
                l is UncheckedGenericType lg
                && r is UncheckedGenericType rg: {

                if (lg.Name != rg.Name) {

                    return false;
                }

                if (lg.Types.Count != rg.Types.Count) {

                    return false;
                }

                for (var i = 0; i < lg.Types.Count; i++) {

                    if (!UncheckedTypeFunctions.Eq(lg.Types[i], rg.Types[i])) {

                        return false;
                    }
                }

                return SpanFunctions.Eq(lg.Span, rg.Span);
            }

            case var _ when 
                l is UncheckedArrayType la
                && r is UncheckedArrayType ra: {

                return UncheckedTypeFunctions.Eq(la.Type, ra.Type)
                    && SpanFunctions.Eq(la.Span, ra.Span);
            }
            
            case var _ when 
                l is UncheckedOptionalType lo
                && r is UncheckedOptionalType ro: {

                return Eq(lo.Type, ro.Type)
                    && SpanFunctions.Eq(lo.Span, ro.Span);
            }

            case var _ when 
                l is UncheckedRawPointerType lp
                && r is UncheckedRawPointerType rp: {

                return Eq(lp.Type, rp.Type)
                    && SpanFunctions.Eq(lp.Span, rp.Span);
            }

            case var _ when 
                l is UncheckedEmptyType le
                && r is UncheckedEmptyType re: {

                return true; // ?
            }

            default: {

                return false;
            }
        }
    }
}

///

public partial class VarDecl {

    public String Name { get; init; }

    public UncheckedType Type { get; set; }

    public bool Mutable { get; set; }

    public Span Span { get; init; }

    ///

    public VarDecl(Span span)
        : this(String.Empty, new UncheckedEmptyType(), false, span) { }

    public VarDecl(
        String name,
        UncheckedType type,
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
            && UncheckedTypeFunctions.Eq(l.Type, r.Type)
            && l.Mutable == r.Mutable;
    }
}

///

public partial class ParsedFile {

    public List<Function> Functions { get; init; }

    public List<Struct> Structs { get; init; }

    ///

    public ParsedFile()
        : this(new List<Function>(), new List<Struct>()) { }

    public ParsedFile(
        List<Function> functions,
        List<Struct> structs) {

        this.Functions = functions;
        this.Structs = structs;
    }
}

///

public partial class Struct {
    
    public String Name { get; init; }

    public List<(String, Span)> GenericParameters { get; init; }

    public List<VarDecl> Fields { get; init; }

    public List<Function> Methods { get; init; }

    public Span Span { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; } 

    ///

    public Struct(
        String name,
        List<(String, Span)> genericParameters,
        List<VarDecl> fields,
        List<Function> methods,
        Span span,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Fields = fields;
        this.Methods = methods;
        this.Span = span;
        this.DefinitionLinkage = definitionLinkage;
        this.DefinitionType = definitionType;
    }
}

///

public enum FunctionLinkage { 

    Internal,
    External,
    ImplicitConstructor
}

public enum DefinitionLinkage {

    Internal,
    External
}

public enum DefinitionType {

    Class,
    Struct
}

public partial class Function {

    public String Name { get; init; }

    public Span NameSpan { get; init; }

    public List<Parameter> Parameters { get; init; }

    public List<(String, Span)> GenericParameters { get; init; }

    public Block Block { get; init; }

    public UncheckedType ReturnType { get; init; }

    public FunctionLinkage Linkage { get; init; }

    ///

    public Function(
        FunctionLinkage linkage)
        : this(
            String.Empty,
            new Span(
                fileId: 0, 
                start: 0, 
                end: 0),
            new List<Parameter>(), 
            new List<(String, Span)>(),
            new Block(), 
            returnType: 
                new UncheckedEmptyType(),
            linkage) { }

    public Function(
        String name,
        Span nameSpan,
        List<Parameter> parameters,
        List<(String, Span)> genericParameters,
        Block block,
        UncheckedType returnType,
        FunctionLinkage linkage) {

        this.Name = name;
        this.NameSpan = nameSpan;
        this.Parameters = parameters;
        this.GenericParameters = genericParameters;
        this.Block = block;
        this.ReturnType = returnType;
        this.Linkage = linkage;
    }
}

///

public partial class Variable {

    public String Name { get; init; }

    public UncheckedType Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public Variable(
        String name,
        UncheckedType ty,
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
                l is ExpressionStatement le
                && r is ExpressionStatement re:

                return ExpressionStatementFunctions.Eq(le, re);

            case var _ when
                l is DeferStatement deferL
                && r is DeferStatement deferR:

                return DeferStatementFunctions.Eq(deferL, deferR);

            ///

            case var _ when
                l is UnsafeBlockStatement lu
                && r is UnsafeBlockStatement ru:

                return UnsafeBlockStatementFunctions.Eq(lu, ru);

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
                l is LoopStatement ll
                && r is LoopStatement rl:
                
                return LoopStatementFunctions.Eq(ll, rl);

            case var _ when
                l is WhileStatement whileL
                && r is WhileStatement whileR:

                return WhileStatementFunctions.Eq(whileL, whileR);

            ///

            case var _ when 
                l is ForStatement lf
                && r is ForStatement rf: {

                return ForStatementFunctions.Eq(lf, rf);
            }

            ///

            case var _ when
                l is BreakStatement lb
                && r is BreakStatement rb: {

                return BreakStatementFunctions.Eq(lb, rb);
            }

            ///

            case var _ when
                l is ContinueStatement lc
                && r is ContinueStatement rc: {

                return ContinueStatementFunctions.Eq(lc, rc);
            }

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

public partial class ExpressionStatement: Statement {

    public Expression Expression { get; init; }

    ///

    public ExpressionStatement(
        Expression expression) {

        this.Expression = expression;
    }
}

public static partial class ExpressionStatementFunctions {

    public static bool Eq(ExpressionStatement le, ExpressionStatement re) {

        return ExpressionFunctions.Eq(le.Expression, re.Expression);
    }
}

///

public partial class DeferStatement: Statement {

    public Statement Statement { get; init; }

    ///

    public DeferStatement(
        Statement statement)
        : base() { 

        this.Statement = statement;
    }
}

public static partial class DeferStatementFunctions {

    public static bool Eq(
        DeferStatement? l,
        DeferStatement? r) {

        return StatementFunctions.Eq(l?.Statement, r?.Statement);
    }
}

///

public partial class UnsafeBlockStatement: Statement {

    public Block Block { get; init; }

    ///

    public UnsafeBlockStatement(
        Block block) {

        this.Block = block;
    }
}

public static partial class UnsafeBlockStatementFunctions {

    public static bool Eq(
        UnsafeBlockStatement? l,
        UnsafeBlockStatement? r) {

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

public partial class LoopStatement: Statement {

    public Block Block { get; init; }

    public LoopStatement(
        Block block) {

        this.Block = block;
    }
}

public static partial class LoopStatementFunctions {

    public static bool Eq(
        LoopStatement? l,
        LoopStatement? r) {

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

public partial class ForStatement: Statement {

    public String IteratorName { get; init; }

    public Expression Range { get; init; }

    public Block Block { get; init; }

    ///

    public ForStatement(
        String iteratorName,
        Expression range,
        Block block) {

        this.IteratorName = iteratorName;
        this.Range = range;
        this.Block = block;
    }
}

public static partial class ForStatementFunctions {

    public static bool Eq(
        ForStatement? l,
        ForStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ForStatement _l = l!;
        ForStatement _r = r!;

        ///

        return 
            _l.IteratorName == _r.IteratorName
            && ExpressionFunctions.Eq(_l.Range, _r.Range)
            && BlockFunctions.Eq(_l.Block, _r.Block);
    }
}

///

public partial class BreakStatement: Statement {

    public BreakStatement() { }
}

public static partial class BreakStatementFunctions {

    public static bool Eq(
        BreakStatement? l,
        BreakStatement? r) {

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

public partial class ContinueStatement: Statement {

    public ContinueStatement() { }
}

public static partial class ContinueStatementFunctions {

    public static bool Eq(
        ContinueStatement? l,
        ContinueStatement? r) {

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

public partial class Expression {

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

    public partial class NumericConstantExpression: Expression {

        public NumericConstant Value { get; init; }

        public Span Span { get; init; }

        ///

        public NumericConstantExpression(
            NumericConstant value,
            Span span) {

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

    public partial class CharacterLiteralExpression: Expression {

        public Char Char { get; init; }

        public Span Span { get; init; }

        ///

        public CharacterLiteralExpression(
            Char c,
            Span span) {

            this.Char = c;
            this.Span = span;
        }
    }

    public partial class ArrayExpression: Expression {

        public List<Expression> Expressions { get; init; }

        public Expression? FillSize { get; init; }

        public Span Span { get; init; }

        ///

        public ArrayExpression(
            List<Expression> expressions,
            Expression? fillSize,
            Span span) {

            this.Expressions = expressions;
            this.FillSize = fillSize;
            this.Span = span;
        }
    }

    public partial class DictionaryExpression: Expression {

        public List<(Expression, Expression)> Entries { get; init; }

        public Span Span { get; init; }

        ///

        public DictionaryExpression(
            List<(Expression, Expression)> entries,
            Span span) {

            this.Entries = entries;
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
    
    public partial class TupleExpression: Expression {

        public List<Expression> Expressions { get; init; }

        public Span Span { get; init; }

        ///

        public TupleExpression(
            List<Expression> expressions,
            Span span) {

            this.Expressions = expressions;
            this.Span = span;
        }
    }

    public partial class RangeExpression: Expression {

        public Expression Start { get; init; }

        public Expression End { get; init; }

        public Span Span { get; init; }

        ///

        public RangeExpression(
            Expression start,
            Expression end,
            Span span) {

            this.Start = start;
            this.End = end;
            this.Span = span;
        }
    }

    public partial class IndexedTupleExpression: Expression {

        public Expression Expression { get; init; }

        public Int64 Index { get; init; }

        public Span Span { get; init; }

        ///

        public IndexedTupleExpression(
            Expression expression,
            Int64 index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class IndexedStructExpression: Expression {

        public Expression Expression { get; init; }
        
        public String Name { get; init; }
        
        public Span Span { get; init; }

        ///

        public IndexedStructExpression(
            Expression expression,
            String name,
            Span span) {

            this.Expression = expression;
            this.Name = name;
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

    public partial class MethodCallExpression: Expression {
        
        public Expression Expression { get; init; }
        
        public Call Call { get; init; }
        
        public Span Span { get; init; }

        ///

        public MethodCallExpression(
            Expression expression, 
            Call call, 
            Span span) {

            this.Expression = expression;
            this.Call = call;
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

            case NumericConstantExpression ne: {

                return ne.Span;
            }

            case QuotedStringExpression qse: {

                return qse.Span;
            }

            case CharacterLiteralExpression cle: {

                return cle.Span;
            }

            case ArrayExpression ve: {

                return ve.Span;
            }

            case DictionaryExpression de: {

                return de.Span;
            }

            case TupleExpression te: {

                return te.Span;
            }

            case RangeExpression re: {

                return re.Span;
            }

            case IndexedExpression ie: {

                return ie.Span;
            }

            case IndexedTupleExpression ite: {

                return ite.Span;
            }

            case IndexedStructExpression ise: {

                return ise.Span;
            }

            case CallExpression ce: {

                return ce.Span;
            }

            case MethodCallExpression mce: {

                return mce.Span;
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

            // case var _ when
            //     l is BooleanExpression lb
            //     && r is BooleanExpression rb: {

            //     return lb.Value == rb.Value
            //         && SpanFunctions.Eq(lb.Span, rb.Span);
            // }

            // case var _ when 
            //     l is NumericConstantExpression ln
            //     && r is NumericConstantExpression rn: {

            //     return NumericConstantFunctions.Eq(ln.Value, rn.Value)
            //         && SpanFunctions.Eq(ln.Span, rn.Span);
            // }

            // case var _ when
            //     l is QuotedStringExpression ls
            //     && r is QuotedStringExpression rs: {

            //     return Equals(ls.Value, rs.Value)
            //         && SpanFunctions.Eq(ls.Span, rs.Span);
            // }

            // case var _ when 
            //     l is CharacterLiteralExpression lc
            //     && r is CharacterLiteralExpression rc: {

            //     return lc.Char == rc.Char
            //         && SpanFunctions.Eq(lc.Span, rc.Span);
            // }

            // case var _ when 
            //     l is ArrayExpression la
            //     && r is ArrayExpression ra: {
                
            //     if (la.Expressions.Count != ra.Expressions.Count) {

            //         return false;
            //     }

            //     for (var i = 0; i < la.Expressions.Count; i++) {

            //         if (!ExpressionFunctions.Eq(la.Expressions[i], ra.Expressions[i])) {

            //             return false;
            //         }
            //     }

            //     if (!ExpressionFunctions.Eq(la.FillSize, ra.FillSize)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(la.Span, ra.Span);
            // }

            // case var _ when
            //     l is DictionaryExpression ld
            //     && r is DictionaryExpression rd: {

            //     if (ld.Entries.Count != rd.Entries.Count) {

            //         return false;
            //     }

            //     for (var i = 0; i < ld.Entries.Count; i++) {

            //         if (!ExpressionFunctions.Eq(ld.Entries[i].Item1, rd.Entries[i].Item1)) {

            //             return false;
            //         }

            //         if (!ExpressionFunctions.Eq(ld.Entries[i].Item2, rd.Entries[i].Item2)) {

            //             return false;
            //         }
            //     }

            //     return SpanFunctions.Eq(ld.Span, rd.Span);
            // }

            // case var _ when 
            //     l is UnaryOpExpression lu
            //     && r is UnaryOpExpression ru: {

            //     if (!ExpressionFunctions.Eq(lu.Expression, ru.Expression)) {

            //         return false;
            //     }

            //     if (!UnaryOperatorFunctions.Eq(lu.Operator, ru.Operator)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lu.Span, ru.Span);
            // }

            // case var _ when
            //     l is BinaryOpExpression lbo
            //     && r is BinaryOpExpression rbo: {

            //     if (!ExpressionFunctions.Eq(lbo.Lhs, rbo.Lhs)) {

            //         return false;
            //     }

            //     if (lbo.Operator != rbo.Operator) {

            //         return false;
            //     }

            //     if (!ExpressionFunctions.Eq(lbo.Rhs, rbo.Rhs)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lbo.Span, rbo.Span);
            // }

            // case var _ when
            //     l is VarExpression lv
            //     && r is VarExpression rv: {

            //     if (lv.Value != rv.Value) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lv.Span, rv.Span);
            // }

            // case var _ when
            //     l is TupleExpression lt
            //     && r is TupleExpression rt: {

            //     if (lt.Expressions.Count != rt.Expressions.Count) {

            //         return false;
            //     }

            //     for (var i = 0; i < lt.Expressions.Count; i++) {

            //         if (!ExpressionFunctions.Eq(lt.Expressions[i], rt.Expressions[i])) {

            //             return false;
            //         }
            //     }

            //     return SpanFunctions.Eq(lt.Span, rt.Span);
            // }

            // case var _ when
            //     l is RangeExpression lr
            //     && r is RangeExpression rr: {

            //     if (!ExpressionFunctions.Eq(lr.Start, rr.Start)) {

            //         return false;
            //     }

            //     if (!ExpressionFunctions.Eq(lr.End, rr.End)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lr.Span, rr.Span);
            // }

            // case var _ when
            //     l is IndexedTupleExpression lt
            //     && r is IndexedTupleExpression rt: {

            //     if (!ExpressionFunctions.Eq(lt.Expression, rt.Expression)) {

            //         return false;
            //     }

            //     if (lt.Index != rt.Index) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lt.Span, rt.Span);
            // }

            // case var _ when
            //     l is IndexedStructExpression ls
            //     && r is IndexedStructExpression rs: {

            //     if (!ExpressionFunctions.Eq(ls.Expression, rs.Expression)) {

            //         return false;
            //     }

            //     if (!Equals(ls.Name, rs.Name)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(ls.Span, rs.Span);
            // }

            // case var _ when
            //     l is CallExpression lc
            //     && r is CallExpression rc: {

            //     if (!CallFunctions.Eq(lc.Call, rc.Call)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lc.Span, rc.Span);
            // }

            // case var _ when
            //     l is MethodCallExpression lm
            //     && r is MethodCallExpression rm: {

            //     if (!ExpressionFunctions.Eq(lm.Expression, rm.Expression)) {

            //         return false;
            //     }

            //     if (!CallFunctions.Eq(lm.Call, rm.Call)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lm.Span, rm.Span);
            // }

            // case var _ when
            //     l is ForcedUnwrapExpression lf
            //     && r is ForcedUnwrapExpression rf: {

            //     if (!ExpressionFunctions.Eq(lf.Expression, rf.Expression)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lf.Span, rf.Span);
            // }

            // case var _ when
            //     l is OptionalNoneExpression ln
            //     && r is OptionalNoneExpression rn: {

            //     return SpanFunctions.Eq(ln.Span, rn.Span);
            // }

            // case var _ when
            //     l is OptionalSomeExpression ls
            //     && r is OptionalSomeExpression rs: {

            //     if (!ExpressionFunctions.Eq(ls.Expression, rs.Expression)) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(ls.Span, rs.Span);
            // }

            // case var _ when
            //     l is OperatorExpression lo
            //     && r is OperatorExpression ro: {

            //     if (lo.Operator != ro.Operator) {

            //         return false;
            //     }

            //     return SpanFunctions.Eq(lo.Span, ro.Span);
            // }

            // case var _ when
            //     l is GarbageExpression lg
            //     && r is GarbageExpression rg: {

            //     return SpanFunctions.Eq(lg.Span, rg.Span);
            // }

            // default: {

            //     return false;
            // }


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
                l is NumericConstantExpression ln
                && r is NumericConstantExpression rn:

                return NumericConstantFunctions.Eq(ln.Value, rn.Value);

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

            // case var _ when 
            //     l is CharacterLiteralExpression lc
            //     && r is CharacterLiteralExpression rc:

            //     return Equals(lc.Char, rc.Char);
            
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

    public static UInt64 Precedence(
        this Expression expr) {

        switch (expr) {

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Multiply 
                || opExpr.Operator == BinaryOperator.Modulo
                || opExpr.Operator == BinaryOperator.Divide:

                return 100;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Add 
                || opExpr.Operator == BinaryOperator.Subtract:

                return 90;

            ///

            case OperatorExpression opExpr when
                opExpr.Operator == BinaryOperator.BitwiseLeftShift
                || opExpr.Operator == BinaryOperator.BitwiseRightShift
                || opExpr.Operator == BinaryOperator.ArithmeticLeftShift
                || opExpr.Operator == BinaryOperator.ArithmeticRightShift: 
                
                return 85;

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
                opExpr.Operator == BinaryOperator.BitwiseAnd:

                return 73;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.BitwiseXor:

                return 72;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.BitwiseOr:

                return 72;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LogicalAnd:

                return 70;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LogicalOr:

                return 69;

            ///

            case OperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Assign
                || opExpr.Operator == BinaryOperator.BitwiseAndAssign
                || opExpr.Operator == BinaryOperator.BitwiseOrAssign
                || opExpr.Operator == BinaryOperator.BitwiseXorAssign
                || opExpr.Operator == BinaryOperator.BitwiseLeftShiftAssign
                || opExpr.Operator == BinaryOperator.BitwiseRightShiftAssign
                || opExpr.Operator == BinaryOperator.AddAssign
                || opExpr.Operator == BinaryOperator.SubtractAssign
                || opExpr.Operator == BinaryOperator.MultiplyAssign
                || opExpr.Operator == BinaryOperator.ModuloAssign
                || opExpr.Operator == BinaryOperator.DivideAssign:

                return 50;

            ///

            default:

                return 0;
        }
    }
}

public partial class TypeCast {

    public UncheckedType Type { get; init; }

    ///

    public TypeCast(
        UncheckedType type) {

        this.Type = type;
    }
}

    public partial class FallibleTypeCast: TypeCast {

        public FallibleTypeCast(UncheckedType type)
            : base(type) { }
    }

    public partial class InfallibleTypeCast: TypeCast {

        public InfallibleTypeCast(UncheckedType type)
            : base(type) { }
    }

    public partial class SaturatingTypeCast: TypeCast {

        public SaturatingTypeCast(UncheckedType type)
            : base(type) { }
    }
    
    public partial class TruncatingTypeCast: TypeCast {

        public TruncatingTypeCast(UncheckedType type)
            : base(type) { }
    }

public static partial class TypeCastFunctions {

    public static bool Eq(TypeCast l, TypeCast r) {

        switch (true) {

            case var _ when l is FallibleTypeCast lf && r is FallibleTypeCast rf:
                return UncheckedTypeFunctions.Eq(lf.Type, rf.Type);

            case var _ when l is InfallibleTypeCast li && r is InfallibleTypeCast ri:
                return UncheckedTypeFunctions.Eq(li.Type, ri.Type);

            case var _ when l is SaturatingTypeCast ls && r is SaturatingTypeCast rs:
                return UncheckedTypeFunctions.Eq(ls.Type, rs.Type);

            case var _ when l is TruncatingTypeCast lt && r is TruncatingTypeCast rt:
                return UncheckedTypeFunctions.Eq(lt.Type, rt.Type);

            default:
                return false;
        }
    }

    public static UncheckedType GetUncheckedType(
        this TypeCast typeCast) {

        switch (typeCast) {

            case FallibleTypeCast f: {

                return f.Type;
            }

            case InfallibleTypeCast i: {

                return i.Type;
            }

            case SaturatingTypeCast s: {

                return s.Type;
            }

            case TruncatingTypeCast t: {

                return t.Type;
            }

            default: {

                throw new Exception();
            }
        }
    }
}

public partial class UnaryOperator {

    public UnaryOperator() { }
}

    public partial class PreIncrementUnaryOperator: UnaryOperator {

        public PreIncrementUnaryOperator() { }
    }

    public partial class PostIncrementUnaryOperator: UnaryOperator {

        public PostIncrementUnaryOperator() { }
    }

    public partial class PreDecrementUnaryOperator: UnaryOperator {

        public PreDecrementUnaryOperator() { }
    }

    public partial class PostDecrementUnaryOperator: UnaryOperator {

        public PostDecrementUnaryOperator() { }
    }

    public partial class NegateUnaryOperator: UnaryOperator {

        public NegateUnaryOperator() { }
    }

    public partial class DereferenceUnaryOperator: UnaryOperator {

        public DereferenceUnaryOperator() { }
    }

    public partial class RawAddressUnaryOperator: UnaryOperator {

        public RawAddressUnaryOperator() { }
    }

    public partial class LogicalNotUnaryOperator: UnaryOperator {

        public LogicalNotUnaryOperator() { }
    }

    public partial class BitwiseNotUnaryOperator: UnaryOperator {

        public BitwiseNotUnaryOperator() { }
    }

    public partial class TypeCastUnaryOperator: UnaryOperator {

        public TypeCast TypeCast { get; init; }

        ///

        public TypeCastUnaryOperator(TypeCast typeCast) {

            this.TypeCast = typeCast;
        }
    }

    public partial class IsUnaryOperator: UnaryOperator {

        public UncheckedType Type { get; init; }

        ///

        public IsUnaryOperator(
            UncheckedType type) {

            this.Type = type;
        }
    }

public static partial class UnaryOperatorFunctions {

    public static bool Eq(UnaryOperator? l, UnaryOperator? r) {

        if (l is null && r is null) {

            return true;
        }

        if (l is null || r is null) {

            return false;
        }

        switch (true) {

            case var _ when l is PreIncrementUnaryOperator && r is PreIncrementUnaryOperator:
            case var _ when l is PostIncrementUnaryOperator && r is PostIncrementUnaryOperator:
            case var _ when l is PreDecrementUnaryOperator && r is PreDecrementUnaryOperator:
            case var _ when l is PostDecrementUnaryOperator && r is PostDecrementUnaryOperator:
            case var _ when l is NegateUnaryOperator && r is NegateUnaryOperator:
            case var _ when l is DereferenceUnaryOperator && r is DereferenceUnaryOperator:
            case var _ when l is RawAddressUnaryOperator && r is RawAddressUnaryOperator:
            case var _ when l is LogicalNotUnaryOperator && r is LogicalNotUnaryOperator:
            case var _ when l is BitwiseNotUnaryOperator && r is BitwiseNotUnaryOperator:
                return true;

            case var _ when l is TypeCastUnaryOperator lt && r is TypeCastUnaryOperator rt:
                return TypeCastFunctions.Eq(lt.TypeCast, rt.TypeCast);


            case var _ when l is IsUnaryOperator li && r is IsUnaryOperator ri:
                return UncheckedTypeFunctions.Eq(li.Type, ri.Type);

            default:
                return false;
        }
    }
}

///

public enum BinaryOperator {

    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    LogicalAnd,
    LogicalOr,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitwiseLeftShift,
    BitwiseRightShift,
    ArithmeticLeftShift,
    ArithmeticRightShift,
    Assign,
    AddAssign,
    SubtractAssign,
    MultiplyAssign,
    DivideAssign,
    ModuloAssign,
    BitwiseAndAssign,
    BitwiseOrAssign,
    BitwiseXorAssign,
    BitwiseLeftShiftAssign,
    BitwiseRightShiftAssign
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

        Trace($"ParseFile");

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

                            var (fun, err) = ParseFunction(tokens, ref index, FunctionLinkage.Internal);

                            error = error ?? err;

                            parsedFile.Functions.Add(fun);

                            break;
                        }

                        ///

                        case "struct": {

                            var (structure, err) = ParseStruct(
                                tokens, 
                                ref index,
                                DefinitionLinkage.Internal,
                                DefinitionType.Struct);

                            error = error ?? err;

                            parsedFile.Structs.Add(structure);

                            break;
                        }

                        ///

                        case "class": {

                            var (structure, err) = ParseStruct(
                                tokens,
                                ref index,
                                DefinitionLinkage.Internal,
                                DefinitionType.Class);

                            err = error ?? err;

                            parsedFile.Structs.Add(structure);
                            
                            break;
                        }

                        ///

                        case "extern": {

                            if (index + 1 < tokens.Count) {

                                switch (tokens.ElementAt(index + 1)) {

                                    case NameToken nt2: {

                                        switch (nt2.Value) {

                                            case "func": {

                                                index += 1;

                                                var (fun, err) = ParseFunction(
                                                    tokens, 
                                                    ref index, 
                                                    FunctionLinkage.External);

                                                error = error ?? err;

                                                parsedFile.Functions.Add(fun);

                                                break;
                                            }

                                            ///

                                            case "struct": {

                                                index += 1;

                                                var (structure, err) = ParseStruct(
                                                    tokens,
                                                    ref index,
                                                    DefinitionLinkage.External,
                                                    DefinitionType.Struct);

                                                error = error ?? err;

                                                parsedFile.Structs.Add(structure);

                                                break;
                                            }

                                            ///

                                            case "class": {

                                                index += 1;

                                                var (structure, err) = ParseStruct(
                                                    tokens,
                                                    ref index,
                                                    DefinitionLinkage.External,
                                                    DefinitionType.Class);

                                                error = error ?? err;

                                                parsedFile.Structs.Add(structure);

                                                break;
                                            }

                                            ///

                                            default: {

                                                Trace("ERROR: unexpected keyword");

                                                error = error ??
                                                    new ParserError(
                                                        "unexpected keyword",
                                                        nt2.Span);

                                                break;
                                            }
                                        }

                                        break;
                                    }

                                    ///

                                    default: {

                                        Trace("ERROR: unexpected keyword");

                                        error = error ??
                                            new ParserError(
                                                "unexpected keyword",
                                                nt.Span);

                                        break;
                                    }
                                }
                            }

                            break;
                        }

                        ///

                        default: {

                            Trace("ERROR: unexpected keyword");

                            index += 1;

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

                    index += 1;

                    error = error ?? new ParserError(
                        "unexpected token (expected keyword)", 
                        t.Span);

                    break;
                }
            }
        }

        return (parsedFile, error);
    }

    ///

    public static (Struct, Error?) ParseStruct(
        List<Token> tokens, 
        ref int index,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType) {

        Trace($"ParseStruct: {tokens.ElementAt(index)}");

        Error? error = null;

        var genericParameters = new List<(String, Span)>();

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the struct

            switch (tokens.ElementAt(index)) {

                case NameToken nt: {

                    index += 1;

                    // Check for generic

                    if (index < tokens.Count) {

                        switch (tokens[index]) {

                            case LessThanToken _: {

                                index += 1;

                                var contGenerics = true;

                                while (contGenerics && index < tokens.Count) {

                                    switch (tokens[index]) {

                                        case GreaterThanToken _: {

                                            index += 1;

                                            contGenerics = false;

                                            break;
                                        }

                                        case CommaToken _:
                                        case EolToken _:

                                            // Treat comma as whitespace? Might require them in the future

                                            index += 1;

                                            break;

                                        case NameToken nt2: {

                                            index += 1;

                                            genericParameters.Add((nt2.Value, tokens[index].Span));

                                            break;
                                        }

                                        default: {

                                            Trace($"ERROR: expected generic parameter, found: {tokens[index]}");

                                            error = error ??
                                                new ParserError(
                                                    "expected generic parameter",
                                                    tokens[index].Span);

                                            contGenerics = false;

                                            break;
                                        }
                                    }
                                }

                                break;
                            }

                            default: {

                                break;
                            }
                        }
                    }

                    // Read in definition

                    if (index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case LCurlyToken _: {

                                index += 1;

                                break;
                            }

                            default: {
        
                                Trace("ERROR: expected '{'");

                                error = error ??
                                    new ParserError(
                                        "expected '{'",
                                        tokens.ElementAt(index).Span);
                                
                                break;
                            }
                        }
                    }
                    else {

                        Trace("ERROR: incomplete struct");

                        error = error ?? 
                            new ParserError(
                                "incomplete struct",
                                tokens.ElementAt(index - 1).Span);
                    }

                    var fields = new List<VarDecl>();

                    var methods = new List<Function>();

                    var contFields = true;

                    while (contFields && index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case RCurlyToken _: {

                                index += 1;

                                contFields = false;

                                break;
                            }

                            case CommaToken _:
                            case EolToken _: {

                                // Treat comma as whitespace? Might require them in the future

                                index += 1;

                                break;
                            }

                            case NameToken nt2 when nt2.Value == "func": {

                                // Lets parse a method

                                var funcLinkage = definitionLinkage switch {

                                    DefinitionLinkage.Internal => FunctionLinkage.Internal,
                                    DefinitionLinkage.External => FunctionLinkage.External,
                                    _ => throw new Exception()
                                };

                                var (funcDecl, err) = ParseFunction(tokens, ref index, funcLinkage);

                                error = error ?? err;

                                methods.Add(funcDecl);

                                break;
                            }

                            case NameToken _: {

                                // Lets parse a parameter

                                var (varDecl, parseVarDeclErr) = ParseVariableDeclaration(tokens, ref index);
                                
                                error = error ?? parseVarDeclErr;

                                // Ignore immutable flag for now
                                
                                varDecl.Mutable = false;

                                // if (IsNullOrWhiteSpace(varDecl.Type.Name)) {
                                if (varDecl.Type is UncheckedEmptyType) {

                                    Trace("ERROR: parameter missing type");

                                    error = error ?? 
                                        new ParserError(
                                            "parameter missing type",
                                            varDecl.Span);
                                }

                                fields.Add(varDecl);

                                break;
                            }

                            default: {

                                Trace($"ERROR: expected field, found: {tokens.ElementAt(index)}");

                                error = error ?? 
                                    new ParserError(
                                        "expected field",
                                        tokens.ElementAt(index).Span);

                                contFields = false;

                                break;
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        Trace("ERROR: incomplete struct");

                        error = error ?? 
                            new ParserError(
                                "incomplete struct",
                                tokens.ElementAt(index - 1).Span);
                    }

                    return (
                        new Struct(
                            name: nt.Value,
                            genericParameters,
                            fields: fields,
                            methods: methods,
                            span: tokens.ElementAt(index - 1).Span,
                            definitionLinkage,
                            definitionType),
                        error
                    );
                }

                default: {

                    Trace("ERROR: expected struct name");

                    error = error ??
                        new ParserError(
                            "expected struct name",
                            tokens.ElementAt(index).Span);
                
                    return (
                        new Struct(
                            name: String.Empty,
                            genericParameters,
                            fields: new List<VarDecl>(),
                            methods: new List<Function>(),
                            span: tokens.ElementAt(index).Span,
                            definitionLinkage,
                            definitionType),
                        error);
                }
            }
        }
        else {

            Trace("ERROR: expected struct name");

            error = error ?? 
                new ParserError(
                    "expected struct name",
                    tokens.ElementAt(index).Span);

            return (
                new Struct(
                    name: String.Empty, 
                    genericParameters,
                    fields: new List<VarDecl>(),
                    methods: new List<Function>(),
                    span: tokens.ElementAt(index).Span,
                    definitionLinkage,
                    definitionType),
                error);
        }
    }

    ///

    public static (Function, Error?) ParseFunction(
        List<Token> tokens,
        ref int index,
        FunctionLinkage linkage) {

        Trace($"ParseFunction: {tokens.ElementAt(index)}");

        Error? error = null;

        var genericParameters = new List<(String, Span)>();

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the function

            switch (tokens.ElementAt(index)) {

                case NameToken funNameToken: {

                    var nameSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    // Check for generic

                    if (index < tokens.Count) {

                        switch (tokens[index]) {

                            case LessThanToken _: {

                                index += 1;

                                var contGenerics = true;

                                while (contGenerics && index < tokens.Count) {

                                    switch (tokens[index]) {

                                        case GreaterThanToken _: {

                                            index += 1;

                                            contGenerics = false;

                                            break;
                                        }

                                        case CommaToken _:
                                        case EolToken _: {

                                            // Treat comma as whitespace? Might require them in the future
                                            
                                            index += 1;

                                            break;
                                        }

                                        case NameToken nt2: {

                                            index += 1;

                                            genericParameters.Add((nt2.Value, tokens[index].Span));

                                            break;
                                        }

                                        default: {

                                            Trace($"ERROR: expected generic parameter, found: {tokens[index]}");

                                            error = error ??
                                                new ParserError(
                                                    "expected generic parameter",
                                                    tokens[index].Span);

                                            contGenerics = false;

                                            break;
                                        }
                                    }
                                }

                                break;
                            }

                            default: {

                                break;
                            }
                        }
                    }

                    // Read in definition

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

                    var currentParamIsMutable = false;

                    var cont = true;

                    while (cont && index < tokens.Count) {

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

                            case NameToken nt when nt.Value == "anon": {

                                currentParamRequiresLabel = false;

                                index += 1;

                                break;
                            }

                            ///

                            case NameToken nt when nt.Value == "var": {

                                currentParamIsMutable = true;

                                index += 1;

                                break;
                            }
                            ///

                            case NameToken nt when nt.Value == "this": {

                                index += 1;

                                parameters.Add(
                                    new Parameter(
                                        requiresLabel: false,
                                        variable: new Variable(
                                            name: "this",
                                            ty: new UncheckedEmptyType(),
                                            mutable: currentParamIsMutable)));

                                break;
                            }

                            case NameToken _: {

                                // Now lets parse a parameter

                                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index);

                                error = error ?? varDeclErr;

                                if (varDecl.Type is UncheckedEmptyType) {

                                    Trace("ERROR: parameter missing type");

                                    error = error ?? 
                                        new ParserError(
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

                    UncheckedType returnType = new UncheckedEmptyType();

                    Expression? fatArrowExpr = null;

                    if ((index + 2) < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case EqualToken _: {

                                index += 1;

                                switch (tokens.ElementAt(index)) {

                                    case GreaterThanToken _: {

                                        index += 1;

                                        var (_fatArrowExpr, fatArrowExprErr) = ParseExpression(
                                            tokens, 
                                            ref index, 
                                            ExpressionKind.ExpressionWithoutAssignment);

                                        returnType = new UncheckedEmptyType();

                                        fatArrowExpr = _fatArrowExpr;

                                        error = error ?? fatArrowExprErr;

                                        index += 1;

                                        break;
                                    }

                                    default: {

                                        Trace("ERROR: expected =>");

                                        error = error ?? 
                                            new ParserError(
                                                "expected =>",
                                                tokens.ElementAt(index - 1).Span);
                                        
                                        break;
                                    }
                                }

                                break;
                            }

                            ///

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

                    if (linkage == FunctionLinkage.External) {

                        return (
                            new Function(
                                name: funNameToken.Value,
                                nameSpan,
                                parameters,
                                genericParameters,
                                block: new Block(),
                                returnType,
                                linkage),
                            error);
                    }

                    Block? block = null;

                    switch (fatArrowExpr) {

                        case Expression fae: {

                            var _block = new Block();

                            _block.Statements.Add(new ReturnStatement(fae));

                            block = _block;

                            break;
                        }

                        default: {

                            var (_block, parseBlockErr) = ParseBlock(tokens, ref index);

                            error = error ?? parseBlockErr;

                            block = _block;

                            break;
                        }
                    }

                    return (
                        new Function(
                            name: funNameToken.Value,
                            nameSpan,
                            parameters,
                            genericParameters,
                            block,
                            returnType,
                            linkage),
                        error);
                }

                ///

                default: {

                    Trace("ERROR: expected function name");

                    return (
                        new Function(FunctionLinkage.Internal),
                        new ParserError(
                            "expected function name", 
                            tokens.ElementAt(index).Span));
                }
            }
        }
        else {

            Trace("ERROR: incomplete function definition");

            return (
                new Function(FunctionLinkage.Internal),
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

                var (stmt, err) = ParseStatement(tokens, ref index);

                error = error ?? err;

                return (
                    new DeferStatement(stmt), 
                    error);
            }

            case NameToken nt when nt.Value == "unsafe": {

                Trace("parsing unsafe");

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new UnsafeBlockStatement(block),
                    error);
            }

            case NameToken nt when nt.Value == "if": {

                return ParseIfStatement(tokens, ref index);
            }

            case NameToken nt when nt.Value == "break": {

                Trace("parsing break");

                index += 1;

                return (
                    new BreakStatement(),
                    null);
            }

            case NameToken nt when nt.Value == "continue": {

                Trace("parsing continue");

                index += 1;

                return (
                    new ContinueStatement(),
                    null);
            }

            case NameToken nt when nt.Value == "loop": {

                Trace("parsing loop");

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;
                
                return (
                    new LoopStatement(block),
                    error);
            }

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

            case NameToken nt when nt.Value == "for": {

                Trace("parsing for");

                index += 1;

                if (tokens.ElementAt(index) is NameToken iter) {

                    index += 1;

                    switch (tokens.ElementAt(index)) {

                        case NameToken keyword when keyword.Value == "in": {

                            index += 1;

                            var (rangeExpr, rangeErr) = ParseExpression(
                                tokens, 
                                ref index, 
                                ExpressionKind.ExpressionWithoutAssignment);

                            error = error ?? rangeErr;

                            var (block, blockErr) = ParseBlock(tokens, ref index);

                            error = error ?? blockErr;

                            return (
                                new ForStatement(iter.Value, rangeExpr, block),
                                error);
                        }

                        default: {

                            return (
                                new GarbageStatement(),
                                error);
                        }
                    }
                }
                else {

                    return (
                        new GarbageStatement(),
                        error);
                }
            }

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

            case LCurlyToken _: {

                Trace("parsing block from statement parser");

                var (block, blockErr) = ParseBlock(tokens, ref index);
            
                error = error ?? blockErr;

                return (new BlockStatement(block), error);
            }

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
                    new ExpressionStatement(expr),
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

            // Test to see if the next token is an operator

            while (tokens[index] is EolToken) {

                break;
            }

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

            var precedence = op.Precedence();

            if (index == tokens.Count) {

                Trace("ERROR: incomplete math expression");

                error = error ?? new ParserError(
                    "incomplete math expression",
                    tokens.ElementAt(index - 1).Span);

                exprStack.Add(new GarbageExpression(tokens.ElementAt(index - 1).Span));
                
                exprStack.Add(new GarbageExpression(tokens.ElementAt(index - 1).Span));

                break;
            }

            while (tokens[index] is EolToken) {

                index += 1;
            }

            var (rhs, rhsErr) = ParseOperand(tokens, ref index);

            error = error ?? rhsErr;

            while (precedence <= lastPrecedence && exprStack.Count > 1) {

                var _rhs = exprStack.Pop();

                var _op = exprStack.Pop();

                lastPrecedence = _op.Precedence();

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

        while (index < tokens.Count && tokens[index] is EolToken) {

            index += 1;
        }

        Expression? expr = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "true": {

                index += 1;

                expr = new BooleanExpression(true, span);

                break;
            }

            case NameToken nt when nt.Value == "false": {

                index += 1;

                expr = new BooleanExpression(false, span);

                break;
            }

            case NameToken nt when nt.Value == "and": {

                index += 1;

                expr = new OperatorExpression(BinaryOperator.LogicalAnd, span);

                break;
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                expr = new OperatorExpression(BinaryOperator.LogicalOr, span);

                break;
            }

            case NameToken nt when nt.Value == "not": {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new LogicalNotUnaryOperator(), _span);

                break;
            }

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

            case LParenToken _: {

                var start = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? exprErr;

                switch (tokens.ElementAt(index)) {

                    case RParenToken _: {

                        index += 1;

                        break;
                    }

                    ///

                    case CommaToken _: {

                        // We have a tuple

                        var exprs = new List<Expression>();

                        exprs.Add(_expr);

                        index += 1;

                        var end = start;

                        var contTuple = true;

                        while (contTuple && index < tokens.Count) {

                            switch (tokens.ElementAt(index)) {

                                case RParenToken _: {

                                    index += 1;

                                    contTuple = false;

                                    break;
                                }

                                case CommaToken _: {

                                    index += 1;

                                    break;
                                }

                                default: {

                                    var (_expr2, expr2Err) = ParseExpression(
                                        tokens, 
                                        ref index,
                                        ExpressionKind.ExpressionWithoutAssignment);

                                    end = _expr2.GetSpan();

                                    error = error ?? expr2Err;

                                    exprs.Add(_expr2);

                                    break;
                                }
                            }
                        }
                    
                        if (index >= tokens.Count) {

                            Trace("ERROR: expected ')'");

                            error = error ?? 
                                new ParserError(
                                    "expected ')'",
                                    tokens.ElementAt(index).Span
                                );
                        }

                        _expr = new TupleExpression(
                            exprs, 
                            new Span(
                                fileId: start.FileId, 
                                start: start.Start, 
                                end: end.End));
                    
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

            case LSquareToken _: {

                var (_expr, exprErr) = ParseArray(tokens, ref index);

                error = error ?? exprErr;

                expr = _expr;

                break;
            }

            case PlusPlusToken _: {
                
                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new PreIncrementUnaryOperator(), _span);

                break;
            }

            case MinusMinusToken _: {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new PreDecrementUnaryOperator(), _span);

                break;
            }

            case MinusToken _: {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new NegateUnaryOperator(), _span);

                break;
            }

            case TildeToken _: {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new BitwiseNotUnaryOperator(), _span);

                break;
            }

            case AsteriskToken _: {

                var startSpan = tokens.ElementAt(index).Span;

                index += 1;

                var (_expr, err) = ParseOperand(tokens, ref index);

                error = error ?? err;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: _expr.GetSpan().End);

                expr = new UnaryOpExpression(_expr, new DereferenceUnaryOperator(), _span);

                break;
            }

            case AmpersandToken _: {

                index += 1;

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case NameToken nt when
                            nt.Value == "raw"
                            && tokens.ElementAt(index).Span.Start == tokens.ElementAt(index - 1).Span.End: {

                            var startSpan = tokens.ElementAt(index).Span;

                            index += 1;

                            var (_expr, err) = ParseOperand(tokens, ref index);

                            error = error ?? err;

                            var _span = new Span(
                                fileId: startSpan.FileId,
                                start: startSpan.Start,
                                end: _expr.GetSpan().End);

                            expr = new UnaryOpExpression(_expr, new RawAddressUnaryOperator(), span);

                            break;
                        }

                        default: {

                            error = error ?? 
                                new ParserError(
                                    "ampersand not currently supported",
                                    tokens.ElementAt(index - 1).Span);

                            expr = new GarbageExpression(tokens.ElementAt(index - 1).Span);

                            break;
                        }
                    }
                }
                else {

                    error = error ??
                        new ParserError(
                            "ampersand not currently supported",
                            tokens.ElementAt(index - 1).Span);

                    expr = new GarbageExpression(tokens.ElementAt(index - 1).Span);
                }

                break;
            }

            case NumberToken numTok: {

                index += 1;

                expr = new NumericConstantExpression(numTok.Value, span);

                break;
            }

            case QuotedStringToken qs: {

                index += 1;

                expr = new QuotedStringExpression(qs.Value, span);

                break;
            }

            case SingleQuotedStringToken ct: {

                index += 1;

                if (ct.Value.FirstOrDefault() is Char c) {

                    expr = new CharacterLiteralExpression(c, span);
                }
                else {

                    expr = new GarbageExpression(span);
                }

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

        var cont = true;

        while (cont && index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case PeriodPeriodToken _: {

                    index += 1;

                    var (endExpr, endErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                    error = error ?? endErr;

                    expr = new RangeExpression(expr, endExpr, span);

                    break;
                }

                case ExclamationToken _: {

                    index += 1;

                    // Forced Optional unwrap

                    expr = new ForcedUnwrapExpression(expr, span);

                    break;
                }

                case PlusPlusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new UnaryOpExpression(expr, new PostIncrementUnaryOperator(), _span);

                    break;
                }

                case NameToken nt when nt.Value == "is": {

                    var endSpan = tokens[index + 1].Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    var (isTypeName, isTypeNameErr) = ParseTypeName(tokens, ref index);

                    error = error ?? isTypeNameErr;

                    index += 1;

                    expr = new UnaryOpExpression(expr, new IsUnaryOperator(isTypeName), _span);

                    break;
                }

                case NameToken nt when nt.Value == "as": {

                    var endSpan = tokens.ElementAt(index + 1).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    TypeCast cast;

                    switch (tokens.ElementAt(index)) {

                        case ExclamationToken _: {

                            index += 1;

                            var (typename, err) = ParseTypeName(tokens, ref index);

                            error = error ?? err;

                            cast = new InfallibleTypeCast(typename);

                            break;
                        }

                        case QuestionToken _: {

                            index += 1;

                            var (typename, err) = ParseTypeName(tokens, ref index);
                            
                            error = error ?? err;
                        
                            cast = new FallibleTypeCast(typename);

                            break;
                        }

                        case NameToken nt2 when nt2.Value == "truncated": {

                            index += 1;

                            var (typename, err) = ParseTypeName(tokens, ref index);

                            error = error ?? err;

                            cast = new TruncatingTypeCast(typename);

                            break;
                        }

                        case NameToken nt2 when nt2.Value == "saturated": {

                            index += 1;

                            var (typename, err) = ParseTypeName(tokens, ref index);

                            error = error ?? err;

                            cast = new SaturatingTypeCast(typename);

                            break;
                        }

                        default: {

                            error = error ?? 
                                new ParserError(
                                    "Invalid cast syntax",
                                    _span);

                            cast = new TruncatingTypeCast(new UncheckedEmptyType());

                            break;
                        }
                    }

                    index += 1;

                    expr = new UnaryOpExpression(expr, new TypeCastUnaryOperator(cast), _span);

                    break;
                }

                case MinusMinusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new UnaryOpExpression(expr, new PostDecrementUnaryOperator(), _span);

                    break;
                }

                case PeriodToken _: {

                    index += 1;

                    if (expr is VarExpression ve) {

                        // attempt namespace lookup

                        var ns = new List<String>();

                        ns.Add(ve.Value);

                        if (index >= tokens.Count) {

                            error = error ?? 
                                new ParserError(
                                    "Incomplete static method call",
                                    tokens.ElementAt(index).Span);
                        }

                        var contNS = true;

                        while (contNS && index < tokens.Count) {

                            switch (tokens.ElementAt(index)) {

                                case NumberToken numberToken: {

                                    index += 1;

                                    var _span = new Span(
                                        fileId: expr.GetSpan().FileId,
                                        start: expr.GetSpan().Start,
                                        end: tokens[index].Span.End);

                                    expr = new IndexedTupleExpression(
                                        expr,
                                        numberToken.Value.IntegerConstant()!.ToInt64(),
                                        _span);

                                    contNS = false;

                                    break;
                                }

                                case NameToken nsNameToken: {

                                    index += 1;

                                    if (index < tokens.Count) {

                                        if (tokens.ElementAt(index) is LParenToken) {

                                            index -= 1;

                                            var (method, parseCallErr) = ParseCall(tokens, ref index);

                                            error = error ?? parseCallErr;

                                            method.Namespace = ns;

                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            // expr = new CallExpression(method, _span);

                                            if (method.Namespace.LastOrDefault() is String n
                                                && !IsNullOrWhiteSpace(n)
                                                && Char.IsUpper(n[0])) {

                                                expr = new CallExpression(method, _span);
                                            }
                                            else {

                                                // Maybe clear namespace too?

                                                expr = new MethodCallExpression(expr, method, span);
                                            }

                                            contNS = false;
                                        }
                                        else if (tokens.ElementAt(index) is PeriodToken) {

                                            switch (tokens.ElementAt(index - 1)) {

                                                case NameToken nsNameToken2: {

                                                    ns.Add(nsNameToken2.Value);

                                                    break;
                                                }

                                                default: {

                                                    error = error ??
                                                        new ParserError(
                                                            "expected namespace",
                                                            tokens.ElementAt(index - 1).Span);

                                                    break;
                                                }
                                            }

                                            index += 1;
                                        }
                                        else {

                                            // index += 1;

                                            // error = error ?? 
                                            //     new ParserError(
                                            //         "Unsupported static method call",
                                            //         tokens.ElementAt(index).Span);

                                            // contNS = false;

                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens[index - 1].Span.End);

                                            expr = new IndexedStructExpression(
                                                expr,
                                                nsNameToken.Value,
                                                _span);

                                            contNS = false;
                                        }
                                    }
                                    else {
                                        
                                        error = error ??
                                            new ParserError(
                                                "Unsupported static method call",
                                                tokens.ElementAt(index).Span);

                                        contNS = false;
                                    }

                                    break;
                                }

                                default: {

                                    index += 1;

                                    error = error ??
                                        new ParserError(
                                            "Unsupported static method call",
                                            tokens.ElementAt(index).Span);

                                    contNS = false;

                                    break;
                                }
                            }
                        }
                    }
                    else {

                        // otherwise assume normal dot operation

                        if (index < tokens.Count) {

                            switch (tokens.ElementAt(index)) {

                                case NumberToken number: {

                                    index += 1;

                                    var _span = new Span(
                                        fileId: expr.GetSpan().FileId,
                                        start: expr.GetSpan().Start,
                                        end: tokens.ElementAt(index).Span.End);

                                    expr = new IndexedTupleExpression(expr, number.Value.IntegerConstant()!.ToInt64(), _span);

                                    break;
                                }

                                case NameToken name: {

                                    index += 1;

                                    if (index < tokens.Count) {

                                        if (tokens.ElementAt(index) is LParenToken) {

                                            index -= 1;

                                            var (method, err) = ParseCall(tokens, ref index);
                                            
                                            error = error ?? err;

                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            expr = new MethodCallExpression(expr, method, _span);
                                        }
                                        else {
        
                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            expr = new IndexedStructExpression(
                                                expr,
                                                name.Value,
                                                _span);
                                        }
                                    }
                                    else {

                                        var _span = new Span(
                                            fileId: expr.GetSpan().FileId,
                                            start: expr.GetSpan().Start,
                                            end: tokens.ElementAt(index).Span.End);

                                        expr = new IndexedStructExpression(
                                            expr,
                                            name.Value,
                                            _span);
                                    }

                                    break;
                                }

                                default: {

                                    index += 1;

                                    error = error ??
                                        new ParserError(
                                            "Unsupported dot operation",
                                            tokens.ElementAt(index).Span);

                                    break;
                                }
                            }
                        }
                        else {

                            index += 1;

                            error = error ?? 
                                new ParserError(
                                    "Incomplete dot operation",
                                    tokens.ElementAt(index).Span);
                        }
                    }

                    break;
                }

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

                default: {

                    cont = false;

                    break;
                }
            }
        }

        return (expr, error);
    }

    ///

    public static (Expression, Error?) ParseOperator(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperator: {tokens.ElementAt(index)}");

        var span = tokens.ElementAt(index).Span;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "and": {

                index += 1;

                return (new OperatorExpression(BinaryOperator.LogicalAnd, span), null);
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                return (new OperatorExpression(BinaryOperator.LogicalOr, span), null);
            }

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

            case PercentToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Modulo, span), null);
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

            case LeftShiftEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.BitwiseLeftShiftAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case RightShiftEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.BitwiseRightShiftAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case AmpersandEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.BitwiseAndAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case PipeEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.BitwiseOrAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case CaretEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.BitwiseXorAssign, span), 
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

            case PercentEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.ModuloAssign, span),
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

            case AmpersandToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseAnd, span), null);
            }

            case PipeToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseOr, span), null);
            }

            case CaretToken _: {
    
                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseXor, span), null);
            }

            case LeftShiftToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseLeftShift, span), null);
            }

            case RightShiftToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseRightShift, span), null);
            }

            case LeftArithmeticShiftToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.ArithmeticLeftShift, span),
                    null);
            }

            case RightArithmeticShiftToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.ArithmeticRightShift, span),
                    null);
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

            case PercentToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.Modulo, span), null);
            }

            case NameToken nt when nt.Value == "and": {

                index += 1;

                return (new OperatorExpression(BinaryOperator.LogicalAnd, span), null);
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                return (new OperatorExpression(BinaryOperator.LogicalOr, span), null);
            }

            case AmpersandToken _: {

                index += 1;
            
                return (new OperatorExpression(BinaryOperator.BitwiseAnd, span), null);
            }

            case PipeToken _: {

                index += 1;
            
                return (new OperatorExpression(BinaryOperator.BitwiseOr, span), null);
            }

            case CaretToken _: {

                index += 1;
            
                return (new OperatorExpression(BinaryOperator.BitwiseXor, span), null);
            }

            case LeftShiftToken _: {

                index += 1;
            
                return (new OperatorExpression(BinaryOperator.BitwiseLeftShift, span), null);
            }

            case RightShiftToken _: {

                index += 1;
            
                return (new OperatorExpression(BinaryOperator.BitwiseRightShift, span), null);
            }

            case LeftArithmeticShiftToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.ArithmeticLeftShift, span),
                    null);
            }

            case RightArithmeticShiftToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.ArithmeticRightShift, span),
                    null);
            }

            case EqualToken _: {
                
                index += 1;
                
                return (new OperatorExpression(BinaryOperator.Assign, span), null);
            }

            case LeftShiftEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseLeftShiftAssign, span), null);
            }

            case RightShiftEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseRightShiftAssign, span), null);
            }

            case AmpersandEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseAndAssign, span), null);
            }

            case PipeEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseOrAssign, span), null);
            }

            case CaretEqualToken _: {

                index += 1;

                return (new OperatorExpression(BinaryOperator.BitwiseXorAssign, span), null);
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

    public static (Expression, Error?) ParseArray(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        var output = new List<Expression>();

        var dictionaryOutput = new List<(Expression, Expression)>();

        var isDictionary = false;

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

        Expression? fillSizeExpr = null;

        ///

        var cont = true;

        while (cont && index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case RSquareToken _: {

                    index += 1;

                    cont = false;

                    break;
                }

                case CommaToken _: {

                    // Treat comma as whitespace? Might require them in the future

                    index += 1;

                    break;
                }

                case EolToken _: {

                    // Treat comma as whitespace? Might require them in the future
                    
                    index += 1;

                    break;
                }

                case SemicolonToken _: {

                    if (output.Count == 1) {

                        index += 1;

                        var (expr, err) = ParseExpression(
                            tokens, 
                            ref index, 
                            ExpressionKind.ExpressionWithoutAssignment);

                        error = error ?? err;

                        fillSizeExpr = expr;
                    }
                    else {

                        error = error ??
                            new ParserError(
                                "Can't fill an Array with more than one expression",
                                tokens.ElementAt(index).Span);
                    }

                    break;
                }

                default: {

                    var (expr, err) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                    error = error ?? err;

                    // output.Add(expr);

                    if (index < tokens.Count) {

                        if (tokens[index] is ColonToken) {

                            if (output.Any()) {

                                error = error ??
                                    new ParserError(
                                        "mixing dictionary and array values",
                                        tokens[index].Span);
                            }

                            isDictionary = true;

                            index += 1;

                            if (index < tokens.Count) {

                                var (value, parseValErr) = ParseExpression(
                                    tokens, 
                                    ref index, 
                                    ExpressionKind.ExpressionWithoutAssignment);

                                error = err ?? parseValErr;

                                dictionaryOutput.Add((expr, value));
                            }
                            else {

                                error = error ??
                                    new ParserError(
                                        "key missing value in dictionary",
                                        tokens[index - 1].Span);
                            }
                        }
                        else if (!isDictionary) {

                            output.Append(expr);
                        }
                    }
                    else if (isDictionary) {

                        output.Append(expr);
                    }

                    break;
                }
            }
        }

        ///

        var end = index - 1;

        ///

        if (isDictionary) {

            return (
                new DictionaryExpression(
                    dictionaryOutput,
                    new Span(
                        fileId: tokens.ElementAt(start).Span.FileId,
                        start: tokens.ElementAt(start).Span.Start,
                        end: tokens.ElementAt(end).Span.End)),
                error);
        }
        else {

            return (
                new ArrayExpression(
                    expressions: output,
                    fillSizeExpr,
                    new Span(
                        fileId: tokens.ElementAt(start).Span.FileId,
                        start: tokens.ElementAt(start).Span.Start,
                        end: tokens.ElementAt(end).Span.End)),
                error);
        }
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
                                    type: new UncheckedEmptyType(),
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
                            type: new UncheckedEmptyType(), 
                            mutable: false, 
                            span: tokens.ElementAt(index - 1).Span), 
                        null);
                }
            
                if (index < tokens.Count) {

                    var mutable = false;

                    if (index + 1 < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case NameToken nt2 when nt2.Value == "var": {

                                index += 1;

                                mutable = true;

                                break;
                            }

                            default: {

                                break;
                            }
                        }
                    }
                    
                    var (varType, typeErr) = ParseTypeName(tokens, ref index);

                    error = error ?? typeErr;

                    var result = new VarDecl(
                        name: varName, 
                        type: varType, 
                        mutable: mutable,
                        span: tokens.ElementAt(index - 3).Span);

                    index += 1;

                    return (result, error);
                }
                else {

                    Trace("ERROR: expected type");

                    return (
                        new VarDecl(
                            nt.Value, 
                            type: new UncheckedEmptyType(),
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

    public static (UncheckedType, Error?) ParseArrayType(
        List<Token> tokens,
        ref int index) {

        // [T] is shorthand for Array<T>

        if (index + 2 >= tokens.Count) {

            return (new UncheckedEmptyType(), null);
        }

        var start = tokens.ElementAt(index).Span;

        if (tokens.ElementAt(index) is LSquareToken) {

            if (tokens.ElementAt(index + 2) is RSquareToken) {

                if (tokens.ElementAt(index + 1) is NameToken nt) {

                    var uncheckedType = new UncheckedArrayType(
                        new UncheckedNameType(nt.Value, nt.Span),
                        new Span(
                            fileId: start.FileId,
                            start: start.Start,
                            end: tokens.ElementAt(index + 2).Span.End));

                    index += 2;

                    return (uncheckedType, null);                    
                }
            }
        }

        return (new UncheckedEmptyType(), null);
    }

    ///

    public static (UncheckedType, Error?) ParseTypeName(
        List<Token> tokens, 
        ref int index) {

        UncheckedType uncheckedType = new UncheckedEmptyType();

        Error? error = null;

        var start = tokens.ElementAt(index).Span;

        var typename = String.Empty;

        Trace($"ParseTypeName: {tokens.ElementAt(index)}");

        var (vectorType, parseVectorTypeErr) = ParseArrayType(tokens, ref index);

        error = error ?? parseVectorTypeErr;

        if (vectorType is UncheckedArrayType vt) {

            return (vt, error);
        }

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                if (nt.Value == "raw") {

                    index += 1;

                    if (index < tokens.Count) {

                        var (childType, err) = ParseTypeName(tokens, ref index);

                        error = error ?? err;

                        uncheckedType = new UncheckedRawPointerType(
                            childType,
                            new Span(
                                fileId: start.FileId,
                                start: start.Start,
                                end: tokens.ElementAt(index).Span.End));
                    }
                }
                else {

                    typename = nt.Value;

                    uncheckedType = new UncheckedNameType(nt.Value, tokens.ElementAt(index).Span);
                }

                break;
            }

            default: {

                Trace("ERROR: expected type name");

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

                uncheckedType = new UncheckedOptionalType(
                    type: uncheckedType,
                    new Span(
                        fileId: start.FileId,
                        start: start.Start,
                        end: tokens.ElementAt(index).Span.End));
            }

            if (index + 1 < tokens.Count
                && tokens[index + 1] is LessThanToken) {

                // Generic type
                
                index += 2;

                var innerTypes = new List<UncheckedType>();

                var contInnerTypes = true;

                while (contInnerTypes && index < tokens.Count) {

                    switch (tokens[index]) {

                        case GreaterThanToken _: {

                            index += 1;

                            contInnerTypes = false;

                            break;
                        }

                        case CommaToken _: {

                            index += 1;

                            break;
                        }

                        case EolToken _: {

                            index += 1;

                            break;
                        }

                        default: {

                            var (innerType, innerTypeNameErr) = ParseTypeName(tokens, ref index);

                            error = error ??innerTypeNameErr;

                            index += 1;

                            innerTypes.Add(innerType);

                            break;
                        }
                    }
                }

                uncheckedType = new UncheckedGenericType(
                    typename,
                    innerTypes,
                    new Span(
                        fileId: start.FileId,
                        start: start.Start,
                        end: tokens[index].Span.End));
            }
        }

        return (uncheckedType, error);
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

                        case EolToken _: {

                            index += 1;

                            break;
                        }

                        case CommaToken _: {

                            // Treat comma as whitespace? Might require them in the future

                            index += 1;

                            break;
                        }

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
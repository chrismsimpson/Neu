
namespace Neu;

public static partial class TraceFunctions {

    public static void Trace(
        String expr) {

        #if NEU_TRACE

        WriteLine($"{expr}");

        #endif
    }
}

public partial class ParsedCall {

    public List<String> Namespace { get; set; }

    public String Name { get; set; }

    public List<(String, ParsedExpression)> Args { get; init; }

    public List<ParsedType> TypeArgs { get; set; }

    ///

    public ParsedCall()
        : this(new List<String>(), String.Empty, new List<(String, ParsedExpression)>(), new List<ParsedType>()) { }

    public ParsedCall(
        List<String> ns,
        String name,
        List<(String, ParsedExpression)> args,
        List<ParsedType> typeArgs) {

        this.Namespace = ns;
        this.Name = name;
        this.Args = args;
        this.TypeArgs = typeArgs;
    }
}

public static partial class ParsedCallFunctions {

    public static bool Eq(
        ParsedCall l, 
        ParsedCall r) {

        if (!Equals(l.Name, r.Name)) {

            return false;
        }

        if (l.Args.Count != r.Args.Count) {

            return false;
        }

        if (l.TypeArgs.Count != r.TypeArgs.Count) {

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

        for (var t = 0; t < l.TypeArgs.Count; t++) {

            if (!ParsedTypeFunctions.Eq(l.TypeArgs[t], r.TypeArgs[t])) {

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

public partial class ParsedType {

    public ParsedType() { }
}

    public partial class ParsedNameType: ParsedType {

        public String Name { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedNameType(
            String name,
            Span span) {

            this.Name = name;
            this.Span = span;
        }
    }

    public partial class ParsedGenericType: ParsedType {

        public String Name { get; init; }

        public List<ParsedType> Types { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedGenericType(
            String name,
            List<ParsedType> types,
            Span span) { 

            this.Name = name;
            this.Types = types;
            this.Span = span;
        }
    }

    public partial class UncheckedArrayType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedArrayType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class UncheckedSetType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedSetType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class UncheckedOptionalType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedOptionalType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }
    
    public partial class UncheckedRawPointerType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public UncheckedRawPointerType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class UncheckedEmptyType: ParsedType {

        public UncheckedEmptyType() { }
    }

///

public static partial class ParsedTypeFunctions {

    public static bool Eq(ParsedType l, ParsedType r) {

        switch (true) {

            case var _ when
                l is ParsedNameType ln
                && r is ParsedNameType rn: {

                return ln.Name == rn.Name
                    && SpanFunctions.Eq(ln.Span, rn.Span);
            }

            case var _ when
                l is ParsedGenericType lg
                && r is ParsedGenericType rg: {

                if (lg.Name != rg.Name) {

                    return false;
                }

                if (lg.Types.Count != rg.Types.Count) {

                    return false;
                }

                for (var i = 0; i < lg.Types.Count; i++) {

                    if (!ParsedTypeFunctions.Eq(lg.Types[i], rg.Types[i])) {

                        return false;
                    }
                }

                return SpanFunctions.Eq(lg.Span, rg.Span);
            }

            case var _ when 
                l is UncheckedArrayType la
                && r is UncheckedArrayType ra: {

                return ParsedTypeFunctions.Eq(la.Type, ra.Type)
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

public partial class ParsedVarDecl {

    public String Name { get; init; }

    public ParsedType Type { get; set; }

    public bool Mutable { get; set; }

    public Span Span { get; init; }

    ///

    public ParsedVarDecl(Span span)
        : this(String.Empty, new UncheckedEmptyType(), false, span) { }

    public ParsedVarDecl(
        String name,
        ParsedType type,
        bool mutable,
        Span span) {

        this.Name = name;
        this.Type = type;
        this.Mutable = mutable;
        this.Span = span;
    }
}

public static partial class ParsedVarDeclFunctions {

    public static bool Eq(
        ParsedVarDecl l,
        ParsedVarDecl r) {

        return l.Name == r.Name 
            && ParsedTypeFunctions.Eq(l.Type, r.Type)
            && l.Mutable == r.Mutable;
    }
}

///

public partial class ParsedFile {

    public List<ParsedFunction> Functions { get; init; }

    public List<ParsedStruct> Structs { get; init; }

    public List<ParsedEnum> Enums { get; init; }

    ///

    public ParsedFile()
        : this(new List<ParsedFunction>(), new List<ParsedStruct>(), new List<ParsedEnum>()) { }

    public ParsedFile(
        List<ParsedFunction> functions,
        List<ParsedStruct> structs,
        List<ParsedEnum> enums) {

        this.Functions = functions;
        this.Structs = structs;
        this.Enums = enums; 
    }
}

///

public partial class ParsedStruct {
    
    public String Name { get; init; }

    public List<(String, Span)> GenericParameters { get; init; }

    public List<ParsedVarDecl> Fields { get; init; }

    public List<ParsedFunction> Methods { get; init; }

    public Span Span { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; } 

    ///

    public ParsedStruct(
        String name,
        List<(String, Span)> genericParameters,
        List<ParsedVarDecl> fields,
        List<ParsedFunction> methods,
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

public partial class EnumVariant {

    public EnumVariant() { }
}

    public partial class UntypedEnumVariant: EnumVariant {

        public String Name { get; init; }

        public Span Span { get; init; }

        ///

        public UntypedEnumVariant(
            String name,
            Span span) {

            this.Name = name;
            this.Span = span;
        }
    }

    public partial class WithValueEnumVariant: EnumVariant {

        public String Name { get; init; }

        public ParsedExpression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public WithValueEnumVariant(
            String name,
            ParsedExpression expression,
            Span span) {

            this.Name = name;
            this.Expression = expression;
            this.Span = span;
        }
    }

    public partial class StructLikeEnumVariant: EnumVariant {

        public String Name { get; init; }

        public List<ParsedVarDecl> Declarations { get; init; }

        public Span Span { get; init; }

        ///

        public StructLikeEnumVariant(
            String name,
            List<ParsedVarDecl> declarations,
            Span span) {

            this.Name = name;
            this.Declarations = declarations;
            this.Span = span;
        }
    }

    public partial class TypedEnumVariant: EnumVariant {

        public String Name { get; init; }

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public TypedEnumVariant(
            String name,
            ParsedType type,
            Span span) {

            this.Name = name;
            this.Type = type;
            this.Span = span;
        }
    }

public partial class ParsedEnum {

    public String Name { get; set; }

    public List<(String, Span)> GenericParameters { get; set; }

    public List<EnumVariant> Variants { get; init; }

    public Span Span { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public ParsedType UnderlyingType { get; set; }

    ///

    public ParsedEnum(
        String name,
        List<(String, Span)> genericParameters,
        List<EnumVariant> variants,
        Span span,
        DefinitionLinkage definitionLinkage,
        ParsedType underlyingType) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Variants = variants;
        this.Span = span;
        this.DefinitionLinkage = definitionLinkage;
        this.UnderlyingType = underlyingType;
    }
}

///

public enum FunctionLinkage {

    Internal,
    External,
    ImplicitConstructor,
    ImplicitEnumConstructor
}

public enum DefinitionLinkage {

    Internal,
    External
}

public enum DefinitionType {

    Class,
    Struct
}

public partial class ParsedFunction {

    public String Name { get; init; }

    public Span NameSpan { get; init; }

    public List<ParsedParameter> Parameters { get; init; }

    public List<(String, Span)> GenericParameters { get; init; }

    public ParsedBlock Block { get; init; }

    public bool Throws { get; init; }

    public ParsedType ReturnType { get; init; }

    public FunctionLinkage Linkage { get; init; }

    ///

    public ParsedFunction(
        FunctionLinkage linkage)
        : this(
            String.Empty,
            new Span(
                fileId: 0, 
                start: 0, 
                end: 0),
            new List<ParsedParameter>(), 
            new List<(String, Span)>(),
            new ParsedBlock(), 
            throws: false,
            returnType: 
                new UncheckedEmptyType(),
            linkage) { }

    public ParsedFunction(
        String name,
        Span nameSpan,
        List<ParsedParameter> parameters,
        List<(String, Span)> genericParameters,
        ParsedBlock block,
        bool throws,
        ParsedType returnType,
        FunctionLinkage linkage) {

        this.Name = name;
        this.NameSpan = nameSpan;
        this.Parameters = parameters;
        this.GenericParameters = genericParameters;
        this.Block = block;
        this.Throws = throws;
        this.ReturnType = returnType;
        this.Linkage = linkage;
    }
}

///

public partial class Variable {

    public String Name { get; init; }

    public ParsedType Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public Variable(
        String name,
        ParsedType ty,
        bool mutable) {

        this.Name = name;
        this.Type = ty;
        this.Mutable = mutable;
    }
}

///

public partial class ParsedParameter {

    public bool RequiresLabel { get; init; }

    public Variable Variable { get; init; }

    ///

    public ParsedParameter(
        bool requiresLabel,
        Variable variable) {

        this.RequiresLabel = requiresLabel;
        this.Variable = variable;
    }
}

///

public partial class ParsedStatement {

    public ParsedStatement() { }
}

public static partial class StatementFunctions {

    public static bool Eq(
        ParsedStatement? l,
        ParsedStatement? r) {

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
                l is ThrowStatement lt
                && r is ThrowStatement rt:

                return ThrowStatementFunctions.Eq(lt, rt);

            ///

            case var _ when
                l is InlineCPPStatement li
                && r is InlineCPPStatement ri:

                return InlineCPPStatementFunctions.Eq(li, ri);

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
        List<ParsedStatement> l,
        List<ParsedStatement> r) {

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

public partial class ExpressionStatement: ParsedStatement {

    public ParsedExpression Expression { get; init; }

    ///

    public ExpressionStatement(
        ParsedExpression expression) {

        this.Expression = expression;
    }
}

public static partial class ExpressionStatementFunctions {

    public static bool Eq(ExpressionStatement le, ExpressionStatement re) {

        return ExpressionFunctions.Eq(le.Expression, re.Expression);
    }
}

///

public partial class DeferStatement: ParsedStatement {

    public ParsedStatement Statement { get; init; }

    ///

    public DeferStatement(
        ParsedStatement statement)
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

public partial class UnsafeBlockStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    ///

    public UnsafeBlockStatement(
        ParsedBlock block) {

        this.Block = block;
    }
}

public static partial class UnsafeBlockStatementFunctions {

    public static bool Eq(
        UnsafeBlockStatement? l,
        UnsafeBlockStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class VarDeclStatement: ParsedStatement {

    public ParsedVarDecl Decl { get; init; }

    public ParsedExpression Expr { get; init; }

    ///

    public VarDeclStatement(
        ParsedVarDecl decl,
        ParsedExpression expr) {

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

        if (!ParsedVarDeclFunctions.Eq(_l.Decl, _r.Decl)) {

            return false;
        }

        return ExpressionFunctions.Eq(_l.Expr, _r.Expr);
    }
}

///

public partial class IfStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    public ParsedBlock Block { get; init; }

    public ParsedStatement? Trailing { get; init; }

    ///

    public IfStatement(
        ParsedExpression expr,
        ParsedBlock block,
        ParsedStatement? trailing)
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
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block)
            && StatementFunctions.Eq(_l.Trailing, _r.Trailing);
    }
}

///

public partial class BlockStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public BlockStatement(
        ParsedBlock block) 
        : base() {

        this.Block = block;
    }
}

public static partial class BlockStatementFunctions {

    public static bool Eq(
        BlockStatement? l,
        BlockStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class LoopStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public LoopStatement(
        ParsedBlock block) {

        this.Block = block;
    }
}

public static partial class LoopStatementFunctions {

    public static bool Eq(
        LoopStatement? l,
        LoopStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class WhileStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public WhileStatement(
        ParsedExpression expr,
        ParsedBlock block) 
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
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block);
    }
}

///

public partial class ForStatement: ParsedStatement {

    public String IteratorName { get; init; }

    public ParsedExpression Range { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public ForStatement(
        String iteratorName,
        ParsedExpression range,
        ParsedBlock block) {

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
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block);
    }
}

///

public partial class BreakStatement: ParsedStatement {

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

public partial class ContinueStatement: ParsedStatement {

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

public partial class ReturnStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    ///

    public ReturnStatement(
        ParsedExpression expr) {

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

public partial class ThrowStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    ///

    public ThrowStatement(
        ParsedExpression expr) {

        this.Expr = expr;
    }
}

public static partial class ThrowStatementFunctions {

    public static bool Eq(
        ThrowStatement? l,
        ThrowStatement? r) {

        return ExpressionFunctions.Eq(l?.Expr, r?.Expr);
    }
}

///

public partial class TryStatement: ParsedStatement {

    public ParsedStatement Statement { get; init; }

    public String Name { get; init; }

    public Span Span { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public TryStatement(
        ParsedStatement statement,
        String name,
        Span span,
        ParsedBlock block) {

        this.Statement = statement;
        this.Name = name;
        this.Span = span;
        this.Block = block;
    }
}

public static partial class TryStatementFunctions {

    public static bool Eq(
        TryStatement? l,
        TryStatement? r) {

        if (l is null && r is null) {

            return true;
        }

        if (l is null || r is null) {

            return false;
        }

        if (!StatementFunctions.Eq(l.Statement, r.Statement)) {

            return false;
        }

        if (!Equals(l.Name, r.Name)) {

            return false;
        }

        if (!SpanFunctions.Eq(l.Span, r.Span)) {

            return false;
        }

        return ParsedBlockFunctions.Eq(l.Block, r.Block);
    }
}

///

public partial class InlineCPPStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public Span Span { get; init; }

    ///

    public InlineCPPStatement(
        ParsedBlock block,
        Span span) {

        this.Block = block;
        this.Span = span;
    }
}

public static partial class InlineCPPStatementFunctions {

    public static bool Eq(
        InlineCPPStatement? l,
        InlineCPPStatement? r) {
        
        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        if (!ParsedBlockFunctions.Eq(l.Block, r.Block)) {

            return false;
        }

        return SpanFunctions.Eq(l.Span, r.Span);
    }
}

///

public partial class GarbageStatement: ParsedStatement {

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

public partial class ParsedBlock {

    public List<ParsedStatement> Statements { get; init; }

    ///

    public ParsedBlock()
        : this(new List<ParsedStatement>()) { }

    public ParsedBlock(
        List<ParsedStatement> statements) {

        this.Statements = statements;
    }
}

public static partial class ParsedBlockFunctions {

    public static bool Eq(
        ParsedBlock? l,
        ParsedBlock? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ParsedBlock _l = l!;
        ParsedBlock _r = r!;

        return StatementFunctions.Eq(_l.Statements, _r.Statements);
    }
}

///

public partial class WhenBody {

    public WhenBody() { }
}

    public partial class ExpressionWhenBody: WhenBody {

        public ParsedExpression Expression { get; init; }

        ///

        public ExpressionWhenBody(
            ParsedExpression expression) {

            this.Expression = expression;
        }
    }

    public partial class BlockWhenBody: WhenBody {

        public ParsedBlock Block { get; init; }

        ///

        public BlockWhenBody(
            ParsedBlock block) {

            this.Block = block;
        }
    }

public partial class WhenCase {

    public WhenCase() { }
}

    public partial class EnumVariantWhenCase: WhenCase {

        public List<(String, Span)> VariantName { get; init; }
        
        public List<(String?, String)> VariantArguments { get; init; }

        public Span ArgumentsSpan { get; init; }
        
        public WhenBody Body { get; init; }

        ///

        public EnumVariantWhenCase(
            List<(String, Span)> variantName,
            List<(String?, String)> variantArguments,
            Span argumentsSpan,
            WhenBody body) {

            this.VariantName = variantName;
            this.VariantArguments = variantArguments;
            this.ArgumentsSpan = argumentsSpan;
            this.Body = body;
        }
    }

///

public partial class ParsedExpression {

    public ParsedExpression() : base() { }
}

    // Standalone

    public partial class BooleanExpression: ParsedExpression {

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

    public partial class NumericConstantExpression: ParsedExpression {

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

    public partial class QuotedStringExpression: ParsedExpression {

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

    public partial class CharacterLiteralExpression: ParsedExpression {

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

    public partial class ArrayExpression: ParsedExpression {

        public List<ParsedExpression> Expressions { get; init; }

        public ParsedExpression? FillSize { get; init; }

        public Span Span { get; init; }

        ///

        public ArrayExpression(
            List<ParsedExpression> expressions,
            ParsedExpression? fillSize,
            Span span) {

            this.Expressions = expressions;
            this.FillSize = fillSize;
            this.Span = span;
        }
    }

    public partial class DictionaryExpression: ParsedExpression {

        public List<(ParsedExpression, ParsedExpression)> Entries { get; init; }

        public Span Span { get; init; }

        ///

        public DictionaryExpression(
            List<(ParsedExpression, ParsedExpression)> entries,
            Span span) {

            this.Entries = entries;
            this.Span = span;
        }
    }

    public partial class SetExpression: ParsedExpression {

        public List<ParsedExpression> Items { get; init; }

        public Span Span { get; init; }

        ///

        public SetExpression(
            List<ParsedExpression> items,
            Span span) {

            this.Items = items;
            this.Span = span;
        }
    }

    public partial class IndexedExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public ParsedExpression Index { get; init; }

        public Span Span { get; init; }

        ///

        public IndexedExpression(
            ParsedExpression expression,
            ParsedExpression index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class UnaryOpExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }
        
        public UnaryOperator Operator { get; init; }
        
        public Span Span { get; init; }

        ///

        public UnaryOpExpression(
            ParsedExpression expression,
            UnaryOperator op,
            Span Span) {

            this.Expression = expression;
            this.Operator = op;
            this.Span = Span;
        }
    }

    public partial class BinaryOpExpression: ParsedExpression {

        public ParsedExpression Lhs { get; init; }
        
        public BinaryOperator Operator { get; init; }
        
        public ParsedExpression Rhs { get; init; }

        public Span Span { get; init; }

        ///

        public BinaryOpExpression(
            ParsedExpression lhs,
            BinaryOperator op,
            ParsedExpression rhs,
            Span span) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Span = span;
        }
    }

    public partial class VarExpression: ParsedExpression {

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
    
    public partial class TupleExpression: ParsedExpression {

        public List<ParsedExpression> Expressions { get; init; }

        public Span Span { get; init; }

        ///

        public TupleExpression(
            List<ParsedExpression> expressions,
            Span span) {

            this.Expressions = expressions;
            this.Span = span;
        }
    }

    public partial class RangeExpression: ParsedExpression {

        public ParsedExpression Start { get; init; }

        public ParsedExpression End { get; init; }

        public Span Span { get; init; }

        ///

        public RangeExpression(
            ParsedExpression start,
            ParsedExpression end,
            Span span) {

            this.Start = start;
            this.End = end;
            this.Span = span;
        }
    }

    public partial class WhenExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public List<WhenCase> Cases { get; init; }

        public Span Span { get; init; }

        ///

        public WhenExpression(
            ParsedExpression expression,
            List<WhenCase> cases,
            Span span) {

            this.Expression = expression;
            this.Cases = cases;
            this.Span = span;
        }
    }

    public partial class IndexedTupleExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Int64 Index { get; init; }

        public Span Span { get; init; }

        ///

        public IndexedTupleExpression(
            ParsedExpression expression,
            Int64 index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class IndexedStructExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }
        
        public String Name { get; init; }
        
        public Span Span { get; init; }

        ///

        public IndexedStructExpression(
            ParsedExpression expression,
            String name,
            Span span) {

            this.Expression = expression;
            this.Name = name;
            this.Span = span;
        }
    }

    public partial class CallExpression: ParsedExpression {

        public ParsedCall Call { get; init; }

        public Span Span { get; init; }

        ///

        public CallExpression(
            ParsedCall call,
            Span span)
            : base() {

            this.Call = call;
            this.Span = span;
        }
    }

    public partial class MethodCallExpression: ParsedExpression {
        
        public ParsedExpression Expression { get; init; }
        
        public ParsedCall Call { get; init; }
        
        public Span Span { get; init; }

        ///

        public MethodCallExpression(
            ParsedExpression expression, 
            ParsedCall call, 
            Span span) {

            this.Expression = expression;
            this.Call = call;
            this.Span = span;
        }
    }

    public partial class ForcedUnwrapExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public ForcedUnwrapExpression(
            ParsedExpression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // FIXME: These should be implemented as `enum` variant values once available.

    public partial class OptionalNoneExpression: ParsedExpression {

        public Span Span { get; init; }
        
        ///

        public OptionalNoneExpression(
            Span span) : base() {

            this.Span = span;
        }
    }

    public partial class OptionalSomeExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public OptionalSomeExpression(
            ParsedExpression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // Not standalone

    public partial class OperatorExpression: ParsedExpression {

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

    public partial class GarbageExpression: ParsedExpression {

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
        this ParsedExpression expr) {

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

            case SetExpression se: {

                return se.Span;
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

            case WhenExpression whenExpression: {

                return whenExpression.Span;
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
        ParsedExpression? l, 
        ParsedExpression? r) {

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

                return ParsedCallFunctions.Eq(callL.Call, callR.Call);

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
                l is CharacterLiteralExpression lc
                && r is CharacterLiteralExpression rc:

                return Equals(lc.Char, rc.Char);
            
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

    public static UInt64 Precedence(
        this ParsedExpression expr) {

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
                opExpr.Operator == BinaryOperator.LogicalOr
                || opExpr.Operator == BinaryOperator.NoneCoalescing:

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

    public ParsedType Type { get; init; }

    ///

    public TypeCast(
        ParsedType type) {

        this.Type = type;
    }
}

    public partial class FallibleTypeCast: TypeCast {

        public FallibleTypeCast(ParsedType type)
            : base(type) { }
    }

    public partial class InfallibleTypeCast: TypeCast {

        public InfallibleTypeCast(ParsedType type)
            : base(type) { }
    }

    public partial class SaturatingTypeCast: TypeCast {

        public SaturatingTypeCast(ParsedType type)
            : base(type) { }
    }
    
    public partial class TruncatingTypeCast: TypeCast {

        public TruncatingTypeCast(ParsedType type)
            : base(type) { }
    }

public static partial class TypeCastFunctions {

    public static bool Eq(TypeCast l, TypeCast r) {

        switch (true) {

            case var _ when l is FallibleTypeCast lf && r is FallibleTypeCast rf:
                return ParsedTypeFunctions.Eq(lf.Type, rf.Type);

            case var _ when l is InfallibleTypeCast li && r is InfallibleTypeCast ri:
                return ParsedTypeFunctions.Eq(li.Type, ri.Type);

            case var _ when l is SaturatingTypeCast ls && r is SaturatingTypeCast rs:
                return ParsedTypeFunctions.Eq(ls.Type, rs.Type);

            case var _ when l is TruncatingTypeCast lt && r is TruncatingTypeCast rt:
                return ParsedTypeFunctions.Eq(lt.Type, rt.Type);

            default:
                return false;
        }
    }

    public static ParsedType GetUncheckedType(
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

        public ParsedType Type { get; init; }

        ///

        public IsUnaryOperator(
            ParsedType type) {

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
                return ParsedTypeFunctions.Eq(li.Type, ri.Type);

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
    BitwiseRightShiftAssign,
    NoneCoalescing
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

                        case "enum": {

                            var (_enum, err) = ParseEnum(tokens, ref index, DefinitionLinkage.Internal);

                            error = error ?? err;

                            parsedFile.Enums.Add(_enum);

                            break;
                        }

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

                case EolToken _: {

                    // ignore

                    index += 1;

                    break;
                }

                case EofToken _: {

                    cont = false;

                    break;
                }

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

    public static void SkipNewLines(
        List<Token> tokens,
        ref int index) {

        while (tokens[index] is EolToken) {

            index += 1;
        }
    }

    public static (ParsedEnum, Error?) ParseEnum(
        List<Token> tokens,
        ref int index,
        DefinitionLinkage definitionLinkage) {

        Trace($"parse_enum({tokens[index]})");

        Error? error = null;
        
        var startIndex = index;

        var _enum = new ParsedEnum(
            name: String.Empty,
            genericParameters: new List<(String, Span)>(),
            variants: new List<EnumVariant>(),
            span: new Span(
                fileId: tokens[startIndex].Span.FileId,
                start: tokens[startIndex].Span.Start,
                end: tokens[index].Span.End),
            definitionLinkage,
            underlyingType: new UncheckedEmptyType());

        index += 1;

        SkipNewLines(tokens, ref index);

        if (tokens.ElementAtOrDefault(index) is NameToken nt) {

            Trace($"enum name: {nt.Value}");

            index += 1;

            _enum.Name = nt.Value;

            // Following the name can either be a `:` to denote the underlying type, a `<` to denote generic parameters, or nothing.

            SkipNewLines(tokens, ref index);

            switch (tokens.ElementAtOrDefault(index)) {

                case ColonToken _: {

                    Trace("enum with underlying type");
                    
                    index += 1;
                    
                    var (_type, parseErr) = ParseTypeName(tokens, ref index);

                    _enum.UnderlyingType = _type;
                    
                    error = error ?? parseErr;

                    Trace($"next token: {tokens[index]}");

                    // index += 1;

                    break;
                }

                case LessThanToken _: {

                    Trace("enum with generic parameters");

                    var (_params, parseErr) = ParseGenericParameters(tokens, ref index);
                    
                    _enum.GenericParameters = _params;

                    error = error ?? parseErr;

                    break;
                }

                default: {

                    break;
                }
            }

            // Now parse the body of the enum, starting with the `{`

            SkipNewLines(tokens, ref index);

            if (!(tokens.ElementAtOrDefault(index) is LCurlyToken)) {

                error = error ?? 
                    new ParserError(
                        "expected `{` to start the enum body",
                        tokens[index].Span);
            }
            else {

                index += 1;
            }

            Trace($"enum body: {tokens[index]}");

            // Variants in one of the following forms:
            // - Ident(name) Colon Type
            // - Ident(name) LCurly struct_body RCurly
            // - Ident(name) Equal Expression
            //    expression should evaluate to the underlying type (not allowed if no underlying type)
            // - Ident(name)

            SkipNewLines(tokens, ref index);

            var cont = true;

            while (cont 
                && index < tokens.Count 
                && !(tokens[index] is RCurlyToken)) {

                Trace($"parse_enum_variant({tokens[index]})");

                var _startIndex = index;
                
                var variantName = tokens.ElementAtOrDefault(index);

                if (!(variantName is NameToken)) {

                    error = error ?? 
                        new ParserError(
                            "expected variant name",
                            tokens[index].Span);

                    cont = false;

                    break;
                }

                var name = (variantName as NameToken)?.Value 
                    ?? throw new Exception();

                index += 1;

                switch (tokens.ElementAtOrDefault(index)) {

                    case ColonToken _: {

                        Trace("variant with type");

                        index += 1;

                        var (varType, typeErr) = ParseTypeName(tokens, ref index);
                        
                        // index += 1;
                        
                        error = error ?? typeErr;
                        
                        _enum.Variants.Add(
                            new TypedEnumVariant(
                                name,
                                varType,
                                new Span(
                                    fileId: tokens[index].Span.FileId,
                                    start: tokens[startIndex].Span.Start,
                                    end: tokens[index].Span.End)));

                        break;
                    }

                    case LCurlyToken _: {

                        index += 1;

                        var members = new List<ParsedVarDecl>();

                        while (index < tokens.Count
                            && !(tokens[index] is RCurlyToken)) {
                            
                            SkipNewLines(tokens, ref index);

                            var (decl, varDeclParseErr) = ParseVariableDeclaration(tokens, ref index);

                            error = error ?? varDeclParseErr;

                            members.Add(decl);

                            // Allow a comma or a newline after each member

                            if (tokens[index] is CommaToken
                                || tokens[index] is EolToken) {

                                index += 1;
                            }
                        }

                        index += 1;

                        _enum.Variants.Add(
                            new StructLikeEnumVariant(
                                name,
                                members,
                                new Span(
                                    fileId: tokens[index].Span.FileId,
                                    start: tokens[startIndex].Span.Start,
                                    end: tokens[index].Span.End)));

                        break;
                    }

                    case EqualToken _: {

                        if (_enum.UnderlyingType is UncheckedEmptyType) {

                            error = error ??
                                new ParserError(
                                    "enums with explicit values must have an underlying type",
                                    tokens[index].Span);
                        }

                        index += 1;

                        var (expr, parseErr) = ParseExpression(
                            tokens, 
                            ref index, 
                            ExpressionKind.ExpressionWithoutAssignment);

                        error = error ?? parseErr;

                        _enum.Variants.Add(
                            new WithValueEnumVariant(
                                name,
                                expr,
                                new Span(
                                    fileId: tokens[index].Span.FileId,
                                    start: tokens[index].Span.Start,
                                    end: tokens[index].Span.End)));

                        break;
                    }

                    case RCurlyToken _: {

                        cont = false;

                        break;
                    }

                    default: {

                        _enum.Variants.Add(
                            new UntypedEnumVariant(
                                name,
                                new Span(
                                    fileId: tokens[index].Span.FileId,
                                    start: tokens[index].Span.Start,
                                    end: tokens[index].Span.End)));

                        break;
                    }
                }

                // Require a comma or a newline after each variant

                if (tokens.ElementAtOrDefault(index) is CommaToken
                    || tokens.ElementAtOrDefault(index) is EolToken) {

                    index += 1;
                }
                else {

                    Trace($"Expected comma or newline after enum variant, found {tokens.ElementAtOrDefault(index)}");

                    error = error ??
                        new ParserError(
                            "expected comma or newline after enum variant",
                            tokens[index].Span);
                }
            }

            if (tokens.Count == index) {

                error = error ??
                    new ParserError(
                        "expected `}` to end the enum body",
                        tokens[index].Span);
            }
            else {

                index += 1;
            }
        }
        else {

            error = error ??
                new ParserError(
                    "expected enum name",
                    tokens[index].Span);
        }

        return (_enum, error);
    }

    public static (List<(String, Span)>, Error?) ParseGenericParameters(
        List<Token> tokens,
        ref int index) {

        if (!(tokens[index] is LessThanToken)) {

            return (new List<(String, Span)>(), null);
        }

        index += 1;

        var genericParameters = new List<(String, Span)>();

        while (index < tokens.Count 
            && !(tokens[index] is GreaterThanToken)) {

            SkipNewLines(tokens, ref index);

            switch (tokens.ElementAtOrDefault(index)) {

                case NameToken nt: {

                    genericParameters.Add((nt.Value, nt.Span));

                    index += 1;

                    switch (tokens.ElementAtOrDefault(index)) {

                        case CommaToken _:
                        case EolToken _: {

                            index += 1;

                            break;
                        }

                        default: {

                            break;
                        }
                    }

                    break;
                }

                default: {

                    return (
                        genericParameters,
                        new ParserError(
                            "expected generic parameter name",
                            tokens[index].Span));
                }
            }
        }

        if (tokens.ElementAtOrDefault(index) is GreaterThanToken) {
            
            index += 1;
        }
        else {

            return (
                genericParameters,
                new ParserError(
                    "expected `>` to end the generic parameters",
                    tokens[index].Span));
        }

        return (genericParameters, null);
    }

    public static (ParsedStruct, Error?) ParseStruct(
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

                    var (genParams, genParamsParseErr) = ParseGenericParameters(tokens, ref index);

                    genericParameters = genParams;

                    error = error ?? genParamsParseErr;

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

                    var fields = new List<ParsedVarDecl>();

                    var methods = new List<ParsedFunction>();

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
                        new ParsedStruct(
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
                        new ParsedStruct(
                            name: String.Empty,
                            genericParameters,
                            fields: new List<ParsedVarDecl>(),
                            methods: new List<ParsedFunction>(),
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
                new ParsedStruct(
                    name: String.Empty, 
                    genericParameters,
                    fields: new List<ParsedVarDecl>(),
                    methods: new List<ParsedFunction>(),
                    span: tokens.ElementAt(index).Span,
                    definitionLinkage,
                    definitionType),
                error);
        }
    }

    public static (ParsedFunction, Error?) ParseFunction(
        List<Token> tokens,
        ref int index,
        FunctionLinkage linkage) {

        Trace($"ParseFunction: {tokens.ElementAt(index)}");

        Error? error = null;

        List<(String, Span)>? genericParameters = null;

        index += 1;

        if (index < tokens.Count) {

            // we're expecting the name of the function

            switch (tokens.ElementAt(index)) {

                case NameToken funNameToken: {

                    var nameSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    // Check for generic

                    var (genParams, genParamParseErr) = ParseGenericParameters(tokens, ref index);

                    error = error ?? genParamParseErr;

                    genericParameters = genParams;

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

                    var parameters = new List<ParsedParameter>();

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
                                    new ParsedParameter(
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
                                    new ParsedParameter(
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

                    var throws = false;

                    if (index + 1 < tokens.Count) {

                        switch (tokens[index]) {

                            case NameToken throwName when throwName.Value == "throws": {

                                index += 1;

                                throws = true;

                                break;
                            }

                            default: {

                                break;
                            }
                        }
                    }

                    ParsedType returnType = new UncheckedEmptyType();

                    ParsedExpression? fatArrowExpr = null;

                    if ((index + 2) < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case FatArrowToken _: {

                                index += 1;

                                var (arrowExpr, arrowExprErr) = ParseExpression(
                                    tokens,
                                    ref index,
                                    ExpressionKind.ExpressionWithoutAssignment);

                                returnType = new UncheckedEmptyType();

                                fatArrowExpr = arrowExpr;

                                error = error ?? arrowExprErr;

                                index += 1;

                                break;
                            }

                            case MinusToken _: {

                                index += 1;

                                switch (tokens.ElementAt(index)) {

                                    case GreaterThanToken: {

                                        index += 1;

                                        var (retType, retTypeErr) = ParseTypeName(tokens, ref index);

                                        returnType = retType;

                                        error = error ?? retTypeErr;

                                        // index += 1;

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

                            default: {

                                break;
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        Trace("ERROR: incomplete function");

                        error = error ?? new ParserError(
                            "incomplete function", 
                            tokens.ElementAt(tokens.Count - 1).Span);
                    }

                    if (linkage == FunctionLinkage.External) {

                        return (
                            new ParsedFunction(
                                name: funNameToken.Value,
                                nameSpan,
                                parameters,
                                genericParameters,
                                block: new ParsedBlock(),
                                throws,
                                returnType,
                                linkage),
                            error);
                    }

                    ParsedBlock? block = null;

                    switch (fatArrowExpr) {

                        case ParsedExpression fae: {

                            var _block = new ParsedBlock();

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
                        new ParsedFunction(
                            name: funNameToken.Value,
                            nameSpan,
                            parameters,
                            genericParameters,
                            block,
                            throws,
                            returnType,
                            linkage),
                        error);
                }

                ///

                default: {

                    Trace("ERROR: expected function name");

                    return (
                        new ParsedFunction(FunctionLinkage.Internal),
                        new ParserError(
                            "expected function name", 
                            tokens.ElementAt(index).Span));
                }
            }
        }
        else {

            Trace("ERROR: incomplete function definition");

            return (
                new ParsedFunction(FunctionLinkage.Internal),
                new ParserError(
                    "incomplete function definition", 
                    tokens.ElementAt(index).Span));
        }
    }

    public static (ParsedBlock, Error?) ParseBlock(List<Token> tokens, ref int index) {

        Trace($"ParseBlock: {tokens.ElementAt(index)}");

        var block = new ParsedBlock();

        Error? error = null;

        if (tokens.ElementAtOrDefault(index) is null) {

            Trace("ERROR: incomplete block");

            error = new ParserError(
                "incomplete block",
                tokens[tokens.Count - 1].Span);

            return (block, error);
        }

        var start = tokens.ElementAt(index).Span;

        index += 1;

        while (index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case RCurlyToken _: {

                    index += 1;

                    return (block, error);
                }

                case SemicolonToken _: {

                    index += 1;

                    break;
                }

                case EolToken _: {

                    index += 1;

                    break;
                }

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
            new ParsedBlock(),
            new ParserError(
                "expected complete block", 
                new Span(
                    fileId: start.FileId,
                    start: start.Start,
                    end: tokens.ElementAt(index - 1).Span.End)));
    }

    public static (ParsedStatement, Error?) ParseStatement(List<Token> tokens, ref int index) {

        Trace($"ParseStatement: {tokens.ElementAt(index)}");

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt when nt.Value == "throw": {

                Trace("parsing throw");

                index += 1;

                var (expr, err) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? err;

                return (
                    new ThrowStatement(expr),
                    error);
            }

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

            case NameToken nt when nt.Value == "try": {

                Trace("parsing try");

                index += 1;

                var (stmt, err) = ParseStatement(tokens, ref index);

                error = error ?? err;

                while (tokens[index] is EolToken) {

                    index += 1;
                }

                var errorName = String.Empty;

                var errorSpan = tokens[index].Span;

                switch (tokens[index]) {

                    case NameToken nt2 when nt2.Value == "catch": {

                        index += 1;

                        // FIXME: Error about missing error binding

                        if (tokens[index] is NameToken nt3) {

                            errorSpan = tokens[index].Span;
                            
                            errorName = nt3.Value;
                            
                            index += 1;
                        }

                        break;
                    }

                    default: {

                        // FIXME: Error about missing "catch"

                        break;
                    }
                }

                var (catchBlock, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new TryStatement(stmt, errorName, errorSpan, catchBlock),
                    error);
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

            case NameToken nt when nt.Value == "cpp": {

                Trace("parsing inline cpp block");

                index += 1;

                var startSpan = tokens[index].Span;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: tokens[index].Span.End);
                    
                return (
                    new InlineCPPStatement(block, _span),
                    error);
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

                if (error != null && index < tokens.Count) {

                    index += 1;
                }

                return (
                    new ExpressionStatement(expr),
                    error);
            }
        }
    }

    public static (ParsedStatement, Error?) ParseIfStatement(
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

        ParsedStatement? elseStmt = null;

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

                                if (ParsedBlockFunctions.Eq(block, elseBlock)) {

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

    public static (ParsedExpression, Error?) ParseExpression(
        List<Token> tokens, 
        ref int index, 
        ExpressionKind exprKind) {

        Trace($"ParseExpression: {tokens.ElementAt(index)}");

        // As the exprStack grows, we increase the required precedence.
        // If, at any time, the operator we're looking at is the same or lower precedence
        // of what is in the expression stack, we collapse the expression stack.

        Error? error = null;

        var exprStack = new List<ParsedExpression>();

        UInt64 lastPrecedence = 1000000;

        var (lhs, lhsErr) = ParseOperand(tokens, ref index);

        error = error ?? lhsErr;

        exprStack.Add(lhs);

        var cont = true;

        while (cont && index < tokens.Count) {

            // Test to see if the next token is an operator

            if (tokens[index] is EolToken) {

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

    public static (WhenCase, Error?) ParsePatternCase(
        List<Token> tokens,
        ref int index) {

        // case:
        // QualifiedName('(' ((name ':')? expression),* ')')? '=>' (expression | block)

        Error? error = null;

        var pattern = new List<(String, Span)>();

        while (tokens.ElementAtOrDefault(index) is NameToken nt) {

            index += 1;

            if (tokens.ElementAtOrDefault(index) is PeriodToken) {

                index += 1;
            }
            else {

                pattern.Add((nt.Value, nt.Span));

                break;
            }

            pattern.Add((nt.Value, nt.Span));
        }

        var arguments = new List<(String?, String)>();
        
        var hasParens = false;

        var argsStart = index;

        if (tokens.ElementAtOrDefault(index) is LParenToken) {

            hasParens = true;
            
            index += 1;

            var cont = true;

            while (cont 
                && index < tokens.Count
                && !(tokens.ElementAtOrDefault(index) is RParenToken)) {

                if (tokens.ElementAtOrDefault(index) is NameToken nt) {

                    if (tokens.ElementAtOrDefault(index + 1) is ColonToken) {

                        index += 1;

                        if (tokens.ElementAtOrDefault(index) is ColonToken) {

                            index += 1;
                        }
                        else {

                            error = new ParserError(
                                "expected ':' after explicit pattern argument name",
                                tokens.ElementAtOrDefault(index)!.Span);

                            cont = false;
                        
                            break;
                        }

                        if (tokens.ElementAtOrDefault(index) is NameToken binding) {

                            index += 1;

                            arguments.Add((nt.Value, binding.Value));
                        }
                        else {

                            error = new ParserError(
                                "expected pattern argument name",
                                tokens.ElementAtOrDefault(index)!.Span);

                            cont = false;

                            break;
                        }

                        if (tokens.ElementAtOrDefault(index) is CommaToken) {

                            index += 1;
                        }
                    }
                    else {

                        if (tokens.ElementAtOrDefault(index) is NameToken binding) {

                            index += 1;

                            arguments.Add((null, binding.Value));
                        }
                        else {

                            error = new ParserError(
                                "expected pattern argument name",
                                tokens.ElementAtOrDefault(index)!.Span);

                            cont = false;

                            break;
                        }

                        if (tokens.ElementAtOrDefault(index) is CommaToken) {

                            index += 1;
                        }
                    }
                }
                else {

                    if (tokens.ElementAtOrDefault(index) is NameToken binding) {

                        index += 1;

                        arguments.Add((null, binding.Value));
                    }
                    else {

                        error = new ParserError(
                            "expected pattern argument name",
                            tokens.ElementAtOrDefault(index)!.Span);

                        cont = false;

                        break;
                    }
                }
            }

        }

        if (hasParens) {

            index += 1;
        }

        var argsEnd = index;

        if (tokens.ElementAtOrDefault(index) is FatArrowToken) {

            index += 1;
        }
        else {

            error = new ParserError(
                "expected '=>' after pattern case",
                tokens.ElementAtOrDefault(index)!.Span);
        }

        if (tokens.ElementAtOrDefault(index) is LCurlyToken) {

            index += 1;

            var (block, err) = ParseBlock(tokens, ref index);

            error = error ?? err;

            return (
                new EnumVariantWhenCase(
                    variantName: pattern,
                    variantArguments: arguments,
                    argumentsSpan: new Span(
                        fileId: tokens[argsStart].Span.FileId,
                        start: tokens[argsStart].Span.Start,
                        end: tokens[argsEnd - 1].Span.End),
                    body: new BlockWhenBody(block)),
                error);
        }
        else {

            var (expr, err) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

            error = error ?? err;

            return (
                new EnumVariantWhenCase(
                    variantName: pattern,
                    variantArguments: arguments,
                    argumentsSpan: new Span(
                        fileId: tokens[argsStart].Span.FileId,
                        start: tokens[argsStart].Span.Start,
                        end: tokens[argsEnd - 1].Span.End),
                    body: new ExpressionWhenBody(expr)),
                error);
        }
    }

    public static (List<WhenCase>, Error?) ParsePatterns(
        List<Token> tokens,
        ref int index) {

        var cases = new List<WhenCase>();
    
        Error? error = null;

        SkipNewLines(tokens, ref index);

        if (!(tokens.ElementAtOrDefault(index) is LCurlyToken)) {

            SkipNewLines(tokens, ref index);

            error = error ?? 
                new ParserError(
                    "expected '{'",
                    tokens[index].Span);

            return (cases, error);
        }

        index += 1;

        while (!(tokens.ElementAtOrDefault(index) is RCurlyToken)) {

            SkipNewLines(tokens, ref index);

            var (pattern, err) = ParsePatternCase(tokens, ref index);

            error = error ?? err;

            cases.Add(pattern);

            if (tokens[index] is EolToken
                || tokens[index] is CommaToken) {

                index += 1;
            }
        }

        SkipNewLines(tokens, ref index);

        if (!(tokens.ElementAtOrDefault(index) is RCurlyToken)) {

            error = error ?? 
                new ParserError(
                    "expected '}'",
                    tokens[index].Span);
        }

        index += 1;

        return (cases, error);
    }

    public static (ParsedExpression, Error?) ParseOperand(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperand: {tokens.ElementAt(index)}");

        Error? error = null;

        var span = tokens.ElementAt(index).Span;

        while (index < tokens.Count && tokens[index] is EolToken) {

            index += 1;
        }

        ParsedExpression? expr = null;

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

            case NameToken nt when nt.Value == "when": {

                var startSpan = tokens[index].Span;

                index += 1;

                var (operand, operandErr) = ParseOperand(tokens, ref index);

                error = error ?? operandErr;

                var (patterns, patternsErr) = ParsePatterns(tokens, ref index);

                error = error ?? patternsErr;

                var _span = new Span(
                    fileId: startSpan.FileId,
                    start: startSpan.Start,
                    end: operand.GetSpan().End);

                expr = new WhenExpression(operand, patterns, _span);

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

                        case LessThanToken _: {

                            // We *try* to see if it's a generic, but the parse errors, we back up and try something else

                            var (call, err) = ParseCall(tokens, ref index);

                            if (err is not null) {

                                switch (nt.Value) {

                                    case "none": {

                                        expr = new OptionalNoneExpression(span);

                                        break;
                                    }

                                    default: {

                                        expr = new VarExpression(nt.Value, span);

                                        break;
                                    }
                                }
                            }
                            else {

                                error = error ?? err;

                                expr = new CallExpression(call, span);
                            }

                            break;
                        }

                        default: {

                            index += 1;

                            switch (nt.Value) {

                                case "none": {

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

                        var exprs = new List<ParsedExpression>(new [] { _expr });

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

            case LCurlyToken _: {

                var (setExpr, err) = ParseSet(tokens, ref index);

                error = error ?? err;

                expr = setExpr;

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

                    // index += 1;

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
                                        else if (tokens.ElementAt(index) is LessThanToken) {

                                            index -= 1;

                                            var (call, parseCallErr) = ParseCall(tokens, ref index);

                                            if (parseCallErr is not null) {

                                                error = error ?? parseCallErr;
                                            }
                                            else {

                                                call.Namespace = ns;

                                                expr = new CallExpression(call, span);
                                            }

                                            contNS = false;
                                        }
                                        else {

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

                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            var (method, err) = ParseCall(tokens, ref index);
                                            
                                            error = error ?? err;

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

    public static (ParsedExpression, Error?) ParseOperator(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperator: {tokens.ElementAt(index)}");

        var span = tokens.ElementAt(index).Span;

        switch (tokens.ElementAt(index)) {

            case QuestionQuestionToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.NoneCoalescing, span),
                    null);
            }

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

    public static (ParsedExpression, Error?) ParseOperatorWithAssignment(
        List<Token> tokens, 
        ref int index) {

        Trace($"ParseOperatorWithAssignment: {tokens.ElementAt(index)}");

        var span = tokens.ElementAt(index).Span;

        switch (tokens.ElementAt(index)) {

            case QuestionQuestionToken _: {

                index += 1;

                return (
                    new OperatorExpression(BinaryOperator.NoneCoalescing, span),
                    null);
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

    public static (ParsedExpression, Error?) ParseSet(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        var output = new List<ParsedExpression>();

        Int32 start;

        if (index < tokens.Count) {

            start = index;

            switch (tokens[index]) {

                case LCurlyToken _: {

                    index += 1;

                    break;
                }

                default: {

                    error = error ??
                        new ParserError(
                            "expected '{'",
                            tokens[index].Span);

                    break;
                }
            }
        }
        else {

            start = index - 1;

            Trace("ERROR: incomplete set");

            error = error ?? 
                new ParserError(
                    "incomplete set",
                    tokens[index - 1].Span);
        }

        var cont = true;

        while (cont && index < tokens.Count) {

            switch (tokens[index]) {

                case RCurlyToken _: {

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

                default: {

                    var (expr, err) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                    error = error ?? err;

                    output.Add(expr);

                    break;
                }
            }
        }

        var end = index - 1;

        return (
            new SetExpression(
                output,
                new Span(
                    fileId: tokens[start].Span.FileId,
                    start: tokens[start].Span.Start,
                    end: tokens[end].Span.End)),
            error);
    } 

    public static (ParsedExpression, Error?) ParseArray(
        List<Token> tokens,
        ref int index) {

        Error? error = null;

        var output = new List<ParsedExpression>();
        var dictOutput = new List<(ParsedExpression, ParsedExpression)>();

        var isDictionary = false;

        var start = index;

        if (index < tokens.Count) {

            switch (tokens.ElementAt(index)) {

                case LSquareToken _: {

                    index += 1;

                    break;
                }

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

        ParsedExpression? fillSizeExpr = null;

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

                                var (dictValue, dictValueErr) = ParseExpression(
                                    tokens, 
                                    ref index, 
                                    ExpressionKind.ExpressionWithoutAssignment);

                                error = error ?? dictValueErr;

                                dictOutput.Add((expr, dictValue));
                            }
                            else {

                                error = error ??
                                    new ParserError(
                                        "key missing value in dictionary",
                                        tokens[index - 1].Span);
                            }
                        }
                        else if (!isDictionary) {

                            output.Add(expr);
                        }
                    }
                    else if (!isDictionary) {

                        output.Add(expr);
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
                    dictOutput,
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

    public static (ParsedVarDecl, Error?) ParseVariableDeclaration(
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

                        default: {

                            return (
                                new ParsedVarDecl(
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
                        new ParsedVarDecl(
                            name: nt.Value, 
                            type: new UncheckedEmptyType(), 
                            mutable: false, 
                            span: tokens.ElementAt(index - 1).Span), 
                        null);
                }
            
                if (index < tokens.Count) {

                    var declSpan = tokens[index - 1].Span;

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

                    var result = new ParsedVarDecl(
                        name: varName, 
                        type: varType, 
                        mutable: mutable,
                        span: declSpan);

                    // index += 1;

                    return (result, error);
                }
                else {

                    Trace("ERROR: expected type");

                    return (
                        new ParsedVarDecl(
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
                    new ParsedVarDecl(tokens.ElementAt(index).Span),
                    new ParserError(
                        "expected name", 
                        tokens.ElementAt(index).Span));
            }
        }
    }

    public static (ParsedType, Error?) ParseShorthandType(
        List<Token> tokens,
        ref int index) {

        if (index + 2 >= tokens.Count) {

            return (new UncheckedEmptyType(), null);
        }

        var start = tokens.ElementAt(index).Span;

        if (tokens[index] is LSquareToken) {

            // [T] is shorthand for Array<T>

            index += 1;

            var (ty, err) = ParseTypeName(tokens, ref index);

            if (tokens[index] is RSquareToken) {

                index += 1;

                return (
                    new UncheckedArrayType(
                        ty, 
                        new Span(
                            fileId: start.FileId, 
                            start: start.Start, 
                            end: tokens[index - 1].Span.End)),
                    err);   
            }

            return (
                new UncheckedEmptyType(),
                new ParserError(
                    "expected ]",
                    tokens[index].Span));
        }
        else if (tokens[index] is LCurlyToken) {

            // {T} is shorthand for Set<T>

            index += 1;

            var (ty, err) = ParseTypeName(tokens, ref index);

            if (tokens[index] is RCurlyToken){

                index += 1;

                return (
                    new UncheckedSetType(
                        ty,
                        new Span(
                            fileId: start.FileId,
                            start: start.Start,
                            end: tokens[index - 1].Span.End)),
                    err);
            }

            // TODO: Add {K:V} shorthand for Dictionary<K,V>?

            return (
                new UncheckedEmptyType(),
                err ?? 
                    new ParserError(
                        "expected ]",
                        tokens[index].Span));
        }
        else {

            return (new UncheckedEmptyType(), null);
        }
    }

    public static (ParsedType, Error?) ParseTypeName(
        List<Token> tokens, 
        ref int index) {

        ParsedType uncheckedType = new UncheckedEmptyType();

        Error? error = null;

        var start = tokens.ElementAt(index).Span;

        var typename = String.Empty;

        Trace($"ParseTypeName: {tokens.ElementAt(index)}");

        // var (vectorType, parseVectorTypeErr) = ParseArrayType(tokens, ref index);

        var (shorthandType, parseShorthandTypeErr) = ParseShorthandType(tokens, ref index);

        error = error ?? parseShorthandTypeErr;

        if (!(shorthandType is UncheckedEmptyType)) {

            return (shorthandType, error);
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

                    uncheckedType = new ParsedNameType(nt.Value, tokens.ElementAt(index).Span);

                    index += 1;
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
        
        if (index < tokens.Count) {

            if (tokens.ElementAt(index) is QuestionToken) {

                // T? is shorthand for Optional<T>

                uncheckedType = new UncheckedOptionalType(
                    type: uncheckedType,
                    new Span(
                        fileId: start.FileId,
                        start: start.Start,
                        end: tokens.ElementAt(index).Span.End));

                index += 1;
            }

            if (index < tokens.Count) {

                var previousIndex = index;

                if (tokens[index] is LessThanToken) {

                    // Generic type
                
                    index += 1;

                    var innerTypes = new List<ParsedType>();

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

                                var i = index;

                                var (innerType, innerTypeNameErr) = ParseTypeName(tokens, ref index);

                                if (i == index) {

                                    // This is not a generic parameter, reset and leave

                                    error = error ?? innerTypeNameErr;

                                    contInnerTypes = false;

                                    break;
                                }
                                
                                error = error ?? innerTypeNameErr;

                                innerTypes.Add(innerType);

                                break;
                            }
                        }
                    }

                    if (index >= tokens.Count) {

                        uncheckedType = new ParsedGenericType(
                            typename,
                            innerTypes,
                            new Span(
                                fileId: start.FileId,
                                start: start.Start,
                                end: tokens[tokens.Count - 1].Span.End));
                    }
                    else {

                        uncheckedType = new ParsedGenericType(
                            typename,
                            innerTypes,
                            new Span(
                                fileId: start.FileId,
                                start: start.Start,
                                end: tokens[index].Span.End));
                    }

                    if (error != null) {

                        index = previousIndex;
                    }
                }
            }
        }

        return (uncheckedType, error);
    }

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

    public static (ParsedCall, Error?) ParseCall(List<Token> tokens, ref int index) {

        Trace($"ParseCall: {tokens.ElementAt(index)}");

        var call = new ParsedCall();

        Error? error = null;

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                call.Name = nt.Value;

                index += 1;

                // this is to allow the lookahead. Without it, we may see something like
                // foo < Bar, think the start of a generic call when it actually isn't

                var indexReset = index;

                if (index < tokens.Count) {

                    if (tokens[index] is LessThanToken) {

                        // Generic type

                        index += 1;

                        var innerTypes = new List<ParsedType>();

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

                                    var i = index;

                                    var (innerTy, innerErr) = ParseTypeName(tokens, ref index);

                                    if (i == index) {

                                        // Can't parse further, this is not a generic call

                                        index = indexReset;

                                        contInnerTypes = false;

                                        break;
                                    }

                                    error = error ?? innerErr;

                                    // index += 1;

                                    innerTypes.Add(innerTy);

                                    break;
                                }
                            }
                        }

                        call.TypeArgs = innerTypes;
                    }
                }

                if (index < tokens.Count) {

                    switch (tokens.ElementAt(index)) {

                        case LParenToken _: {

                            index += 1;

                            break;
                        }

                        ///

                        case var t: {

                            Trace("ERROR: expected '('");

                            index = indexReset;

                            return (
                                call,
                                new ParserError("expected '('", t.Span));
                        }
                    }
                }
                else {

                    Trace("ERROR: incomplete function");

                    index = indexReset;

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
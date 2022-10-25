
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

            if (!ParsedExpressionFunctions.Eq(argL.Item2, argR.Item2)) {

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

    public partial class ParsedArrayType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedArrayType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class ParsedDictionaryType: ParsedType {

        public ParsedType Key { get; init; }
        
        public ParsedType Value { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedDictionaryType(
            ParsedType key,
            ParsedType value,
            Span span) {

            this.Key = key;
            this.Value = value;
            this.Span = span;
        }
    }

    public partial class ParsedSetType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedSetType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class ParsedOptionalType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedOptionalType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }
    
    public partial class ParsedRawPointerType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedRawPointerType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class ParsedWeakPointerType: ParsedType {

        public ParsedType Type { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedWeakPointerType(
            ParsedType type,
            Span span) {

            this.Type = type;
            this.Span = span;
        }
    }

    public partial class ParsedEmptyType: ParsedType {

        public ParsedEmptyType() { }
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
                l is ParsedArrayType la
                && r is ParsedArrayType ra: {

                return ParsedTypeFunctions.Eq(la.Type, ra.Type)
                    && SpanFunctions.Eq(la.Span, ra.Span);
            }
            
            case var _ when 
                l is ParsedOptionalType lo
                && r is ParsedOptionalType ro: {

                return Eq(lo.Type, ro.Type)
                    && SpanFunctions.Eq(lo.Span, ro.Span);
            }

            case var _ when 
                l is ParsedRawPointerType lp
                && r is ParsedRawPointerType rp: {

                return Eq(lp.Type, rp.Type)
                    && SpanFunctions.Eq(lp.Span, rp.Span);
            }

            case var _ when 
                l is ParsedWeakPointerType lw
                && r is ParsedWeakPointerType rw: {

                return Eq(lw.Type, rw.Type)
                    && SpanFunctions.Eq(lw.Span, rw.Span);
            }

            case var _ when 
                l is ParsedEmptyType le
                && r is ParsedEmptyType re: {

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

    public Visibility Visibility { get; init; }

    ///

    public ParsedVarDecl(Span span)
        : this(String.Empty, new ParsedEmptyType(), false, span, Visibility.Public) { }

    public ParsedVarDecl(
        String name,
        ParsedType type,
        bool mutable,
        Span span,
        Visibility visibility) {

        this.Name = name;
        this.Type = type;
        this.Mutable = mutable;
        this.Span = span;
        this.Visibility = visibility;
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

public partial class ParsedNamespace {

    public String? Name { get; set; }

    public List<ParsedFunction> Functions { get; init; }

    public List<ParsedStruct> Structs { get; init; }

    public List<ParsedEnum> Enums { get; init; }

    public List<ParsedNamespace> Namespaces { get; init; }

    ///

    public ParsedNamespace()
        : this(null, new List<ParsedFunction>(), new List<ParsedStruct>(), new List<ParsedEnum>(), new List<ParsedNamespace>()) { }

    public ParsedNamespace(
        String? name,
        List<ParsedFunction> functions,
        List<ParsedStruct> structs,
        List<ParsedEnum> enums,
        List<ParsedNamespace> namespaces) {

        this.Name = name;
        this.Functions = functions;
        this.Structs = structs;
        this.Enums = enums; 
        this.Namespaces = namespaces;
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

    public bool IsRecursive { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public ParsedType UnderlyingType { get; set; }

    ///

    public ParsedEnum(
        String name,
        List<(String, Span)> genericParameters,
        List<EnumVariant> variants,
        Span span,
        bool isRecursive,
        DefinitionLinkage definitionLinkage,
        ParsedType underlyingType) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Variants = variants;
        this.Span = span;
        this.IsRecursive = isRecursive;
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

    public Visibility Visibility { get; init; }

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
            visibility: Visibility.Public,
            new Span(
                fileId: 0, 
                start: 0, 
                end: 0),
            new List<ParsedParameter>(), 
            new List<(String, Span)>(),
            new ParsedBlock(), 
            throws: false,
            returnType: 
                new ParsedEmptyType(),
            linkage) { }

    public ParsedFunction(
        String name,
        Visibility visibility,
        Span nameSpan,
        List<ParsedParameter> parameters,
        List<(String, Span)> genericParameters,
        ParsedBlock block,
        bool throws,
        ParsedType returnType,
        FunctionLinkage linkage) {

        this.Name = name;
        this.Visibility = visibility;
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

public enum Visibility {
    
    Public,
    Private
}

///

public partial class ParsedParameter {

    public bool RequiresLabel { get; init; }

    public ParsedVariable Variable { get; init; }

    ///

    public ParsedParameter(
        bool requiresLabel,
        ParsedVariable variable) {

        this.RequiresLabel = requiresLabel;
        this.Variable = variable;
    }
}

///

public partial class ParsedVariable {

    public String Name { get; init; }

    public ParsedType Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public ParsedVariable(
        String name,
        ParsedType ty,
        bool mutable) {

        this.Name = name;
        this.Type = ty;
        this.Mutable = mutable;
    }
}

///

public partial class ParsedStatement {

    public ParsedStatement() { }
}

public static partial class ParsedStatementFunctions {

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
                l is ParsedExpressionStatement le
                && r is ParsedExpressionStatement re:

                return ParsedExpressionStatementFunctions.Eq(le, re);

            case var _ when
                l is ParsedDeferStatement deferL
                && r is ParsedDeferStatement deferR:

                return ParsedDeferStatementFunctions.Eq(deferL, deferR);

            ///

            case var _ when
                l is ParsedUnsafeBlockStatement lu
                && r is ParsedUnsafeBlockStatement ru:

                return ParsedUnsafeBlockStatementFunctions.Eq(lu, ru);

            ///

            case var _ when 
                l is ParsedVarDeclStatement vdsL
                && r is ParsedVarDeclStatement vdsR:

                return ParsedVarDeclStatementFunctions.Eq(vdsL, vdsR);

            ///

            case var _ when
                l is ParsedIfStatement ifL
                && r is ParsedIfStatement ifR:

                return ParsedIfStatementFunctions.Eq(ifL, ifR);

            ///

            case var _ when
                l is ParsedBlockStatement blockL
                && r is ParsedBlockStatement blockR:

                return ParsedBlockStatementFunctions.Eq(blockL, blockR);

            ///

            case var _ when 
                l is ParsedLoopStatement ll
                && r is ParsedLoopStatement rl:
                
                return ParsedLoopStatementFunctions.Eq(ll, rl);

            case var _ when
                l is ParsedWhileStatement whileL
                && r is ParsedWhileStatement whileR:

                return ParsedWhileStatementFunctions.Eq(whileL, whileR);

            ///

            case var _ when 
                l is ParsedForStatement lf
                && r is ParsedForStatement rf: {

                return ParsedForStatementFunctions.Eq(lf, rf);
            }

            ///

            case var _ when
                l is ParsedBreakStatement lb
                && r is ParsedBreakStatement rb: {

                return ParsedBreakStatementFunctions.Eq(lb, rb);
            }

            ///

            case var _ when
                l is ParsedContinueStatement lc
                && r is ParsedContinueStatement rc: {

                return ParsedContinueStatementFunctions.Eq(lc, rc);
            }

            ///

            case var _ when
                l is ParsedReturnStatement retL
                && r is ParsedReturnStatement retR:

                return ParsedReturnStatementFunctions.Eq(retL, retR);

            ///

            case var _ when
                l is ParsedThrowStatement lt
                && r is ParsedThrowStatement rt:

                return ParsedThrowStatementFunctions.Eq(lt, rt);

            ///

            case var _ when
                l is ParsedInlineCPPStatement li
                && r is ParsedInlineCPPStatement ri:

                return ParsedInlineCPPStatementFunctions.Eq(li, ri);

            ///

            case var _ when
                l is ParsedGarbageStatement gL
                && r is ParsedGarbageStatement gR:

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

            if (!ParsedStatementFunctions.Eq(l.ElementAt(i), r.ElementAt(i))) {

                return false;
            }
        }

        return true;
    }
}

///

public partial class ParsedExpressionStatement: ParsedStatement {

    public ParsedExpression Expression { get; init; }

    ///

    public ParsedExpressionStatement(
        ParsedExpression expression) {

        this.Expression = expression;
    }
}

public static partial class ParsedExpressionStatementFunctions {

    public static bool Eq(ParsedExpressionStatement le, ParsedExpressionStatement re) {

        return ParsedExpressionFunctions.Eq(le.Expression, re.Expression);
    }
}

///

public partial class ParsedDeferStatement: ParsedStatement {

    public ParsedStatement Statement { get; init; }

    ///

    public ParsedDeferStatement(
        ParsedStatement statement)
        : base() { 

        this.Statement = statement;
    }
}

public static partial class ParsedDeferStatementFunctions {

    public static bool Eq(
        ParsedDeferStatement? l,
        ParsedDeferStatement? r) {

        return ParsedStatementFunctions.Eq(l?.Statement, r?.Statement);
    }
}

///

public partial class ParsedUnsafeBlockStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    ///

    public ParsedUnsafeBlockStatement(
        ParsedBlock block) {

        this.Block = block;
    }
}

public static partial class ParsedUnsafeBlockStatementFunctions {

    public static bool Eq(
        ParsedUnsafeBlockStatement? l,
        ParsedUnsafeBlockStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class ParsedVarDeclStatement: ParsedStatement {

    public ParsedVarDecl Decl { get; init; }

    public ParsedExpression Expr { get; init; }

    ///

    public ParsedVarDeclStatement(
        ParsedVarDecl decl,
        ParsedExpression expr) {

        this.Decl = decl;
        this.Expr = expr;
    }
}

public static partial class ParsedVarDeclStatementFunctions {

    public static bool Eq(
        ParsedVarDeclStatement? l,
        ParsedVarDeclStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ParsedVarDeclStatement _l = l!;
        ParsedVarDeclStatement _r = r!;

        ///

        if (!ParsedVarDeclFunctions.Eq(_l.Decl, _r.Decl)) {

            return false;
        }

        return ParsedExpressionFunctions.Eq(_l.Expr, _r.Expr);
    }
}

///

public partial class ParsedIfStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    public ParsedBlock Block { get; init; }

    public ParsedStatement? Trailing { get; init; }

    ///

    public ParsedIfStatement(
        ParsedExpression expr,
        ParsedBlock block,
        ParsedStatement? trailing)
        : base() {

        this.Expr = expr;
        this.Block = block;
        this.Trailing = trailing;
    }
}

public static partial class ParsedIfStatementFunctions {

    public static bool Eq(
        ParsedIfStatement? l,
        ParsedIfStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ParsedIfStatement _l = l!;
        ParsedIfStatement _r = r!;

        ///

        return ParsedExpressionFunctions.Eq(_l.Expr, _r.Expr)
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block)
            && ParsedStatementFunctions.Eq(_l.Trailing, _r.Trailing);
    }
}

///

public partial class ParsedBlockStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public ParsedBlockStatement(
        ParsedBlock block) 
        : base() {

        this.Block = block;
    }
}

public static partial class ParsedBlockStatementFunctions {

    public static bool Eq(
        ParsedBlockStatement? l,
        ParsedBlockStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class ParsedLoopStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public ParsedLoopStatement(
        ParsedBlock block) {

        this.Block = block;
    }
}

public static partial class ParsedLoopStatementFunctions {

    public static bool Eq(
        ParsedLoopStatement? l,
        ParsedLoopStatement? r) {

        return ParsedBlockFunctions.Eq(l?.Block, r?.Block);
    }
}

///

public partial class ParsedWhileStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public ParsedWhileStatement(
        ParsedExpression expr,
        ParsedBlock block) 
        : base() {

        this.Expr = expr;
        this.Block = block;
    }
}

public static partial class ParsedWhileStatementFunctions {

    public static bool Eq(
        ParsedWhileStatement? l,
        ParsedWhileStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ParsedWhileStatement _l = l!;
        ParsedWhileStatement _r = r!;

        ///

        return ParsedExpressionFunctions.Eq(_l.Expr, _r.Expr) 
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block);
    }
}

///

public partial class ParsedForStatement: ParsedStatement {

    public String IteratorName { get; init; }

    public ParsedExpression Range { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public ParsedForStatement(
        String iteratorName,
        ParsedExpression range,
        ParsedBlock block) {

        this.IteratorName = iteratorName;
        this.Range = range;
        this.Block = block;
    }
}

public static partial class ParsedForStatementFunctions {

    public static bool Eq(
        ParsedForStatement? l,
        ParsedForStatement? r) {

        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        ParsedForStatement _l = l!;
        ParsedForStatement _r = r!;

        ///

        return 
            _l.IteratorName == _r.IteratorName
            && ParsedExpressionFunctions.Eq(_l.Range, _r.Range)
            && ParsedBlockFunctions.Eq(_l.Block, _r.Block);
    }
}

///

public partial class ParsedBreakStatement: ParsedStatement {

    public ParsedBreakStatement() { }
}

public static partial class ParsedBreakStatementFunctions {

    public static bool Eq(
        ParsedBreakStatement? l,
        ParsedBreakStatement? r) {

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

public partial class ParsedContinueStatement: ParsedStatement {

    public ParsedContinueStatement() { }
}

public static partial class ParsedContinueStatementFunctions {

    public static bool Eq(
        ParsedContinueStatement? l,
        ParsedContinueStatement? r) {

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

public partial class ParsedReturnStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    ///

    public ParsedReturnStatement(
        ParsedExpression expr) {

        this.Expr = expr;
    }
}

public static partial class ParsedReturnStatementFunctions {

    public static bool Eq(
        ParsedReturnStatement? l,
        ParsedReturnStatement? r) {

        return ParsedExpressionFunctions.Eq(l?.Expr, r?.Expr);
    }
}

///

public partial class ParsedThrowStatement: ParsedStatement {

    public ParsedExpression Expr { get; init; }

    ///

    public ParsedThrowStatement(
        ParsedExpression expr) {

        this.Expr = expr;
    }
}

public static partial class ParsedThrowStatementFunctions {

    public static bool Eq(
        ParsedThrowStatement? l,
        ParsedThrowStatement? r) {

        return ParsedExpressionFunctions.Eq(l?.Expr, r?.Expr);
    }
}

///

public partial class ParsedTryStatement: ParsedStatement {

    public ParsedStatement Statement { get; init; }

    public String Name { get; init; }

    public Span Span { get; init; }

    public ParsedBlock Block { get; init; }

    ///

    public ParsedTryStatement(
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

public static partial class ParsedTryStatementFunctions {

    public static bool Eq(
        ParsedTryStatement? l,
        ParsedTryStatement? r) {

        if (l is null && r is null) {

            return true;
        }

        if (l is null || r is null) {

            return false;
        }

        if (!ParsedStatementFunctions.Eq(l.Statement, r.Statement)) {

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

public partial class ParsedInlineCPPStatement: ParsedStatement {

    public ParsedBlock Block { get; init; }

    public Span Span { get; init; }

    ///

    public ParsedInlineCPPStatement(
        ParsedBlock block,
        Span span) {

        this.Block = block;
        this.Span = span;
    }
}

public static partial class ParsedInlineCPPStatementFunctions {

    public static bool Eq(
        ParsedInlineCPPStatement? l,
        ParsedInlineCPPStatement? r) {
        
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

public partial class ParsedGarbageStatement: ParsedStatement {

    public ParsedGarbageStatement() { }
}

public static partial class GarbageStatementFunctions {

    public static bool Eq(
        ParsedGarbageStatement? l,
        ParsedGarbageStatement? r) {
        
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

        return ParsedStatementFunctions.Eq(_l.Statements, _r.Statements);
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

    public partial class ParsedBooleanExpression: ParsedExpression {

        public bool Value { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedBooleanExpression(
            bool value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class ParsedNumericConstantExpression: ParsedExpression {

        public NumericConstant Value { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedNumericConstantExpression(
            NumericConstant value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class ParsedQuotedStringExpression: ParsedExpression {

        public String Value { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedQuotedStringExpression(
            String value,
            Span span)
            : base() { 
            
            this.Value = value;
            this.Span = span;
        }
    }

    public partial class ParsedCharacterLiteralExpression: ParsedExpression {

        public Char Char { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedCharacterLiteralExpression(
            Char c,
            Span span) {

            this.Char = c;
            this.Span = span;
        }
    }

    public partial class ParsedArrayExpression: ParsedExpression {

        public List<ParsedExpression> Expressions { get; init; }

        public ParsedExpression? FillSize { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedArrayExpression(
            List<ParsedExpression> expressions,
            ParsedExpression? fillSize,
            Span span) {

            this.Expressions = expressions;
            this.FillSize = fillSize;
            this.Span = span;
        }
    }

    public partial class ParsedDictionaryExpression: ParsedExpression {

        public List<(ParsedExpression, ParsedExpression)> Entries { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedDictionaryExpression(
            List<(ParsedExpression, ParsedExpression)> entries,
            Span span) {

            this.Entries = entries;
            this.Span = span;
        }
    }

    public partial class ParsedSetExpression: ParsedExpression {

        public List<ParsedExpression> Items { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedSetExpression(
            List<ParsedExpression> items,
            Span span) {

            this.Items = items;
            this.Span = span;
        }
    }

    public partial class ParsedIndexedExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public ParsedExpression Index { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedIndexedExpression(
            ParsedExpression expression,
            ParsedExpression index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class ParsedUnaryOpExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }
        
        public UnaryOperator Operator { get; init; }
        
        public Span Span { get; init; }

        ///

        public ParsedUnaryOpExpression(
            ParsedExpression expression,
            UnaryOperator op,
            Span Span) {

            this.Expression = expression;
            this.Operator = op;
            this.Span = Span;
        }
    }

    public partial class ParsedBinaryOpExpression: ParsedExpression {

        public ParsedExpression Lhs { get; init; }
        
        public BinaryOperator Operator { get; init; }
        
        public ParsedExpression Rhs { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedBinaryOpExpression(
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

    public partial class ParsedVarExpression: ParsedExpression {

        public String Value { get; init; }
        
        public Span Span { get; init; }

        ///

        public ParsedVarExpression(
            String value,
            Span span) 
            : base() {

            this.Value = value;
            this.Span = span;
        }
    }
    
    public partial class ParsedNamespacedVarExpression: ParsedExpression {

        public String Name { get; init; }

        public List<String> Items { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedNamespacedVarExpression(
            String name,
            List<String> items,
            Span span) {

            this.Name = name;
            this.Items = items;
            this.Span = span;
        }
    }

    public partial class ParsedTupleExpression: ParsedExpression {

        public List<ParsedExpression> Expressions { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedTupleExpression(
            List<ParsedExpression> expressions,
            Span span) {

            this.Expressions = expressions;
            this.Span = span;
        }
    }

    public partial class ParsedRangeExpression: ParsedExpression {

        public ParsedExpression Start { get; init; }

        public ParsedExpression End { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedRangeExpression(
            ParsedExpression start,
            ParsedExpression end,
            Span span) {

            this.Start = start;
            this.End = end;
            this.Span = span;
        }
    }

    public partial class ParsedWhenExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public List<WhenCase> Cases { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedWhenExpression(
            ParsedExpression expression,
            List<WhenCase> cases,
            Span span) {

            this.Expression = expression;
            this.Cases = cases;
            this.Span = span;
        }
    }

    public partial class ParsedIndexedTupleExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Int64 Index { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedIndexedTupleExpression(
            ParsedExpression expression,
            Int64 index,
            Span span) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
        }
    }

    public partial class ParsedIndexedStructExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }
        
        public String Name { get; init; }
        
        public Span Span { get; init; }

        ///

        public ParsedIndexedStructExpression(
            ParsedExpression expression,
            String name,
            Span span) {

            this.Expression = expression;
            this.Name = name;
            this.Span = span;
        }
    }

    public partial class ParsedCallExpression: ParsedExpression {

        public ParsedCall Call { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedCallExpression(
            ParsedCall call,
            Span span)
            : base() {

            this.Call = call;
            this.Span = span;
        }
    }

    public partial class ParsedMethodCallExpression: ParsedExpression {
        
        public ParsedExpression Expression { get; init; }
        
        public ParsedCall Call { get; init; }
        
        public Span Span { get; init; }

        ///

        public ParsedMethodCallExpression(
            ParsedExpression expression, 
            ParsedCall call, 
            Span span) {

            this.Expression = expression;
            this.Call = call;
            this.Span = span;
        }
    }

    public partial class ParsedForcedUnwrapExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedForcedUnwrapExpression(
            ParsedExpression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // FIXME: These should be implemented as `enum` variant values once available.

    public partial class ParsedOptionalNoneExpression: ParsedExpression {

        public Span Span { get; init; }
        
        ///

        public ParsedOptionalNoneExpression(
            Span span) : base() {

            this.Span = span;
        }
    }

    public partial class ParsedOptionalSomeExpression: ParsedExpression {

        public ParsedExpression Expression { get; init; }

        public Span Span { get; init; }

        ///

        public ParsedOptionalSomeExpression(
            ParsedExpression expression,
            Span span) {

            this.Expression = expression;
            this.Span = span;
        }
    }

    // Not standalone

    public partial class ParsedOperatorExpression: ParsedExpression {

        public BinaryOperator Operator { get; init; }
        
        public Span Span { get; init; }

        ///

        public ParsedOperatorExpression(
            BinaryOperator op,
            Span span)
            : base() {

            this.Operator = op;
            this.Span = span;
        }
    }
    
    // Parsing error

    public partial class ParsedGarbageExpression: ParsedExpression {

        public Span Span { get; init; }

        ///

        public ParsedGarbageExpression(
            Span span) 
            : base() {

            this.Span = span;
        }
    }

///

public static partial class ParsedExpressionFunctions {

    public static Span GetSpan(
        this ParsedExpression expr) {

        switch (expr) {

            case ParsedBooleanExpression be: {

                return be.Span;
            }

            case ParsedNumericConstantExpression ne: {

                return ne.Span;
            }

            case ParsedQuotedStringExpression qse: {

                return qse.Span;
            }

            case ParsedCharacterLiteralExpression cle: {

                return cle.Span;
            }

            case ParsedArrayExpression ve: {

                return ve.Span;
            }

            case ParsedDictionaryExpression de: {

                return de.Span;
            }

            case ParsedSetExpression se: {

                return se.Span;
            }

            case ParsedTupleExpression te: {

                return te.Span;
            }

            case ParsedRangeExpression re: {

                return re.Span;
            }

            case ParsedIndexedExpression ie: {

                return ie.Span;
            }

            case ParsedIndexedTupleExpression ite: {

                return ite.Span;
            }

            case ParsedIndexedStructExpression ise: {

                return ise.Span;
            }

            case ParsedCallExpression ce: {

                return ce.Span;
            }

            case ParsedMethodCallExpression mce: {

                return mce.Span;
            }

            case ParsedUnaryOpExpression uoe: {

                return uoe.Span;
            }

            case ParsedBinaryOpExpression boe: {

                return boe.Span;
            }

            case ParsedVarExpression ve: {

                return ve.Span;
            }

            case ParsedNamespacedVarExpression ne: {

                return ne.Span;
            }            

            case ParsedOperatorExpression oe: {

                return oe.Span;
            }

            case ParsedOptionalNoneExpression optNoneExpr: {

                return optNoneExpr.Span;
            }

            case ParsedOptionalSomeExpression optSomeExpr: {

                return optSomeExpr.Span;
            }

            case ParsedForcedUnwrapExpression forceUnwrapExpr: {

                return forceUnwrapExpr.Span;
            }

            case ParsedWhenExpression whenExpression: {

                return whenExpression.Span;
            }

            case ParsedGarbageExpression ge: {

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
                l is ParsedBooleanExpression boolL
                && r is ParsedBooleanExpression boolR:

                return boolL.Value == boolR.Value;

            ///

            case var _ when
                l is ParsedCallExpression callL
                && r is ParsedCallExpression callR:

                return ParsedCallFunctions.Eq(callL.Call, callR.Call);

            ///

            case var _ when 
                l is ParsedNumericConstantExpression ln
                && r is ParsedNumericConstantExpression rn:

                return NumericConstantFunctions.Eq(ln.Value, rn.Value);

            ///

            case var _ when
                l is ParsedQuotedStringExpression strL
                && r is ParsedQuotedStringExpression strR:

                return Equals(strL.Value, strR.Value);

            ///

            case var _ when 
                l is ParsedCharacterLiteralExpression lc
                && r is ParsedCharacterLiteralExpression rc:

                return Equals(lc.Char, rc.Char);
            
            ///

            case var _ when
                l is ParsedBinaryOpExpression lbo
                && r is ParsedBinaryOpExpression rbo: 

                return ParsedExpressionFunctions.Eq(lbo.Lhs, rbo.Lhs)
                    && lbo.Operator == rbo.Operator
                    && ParsedExpressionFunctions.Eq(lbo.Rhs, rbo.Rhs);

            ///

            case var _ when
                l is ParsedVarExpression varL
                && r is ParsedVarExpression varR:

                return varL.Value == varR.Value;

            case var _ when 
                l is ParsedNamespacedVarExpression ln
                && r is ParsedNamespacedVarExpression rn: {

                if (!Equals(ln.Name, rn.Name)) {

                    return false;
                }

                if (ln.Items.Count != rn.Items.Count) {

                    return false;
                }

                for (var i = 0; i < ln.Items.Count; i++) {

                    if (!Equals(ln.Items[i], rn.Items[i])) {

                        return false;
                    }
                }

                return true;
            }

                

            ///

            case var _ when
                l is ParsedOperatorExpression opL
                && r is ParsedOperatorExpression opR:

                return opL.Operator == opR.Operator;

            ///

            case var _ when
                l is ParsedGarbageExpression
                && r is ParsedGarbageExpression:

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

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Multiply 
                || opExpr.Operator == BinaryOperator.Modulo
                || opExpr.Operator == BinaryOperator.Divide:

                return 100;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.Add 
                || opExpr.Operator == BinaryOperator.Subtract:

                return 90;

            ///

            case ParsedOperatorExpression opExpr when
                opExpr.Operator == BinaryOperator.BitwiseLeftShift
                || opExpr.Operator == BinaryOperator.BitwiseRightShift
                || opExpr.Operator == BinaryOperator.ArithmeticLeftShift
                || opExpr.Operator == BinaryOperator.ArithmeticRightShift: 
                
                return 85;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LessThan
                || opExpr.Operator == BinaryOperator.LessThanOrEqual
                || opExpr.Operator == BinaryOperator.GreaterThan
                || opExpr.Operator == BinaryOperator.GreaterThanOrEqual
                || opExpr.Operator == BinaryOperator.Equal
                || opExpr.Operator == BinaryOperator.NotEqual:
                
                return 80;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.BitwiseAnd:

                return 73;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.BitwiseXor:

                return 72;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.BitwiseOr:

                return 72;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LogicalAnd:

                return 70;

            ///

            case ParsedOperatorExpression opExpr when 
                opExpr.Operator == BinaryOperator.LogicalOr
                || opExpr.Operator == BinaryOperator.NoneCoalescing:

                return 69;

            ///

            case ParsedOperatorExpression opExpr when 
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

    public static (ParsedNamespace, Error?) ParseNamespace(
        List<Token> tokens,
        ref int index) {

        Trace($"ParseNamespace");

        Error? error = null;

        var parsedNamespace = new ParsedNamespace();

        // var index = 0;

        var cont = true;

        while (index < tokens.Count && cont) {

            var token = tokens.ElementAt(index);

            switch (token) {

                case NameToken nt: {

                    switch (nt.Value) {

                        case "func": {

                            var (fun, err) = ParseFunction(
                                tokens, 
                                ref index, 
                                FunctionLinkage.Internal,
                                Visibility.Public);

                            error = error ?? err;

                            parsedNamespace.Functions.Add(fun);

                            break;
                        }

                        case "ref": {

                            index += 1;

                            var (_enum, enumErr) = ParseEnum(tokens, ref index, DefinitionLinkage.Internal, true);

                            error = error ?? enumErr;

                            parsedNamespace.Enums.Add(_enum);

                            break;
                        }

                        case "enum": {

                            var (_enum, err) = ParseEnum(tokens, ref index, DefinitionLinkage.Internal, false);

                            error = error ?? err;

                            parsedNamespace.Enums.Add(_enum);

                            break;
                        }

                        case "struct": {

                            var (structure, err) = ParseStruct(
                                tokens, 
                                ref index,
                                DefinitionLinkage.Internal,
                                DefinitionType.Struct);

                            error = error ?? err;

                            parsedNamespace.Structs.Add(structure);

                            break;
                        }

                        case "class": {

                            var (structure, err) = ParseStruct(
                                tokens,
                                ref index,
                                DefinitionLinkage.Internal,
                                DefinitionType.Class);

                            err = error ?? err;

                            parsedNamespace.Structs.Add(structure);
                            
                            break;
                        }

                        case "namespace": {

                            index += 1;

                            if (index + 2 < tokens.Count) {

                                // First is the name
                                // Then the LCurly and RCurly, then we parse the contents inside

                                String? name = null;

                                if (tokens[index] is NameToken namespaceName) {

                                    index += 1;

                                    name = namespaceName.Value;
                                }

                                if (tokens[index] is LCurlyToken) {

                                    index += 1;

                                    var (ns, nsErr) = ParseNamespace(tokens, ref index);

                                    error = error ?? nsErr;

                                    index += 1;

                                    if (index < tokens.Count
                                        && tokens[index] is RCurlyToken) {

                                            index += 1;
                                    }

                                    ns.Name = name;

                                    parsedNamespace.Namespaces.Add(ns);
                                }
                            }

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
                                                    FunctionLinkage.External,
                                                    Visibility.Public);

                                                error = error ?? err;

                                                parsedNamespace.Functions.Add(fun);

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

                                                parsedNamespace.Structs.Add(structure);

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

                                                parsedNamespace.Structs.Add(structure);

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

                case RCurlyToken _: {

                    cont = false;

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

        return (parsedNamespace, error);
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
        DefinitionLinkage definitionLinkage,
        bool isRecursive) {

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
            isRecursive,
            definitionLinkage,
            underlyingType: new ParsedEmptyType());

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
            // - Ident(name) LParen Type RParen
            // - Ident(name) LParen struct_body RParen
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

                    case LParenToken _: {

                        Trace("variant with type");

                        index += 1;

                        var members = new List<ParsedVarDecl>();

                        while (index < tokens.Count
                            && !(tokens[index] is RParenToken)) {
                            
                            SkipNewLines(tokens, ref index);

                            // Fields in struct-like enums are always public.

                            var (decl, varDeclParseErr) = ParseVariableDeclaration(tokens, ref index, Visibility.Public);

                            error = error ?? varDeclParseErr;

                            if (decl.Name == _enum.Name && decl.Type is ParsedEmptyType && !isRecursive) {

                                error = error ?? 
                                    new ParserError(
                                        "use 'ref enum' to make the enum recursive",
                                        tokens[index - 1].Span);
                            }
                            else {

                                switch (decl.Type) {

                                    case ParsedNameType n when n.Name == _enum.Name && !isRecursive: {

                                        error = error ?? 
                                            new ParserError(
                                                "use 'ref enum' to make the enum recursive",
                                                tokens[index - 1].Span);

                                        break;
                                    }

                                    default: {

                                        break;
                                    }
                                }
                            }

                            members.Add(decl);

                            // Allow a comma or a newline after each member

                            if (tokens[index] is CommaToken
                                || tokens[index] is EolToken) {

                                index += 1;
                            }
                        }

                        index += 1;

                        if (members.Count == 1 && members[0].Type is ParsedEmptyType) {

                            // We have a simple value (non-struct) case

                            _enum.Variants.Add(
                                new TypedEnumVariant(
                                    name,
                                    new ParsedNameType(members[0].Name, members[0].Span),
                                    new Span(
                                        fileId: tokens[index].Span.FileId,
                                        start: tokens[startIndex].Span.Start,
                                        end: tokens[index].Span.End)));
                        }
                        else {

                            _enum.Variants.Add(
                                new StructLikeEnumVariant(
                                    name,
                                    members,
                                    new Span(
                                        fileId: tokens[index].Span.FileId,
                                        start: tokens[startIndex].Span.Start,
                                        end: tokens[index].Span.End)));
                        }

                        break;
                    }

                    case EqualToken _: {

                        if (_enum.UnderlyingType is ParsedEmptyType) {

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

                    Visibility? lastVisibility = null;

                    var contFields = true;

                    while (contFields && index < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case RCurlyToken _: {

                                index += 1;

                                if (lastVisibility is Visibility) {

                                    error = error ?? 
                                        new ParserError(
                                            "Expected function or parameter after visibility modifier",
                                            tokens[index].Span);
                                }

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

                                var visibility = lastVisibility ?? definitionType switch {
                                    DefinitionType.Class => Visibility.Private,
                                    DefinitionType.Struct => Visibility.Public,
                                    _ => throw new Exception()
                                };

                                lastVisibility = null;

                                var (funcDecl, err) = ParseFunction(tokens, ref index, funcLinkage, visibility);

                                error = error ?? err;

                                methods.Add(funcDecl);

                                break;
                            }

                            case NameToken nt2 when nt2.Value == "public": {

                                lastVisibility = Visibility.Public;

                                index += 1;

                                continue;
                            }

                            case NameToken nt2 when nt2.Value == "private": {

                                lastVisibility = Visibility.Private;

                                index += 1;

                                continue;
                            }

                            case NameToken _: {

                                // Lets parse a parameter

                                var visibility = lastVisibility
                                    ?? (definitionType == DefinitionType.Class 
                                        ? Visibility.Private 
                                        : Visibility.Public);

                                lastVisibility = null;

                                var (varDecl, parseVarDeclErr) = ParseVariableDeclaration(tokens, ref index, visibility);
                                
                                error = error ?? parseVarDeclErr;

                                // Ignore immutable flag for now
                                
                                varDecl.Mutable = false;

                                if (varDecl.Type is ParsedEmptyType) {

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

                        if (lastVisibility is Visibility v) {

                            error = error ?? 
                                new ParserError(
                                    "Expected function or parameter after visibility modifier",
                                    tokens[index].Span);
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
        FunctionLinkage linkage,
        Visibility visibility) {

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
                                        variable: new ParsedVariable(
                                            name: "this",
                                            ty: new ParsedEmptyType(),
                                            mutable: currentParamIsMutable)));

                                break;
                            }

                            case NameToken _: {

                                // Now lets parse a parameter

                                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index, Visibility.Public);

                                error = error ?? varDeclErr;

                                if (varDecl.Type is ParsedEmptyType) {

                                    Trace("ERROR: parameter missing type");

                                    error = error ?? 
                                        new ParserError(
                                            "parameter missing type",
                                            varDecl.Span);
                                }
                                
                                parameters.Add(
                                    new ParsedParameter(
                                        requiresLabel: currentParamRequiresLabel,
                                        variable: new ParsedVariable(
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

                    ParsedType returnType = new ParsedEmptyType();

                    ParsedExpression? fatArrowExpr = null;

                    if ((index + 2) < tokens.Count) {

                        switch (tokens.ElementAt(index)) {

                            case FatArrowToken _: {

                                index += 1;

                                var (arrowExpr, arrowExprErr) = ParseExpression(
                                    tokens,
                                    ref index,
                                    ExpressionKind.ExpressionWithoutAssignment);

                                returnType = new ParsedEmptyType();

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
                                visibility: visibility,
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

                            _block.Statements.Add(new ParsedReturnStatement(fae));

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
                            visibility: visibility,
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
                    new ParsedThrowStatement(expr),
                    error);
            }

            case NameToken nt when nt.Value == "defer": {

                Trace("parsing defer");

                index += 1;

                var (stmt, err) = ParseStatement(tokens, ref index);

                error = error ?? err;

                return (
                    new ParsedDeferStatement(stmt), 
                    error);
            }

            case NameToken nt when nt.Value == "unsafe": {

                Trace("parsing unsafe");

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;

                return (
                    new ParsedUnsafeBlockStatement(block),
                    error);
            }

            case NameToken nt when nt.Value == "if": {

                return ParseIfStatement(tokens, ref index);
            }

            case NameToken nt when nt.Value == "break": {

                Trace("parsing break");

                index += 1;

                return (
                    new ParsedBreakStatement(),
                    null);
            }

            case NameToken nt when nt.Value == "continue": {

                Trace("parsing continue");

                index += 1;

                return (
                    new ParsedContinueStatement(),
                    null);
            }

            case NameToken nt when nt.Value == "loop": {

                Trace("parsing loop");

                index += 1;

                var (block, blockErr) = ParseBlock(tokens, ref index);

                error = error ?? blockErr;
                
                return (
                    new ParsedLoopStatement(block),
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
                    new ParsedWhileStatement(condExpr, block),
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
                    new ParsedTryStatement(stmt, errorName, errorSpan, catchBlock),
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
                                new ParsedForStatement(iter.Value, rangeExpr, block),
                                error);
                        }

                        default: {

                            return (
                                new ParsedGarbageStatement(),
                                error);
                        }
                    }
                }
                else {

                    return (
                        new ParsedGarbageStatement(),
                        error);
                }
            }

            case NameToken nt when nt.Value == "return": {

                Trace("parsing return");

                index += 1;

                var (expr, exprErr) = ParseExpression(tokens, ref index, ExpressionKind.ExpressionWithoutAssignment);

                error = error ?? exprErr;

                return (
                    new ParsedReturnStatement(expr),
                    error
                );
            }

            case NameToken nt when nt.Value == "let" || nt.Value == "var": {

                Trace("parsing let/var");

                var mutable = nt.Value == "var";

                index += 1;

                var (varDecl, varDeclErr) = ParseVariableDeclaration(tokens, ref index, Visibility.Public);

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
                                    new ParsedVarDeclStatement(varDecl, expr),
                                    error
                                );
                            }
                            else {

                                return (
                                    new ParsedGarbageStatement(),
                                    new ParserError(
                                        "expected initializer", 
                                        tokens.ElementAt(index - 1).Span)
                                );
                            }
                        }

                        ///

                        default: {

                            return (
                                new ParsedGarbageStatement(),
                                new ParserError(
                                    "expected initializer", 
                                    tokens.ElementAt(index - 1).Span));
                        }
                    }
                }
                else {

                    return (
                        new ParsedGarbageStatement(),
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
                    new ParsedInlineCPPStatement(block, _span),
                    error);
            }

            case LCurlyToken _: {

                Trace("parsing block from statement parser");

                var (block, blockErr) = ParseBlock(tokens, ref index);
            
                error = error ?? blockErr;

                return (new ParsedBlockStatement(block), error);
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
                    new ParsedExpressionStatement(expr),
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
                    new ParsedGarbageStatement(),
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

                                elseStmt = new ParsedBlockStatement(elseBlock);

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

        return (new ParsedIfStatement(cond, block, elseStmt), error);
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

                exprStack.Add(new ParsedGarbageExpression(tokens.ElementAt(index - 1).Span));
                
                exprStack.Add(new ParsedGarbageExpression(tokens.ElementAt(index - 1).Span));

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

                    case ParsedOperatorExpression oe: {

                        var span = new Span(
                            fileId: _lhs.GetSpan().FileId,
                            start: _lhs.GetSpan().Start,
                            end: _rhs.GetSpan().End);

                        exprStack.Add(new ParsedBinaryOpExpression(_lhs, oe.Operator, _rhs, span));

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

                case ParsedOperatorExpression oe: {

                    var span = new Span(
                        fileId: _lhs.GetSpan().FileId,
                        start: _lhs.GetSpan().Start,
                        end: _rhs.GetSpan().End);

                    exprStack.Add(new ParsedBinaryOpExpression(_lhs, oe.Operator, _rhs, span));

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

                expr = new ParsedBooleanExpression(true, span);

                break;
            }

            case NameToken nt when nt.Value == "false": {

                index += 1;

                expr = new ParsedBooleanExpression(false, span);

                break;
            }

            case NameToken nt when nt.Value == "and": {

                index += 1;

                expr = new ParsedOperatorExpression(BinaryOperator.LogicalAnd, span);

                break;
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                expr = new ParsedOperatorExpression(BinaryOperator.LogicalOr, span);

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

                expr = new ParsedUnaryOpExpression(_expr, new LogicalNotUnaryOperator(), _span);

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

                expr = new ParsedWhenExpression(operand, patterns, _span);

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

                                    expr = new ParsedOptionalSomeExpression(someExpr, span);

                                    break;
                                }

                                ///

                                default: {

                                    var (call, parseCallErr) = ParseCall(tokens, ref index);

                                    error = error ?? parseCallErr;

                                    expr = new ParsedCallExpression(call, span);

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

                                        expr = new ParsedOptionalNoneExpression(span);

                                        break;
                                    }

                                    default: {

                                        expr = new ParsedVarExpression(nt.Value, span);

                                        break;
                                    }
                                }
                            }
                            else {

                                error = error ?? err;

                                expr = new ParsedCallExpression(call, span);
                            }

                            break;
                        }

                        default: {

                            index += 1;

                            switch (nt.Value) {

                                case "none": {

                                    expr = new ParsedOptionalNoneExpression(span);

                                    break;
                                }

                                ///

                                default: {

                                    expr = new ParsedVarExpression(nt.Value, span);

                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
                else {

                    index += 1;

                    expr = new ParsedVarExpression(nt.Value, span);
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

                        _expr = new ParsedTupleExpression(
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

                expr = new ParsedUnaryOpExpression(_expr, new PreIncrementUnaryOperator(), _span);

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

                expr = new ParsedUnaryOpExpression(_expr, new PreDecrementUnaryOperator(), _span);

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

                expr = new ParsedUnaryOpExpression(_expr, new NegateUnaryOperator(), _span);

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

                expr = new ParsedUnaryOpExpression(_expr, new BitwiseNotUnaryOperator(), _span);

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

                expr = new ParsedUnaryOpExpression(_expr, new DereferenceUnaryOperator(), _span);

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

                            expr = new ParsedUnaryOpExpression(_expr, new RawAddressUnaryOperator(), span);

                            break;
                        }

                        default: {

                            error = error ?? 
                                new ParserError(
                                    "ampersand not currently supported",
                                    tokens.ElementAt(index - 1).Span);

                            expr = new ParsedGarbageExpression(tokens.ElementAt(index - 1).Span);

                            break;
                        }
                    }
                }
                else {

                    error = error ??
                        new ParserError(
                            "ampersand not currently supported",
                            tokens.ElementAt(index - 1).Span);

                    expr = new ParsedGarbageExpression(tokens.ElementAt(index - 1).Span);
                }

                break;
            }

            case NumberToken numTok: {

                index += 1;

                expr = new ParsedNumericConstantExpression(numTok.Value, span);

                break;
            }

            case QuotedStringToken qs: {

                index += 1;

                expr = new ParsedQuotedStringExpression(qs.Value, span);

                break;
            }

            case SingleQuotedStringToken ct: {

                index += 1;

                if (ct.Value.FirstOrDefault() is Char c) {

                    expr = new ParsedCharacterLiteralExpression(c, span);
                }
                else {

                    expr = new ParsedGarbageExpression(span);
                }

                break;
            }

            default: {

                Trace("ERROR: unsupported expression");

                error = error ??
                    new ParserError(
                        "unsupported expression",
                        tokens.ElementAt(index).Span);

                expr = new ParsedGarbageExpression(span);

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

                    expr = new ParsedRangeExpression(expr, endExpr, span);

                    break;
                }

                case ExclamationToken _: {

                    index += 1;

                    // Forced Optional unwrap

                    expr = new ParsedForcedUnwrapExpression(expr, span);

                    break;
                }

                case PlusPlusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new ParsedUnaryOpExpression(expr, new PostIncrementUnaryOperator(), _span);

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

                    expr = new ParsedUnaryOpExpression(expr, new IsUnaryOperator(isTypeName), _span);

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

                            cast = new TruncatingTypeCast(new ParsedEmptyType());

                            break;
                        }
                    }

                    // index += 1;

                    expr = new ParsedUnaryOpExpression(expr, new TypeCastUnaryOperator(cast), _span);

                    break;
                }

                case MinusMinusToken _: {

                    var endSpan = tokens.ElementAt(index).Span;

                    index += 1;

                    var _span = new Span(
                        fileId: expr.GetSpan().FileId,
                        start: expr.GetSpan().Start,
                        end: endSpan.End);

                    expr = new ParsedUnaryOpExpression(expr, new PostDecrementUnaryOperator(), _span);

                    break;
                }

                case PeriodToken _: {

                    index += 1;

                    if (expr is ParsedVarExpression ve) {

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

                                    expr = new ParsedIndexedTupleExpression(
                                        expr,
                                        numberToken.Value.IntegerConstant()!.ToInt64(),
                                        _span);

                                    contNS = false;

                                    break;
                                }

                                case NameToken nsNameToken: {

                                    index += 1;

                                    if (index < tokens.Count) {

                                        if (tokens[index] is LParenToken) {

                                            index -= 1;

                                            var (method, parseCallErr) = ParseCall(tokens, ref index);

                                            error = error ?? parseCallErr;

                                            method.Namespace = ns;

                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            if (method.Namespace.LastOrDefault() is String n
                                                && !IsNullOrWhiteSpace(n)
                                                && Char.IsUpper(n[0])) {

                                                expr = new ParsedCallExpression(method, _span);
                                            }
                                            else {

                                                // Maybe clear namespace too?

                                                expr = new ParsedMethodCallExpression(expr, method, span);
                                            }

                                            contNS = false;
                                        }
                                        else if (tokens[index] is PeriodToken) {

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
                                        else if (tokens[index] is LessThanToken) {

                                            index -= 1;

                                            var (call, parseCallErr) = ParseCall(tokens, ref index);

                                            if (parseCallErr is not null) {

                                                error = error ?? parseCallErr;
                                            }
                                            else {

                                                call.Namespace = ns;

                                                expr = new ParsedCallExpression(call, span);
                                            }

                                            contNS = false;
                                        } 
                                        else {

                                            if (ns.Count == 1 
                                                && ns[0].Length > 0 
                                                && Char.IsLower(ns[0][0]))  {

                                                var _span = new Span(
                                                    fileId: expr.GetSpan().FileId,
                                                    start: expr.GetSpan().Start,
                                                    end: tokens[index - 1].Span.End);

                                                expr = new ParsedIndexedStructExpression(
                                                    expr,
                                                    nsNameToken.Value,
                                                    _span);

                                                contNS = false;
                                            }
                                            else {

                                                String name = tokens[index - 1] switch {

                                                    NameToken nt => nt.Value,
                                                    _ => throw new Exception()
                                                };

                                                // Just a reference to a variable in a namespace
                                                
                                                expr = new ParsedNamespacedVarExpression(
                                                    name,
                                                    ns,
                                                    new Span(
                                                        fileId: span.FileId, 
                                                        start: span.Start, 
                                                        end: tokens[index].Span.End));

                                                contNS = false;
                                            }
                                                
                                            break;
                                        }
                                    }
                                    else {

                                        error = error ?? 
                                            new ParserError(
                                                "Unsupported static method call",
                                                tokens[index].Span);
                                        
                                        contNS = false;
                                    }

                                    break;
                                }

                                default: {

                                    // Indexed struct?

                                    index += 1;

                                    error = error ?? 
                                        new ParserError(
                                            "Unsupported static method call",
                                            tokens[index].Span);

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

                                    expr = new ParsedIndexedTupleExpression(expr, number.Value.IntegerConstant()!.ToInt64(), _span);

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

                                            expr = new ParsedMethodCallExpression(expr, method, _span);
                                        }
                                        else {
        
                                            var _span = new Span(
                                                fileId: expr.GetSpan().FileId,
                                                start: expr.GetSpan().Start,
                                                end: tokens.ElementAt(index).Span.End);

                                            expr = new ParsedIndexedStructExpression(
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

                                        expr = new ParsedIndexedStructExpression(
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

                        expr = new ParsedIndexedExpression(
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
                    new ParsedOperatorExpression(BinaryOperator.NoneCoalescing, span),
                    null);
            }

            case NameToken nt when nt.Value == "and": {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.LogicalAnd, span), null);
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.LogicalOr, span), null);
            }

            case PlusToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Add, span), null);
            }

            case MinusToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Subtract, span), null);
            }

            case AsteriskToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Multiply, span), null);
            }

            case ForwardSlashToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Divide, span), null);
            }

            case PercentToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Modulo, span), null);
            }

            case EqualToken _: {

                Trace("ERROR: assignment not allowed in this position");
                
                index += 1;
                
                return (
                    new ParsedOperatorExpression(BinaryOperator.Assign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case LeftShiftEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.BitwiseLeftShiftAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case RightShiftEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.BitwiseRightShiftAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case AmpersandEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.BitwiseAndAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case PipeEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.BitwiseOrAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case CaretEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.BitwiseXorAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case PlusEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.AddAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }
            
            case MinusEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.SubtractAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case AsteriskEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.MultiplyAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case ForwardSlashEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.DivideAssign, span), 
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case PercentEqualToken _: {

                Trace("ERROR: assignment not allowed in this position");

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ModuloAssign, span),
                    new ValidationError(
                        "assignment is not allowed in this position",
                        span));
            }

            case DoubleEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.Equal, span), null);
            }
            
            case NotEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.NotEqual, span), null);
            }
            
            case LessThanToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.LessThan, span), null);
            }
            
            case LessThanOrEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.LessThanOrEqual, span), null);
            }
            
            case GreaterThanToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.GreaterThan, span), null);
            }
            
            case GreaterThanOrEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.GreaterThanOrEqual, span), null);
            }

            case AmpersandToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseAnd, span), null);
            }

            case PipeToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseOr, span), null);
            }

            case CaretToken _: {
    
                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseXor, span), null);
            }

            case LeftShiftToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseLeftShift, span), null);
            }

            case RightShiftToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseRightShift, span), null);
            }

            case LeftArithmeticShiftToken _: {

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ArithmeticLeftShift, span),
                    null);
            }

            case RightArithmeticShiftToken _: {

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ArithmeticRightShift, span),
                    null);
            }

            default: {

                Trace("ERROR: unsupported operator (possibly just the end of an expression)");

                return (
                    new ParsedGarbageExpression(span),
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
                    new ParsedOperatorExpression(BinaryOperator.NoneCoalescing, span),
                    null);
            }

            case PlusToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Add, span), null);
            }

            case MinusToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Subtract, span), null);
            }

            case AsteriskToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Multiply, span), null);
            }

            case ForwardSlashToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Divide, span), null);
            }

            case PercentToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.Modulo, span), null);
            }

            case NameToken nt when nt.Value == "and": {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.LogicalAnd, span), null);
            }

            case NameToken nt when nt.Value == "or": {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.LogicalOr, span), null);
            }

            case AmpersandToken _: {

                index += 1;
            
                return (new ParsedOperatorExpression(BinaryOperator.BitwiseAnd, span), null);
            }

            case PipeToken _: {

                index += 1;
            
                return (new ParsedOperatorExpression(BinaryOperator.BitwiseOr, span), null);
            }

            case CaretToken _: {

                index += 1;
            
                return (new ParsedOperatorExpression(BinaryOperator.BitwiseXor, span), null);
            }

            case LeftShiftToken _: {

                index += 1;
            
                return (new ParsedOperatorExpression(BinaryOperator.BitwiseLeftShift, span), null);
            }

            case RightShiftToken _: {

                index += 1;
            
                return (new ParsedOperatorExpression(BinaryOperator.BitwiseRightShift, span), null);
            }

            case LeftArithmeticShiftToken _: {

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ArithmeticLeftShift, span),
                    null);
            }

            case RightArithmeticShiftToken _: {

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ArithmeticRightShift, span),
                    null);
            }

            case EqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.Assign, span), null);
            }

            case LeftShiftEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseLeftShiftAssign, span), null);
            }

            case RightShiftEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseRightShiftAssign, span), null);
            }

            case AmpersandEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseAndAssign, span), null);
            }

            case PipeEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseOrAssign, span), null);
            }

            case CaretEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.BitwiseXorAssign, span), null);
            }

            case PlusEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.AddAssign, span), null);
            }
            
            case MinusEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.SubtractAssign, span), null);
            }

            case AsteriskEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.MultiplyAssign, span), null);
            }

            case ForwardSlashEqualToken _: {

                index += 1;

                return (new ParsedOperatorExpression(BinaryOperator.DivideAssign, span), null);
            }

            case PercentEqualToken _: {

                index += 1;

                return (
                    new ParsedOperatorExpression(BinaryOperator.ModuloAssign, span),
                    null);
            }

            case DoubleEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.Equal, span), null);
            }
            
            case NotEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.NotEqual, span), null);
            }
            
            case LessThanToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.LessThan, span), null);
            }
            
            case LessThanOrEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.LessThanOrEqual, span), null);
            }
            
            case GreaterThanToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.GreaterThan, span), null);
            }
            
            case GreaterThanOrEqualToken _: {
                
                index += 1;
                
                return (new ParsedOperatorExpression(BinaryOperator.GreaterThanOrEqual, span), null);
            }

            default: {

                Trace("ERROR: unsupported operator (possibly just the end of an expression)");

                return (
                    new ParsedGarbageExpression(span),
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
            new ParsedSetExpression(
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
                new ParsedDictionaryExpression(
                    dictOutput,
                    new Span(
                        fileId: tokens.ElementAt(start).Span.FileId,
                        start: tokens.ElementAt(start).Span.Start,
                        end: tokens.ElementAt(end).Span.End)),
                error);
        }
        else {

            return (
                new ParsedArrayExpression(
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
        ref int index,
        Visibility visibility) {

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
                                    type: new ParsedEmptyType(),
                                    mutable: false,
                                    span: tokens.ElementAt(index - 1).Span,
                                    visibility),
                                null);
                        }
                    }
                }
                else {

                    return (
                        new ParsedVarDecl(
                            name: nt.Value, 
                            type: new ParsedEmptyType(), 
                            mutable: false, 
                            span: tokens.ElementAt(index - 1).Span,
                            visibility), 
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
                        span: declSpan,
                        visibility);

                    // index += 1;

                    return (result, error);
                }
                else {

                    Trace("ERROR: expected type");

                    return (
                        new ParsedVarDecl(
                            nt.Value, 
                            type: new ParsedEmptyType(),
                            mutable: false,
                            span: tokens.ElementAt(index - 2).Span,
                            visibility), 
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

            return (new ParsedEmptyType(), null);
        }

        var start = tokens.ElementAt(index).Span;

        if (tokens[index] is LSquareToken) {

            // [T] is shorthand for Array<T>

            // [K:V] is shorthand for Dictionary<K, V>

            index += 1;

            var (parsedTy, err) = ParseTypeName(tokens, ref index);

            switch (tokens[index]) {

                case RSquareToken _: {

                    index += 1;

                    return (
                        new ParsedArrayType(
                            parsedTy, 
                            new Span(
                                fileId: start.FileId, 
                                start: start.Start, 
                                end: tokens[index - 1].Span.End)),
                        err);   
                }

                case ColonToken _: {

                    index += 1;

                    var (valueParsedType, valueErr) = ParseTypeName(tokens, ref index);

                    if (index < tokens.Count) {

                        if (tokens[index] is RSquareToken) {

                            index += 1;

                            return (
                                new ParsedDictionaryType(
                                    parsedTy,
                                    valueParsedType,
                                    new Span(
                                        fileId: start.FileId,
                                        start: start.Start,
                                        end: tokens[index - 1].Span.End)),
                                err ?? valueErr);
                        }
                    }

                    break;
                }

                default: {

                    break;
                }
            }

            return (
                new ParsedEmptyType(),
                new ParserError(
                    "expected ]",
                    tokens[index].Span));
        }
        else if (tokens[index] is LCurlyToken) {

            // {T} is shorthand for Set<T>

            index += 1;

            var (parsedType, err) = ParseTypeName(tokens, ref index);

            if (tokens[index] is RCurlyToken){

                index += 1;

                return (
                    new ParsedSetType(
                        parsedType,
                        new Span(
                            fileId: start.FileId,
                            start: start.Start,
                            end: tokens[index - 1].Span.End)),
                    err);
            }

            // TODO: Add {K:V} shorthand for Dictionary<K,V>?

            return (
                new ParsedEmptyType(),
                err ?? 
                    new ParserError(
                        "expected ]",
                        tokens[index].Span));
        }
        else {

            return (new ParsedEmptyType(), null);
        }
    }

    public static (ParsedType, Error?) ParseTypeName(
        List<Token> tokens, 
        ref int index) {

        ParsedType uncheckedType = new ParsedEmptyType();

        Error? error = null;

        var start = tokens.ElementAt(index).Span;

        var typename = String.Empty;

        Trace($"ParseTypeName: {tokens.ElementAt(index)}");

        // var (vectorType, parseVectorTypeErr) = ParseArrayType(tokens, ref index);

        var (shorthandType, parseShorthandTypeErr) = ParseShorthandType(tokens, ref index);

        error = error ?? parseShorthandTypeErr;

        if (!(shorthandType is ParsedEmptyType)) {

            return (shorthandType, error);
        }

        switch (tokens.ElementAt(index)) {

            case NameToken nt: {

                if (nt.Value == "raw" || nt.Value == "weak") {

                    index += 1;

                    if (index < tokens.Count) {

                        var (childParsedType, err) = ParseTypeName(tokens, ref index);

                        error = error ?? err;

                        if (nt.Value == "raw") {
    
                            uncheckedType = new ParsedRawPointerType(
                                childParsedType,
                                new Span(
                                    fileId: start.FileId,
                                    start: start.Start,
                                    end: tokens.ElementAt(index - 1).Span.End));
                        }
                        else if (nt.Value == "weak") {

                            uncheckedType = new ParsedWeakPointerType(
                                childParsedType,
                                new Span(
                                    fileId: start.FileId,
                                    start: start.Start,
                                    end: tokens.ElementAt(index - 1).Span.End));
                        }
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

                uncheckedType = new ParsedOptionalType(
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

                while (cont && index < tokens.Count) {

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

                            var (expr, exprError) = ParseExpression(
                                tokens, 
                                ref index, 
                                ExpressionKind.ExpressionWithoutAssignment);

                            if (exprError is Error _exprError) {

                                Trace("ERROR: error while parsing expression in function call parameter");

                                error = error ?? _exprError;

                                cont = false;

                                break;
                            }
                            else {

                                error = error ?? exprError;
                            }

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
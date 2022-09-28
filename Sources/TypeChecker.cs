
namespace Neu;

public partial class CheckedFile {

    public List<CheckedFunction> CheckedFunctions { get; init; }

    ///

    public CheckedFile()
        : this(new List<CheckedFunction>()) { }

    public CheckedFile(
        List<CheckedFunction> checkedFunctions) {

        this.CheckedFunctions = checkedFunctions;
    }
}

///

public partial class CheckedFunction { 

    public String Name { get; init; }
    
    public NeuType ReturnType { get; init; }
    
    // public List<(String, NeuType)> Parameters { get; init; }
    public List<Variable> Parameters { get; init; }
    
    public CheckedBlock Block { get; init; }

    ///

    public CheckedFunction(
        String name,
        NeuType returnType,
        // List<(String, NeuType)> parameters,
        List<Variable> parameters,
        CheckedBlock block) { 

        this.Name = name;
        this.ReturnType = returnType;
        this.Parameters = parameters;
        this.Block = block;
    }
}

///

public partial class CheckedBlock {

    public List<CheckedStatement> Stmts { get; init; }

    ///

    public CheckedBlock() 
        : this(new List<CheckedStatement>()) { }

    public CheckedBlock(
        List<CheckedStatement> stmts) { 

        this.Stmts = stmts;
    }
}

///

public partial class CheckedStatement {

    public CheckedStatement() { }
}

///

    public partial class CheckedDeferStatement: CheckedStatement {

        public CheckedBlock Block { get; init; }

        ///

        public CheckedDeferStatement(
            CheckedBlock block) {

            this.Block = block;
        }
    }

    public partial class CheckedVarDeclStatement: CheckedStatement {

        public VarDecl VarDecl { get; init; } 
        
        public CheckedExpression Expr { get; init; }

        ///

        public CheckedVarDeclStatement(
            VarDecl varDecl,
            CheckedExpression expr) {

            this.VarDecl = varDecl;
            this.Expr = expr;
        }
    }
    
    public partial class CheckedIfStatement: CheckedStatement {

        public CheckedExpression Expr { get; init; } 
        
        public CheckedBlock Block { get; init; }

        public CheckedStatement? Trailing { get; init; }

        ///

        public CheckedIfStatement(
            CheckedExpression expr,
            CheckedBlock block,
            CheckedStatement? trailing) {

            this.Expr = expr;
            this.Block = block;
            this.Trailing = trailing;
        }
    }

    public partial class CheckedBlockStatement: CheckedStatement {

        public CheckedBlock Block { get; init; }

        ///

        public CheckedBlockStatement(
            CheckedBlock block) {

            this.Block = block;
        }
    }

    public partial class CheckedWhileStatement: CheckedStatement {

        public CheckedExpression Expression { get; init; }
        
        public CheckedBlock Block { get; init; }

        ///

        public CheckedWhileStatement(
            CheckedExpression expression,
            CheckedBlock block) {

            this.Expression = expression;
            this.Block = block;
        }
    }

    public partial class CheckedReturnStatement: CheckedStatement {

        public CheckedExpression Expr { get; init; } 

        public CheckedReturnStatement(
            CheckedExpression expr) { 

            this.Expr = expr;
        }
    }

    public partial class CheckedGarbageStatement: CheckedStatement {

        public CheckedGarbageStatement() { }
    }

///

public partial class CheckedExpression: CheckedStatement {

    public CheckedExpression() { }
}

    // Standalone

    public partial class CheckedBooleanExpression: CheckedExpression {

        public bool Value { get; init; }

        ///

        public CheckedBooleanExpression(
            bool value) {

            this.Value = value;
        }
    }

    public partial class CheckedCallExpression: CheckedExpression {

        public CheckedCall Call { get; init; }
        
        public NeuType Type { get; init; }

        ///

        public CheckedCallExpression(
            CheckedCall call,
            NeuType type) {

            this.Call = call;
            this.Type = type;
        }
    }

    public partial class CheckedInt64Expression: CheckedExpression {

        public Int64 Value { get; init; }

        ///

        public CheckedInt64Expression(
            Int64 value) {

            this.Value = value;
        }
    }

    public partial class CheckedQuotedStringExpression: CheckedExpression {

        public String Value { get; init; }

        ///

        public CheckedQuotedStringExpression(
            String value) {

            this.Value = value;
        }
    }

    public partial class CheckedBinaryOpExpression: CheckedExpression {

        public CheckedExpression Lhs { get; init; }

        public Operator Operator { get; init; }

        public CheckedExpression Rhs { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedBinaryOpExpression(
            CheckedExpression lhs,
            Operator op,
            CheckedExpression rhs,
            NeuType type) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Type = type;
        }
    }



    public partial class CheckedVectorExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }
        
        public NeuType Type { get; init; }

        ///

        public CheckedVectorExpression(
            List<CheckedExpression> expressions,
            NeuType type) 
            : base() {

            this.Expressions = expressions;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }
        
        public CheckedExpression Index { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedIndexedExpression(
            CheckedExpression expression,
            CheckedExpression index,
            NeuType type) 
            : base() {

            this.Expression = expression;
            this.Index = index;
            this.Type = type;
        }
    }





    public partial class CheckedVarExpression: CheckedExpression {
        
        // public String Name { get; init; }

        // public NeuType Type { get; init; }

        public Variable Variable { get; init; }

        ///

        public CheckedVarExpression(
            // String name,
            // NeuType type
            Variable variable
            ) {

            // this.Name = name;
            // this.Type = type;
            this.Variable = variable;
        }
    }

    // Parsing error

    public partial class CheckedGarbageExpression: CheckedExpression {

        public CheckedGarbageExpression() { }
    }

///

public static partial class CheckedExpressionFunctions {

    public static NeuType GetNeuType(
        this CheckedExpression expr) {

        switch (expr) {

            case CheckedBooleanExpression _: {

                return new BoolType();
            }

            case CheckedCallExpression e: {

                return e.Type;
            }
            
            case CheckedInt64Expression _: {

                return new Int64Type();
            }

            case CheckedQuotedStringExpression _: {

                return new StringType();   
            }

            case CheckedBinaryOpExpression e: {

                return e.Type;
            }

            case CheckedVarExpression ve: {

                return ve.Variable.Type;
            }

            case CheckedVectorExpression vecExpr: {

                return vecExpr.Type;
            }

            case CheckedIndexedExpression ie: {

                return ie.Type;
            }

            case CheckedGarbageExpression _: {

                return new UnknownType();
            }

            default:

                throw new Exception();
        }
    }
}

///

public partial class CheckedCall {
    
    public String Name { get; init; }
    
    public List<(String, CheckedExpression)> Args { get; init; }
    
    public NeuType Type { get; init; }

    ///

    public CheckedCall(
        String name,
        List<(String, CheckedExpression)> args,
        NeuType type) {

        this.Name = name;
        this.Args = args;
        this.Type = type;
    }
}

public partial class Stack {
    
    public List<StackFrame> Frames { get; init; }

    ///

    public Stack()
        : this(new List<StackFrame>()) { }

    public Stack(
        List<StackFrame> frames) {

        this.Frames = frames;
    }
}

///

public static partial class StackFunctions {

    public static void PushFrame(
        this Stack s) {

        s.Frames.Add(new StackFrame());
    }    

    public static void PopFrame(
        this Stack s) {

        s.Frames.Pop();
    }

    public static Error? AddVar(
        this Stack s, 
        Variable v, 
        Span span) {

        if (s.Frames.Last() is StackFrame frame) {

            foreach (var existingVar in frame.Vars) {

                if (v.Name == existingVar.Name) {

                    return new TypeCheckError(
                        $"redefinition of {v.Name}",
                        span);
                }
            }

            frame.Vars.Add(v);
        }

        return null;
    }

    public static Variable? FindVar(this Stack s, String varName) {

        for (var i = s.Frames.Count - 1; i >= 0; --i) {

            var frame = s.Frames.ElementAt(i);

            foreach (var v in frame.Vars) {

                if (v.Name == varName) {

                    return v;
                }
            }
        }

        return null;
    }
}

///


public partial class StackFrame {
    
    // public List<(String, NeuType)> Vars { get; init; }
    public List<Variable> Vars { get; init; }

    ///

    public StackFrame() 
        // : this(new List<(String, NeuType)>()) { }
        : this(new List<Variable>()) { }

    public StackFrame(
        // List<(String, NeuType)> vars) {
        List<Variable> vars) {

        this.Vars = vars;
    }
}

///

public static partial class TypeCheckerFunctions {

    public static (CheckedFile, Error?) TypeCheckFile(
        ParsedFile file) {

        var stack = new Stack();

        return TypeCheckFileHelper(file, stack);
    }

    public static (CheckedFile, Error?) TypeCheckFileHelper(
        ParsedFile file,
        Stack stack) {

        var output = new CheckedFile();

        Error? error = null;

        foreach (var fun in file.Functions) {

            var (checkedFun, funErr) = TypeCheckFunction(fun, stack, file);

            error = error ?? funErr;

            output.CheckedFunctions.Add(checkedFun);
        }

        return (output, error);
    }

    public static (CheckedFunction, Error?) TypeCheckFunction(
        Function fun,
        Stack stack,
        ParsedFile file) {

        Error? error = null;

        stack.PushFrame();

        foreach (var p in fun.Parameters) {
            
            if (stack.AddVar(p, fun.NameSpan) is Error e) {

                error = error ?? e;
            }
        }

        var (block, blockErr) = TypeCheckBlock(fun.Block, stack, file);

        error = error ?? blockErr;

        stack.PopFrame();

        var output = new CheckedFunction(
            name: fun.Name,
            returnType: fun.ReturnType,
            parameters: fun.Parameters,
            block);

        return (output, error);
    }

    public static (CheckedBlock, Error?) TypeCheckBlock(
        Block block,
        Stack stack,
        ParsedFile file) {

        Error? error = null;

        var checkedBlock = new CheckedBlock();

        stack.PushFrame();

        foreach (var stmt in block.Statements) {

            var (checkedStmt, err) = TypeCheckStatement(stmt, stack, file);

            error = error ?? err;

            checkedBlock.Stmts.Add(checkedStmt);
        }

        stack.PopFrame();

        return (checkedBlock, error);
    }

    public static bool CompareTypes(NeuType a, NeuType b) {

        switch (true) {

            case var _ when a is BoolType && b is BoolType:             return true;
            case var _ when a is StringType && b is StringType:         return true;
            case var _ when a is Int8Type && b is Int8Type:             return true;
            case var _ when a is Int16Type && b is Int16Type:           return true;
            case var _ when a is Int32Type && b is Int32Type:           return true;
            case var _ when a is Int64Type && b is Int64Type:           return true;
            case var _ when a is UInt8Type && b is UInt8Type:           return true;
            case var _ when a is UInt16Type && b is UInt16Type:         return true;
            case var _ when a is UInt32Type && b is UInt32Type:         return true;
            case var _ when a is UInt64Type && b is UInt64Type:         return true;
            case var _ when a is FloatType && b is FloatType:           return true;
            case var _ when a is DoubleType && b is DoubleType:         return true;
            case var _ when a is VoidType && b is VoidType:             return true;

            // case var _ when a is VectorType va && b is VectorType vb:   return CompareTypes(va.Type, vb.Type);

            case var _ when a is UnknownType && b is UnknownType:       return true;

            default:                                                    return false;
        }
    }

    public static (CheckedStatement, Error?) TypeCheckStatement(
        Statement stmt,
        Stack stack,
        ParsedFile file) {

        Error? error = null;

        switch (stmt) {

            case Expression e: {

                var (checkedExpr, exprErr) = TypeCheckExpression(e, stack, file);

                return (
                    checkedExpr,
                    exprErr);
            }

            case DeferStatement ds: {

                var (checkedBlock, blockErr) = TypeCheckBlock(ds.Block, stack, file);

                return (
                    new CheckedDeferStatement(checkedBlock),
                    blockErr);
            }

            case VarDeclStatement vds: {

                var (checkedExpr, exprErr) = TypeCheckExpression(vds.Expr, stack, file);

                error = error ?? exprErr;

                if (vds.Decl.Type is UnknownType) {

                    vds.Decl.Type = checkedExpr.GetNeuType();
                }

                // Taking this out for now until we have better number type support
                // else if (!NeuTypeFunctions.Eq(vds.Decl.Type, checkedExpr.GetNeuType())) {
                // // else if (vds.Decl.Type != checkedExpr.GetNeuType()) {

                //     error = error ?? new TypeCheckError(
                //         "mismatch between declaration and initializer",
                //         vds.Expr.GetSpan());
                // }

                var v = new Variable(
                    name: vds.Decl.Name, 
                    ty: vds.Decl.Type, 
                    mutable: vds.Decl.Mutable);

                if (stack.AddVar(v, vds.Decl.Span) is Error e) {

                    error = error ?? e;
                }

                return (
                    new CheckedVarDeclStatement(vds.Decl, checkedExpr),
                    error);
            }

            case IfStatement ifStmt: {

                var (checkedCond, exprErr) = TypeCheckExpression(ifStmt.Expr, stack, file);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ifStmt.Block, stack, file);
                
                error = error ?? blockErr;

                CheckedStatement? elseOutput = null;

                if (ifStmt.Trailing is Statement elseStmt) {

                    var (checkedElseStmt, checkedElseStmtErr) = TypeCheckStatement(elseStmt, stack, file);

                    error = error ?? checkedElseStmtErr;

                    elseOutput = checkedElseStmt;
                }
                else {

                    elseOutput = null;
                }

                return (
                    new CheckedIfStatement(checkedCond, checkedBlock, elseOutput), 
                    error);
            }

            case WhileStatement ws: {

                var (checkedCond, exprErr) = TypeCheckExpression(ws.Expr, stack, file);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ws.Block, stack, file);
                
                error = error ?? blockErr;

                return (
                    new CheckedWhileStatement(checkedCond, checkedBlock), 
                    error);
            }

            case ReturnStatement rs: {

                var (output, outputErr) = TypeCheckExpression(rs.Expr, stack, file);

                return (
                    new CheckedReturnStatement(output), 
                    outputErr);
            }

            case BlockStatement bs: {

                var (checkedBlock, checkedBlockErr) = TypeCheckBlock(bs.Block, stack, file);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    checkedBlockErr);
            }
            
            case GarbageStatement _: {

                return (
                    new CheckedGarbageStatement(),
                    null);
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (CheckedExpression, Error?) TypeCheckExpression(
        Expression expr,
        Stack stack,
        ParsedFile file) {

        Error? error = null;

        switch (expr) {

            case BinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, stack, file);

                error = error ?? checkedLhsErr;

                var opSpan = e.Op.GetSpan();

                var op = e.Op switch {
                    OperatorExpression o => o.Operator,
                    _ => throw new Exception()
                };

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, stack, file);

                error = error ?? checkedRhsErr;

                error = error ?? TypeCheckOperation(
                    checkedLhs,
                    op,
                    checkedRhs,
                    opSpan);
                
                // TODO: actually do the binary operator typecheck against safe operations
                // For now, use a type we know

                var ty = checkedLhs.GetNeuType();

                return (
                    new CheckedBinaryOpExpression(
                        checkedLhs, 
                        op, 
                        checkedRhs, 
                        ty),
                    error);
            }

            case BooleanExpression e: {

                return (
                    new CheckedBooleanExpression(e.Value),
                    null);
            }

            case CallExpression e: {

                var (checkedCall, checkedCallErr) = TypeCheckCall(e.Call, stack, e.Span, file);

                return (
                    new CheckedCallExpression(checkedCall, new UnknownType()),
                    checkedCallErr);
            }

            case Int64Expression e: {

                return (
                    new CheckedInt64Expression(e.Value),
                    null);
            }

            case QuotedStringExpression e: {

                return (
                    new CheckedQuotedStringExpression(e.Value),
                    null);
            }

            case VarExpression e: {

                if (stack.FindVar(e.Value) is Variable v) {

                    return (
                        // new CheckedVarExpression(e.Value, ty),
                        new CheckedVarExpression(v),
                        null);
                }
                else {
                    
                    return (
                        // new CheckedVarExpression(e.Value, new UnknownType()),
                        new CheckedVarExpression(
                            new Variable(
                                e.Value, 
                                ty: new UnknownType(), 
                                mutable: false)
                        ),
                        new TypeCheckError(
                            "variable not found",
                            e.Span));
                }
            }

            ///

            case VectorExpression ve: {

                NeuType innerType = new UnknownType();

                var output = new List<CheckedExpression>();

                ///

                foreach (var v in ve.Expressions) {

                    var (checkedExpr, err) = TypeCheckExpression(v, stack, file);

                    error = error ?? err;

                    if (innerType is UnknownType) {

                        innerType = checkedExpr.GetNeuType();
                    }
                    else {

                        // if (innerType != checkedExpr.GetNeuType()) {
                        if (!CompareTypes(innerType, checkedExpr.GetNeuType())) {

                            error = error ?? 
                                new TypeCheckError(
                                    "value does not match type of previous values",
                                    v.GetSpan());
                        }
                    }

                    output.Add(checkedExpr);
                }

                ///

                return (
                    new CheckedVectorExpression(
                        expressions: output,
                        new VectorType(innerType)),
                    error);
            }

            ///

            case IndexedExpression ie: {

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, stack, file);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, stack, file);
            
                error = error ?? typeCheckIdxErr;

                NeuType ty = new UnknownType();

                switch (checkedExpr.GetNeuType()) {

                    case VectorType vt: {

                        switch (vt.Type) {

                            case Int64Type _: {

                                ty = vt.Type;

                                break;
                            }

                            ///

                            default: {

                                error = error ?? 
                                    new TypeCheckError(
                                        "index is not an integer",
                                        ie.Index.GetSpan());

                                break;
                            }
                        }

                        break;
                    }

                    ///

                    case var n: {

                        error = error ?? 
                            new TypeCheckError(
                                "index used on value that can't be indexed",
                                expr.GetSpan());

                        break;
                    }
                }

                return (
                    new CheckedIndexedExpression(
                        checkedExpr,
                        checkedIdx,
                        ty),
                    error);
            }

            ///

            case OperatorExpression e: {

                return (
                    new CheckedGarbageExpression(),
                    new TypeCheckError(
                        "garbage in expression", 
                        e.Span));
            }

            case GarbageExpression e: {

                return (
                    new CheckedGarbageExpression(),
                    new TypeCheckError(
                        "garbage in expression",
                        e.Span));
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static Error? TypeCheckOperation(
        CheckedExpression lhs,
        Operator op,
        CheckedExpression rhs,
        Span span) {

        switch (op) {

            case Operator.Assign:
            case Operator.AddAssign:
            case Operator.SubtractAssign:
            case Operator.MultiplyAssign:
            case Operator.DivideAssign: {

                var lhsTy = lhs.GetNeuType();
                var rhsTy = rhs.GetNeuType();

                if (!NeuTypeFunctions.Eq(lhsTy, rhsTy)) {

                    return new TypeCheckError(
                        $"assignment between incompatible types ({lhsTy} and {rhsTy})",
                        span);
                }

                switch (lhs) {

                    case CheckedVarExpression v: {

                        if (!v.Variable.Mutable) {

                            return new TypeCheckError(
                                "assignment to immutable variable", 
                                span);
                        }

                        break;
                    }
                    
                    default: {

                        break;
                    }
                }

                break;
            }

            default: {

                break;
            }
        }

        return null;
    }

    public static (Function?, Error?) ResolveCall(
        Call call,
        Span span,
        ParsedFile file) {

        Function? callee = null;
        Error? error = null;

        // FIXME: Support function overloading

        foreach (var f in file.Functions) {

            if (f.Name == call.Name) {

                callee = f;

                break;
            }
        }

        if (callee == null) {

            error = new TypeCheckError(
                "call to unknown function",
                span);
        }

        return (callee, error);
    }

    public static (CheckedCall, Error?) TypeCheckCall(
        Call call, 
        Stack stack,
        Span span,
        ParsedFile file) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        switch (call.Name) {

            case "print": {

                // FIXME: This is a hack since print() is hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, stack, file);

                    error = error ?? checkedArgErr;

                    checkedArgs.Add((arg.Item1, checkedArg));
                }

                break;
            }

            ///

            default: {

                var (callee, resolveErr) = ResolveCall(call, span, file);

                error = error ?? resolveErr;

                if (callee != null) {

                    // Check that we have the right number of arguments

                    if (callee.Parameters.Count != call.Args.Count) {

                        error = error ?? new ParserError(
                            "wrong number of arguments", 
                            span);
                    }
                    else {

                        var idx = 0;

                        while (idx < call.Args.Count) {

                            var (checkedArg, checkedArgErr) = TypeCheckExpression(call.Args[idx].Item2, stack, file);

                            error = error ?? checkedArgErr;

                            if (!NeuTypeFunctions.Eq(checkedArg.GetNeuType(), callee.Parameters[idx].Type)) {

                                error = error ?? new TypeCheckError(
                                    "Parameter type mismatch",
                                    call.Args[idx].Item2.GetSpan());
                            }

                            checkedArgs.Add((call.Args[idx].Item1, checkedArg));

                            idx += 1;
                        }
                    }
                }

                break;
            }
        }

        // foreach (var arg in call.Args) {

        //     var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, stack, file);

        //     error = error ?? checkedArgErr;

        //     checkedArgs.Add((arg.Item1, checkedArg));
        // }

        return (
            new CheckedCall(
                call.Name, 
                checkedArgs, 
                new UnknownType()),
            error);
    }
}
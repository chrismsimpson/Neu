
namespace Neu;

public partial class NeuType {

    public NeuType() { }
}

public static partial class NeuTypeFunctions {

    public static bool Eq(
        NeuType? l,
        NeuType? r) {
        
        if (l == null && r == null) {

            return true;
        }

        if (l == null || r == null) {

            return false;
        }

        switch (true) {

            case var _ when 
                l is BoolType 
                && r is BoolType:         
                
                return true;

            ///

            case var _ when 
                l is StringType 
                && r is StringType:     
                
                return true;

            ///
            
            case var _ when 
                l is Int8Type 
                && r is Int8Type:         
                
                return true;

            ///
            
            case var _ when 
                l is Int16Type 
                && r is Int16Type:       
                
                return true;

            ///
            
            case var _ when 
                l is Int32Type 
                && r is Int32Type:       
                
                return true;

            ///
            
            case var _ when 
                l is Int64Type 
                && r is Int64Type:       
                    
                return true;

            ///
            
            case var _ when 
                l is UInt8Type 
                && r is UInt8Type:       
                
                return true;

            ///
            
            case var _ when 
                l is UInt16Type 
                && r is UInt16Type:     
                
                return true;

            ///
                
            case var _ when 
                l is UInt32Type 
                && r is UInt32Type:     
                
                return true;

            ///
            
            case var _ when
                 l is UInt64Type 
                && r is UInt64Type:     
                
                return true;

            ///
            
            case var _ when 
                l is FloatType 
                && r is FloatType:       
                
                return true;

            ///
            
            case var _ when 
                l is DoubleType 
                && r is DoubleType:     
                
                return true;

            ///
            
            case var _ when 
                l is VoidType 
                && r is VoidType:         
                
                return true;

            ///
            
            case var _ when 
                l is UnknownType 
                && r is UnknownType:   
                    
                return true;

            ///
            
            default:                                                
                
                return false;
        }
    }
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

public partial class VectorType : NeuType {

    public NeuType Type { get; init; }

    ///

    public VectorType(
        NeuType type) : base() {
        
        this.Type = type;
    }
}

public partial class TupleType : NeuType {

    public List<NeuType> Types { get; init; }

    ///

    public TupleType(
        List<NeuType> types) {

        this.Types = types;
    }
}

public partial class OptionalType : NeuType {

    public NeuType Type { get; init; }

    ///

    public OptionalType(
        NeuType type) {

        this.Type = type;
    }
}

public partial class StructType : NeuType {

    public UInt64 StructId { get; init; }

    ///

    public StructType(
        UInt64 structId) {

        this.StructId = structId;
    }
}

public partial class UnknownType : NeuType {

    public UnknownType()
        : base() { }
}

///

public partial class CheckedFile {

    public List<CheckedFunction> Functions { get; init; }

    public List<CheckedStruct> Structs { get; init; }

    ///

    public CheckedFile()
        : this(
            new List<CheckedFunction>(),
            new List<CheckedStruct>()) { }

    public CheckedFile(
        List<CheckedFunction> functions,
        List<CheckedStruct> structs) {

        this.Functions = functions;
        this.Structs = structs;
    }
}

public static partial class CheckedFileFunctions {

    public static (CheckedStruct, UInt64)? GetStruct(
        this CheckedFile checkedFile,
        String name) {
        
        for (var idx = 0; idx < checkedFile.Structs.Count; idx++) {

            var structure = checkedFile.Structs[idx];

            if (structure.Name == name) {

                return (structure, ToUInt64(idx));
            }
        }

        return null;
    }

    public static CheckedFunction? GetFunc(
        this CheckedFile checkedFile,
        String name) {

        foreach (var func in checkedFile.Functions) {

            if (func.Name == name) {

                return func;
            }
        }

        return null;
    }
}

///

public partial class CheckedStruct {

    public String Name { get; init; }

    public List<CheckedVarDecl> Members { get; set; }

    ///

    public CheckedStruct(
        String name,
        List<CheckedVarDecl> members) {

        this.Name = name;
        this.Members = members;
    }
}

///

public partial class CheckedParameter {

    public bool RequiresLabel { get; init; }

    public CheckedVariable Variable { get; init; }

    ///

    public CheckedParameter(
        bool requiresLabel,
        CheckedVariable variable) {

        this.RequiresLabel = requiresLabel;
        this.Variable = variable;
    }
}

///

public partial class CheckedFunction { 

    public String Name { get; init; }
    
    public NeuType ReturnType { get; set; }
    
    public List<CheckedParameter> Parameters { get; init; }
    
    public CheckedBlock Block { get; set; }

    public FunctionLinkage Linkage { get; init; }

    ///

    public CheckedFunction(
        String name,
        NeuType returnType,
        List<CheckedParameter> parameters,
        CheckedBlock block,
        FunctionLinkage linkage) { 

        this.Name = name;
        this.ReturnType = returnType;
        this.Parameters = parameters;
        this.Block = block;
        this.Linkage = linkage;
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

public partial class CheckedVarDecl { 

    public String Name { get; init; }

    public NeuType Type { get; init; }

    public bool Mutable { get; init; }

    public Span Span { get; init; }

    ///

    public CheckedVarDecl(
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

///

public partial class CheckedVariable { 

    public String Name { get; init; }

    public NeuType Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public CheckedVariable(
        String name,
        NeuType type,
        bool mutable) {

        this.Name = name;
        this.Type = type;
        this.Mutable = mutable;
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

        public CheckedVarDecl VarDecl { get; init; } 
        
        public CheckedExpression Expr { get; init; }

        ///

        public CheckedVarDeclStatement(
            CheckedVarDecl varDecl,
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

    public partial class CheckedUnaryOpExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public UnaryOperator Operator { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedUnaryOpExpression(
            CheckedExpression expression,
            UnaryOperator op,
            NeuType type) {

            this.Expression = expression;
            this.Operator = op;
            this.Type = type;
        }
    }

    public partial class CheckedBinaryOpExpression: CheckedExpression {

        public CheckedExpression Lhs { get; init; }

        public BinaryOperator Operator { get; init; }

        public CheckedExpression Rhs { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedBinaryOpExpression(
            CheckedExpression lhs,
            BinaryOperator op,
            CheckedExpression rhs,
            NeuType type) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Type = type;
        }
    }

    public partial class CheckedTupleExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedTupleExpression(
            List<CheckedExpression> expressions,
            NeuType type) {

            this.Expressions = expressions;
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

    public partial class CheckedIndexedTupleExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public UInt64 Index { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedIndexedTupleExpression(
            CheckedExpression expression,
            UInt64 index,
            NeuType type) {

            this.Expression = expression;
            this.Index = index;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedStructExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public String Name { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedIndexedStructExpression(
            CheckedExpression expression,
            String name,
            NeuType type) {

            this.Expression = expression;
            this.Name = name;
            this.Type = type;
        }
    }

    public partial class CheckedVarExpression: CheckedExpression {
        
        public CheckedVariable Variable { get; init; }

        ///

        public CheckedVarExpression(
            CheckedVariable variable) {

            this.Variable = variable;
        }
    }

    public partial class CheckedOptionalNoneExpression: CheckedExpression {

        public NeuType Type { get; init; }

        ///

        public CheckedOptionalNoneExpression(
            NeuType type) {

            this.Type = type;
        }
    }

    public partial class CheckedOptionalSomeExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedOptionalSomeExpression(
            CheckedExpression expression,
            NeuType type) {

            this.Expression = expression;
            this.Type = type;
        }
    }

    public partial class CheckedForceUnwrapExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedForceUnwrapExpression(
            CheckedExpression expression,
            NeuType type) {

            this.Expression = expression;
            this.Type = type;
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

            case CheckedUnaryOpExpression u: {

                return u.Type;
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

            case CheckedTupleExpression tupleExpr: {

                return tupleExpr.Type;
            }

            case CheckedIndexedExpression ie: {

                return ie.Type;
            }

            case CheckedIndexedTupleExpression ite: {

                return ite.Type;
            }

            case CheckedIndexedStructExpression ise: {

                return ise.Type;
            }

            case CheckedOptionalNoneExpression ckdOptNoneExpr: {

                return ckdOptNoneExpr.Type;
            }

            case CheckedOptionalSomeExpression ckdOptSomeExpr: {

                return ckdOptSomeExpr.Type;
            }

            case CheckedForceUnwrapExpression ckdForceUnwrapExpr: {

                return ckdForceUnwrapExpr.Type;
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
        CheckedVariable v, 
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

    public static CheckedVariable? FindVar(this Stack s, String varName) {

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

    public static ErrorOrVoid AddStruct(
        this Stack s, 
        String name,
        UInt64 structId,
        Span span) {

        if (s.Frames.LastOrDefault() is StackFrame frame) {

            foreach (var (existingStruct, existingStructId) in frame.Structs) {

                if (name == existingStruct) {

                    return new ErrorOrVoid(new TypeCheckError($"redefinition of {name}", span));
                }
            }

            frame.Structs.Add((name, structId));
        }

        return new ErrorOrVoid();
    }

    public static UInt64? FindStruct(
        this Stack s,
        String structure) {

        foreach (var frame in s.Frames) {

            foreach (var st in frame.Structs) {

                if (st.Item1 == structure) {

                    return st.Item2;
                }
            }
        }

        ///

        return null;
    }
}

///

public partial class StackFrame {
    
    public List<CheckedVariable> Vars { get; init; }

    public List<(String, UInt64)> Structs { get; init; }

    ///

    public StackFrame() 
        : this(
            new List<CheckedVariable>(),
            new List<(String, UInt64)>()) { }

    public StackFrame(
        List<CheckedVariable> vars,
        List<(String, UInt64)> structs) {

        this.Vars = vars;
        this.Structs = structs;
    }
}

///

public static partial class TypeCheckerFunctions {

    public static (CheckedFile, Error?) TypeCheckFile(
        ParsedFile file) {

        var stack = new Stack();

        return TypeCheckFileHelper(file, stack);
    }

    // public static (CheckedFile, Error?) TypeCheckFileHelper(
    //     ParsedFile file,
    //     Stack stack) {

    //     var output = new CheckedFile();

    //     Error? error = null;

    //     foreach (var fun in file.Functions) {

    //         var (checkedFun, funErr) = TypeCheckFunction(fun, stack, file);

    //         error = error ?? funErr;

    //         output.CheckedFunctions.Add(checkedFun);
    //     }

    //     return (output, error);
    // }

    public static (CheckedFile, Error?) TypeCheckFileHelper(
        ParsedFile parsedFile,
        Stack stack) {

        var file = new CheckedFile();

        Error? error = null;

        foreach (var structure in parsedFile.Structs) {

            // Ensure we know the types ahead of time, so they can be recursive
        
            TypeCheckStructPredecl(structure, stack, file);
        }

        foreach (var structure in parsedFile.Structs) {

            error = error ?? TypeCheckStruct(structure, stack, file);
        }

        foreach (var fun in parsedFile.Functions) {
            
            // Ensure we know the function ahead of time, so they can be recursive
            
            error = error ?? TypeCheckFuncPredecl(fun, stack, file);
        }

        foreach (var fun in parsedFile.Functions) {

            error = error ?? TypeCheckFunc(fun, stack, file);
        }

        return (file, error);
    }

    public static void TypeCheckStructPredecl(Struct structure, Stack stack, CheckedFile file) {

        file.Structs.Add(
            new CheckedStruct(
                name: structure.Name,
                members: new List<CheckedVarDecl>()));
    }

    public static Error? TypeCheckStruct(
        Struct structure,
        Stack stack,
        CheckedFile file) {

        Error? error = null;

        var members = new List<CheckedVarDecl>();

        foreach (var uncheckedMember in structure.Members) {

            var (checkedMemberType, checkedMemberTypeErr) = TypeCheckTypeName(uncheckedMember.Type, stack);

            error = error ?? checkedMemberTypeErr;

            members.Add(
                new CheckedVarDecl(
                    name: uncheckedMember.Name,
                    type: checkedMemberType, 
                    mutable: uncheckedMember.Mutable, 
                    span: uncheckedMember.Span));
        }

        var constructorParams = new List<CheckedParameter>();

        foreach (var member in members) {

            constructorParams.Add(
                new CheckedParameter(
                    requiresLabel: true,
                    variable: 
                        new CheckedVariable(
                            name: member.Name,
                            type: member.Type,
                            mutable: member.Mutable)));
        }

        var (_, _structId) = file
            .GetStruct(structure.Name)
            ?? throw new Exception("Internal error: we previously defined the struct but it's now missing");

        var checkedConstructor = new CheckedFunction(
            name: structure.Name,
            block: new CheckedBlock(),
            linkage: FunctionLinkage.ImplicitConstructor,
            parameters: constructorParams,
            returnType: new StructType(_structId));

        file.Functions.Add(checkedConstructor);

        var (checkedStruct, structId) = file
            .GetStruct(structure.Name) ??
            throw new Exception("Internal error: we previously defined the struct but it's now missing");

        checkedStruct.Members = members;

        switch (stack.AddStruct(structure.Name, structId, structure.Span).Error) {

            case Error e: {

                error = error ?? e;

                break;
            }

            default: {

                break;
            }
        }

        return error;
    }

    public static Error? TypeCheckFuncPredecl(
        Function func,
        Stack stack,
        CheckedFile file) {

        Error? error = null;

        var checkedFunction = new CheckedFunction(
            name: func.Name,
            returnType: new UnknownType(),
            parameters: new List<CheckedParameter>(),
            block: new CheckedBlock(),
            linkage: func.Linkage);

        foreach (var param in func.Parameters) {

            var (paramType, typeCheckNameErr) = TypeCheckTypeName(param.Variable.Type, stack);

            error = error ?? typeCheckNameErr;

            var checkedVariable = new CheckedVariable(
                name: param.Variable.Name,
                type: paramType,
                mutable: param.Variable.Mutable);

            checkedFunction.Parameters.Add(
                new CheckedParameter(
                    requiresLabel: param.RequiresLabel, 
                    variable: checkedVariable));
        }

        file.Functions.Add(checkedFunction);

        return error;
    }

    // public static (CheckedFunction, Error?) TypeCheckFunction(
    //     Function fun,
    //     Stack stack,
    //     ParsedFile file) {

    //     Error? error = null;

    //     stack.PushFrame();

    //     foreach (var p in fun.Parameters) {
            
    //         if (stack.AddVar(p.Variable, fun.NameSpan) is Error e) {

    //             error = error ?? e;
    //         }
    //     }

    //     var (block, blockErr) = TypeCheckBlock(fun.Block, stack, file);

    //     error = error ?? blockErr;

    //     stack.PopFrame();

    //     NeuType? returnType = null;

    //     switch (fun.ReturnType) {

    //         case UnknownType _: {

    //             switch (block.Stmts.FirstOrDefault()) {

    //                 case CheckedReturnStatement rs: {

    //                     returnType = rs.Expr.GetNeuType();

    //                     break;
    //                 }

    //                 default: {

    //                     returnType = new UnknownType();

    //                     break;
    //                 }
    //             }

    //             break;
    //         }

    //         default: {
                
    //             returnType = fun.ReturnType;

    //             break;
    //         }
    //     }

    //     var output = new CheckedFunction(
    //         name: fun.Name,
    //         returnType: returnType,
    //         parameters: fun.Parameters,
    //         block,
    //         linkage: fun.Linkage);

    //     return (output, error);
    // }

    public static Error? TypeCheckFunc(
        Function func,
        Stack stack,
        CheckedFile file) {

        Error? error = null;

        stack.PushFrame();

        var checkedFunction = file
            .GetFunc(func.Name)
            ?? throw new Exception("Internal error: we just pushed the checked function, but it's not present");

        foreach (var param in checkedFunction.Parameters) {

            if (stack.AddVar(param.Variable, func.NameSpan) is Error e) {

                error = error ?? e;
            }
        }

        var (block, typeCheckBlockErr) = TypeCheckBlock(func.Block, stack, file);

        error = error ?? typeCheckBlockErr;

        stack.PopFrame();

        var (funcReturnType, typeCheckReturnTypeErr) = TypeCheckTypeName(func.ReturnType, stack);

        error = error ?? typeCheckReturnTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        NeuType? returnType = null;

        switch (funcReturnType) {

            case UnknownType _: {

                switch (block.Stmts.FirstOrDefault()) {

                    case CheckedReturnStatement rs: {

                        returnType = rs.Expr.GetNeuType();

                        break;
                    }

                    default: {

                        returnType = new VoidType();

                        break;
                    }
                } 

                break;
            }

            default: {

                returnType = funcReturnType;

                break;
            }
        }

        checkedFunction = file.GetFunc(func.Name)
            ?? throw new Exception("Internal error: we just pushed the checked function, but it's not present");

        checkedFunction.Block = block;

        checkedFunction.ReturnType = returnType;

        return error;
    }

    public static (CheckedBlock, Error?) TypeCheckBlock(
        Block block,
        Stack stack,
        CheckedFile file) {

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
        CheckedFile file) {

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

                var (checkedType, chkTypeErr) = TypeCheckTypeName(vds.Decl.Type, stack);

                if (checkedType is UnknownType && checkedExpr.GetNeuType() is not UnknownType) {

                    checkedType = checkedExpr.GetNeuType();
                }
                else {

                    error = error ?? chkTypeErr;
                }

                var checkedVarDecl = new CheckedVarDecl(
                    name: vds.Decl.Name,
                    type: checkedType,
                    span: vds.Decl.Span,
                    mutable: vds.Decl.Mutable);

                // Taking this out for now until we have better number type support
                // else if (!NeuTypeFunctions.Eq(vds.Decl.Type, checkedExpr.GetNeuType())) {
                //     error = error ?? new TypeCheckError(
                //         "mismatch between declaration and initializer",
                //         vds.Expr.GetSpan());
                // }

                if (stack.AddVar(
                    new CheckedVariable(
                        name: checkedVarDecl.Name, 
                        type: checkedVarDecl.Type, 
                        mutable: checkedVarDecl.Mutable),
                    checkedVarDecl.Span) is Error e) {

                    error = error ?? e;
                }

                return (
                    new CheckedVarDeclStatement(checkedVarDecl, checkedExpr),
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
        CheckedFile file) {

        Error? error = null;

        switch (expr) {

            case BinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, stack, file);

                error = error ?? checkedLhsErr;

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, stack, file);

                error = error ?? checkedRhsErr;

                error = error ?? TypeCheckBinaryOperation(
                    checkedLhs,
                    e.Operator,
                    checkedRhs,
                    e.Span);
                
                // TODO: actually do the binary operator typecheck against safe operations
                // For now, use a type we know

                var ty = checkedLhs.GetNeuType();

                return (
                    new CheckedBinaryOpExpression(
                        checkedLhs, 
                        e.Operator, 
                        checkedRhs, 
                        ty),
                    error);
            }

            case UnaryOpExpression u: {

                var (checkedExpr, checkedExprErr) = TypeCheckExpression(u.Expression, stack, file);

                error = error ?? checkedExprErr;

                error = error ?? TypeCheckUnaryOperation(checkedExpr, u.Operator, u.Span);

                var ty = checkedExpr.GetNeuType();

                return (
                    new CheckedUnaryOpExpression(checkedExpr, u.Operator, ty),
                    error);
            }

            case OptionalNoneExpression e: {

                return (
                    new CheckedOptionalNoneExpression(
                        new UnknownType()),
                    error);
            }

            case OptionalSomeExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, stack, file);

                error = error ?? ckdExprError;

                var type = ckdExpr.GetNeuType();

                return (
                    new CheckedOptionalSomeExpression(ckdExpr, type),
                    error);
            }

            case ForcedUnwrapExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, stack, file);

                NeuType type = new UnknownType();

                switch (ckdExpr.GetNeuType()) {

                    case OptionalType opt: {

                        type = opt.Type;

                        break;
                    }

                    ///

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "Forced unwrap only works on Optional",
                                e.Expression.GetSpan());

                        break;
                    }
                }

                return (
                    new CheckedForceUnwrapExpression(ckdExpr, type),
                    error);
            }

            case BooleanExpression e: {

                return (
                    new CheckedBooleanExpression(e.Value),
                    null);
            }

            case CallExpression e: {

                var (checkedCall, checkedCallErr) = TypeCheckCall(e.Call, stack, e.Span, file);

                var ty = checkedCall.Type;

                return (
                    new CheckedCallExpression(checkedCall, ty),
                    error ?? checkedCallErr);
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

                if (stack.FindVar(e.Value) is CheckedVariable v) {

                    return (
                        new CheckedVarExpression(v),
                        null);
                }
                else {
                    
                    return (
                        new CheckedVarExpression(
                            new CheckedVariable(
                                e.Value, 
                                type: new UnknownType(), 
                                mutable: false)
                        ),
                        new TypeCheckError(
                            "variable not found",
                            e.Span));
                }
            }

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
                                    "does not match type of previous values in vector",
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

            case TupleExpression te: {

                var checkedItems = new List<CheckedExpression>();

                var checkedTypes = new List<NeuType>();

                foreach (var item in te.Expressions) {

                    var (checkedItemExpr, typeCheckItemExprErr) = TypeCheckExpression(item, stack, file);

                    error = error ?? typeCheckItemExprErr;

                    checkedTypes.Add(checkedItemExpr.GetNeuType());

                    checkedItems.Add(checkedItemExpr);
                }

                return (
                    new CheckedTupleExpression(
                        checkedItems, 
                        new TupleType(checkedTypes)),
                    error);
            }

            case IndexedExpression ie: {

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, stack, file);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, stack, file);
            
                error = error ?? typeCheckIdxErr;

                NeuType ty = new UnknownType();

                switch (checkedExpr.GetNeuType()) {

                    case VectorType vt: {

                        switch (checkedIdx.GetNeuType()) {

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

            case IndexedTupleExpression ite: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ite.Expression, stack, file);

                error = error ?? chkExprErr;

                NeuType ty = new UnknownType();

                switch (checkedExpr.GetNeuType()) {

                    case TupleType tt: {

                        switch (tt.Types[ToInt32(ite.Index)]) {

                            case NeuType t: {

                                ty = t;

                                break;
                            }

                            default: {

                                error = error ?? 
                                    new TypeCheckError(
                                        "tuple index past the end of the tuple",
                                        ite.Span);

                                break;
                            }
                        }

                        break;
                    }

                    default: {

                        error = error ?? 
                            new TypeCheckError(
                                "tuple index used non-tuple value",
                                ite.Expression.GetSpan());

                        break;
                    }
                }

                return (
                    new CheckedIndexedTupleExpression(checkedExpr, ite.Index, ty),
                    error);
            }

            case IndexedStructExpression ise: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ise.Expression, stack, file);

                error = error ?? chkExprErr;

                NeuType ty = new UnknownType();

                // FIXME: add real name binding to structs so that we can find the proper field

                return (
                    new CheckedIndexedStructExpression(checkedExpr, ise.Name, ty),
                    error);
            }

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

    public static Error? TypeCheckUnaryOperation(
        CheckedExpression expr,
        UnaryOperator op,
        Span span) {

        switch (expr.GetNeuType()) {

            case Int8Type:
            case Int16Type:
            case Int32Type:
            case Int64Type:
            case UInt8Type:
            case UInt16Type:
            case UInt32Type:
            case UInt64Type:
            case FloatType:
            case DoubleType: {

                switch (expr) {

                    case CheckedVarExpression v: {

                        if (!v.Variable.Mutable) {

                            switch (op) {

                                case UnaryOperator.PreIncrement:
                                case UnaryOperator.PostIncrement: {

                                    return new TypeCheckError(
                                        "increment on immutable variable",
                                        span);
                                }

                                case UnaryOperator.PreDecrement:
                                case UnaryOperator.PostDecrement: {

                                    return new TypeCheckError(
                                        "decrement on immutable variable",
                                        span);
                                }

                                default: {

                                    throw new Exception(); // assume not reached
                                }
                            }
                        }
                        else {

                            return null;
                        }
                    }

                    default: {

                        // TODO: we probably want to check if what we're working on can be updated

                        return null;
                    }
                }
            }

            default: {

                return new TypeCheckError(
                    "unary operation on non-numeric value", 
                    span);
            }
        }
    }

    public static Error? TypeCheckBinaryOperation(
        CheckedExpression lhs,
        BinaryOperator op,
        CheckedExpression rhs,
        Span span) {

        switch (op) {

            case BinaryOperator.Assign:
            case BinaryOperator.AddAssign:
            case BinaryOperator.SubtractAssign:
            case BinaryOperator.MultiplyAssign:
            case BinaryOperator.DivideAssign: {

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

    public static (CheckedFunction?, Error?) ResolveCall(
        Call call,
        Span span,
        CheckedFile file) {

        CheckedFunction? callee = null;
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
        CheckedFile file) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        NeuType returnType = new UnknownType();

        switch (call.Name) {

            case "print": {

                // FIXME: This is a hack since print() is hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, stack, file);

                    error = error ?? checkedArgErr;

                    returnType = new VoidType();

                    checkedArgs.Add((arg.Item1, checkedArg));
                }

                break;
            }

            ///

            default: {

                var (callee, resolveErr) = ResolveCall(call, span, file);

                error = error ?? resolveErr;

                if (callee != null) {

                    returnType = callee.ReturnType;

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

                            if (callee.Parameters[idx].RequiresLabel
                                && call.Args[idx].Item1 != callee.Parameters[idx].Variable.Name) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Wrong parameter name in argument label",
                                        call.Args[idx].Item2.GetSpan());
                            }

                            if (!NeuTypeFunctions.Eq(checkedArg.GetNeuType(), callee.Parameters[idx].Variable.Type)) {

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

        return (
            new CheckedCall(
                call.Name, 
                checkedArgs, 
                returnType),
            error);
    }

    public static (NeuType, Error?) TypeCheckTypeName(
        UncheckedType uncheckedType,
        Stack stack) {

        NeuType? ty = null;

        Error? error = null;

        switch (uncheckedType.Name) {

            case "Int8":

                ty = new Int8Type();

                break;

            case "Int16":

                ty = new Int16Type();

                break;

            case "Int32":

                ty = new Int32Type();

                break;

            case "Int64":

                ty = new Int64Type();

                break;

            case "UInt8":

                ty = new UInt8Type();

                break;

            case "UInt16":

                ty = new UInt16Type();

                break;

            case "UInt32":

                ty = new UInt32Type();

                break;

            case "UInt64":

                ty = new UInt64Type();

                break;

            case "Float":

                ty = new FloatType();

                break;

            case "Double":

                ty = new DoubleType();

                break;

            case "String":  

                ty = new StringType();

                break;

            case "Bool":

                ty = new BoolType();

                break;

            case "Void":

                ty = new VoidType();

                break;

            case var x when IsNullOrWhiteSpace(x):

                ty = new UnknownType();

                break;

            case var x: {

                var structure = stack.FindStruct(x);

                switch (structure) {

                    case UInt64 structId: {

                        ty = new StructType(structId);

                        break;
                    }

                    default: {

                        ty = new UnknownType();

                        error = new TypeCheckError(
                            "unknown type",
                            uncheckedType.Span);

                        break;
                    }
                }
            }

            break;
        }

        if (uncheckedType.Optional) {

            return (
                new OptionalType(ty),
                error);
        }
        else {

            return (
                ty, 
                error);
        }
    }
}
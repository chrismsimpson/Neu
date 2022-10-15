
namespace Neu;

public enum SafetyMode {

    Safe,
    Unsafe
}

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

            case var _ when l is TypeVariable lt && r is TypeVariable rt:           return lt.Name == rt.Name;

            case var _ when l is GenericType lg && r is GenericType rg: {

                if (lg.ParentStructId != rg.ParentStructId) {

                    return false;
                }

                if (lg.InnerTypeIds.Count != rg.InnerTypeIds.Count) {

                    return false;
                }

                for (var i = 0; i < lg.InnerTypeIds.Count; i++) {

                    if (lg.InnerTypeIds[i] != rg.InnerTypeIds[i]) {

                        return false;
                    }
                }

                return true;
            }

            case var _ when l is GenericInstance lgi && r is GenericInstance rgi: {

                if (lgi.StructId != rgi.StructId) {

                    return false;
                }

                if (lgi.TypeIds.Count != rgi.TypeIds.Count) {

                    return false;
                }

                for (var i = 0; i < lgi.TypeIds.Count; i++) {

                    if (lgi.TypeIds[i] != rgi.TypeIds[i]) {

                        return false;
                    }
                }

                return true;
            }

            case var _ when l is GenericEnumInstance le && r is GenericEnumInstance re: {

                if (le.EnumId != re.EnumId) {

                    return false;
                }

                if (le.TypeIds.Count != re.TypeIds.Count) {

                    return false;
                }

                for (var i = 0; i < le.TypeIds.Count; i++) {

                    if (le.TypeIds[i] != re.TypeIds[i]) {

                        return false;
                    }
                }

                return true;
            }

            case var _ when l is StructType ls && r is StructType rs:               return ls.StructId == rs.StructId;
            
            case var _ when l is RawPointerType lp && r is RawPointerType rp:       return lp.TypeId == rp.TypeId;

            default:                                                                return false;
        }
    }
}

public partial class Builtin: NeuType {

    public Builtin() { }
}

public partial class TypeVariable: NeuType {

    public String Name { get; init; }

    ///

    public TypeVariable(
        String name) {

        this.Name = name;
    }
}

public partial class GenericType: NeuType {

    public Int32 ParentStructId { get; init; }

    public List<Int32> InnerTypeIds { get; init; }

    ///

    public GenericType(
        Int32 parentStructId,
        List<Int32> innerTypeIds) {

        this.ParentStructId = parentStructId;
        this.InnerTypeIds = innerTypeIds;
    }
}

// A GenericInstance is a generic that has known values for its type param
// For example Foo<Bar> is an instance of the generic Foo<T>

public partial class GenericInstance: NeuType {

    public Int32 StructId { get; init; }

    public List<Int32> TypeIds { get; init; }

    ///

    public GenericInstance(
        Int32 structId,
        List<Int32> typeIds) { 

        this.StructId = structId;
        this.TypeIds = typeIds;
    }
}

public partial class GenericEnumInstance: NeuType {

    public Int32 EnumId { get; init; }

    public List<Int32> TypeIds { get; init; }

    ///

    public GenericEnumInstance(
        Int32 enumId,
        List<Int32> typeIds) {

        this.EnumId = enumId;
        this.TypeIds = typeIds;
    }
}

public partial class StructType : NeuType {

    public Int32 StructId { get; init; }

    ///

    public StructType(
        Int32 structId) {

        this.StructId = structId;
    }
}

public partial class EnumType : NeuType {

    public Int32 EnumId { get; init; }

    ///

    public EnumType(
        Int32 enumId) {

        this.EnumId = enumId;
    }
}

public partial class RawPointerType: NeuType {

    public Int32 TypeId { get; init; }

    ///

    public RawPointerType(
        Int32 typeId) {

        this.TypeId = typeId;
    }
}

public static partial class NeuTypeFunctions {

    public static bool IsInteger(
        Int32 typeId) {

        // switch (typeId) {

        //     case Compiler.CCharTypeId:
        //     case Compiler.CIntTypeId:
        //     case Compiler.Int8TypeId:
        //     case Compiler.Int16TypeId:
        //     case Compiler.Int32TypeId:
        //     case Compiler.Int64TypeId:
        //     case Compiler.IntTypeId:
        //     case Compiler.UInt8TypeId:
        //     case Compiler.UInt16TypeId:
        //     case Compiler.UInt32TypeId:
        //     case Compiler.UInt64TypeId:
        //     case Compiler.UIntTypeId:
        //         return true;
            
        //     default: 
        //         return false;
        // }

        return (new [] { Compiler.CCharTypeId, Compiler.CIntTypeId, Compiler.Int8TypeId, Compiler.Int16TypeId, Compiler.Int32TypeId, Compiler.Int64TypeId, Compiler.IntTypeId, Compiler.UInt8TypeId, Compiler.UInt16TypeId, Compiler.UInt32TypeId, Compiler.UInt64TypeId, Compiler.UIntTypeId }).Contains(typeId);
    }

    public static bool IsSigned(
        Int32 typeId) {

        switch (typeId) {

            case Compiler.CCharTypeId:

                // We're gonna assume false here because we don't
                // have direct access to C's char type and C#'s
                // byte type is unsigned

                return false;
            
            case Compiler.UIntTypeId:
            case Compiler.UInt8TypeId:
            case Compiler.UInt16TypeId:
            case Compiler.UInt32TypeId:
            case Compiler.UInt64TypeId:
                return false;

            default:
                return true;
        }
    }

    public static UInt32 GetBits(
        Int32 typeId) {

        switch (typeId) {

            case Compiler.BoolTypeId:       return 8;
            case Compiler.Int8TypeId:       return 8;
            case Compiler.Int16TypeId:      return 16;
            case Compiler.Int32TypeId:      return 32;
            case Compiler.Int64TypeId:      return 64;
            case Compiler.UInt8TypeId:      return 8;
            case Compiler.UInt16TypeId:     return 16;
            case Compiler.UInt32TypeId:     return 32;
            case Compiler.UInt64TypeId:     return 64;
            
            case Compiler.FloatTypeId:      return 32;
            case Compiler.DoubleTypeId:     return 64;
            
            case Compiler.CCharTypeId:      return 8;
            case Compiler.CIntTypeId:       return 64;

            case Compiler.IntTypeId:        return 64;
            case Compiler.UIntTypeId:       return 64;

            default: throw new Exception($"GetBits not supported for type {typeId}");
        }
    }

    public static bool CanFitInteger(
        Int32 typeId,
        IntegerConstant value) {

        switch (value) {

            case SignedIntegerConstant si: {

                switch (typeId) {

                    case Compiler.CCharTypeId: return si.Value >= byte.MinValue && si.Value <= byte.MaxValue;
                    case Compiler.CIntTypeId: return si.Value >= int.MinValue && si.Value <= int.MaxValue;

                    case Compiler.Int8TypeId: return si.Value >= sbyte.MinValue && si.Value <= sbyte.MaxValue;
                    case Compiler.Int16TypeId: return si.Value >= short.MinValue && si.Value <= short.MaxValue;
                    case Compiler.Int32TypeId: return si.Value >= int.MinValue && si.Value <= int.MaxValue;
                    case Compiler.Int64TypeId: return true;
                    // FIXME: Don't assume that UInt is 64-bit
                    case Compiler.IntTypeId: return true;
                    case Compiler.UInt8TypeId: return si.Value >= 0 && si.Value <= byte.MaxValue;
                    case Compiler.UInt16TypeId: return si.Value >= 0 && si.Value <= ushort.MaxValue;
                    case Compiler.UInt32TypeId: return si.Value >= 0 && si.Value <= uint.MaxValue;
                    case Compiler.UInt64TypeId: return si.Value >= 0;
                    // FIXME: Don't assume that UInt is 64-bit
                    case Compiler.UIntTypeId: return si.Value >= 0;

                    default: return false;
                }
            }

            case UnsignedIntegerConstant ui: {

                switch (typeId) {

                    case Compiler.Int8TypeId: return ui.Value <= ToUInt64(sbyte.MaxValue);
                    case Compiler.Int16TypeId: return ui.Value <= ToUInt64(short.MaxValue);
                    case Compiler.Int32TypeId: return ui.Value <= ToUInt64(int.MaxValue);
                    case Compiler.Int64TypeId: return ui.Value <= ToUInt64(long.MaxValue);
                    // FIXME: Don't assume that usize is 64-bit
                    case Compiler.IntTypeId: return ui.Value <= ToUInt64(long.MaxValue);
                    case Compiler.UInt8TypeId: return ui.Value <= ToUInt64(byte.MaxValue);
                    case Compiler.UInt16TypeId: return ui.Value <= ToUInt64(ushort.MaxValue);
                    case Compiler.UInt32TypeId: return ui.Value <= ToUInt64(uint.MaxValue);
                    // FIXME: Don't assume that usize is 64-bit
                    case Compiler.UIntTypeId: return true;
                    case Compiler.UInt64TypeId: return true;
                    default: return false;
                }
            }

            default: {

                throw new Exception();
            }
        }
    }
}

///

public partial class Project {

    public List<CheckedFunction> Functions { get; init; }

    public List<CheckedStruct> Structs { get; init; }

    public List<CheckedEnum> Enums { get; init; }
    
    public List<Scope> Scopes { get; init; }

    public List<NeuType> Types { get; init; }

    public Int32? CurrentFunctionIndex { get; set; } 

    ///

    public Project() {

        // Top-level (project-global) scope has no parent scope
        // and is the parent scope of all file scopes
        // var projectGlobalScope = new Scope()

        var projectGlobalScope = new Scope(null);

        this.Functions = new List<CheckedFunction>();
        this.Structs = new List<CheckedStruct>();
        this.Enums = new List<CheckedEnum>();
        this.Scopes = new List<Scope>(new [] { projectGlobalScope });
        this.Types = new List<NeuType>();
        this.CurrentFunctionIndex = null;
    }
}

public static partial class ProjectFunctions {

    public static Int32 FindOrAddTypeId(
        this Project project,
        NeuType ty) {

        for (var idx = 0; idx < project.Types.Count; idx++) {

            var t = project.Types[idx];

            if (NeuTypeFunctions.Eq(t, ty)) {

                return idx;
            }
        }

        // in the future, we may want to group related types (like instantiations of the same generic)

        project.Types.Add(ty);

        return project.Types.Count - 1;
    }

    public static Int32 CreateScope(
        this Project project,
        Int32 parentId) {

        project.Scopes.Add(new Scope(parentId));

        return project.Scopes.Count - 1;
    }

    public static ErrorOrVoid AddVarToScope(
        this Project project,
        Int32 scopeId,
        CheckedVariable var,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var existingVar in scope.Vars) {

            if (var.Name == existingVar.Name) {

                return new ErrorOrVoid(
                    new TypeCheckError(
                        $"redefinition of variable {var.Name}",
                        span));
            }
        }

        scope.Vars.Add(var);

        return new ErrorOrVoid();
    }

    public static CheckedVariable? FindVarInScope(
        this Project project,
        Int32 scopeId,
        String var) {

        Int32? currentId = scopeId;

        while (currentId != null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var v in scope.Vars) {

                if (v.Name == var) {

                    return v;
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }

    public static ErrorOrVoid AddStructToScope(
        this Project project,
        Int32 scopeId,
        String name,
        Int32 structId,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var (existingStruct, _) in scope.Structs) {

            if (name == existingStruct) {

                return new ErrorOrVoid(
                    new TypeCheckError(
                        $"redefinition of struct/class {name}",
                        span));
            }
        }

        scope.Structs.Add((name, structId));

        return new ErrorOrVoid();
    }


    public static ErrorOrVoid AddEnumToScope(
        this Project project,
        Int32 scopeId,
        String name,
        Int32 enumId,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var (existingEnum, _) in scope.Enums) {

            if (name == existingEnum) {

                return new ErrorOrVoid(
                    new TypeCheckError(
                        $"redefinition of enum {name}",
                        span));
            }
        }

        scope.Enums.Add((name, enumId));

        return new ErrorOrVoid();
    }

    public static Int32? FindStructInScope(
        this Project project,
        Int32 scopeId,
        String structure) {

        Int32? currentId = scopeId;

        while (currentId != null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var s in scope.Structs) {

                if (s.Item1 == structure) {

                    return s.Item2;
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }

    public static Int32? FindEnumInScope(
        this Project project,
        Int32 scopeId,
        String enumName) {

        Int32? currentId = scopeId;

        while (currentId != null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var e in scope.Enums) {

                if (e.Item1 == enumName) {

                    return e.Item2;
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }

    public static ErrorOrVoid AddFuncToScope(
        this Project project,
        Int32 scopeId,
        String name,
        Int32 funcId,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var (existingFunc, _) in scope.Funcs) {

            if (name == existingFunc) {

                return new ErrorOrVoid(
                    new TypeCheckError(
                        $"redefinition of function {name}",
                        span));
            }
        }

        scope.Funcs.Add((name, funcId));

        return new ErrorOrVoid();
    }

    public static Int32? FindFuncInScope(
        this Project project,
        Int32 scopeId,
        String funcName) {

        Int32? currentId = scopeId;

        while (currentId != null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var s in scope.Funcs) {

                if (s.Item1 == funcName) {

                    return s.Item2;
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }

    public static ErrorOrVoid AddTypeToScope(
        this Project project,
        Int32 scopeId,
        String typeName,
        Int32 typeId,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var (existingType, _) in scope.Types) {

            if (typeName == existingType) {

                return new ErrorOrVoid(
                    new TypeCheckError(
                        $"redefinition of type {typeName}",
                        span));
            }
        }

        scope.Types.Add((typeName, typeId));

        return new ErrorOrVoid();
    }

    public static Int32? FindTypeInScope(
        this Project project,
        Int32 scopeId,
        String typeName) {

        Int32? currentId = scopeId;

        while (currentId != null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var s in scope.Types) {

                if (s.Item1 == typeName) {
                    
                        return s.Item2;
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }
}

///

public partial class CheckedStruct {

    public String Name { get; init; }

    public List<Int32> GenericParameters { get; init; }

    public List<CheckedVarDecl> Fields { get; set; }

    public Int32 ScopeId { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; }

    ///

    public CheckedStruct(
        String name,
        List<Int32> genericParameters,
        List<CheckedVarDecl> fields,
        Int32 scopeId,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Fields = fields;
        this.ScopeId = scopeId;
        this.DefinitionLinkage = definitionLinkage;
        this.DefinitionType = definitionType;
    }
}

///

public partial class CheckedEnum {

    public String Name { get; init; }

    public List<Int32> GenericParameters { get; init; }

    public List<CheckedEnumVariant> Variants { get; set; }

    public Int32 ScopeId { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public Int32? UnderlyingType { get; init; }

    public Span Span { get; init; }

    ///

    public CheckedEnum(
        String name,
        List<Int32> genericParameters,
        List<CheckedEnumVariant> variants,
        Int32 scopeId,
        DefinitionLinkage definitionLinkage,
        Int32? underlyingType,
        Span span) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Variants = variants;
        this.ScopeId = scopeId;
        this.DefinitionLinkage = definitionLinkage;
        this.UnderlyingType = underlyingType;
        this.Span = span;
    }
}

public partial class CheckedEnumVariant {

    public CheckedEnumVariant() { }
}

    public partial class CheckedUntypedEnumVariant: CheckedEnumVariant {

        public String Name { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedUntypedEnumVariant(
            String name,
            Span span) {

            this.Name = name;
            this.Span = span;
        }
    }

    public partial class CheckedTypedEnumVariant: CheckedEnumVariant { 
        
        public String Name { get; init; }
        
        public Int32 TypeId { get; init; }

        public Span Span { get; init; }

        ///
        
        public CheckedTypedEnumVariant(
            String name,
            Int32 typeId,
            Span span) {

            this.Name = name;
            this.TypeId = typeId;
            this.Span = span;
        }
    }
    
    public partial class CheckedWithValueEnumVariant: CheckedEnumVariant {
        
        public String Name { get; init; }
        
        public CheckedExpression Expression { get; init; }
        
        public Span Span { get; init; }

        ///

        public CheckedWithValueEnumVariant(
            String name,
            CheckedExpression expression,
            Span span) {

            this.Name = name;
            this.Expression = expression;
            this.Span = span;
        }
    }
    
    public partial class CheckedStructLikeEnumVariant: CheckedEnumVariant {
        
        public String Name { get; init; }
        
        public List<CheckedVarDecl> Decls { get; init; }
        
        public Span Span { get; init; }

        ///

        public CheckedStructLikeEnumVariant(
            String name,
            List<CheckedVarDecl> decls,
            Span span) {

            this.Name = name;
            this.Decls = decls;
            this.Span = span;
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

public partial class FunctionGenericParameter {

    public FunctionGenericParameter() { }
}

    public partial class InferenceGuideFunctionGenericParameter: FunctionGenericParameter {

        public Int32 TypeId { get; init; }

        ///

        public InferenceGuideFunctionGenericParameter(
            Int32 typeId) {

            this.TypeId = typeId;
        }
    }

    public partial class ParameterFunctionGenericParameter: FunctionGenericParameter {

        public Int32 TypeId { get; init; }

        ///

        public ParameterFunctionGenericParameter(
            Int32 typeId) {

            this.TypeId = typeId;
        }
    }

///

public partial class CheckedFunction { 

    public String Name { get; init; }

    public bool Throws { get; init; }
    
    public Int32 ReturnType { get; set; }
    
    public List<CheckedParameter> Parameters { get; init; }

    public List<FunctionGenericParameter> GenericParameters { get; set; }

    public Int32 FuncScopeId { get; init; }
    
    public CheckedBlock Block { get; set; }

    public FunctionLinkage Linkage { get; init; }

    ///

    public CheckedFunction(
        String name,
        bool throws,
        Int32 returnType,
        List<CheckedParameter> parameters,
        List<FunctionGenericParameter> genericParameters,
        Int32 funcScopeId,
        CheckedBlock block,
        FunctionLinkage linkage) { 

        this.Name = name;
        this.Throws = throws;
        this.ReturnType = returnType;
        this.Parameters = parameters;
        this.GenericParameters = genericParameters;
        this.FuncScopeId = funcScopeId;
        this.Block = block;
        this.Linkage = linkage;
    }
}

public static partial class CheckedFunctionFunctions {

    public static bool IsStatic(
        this CheckedFunction func) {

        foreach (var p in func.Parameters) {

            if (p.Variable.Name == "this") {

                return false;
            }
        }

        return true;
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

    public Int32 Type { get; init; }

    public bool Mutable { get; init; }

    public Span Span { get; init; }

    ///

    public CheckedVarDecl(
        String name,
        Int32 type,
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

    public Int32 Type { get; init; }

    public bool Mutable { get; init; }

    ///

    public CheckedVariable(
        String name,
        Int32 type,
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

    public partial class CheckedExpressionStatement: CheckedStatement {

        public CheckedExpression Expression { get; init; }

        ///

        public CheckedExpressionStatement(
            CheckedExpression expression) {

            this.Expression = expression;
        }
    }

    public partial class CheckedDeferStatement: CheckedStatement {

        public CheckedStatement Statement { get; init; }

        ///

        public CheckedDeferStatement(
            CheckedStatement statement) {

            this.Statement = statement;
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

    public partial class CheckedLoopStatement: CheckedStatement {

        public CheckedBlock Block { get; init; }

        ///

        public CheckedLoopStatement(
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

    public partial class CheckedForStatement: CheckedStatement {

        public String IteratorName { get; init; }

        public CheckedExpression Range { get; init; }

        public CheckedBlock Block { get; init; }

        ///

        public CheckedForStatement(
            String iteratorName,
            CheckedExpression range,
            CheckedBlock block) {

            this.IteratorName = iteratorName;
            this.Range = range;
            this.Block = block;
        }
    }

    public partial class CheckedBreakStatement: CheckedStatement {

        public CheckedBreakStatement() { }
    }

    public partial class CheckedContinueStatement: CheckedStatement {

        public CheckedContinueStatement() { }
    }

    public partial class CheckedThrowStatement: CheckedStatement {

        public CheckedExpression Expression { get; init; }

        ///

        public CheckedThrowStatement(
            CheckedExpression expression) {

            this.Expression = expression;
        }
    }

    public partial class CheckedTryStatement: CheckedStatement {

        public CheckedStatement Statement { get; init; }

        public String Name { get; init; }

        public CheckedBlock Block { get; init; }
        
        ///

        public CheckedTryStatement(
            CheckedStatement statement,
            String name,
            CheckedBlock block) {

            this.Statement = statement;
            this.Name = name;
            this.Block = block;
        }
    }

    public partial class CheckedInlineCppStatement: CheckedStatement {

        public List<String> Lines { get; init; }

        ///

        public CheckedInlineCppStatement(
            List<String> lines) {

            this.Lines = lines;
        }
    }

    public partial class CheckedGarbageStatement: CheckedStatement {

        public CheckedGarbageStatement() { }
    }

///

public partial class IntegerConstant {

    public IntegerConstant() { }
}

    public partial class SignedIntegerConstant: IntegerConstant {

        public Int64 Value { get; init; }

        ///

        public SignedIntegerConstant(
            Int64 value) {

            this.Value = value;
        }
    }

    public partial class UnsignedIntegerConstant: IntegerConstant {

        public UInt64 Value { get; init; }

        ///

        public UnsignedIntegerConstant(
            UInt64 value) {

            this.Value = value;
        }
    }

public static partial class IntegerConstantFunctions {

    public static Int64 ToInt64(
        this IntegerConstant i) {

        switch (i) {

            case SignedIntegerConstant s: {

                return s.Value;
            }

            case UnsignedIntegerConstant u: {

                return System.Convert.ToInt64(u.Value);
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (NumericConstant?, Int32) Promote(
        this IntegerConstant i,
        Int32 typeId) {

        if (!NeuTypeFunctions.CanFitInteger(typeId, i)) {

            return (null, Compiler.UnknownTypeId);
        }

        var bits = NeuTypeFunctions.GetBits(typeId);

        var signed = NeuTypeFunctions.IsSigned(typeId);



        // NumericConstant newConstant = i switch {
        //     SignedIntegerConstant si => typeId switch {      
        //         Compiler.Int8TypeId => new Int8Constant(ToSByte(si.Value)),
        //         Compiler.Int16TypeId => new Int16Constant(ToInt16(si.Value)),
        //         Compiler.Int32TypeId => new Int32Constant(ToInt32(si.Value)),
        //         Compiler.Int64TypeId => new Int64Constant(si.Value),
        //         Compiler.IntTypeId => new IntConstant(si.Value),
        //         Compiler.UInt8TypeId => new UInt8Constant(ToByte(si.Value)),
        //         Compiler.UInt16TypeId => new UInt16Constant(ToUInt16(si.Value)),
        //         Compiler.UInt32TypeId => new UInt32Constant(ToUInt32(si.Value)),
        //         Compiler.UInt64TypeId => new UInt64Constant(ToUInt64(si.Value)),
        //         Compiler.UIntTypeId => new UIntConstant(ToUInt64(si.Value)),
        //         _ => throw new Exception("Bogus state in IntegerConstant.promote")
        //     },
        //     UnsignedIntegerConstant ui => typeId switch {
        //         Compiler.Int8TypeId => new Int8Constant(ToSByte(ui.Value)),
        //         Compiler.Int16TypeId => new Int16Constant(ToInt16(ui.Value)),
        //         Compiler.Int32TypeId => new Int32Constant(ToInt32(ui.Value)),
        //         Compiler.Int64TypeId => new Int64Constant(System.Convert.ToInt64(ui.Value)),
        //         Compiler.IntTypeId => new IntConstant(System.Convert.ToInt64(ui.Value)),
        //         Compiler.UInt8TypeId => new UInt8Constant(ToByte(ui.Value)),
        //         Compiler.UInt16TypeId => new UInt16Constant(ToUInt16(ui.Value)),
        //         Compiler.UInt32TypeId => new UInt32Constant(ToUInt32(ui.Value)),
        //         Compiler.UInt64TypeId => new UInt64Constant(ui.Value),
        //         Compiler.UIntTypeId => new UIntConstant(ui.Value),
        //         _ => throw new Exception("Bogus state in IntegerConstant.promote")
        //     },
        //     _ => throw new Exception()
        // };

        NumericConstant newConstant = i switch {

            SignedIntegerConstant si => (bits, signed) switch {

                (8, false) => new UInt8Constant(ToByte(si.Value)),
                (16, false) => new UInt16Constant(ToUInt16(si.Value)),
                (32, false) => new UInt32Constant(ToUInt32(si.Value)),
                (64, false) => new UInt64Constant(ToUInt64(si.Value)),
                
                (8, true) => new Int8Constant(ToSByte(si.Value)),
                (16, true) => new Int16Constant(ToInt16(si.Value)),
                (32, true) => new Int32Constant(ToInt32(si.Value)),
                (64, true) => new Int64Constant(si.Value),

                _ => throw new Exception("Numeric constants can only be 8, 16, 32, or 64 bits long")
            },

            UnsignedIntegerConstant ui => (bits, signed) switch {

                (8, false) => new UInt8Constant(ToByte(ui.Value)),
                (16, false) => new UInt16Constant(ToUInt16(ui.Value)),
                (32, false) => new UInt32Constant(ToUInt32(ui.Value)),
                (64, false) => new UInt64Constant(ui.Value),
                
                (8, true) => new Int8Constant(ToSByte(ui.Value)),
                (16, true) => new Int16Constant(ToInt16(ui.Value)),
                (32, true) => new Int32Constant(ToInt32(ui.Value)),
                (64, true) => new Int64Constant(System.Convert.ToInt64(ui.Value)),
                
                _ => throw new Exception("Numeric constants can only be 8, 16, 32, or 64 bits long")
            },

            _ => throw new Exception()
        };

        return (newConstant, typeId);
    }
}

///

public partial class NumericConstant {

    public NumericConstant() { }

    public override string ToString() {

        switch (this) {

            case Int64Constant i64:
                return $"Int64({i64.Value})";

            default: 
                return base.ToString() ?? "";
        }
    }
}
    
    public partial class Int8Constant: NumericConstant {

        public sbyte Value { get; init; }

        ///

        public Int8Constant(
            sbyte value) {

            this.Value = value;
        }
    }

    public partial class Int16Constant: NumericConstant {

        public short Value { get; init; }

        ///

        public Int16Constant(
            short value) {

            this.Value = value;
        }
    }

    public partial class Int32Constant: NumericConstant {

        public int Value { get; init; }

        ///

        public Int32Constant(
            int value) {

            this.Value = value;
        }
    }

    public partial class Int64Constant: NumericConstant {

        public long Value { get; init; }

        ///

        public Int64Constant(
            long value) {

            this.Value = value;
        }
    }

    public partial class IntConstant: NumericConstant {

        public long Value { get; init; }

        ///

        public IntConstant(
            long value) {

            this.Value = value;
        }
    }

    public partial class UInt8Constant: NumericConstant {

        public byte Value { get; init; }

        ///

        public UInt8Constant(
            byte value) {
            
            this.Value = value;
        }
    }

    public partial class UInt16Constant: NumericConstant {

        public ushort Value { get; init; }

        ///

        public UInt16Constant(
            ushort value) {
            
            this.Value = value;
        }
    }

    public partial class UInt32Constant: NumericConstant {

        public uint Value { get; init; }

        ///

        public UInt32Constant(
            uint value) {

            this.Value = value;
        }
    }

    public partial class UInt64Constant: NumericConstant {

        public ulong Value { get; init; }

        ///

        public UInt64Constant(
            ulong value) {

            this.Value = value;
        }
    }

    public partial class UIntConstant: NumericConstant {

        public ulong Value { get; init; }

        ///

        public UIntConstant(
            ulong value) {

            this.Value = value;
        }
    }

public static partial class NumericConstantFunctions {

    public static bool Eq(NumericConstant l, NumericConstant r) {

        switch (true) {

            case var _ when l is Int8Constant li8 && r is Int8Constant ri8:             return li8.Value == ri8.Value;
            case var _ when l is Int16Constant li16 && r is Int16Constant ri16:         return li16.Value == ri16.Value;
            case var _ when l is Int32Constant li32 && r is Int32Constant ri32:         return li32.Value == ri32.Value;
            case var _ when l is Int64Constant li64 && r is Int64Constant ri64:         return li64.Value == ri64.Value;
            case var _ when l is IntConstant li && r is IntConstant ri:                 return li.Value == ri.Value;
            case var _ when l is UInt8Constant lu8 && r is UInt8Constant ru8:           return lu8.Value == ru8.Value;
            case var _ when l is UInt16Constant lu16 && r is UInt16Constant ru16:       return lu16.Value == ru16.Value;
            case var _ when l is UInt32Constant lu32 && r is UInt32Constant ru32:       return lu32.Value == ru32.Value;
            case var _ when l is UInt64Constant lu64 && r is UInt64Constant ru64:       return lu64.Value == ru64.Value;
            case var _ when l is UIntConstant lu && r is UIntConstant ru:               return lu.Value == ru.Value;
            default:                                                                    return false;
        }
    }

    public static IntegerConstant? IntegerConstant(
        this NumericConstant n) {

        switch (n) {
            case Int8Constant i8: return new SignedIntegerConstant(ToInt64(i8.Value));
            case Int16Constant i16: return new SignedIntegerConstant(ToInt64(i16.Value));
            case Int32Constant i32: return new SignedIntegerConstant(ToInt64(i32.Value));
            case Int64Constant i64: return new SignedIntegerConstant(i64.Value);
            case IntConstant i: return new SignedIntegerConstant(i.Value);
            case UInt8Constant u8: return new UnsignedIntegerConstant(ToUInt64(u8.Value));
            case UInt16Constant u16: return new UnsignedIntegerConstant(ToUInt64(u16.Value));
            case UInt32Constant u32: return new UnsignedIntegerConstant(ToUInt64(u32.Value));
            case UInt64Constant u64: return new UnsignedIntegerConstant(u64.Value);
            case UIntConstant u: return new UnsignedIntegerConstant(u.Value);
            default: throw new Exception();
        }
    }

    public static Int32 GetNeuType(
        this NumericConstant n) {

        switch (n) {
            case Int8Constant i8: return Compiler.Int8TypeId;
            case Int16Constant i16: return Compiler.Int16TypeId;
            case Int32Constant i32: return Compiler.Int32TypeId;
            case Int64Constant i64: return Compiler.Int64TypeId;
            case IntConstant i: return Compiler.IntTypeId;
            case UInt8Constant u8: return Compiler.UInt8TypeId;
            case UInt16Constant u16: return Compiler.UInt16TypeId;
            case UInt32Constant u32: return Compiler.UInt32TypeId;
            case UInt64Constant u64: return Compiler.UInt64TypeId;
            case UIntConstant u: return Compiler.UIntTypeId;
            default: throw new Exception();
        }
    }
}

///

public partial class CheckedTypeCast {

    public CheckedTypeCast() { }
}

    public partial class CheckedFallibleTypeCast: CheckedTypeCast {

        public Int32 TypeId { get; init; }

        ///

        public CheckedFallibleTypeCast(Int32 typeId) {

            this.TypeId = typeId;
        }
    }

    public partial class CheckedInfallibleTypeCast: CheckedTypeCast {

        public Int32 TypeId { get; init; }

        ///

        public CheckedInfallibleTypeCast(Int32 typeId) {

            this.TypeId = typeId;
        }
    }

    public partial class CheckedSaturatingTypeCast: CheckedTypeCast {

        public Int32 TypeId { get; init; }

        ///

        public CheckedSaturatingTypeCast(Int32 typeId) {

            this.TypeId = typeId;
        }
    }

    public partial class CheckedTruncatingTypeCast: CheckedTypeCast {

        public Int32 TypeId { get; init; }

        ///

        public CheckedTruncatingTypeCast(Int32 typeId) {

            this.TypeId = typeId;
        }
    }

public static partial class CheckedTypeCastFunctions {

    public static bool Eq(CheckedTypeCast l, CheckedTypeCast r) {

        switch (true) {

            case var _ when l is CheckedFallibleTypeCast lc && r is CheckedFallibleTypeCast rc:
                return lc.TypeId == rc.TypeId;

            case var _ when l is CheckedInfallibleTypeCast lc && r is CheckedInfallibleTypeCast rc:
                return lc.TypeId == rc.TypeId;

            case var _ when l is CheckedSaturatingTypeCast lc && r is CheckedSaturatingTypeCast rc:
                return lc.TypeId == rc.TypeId;

            case var _ when l is CheckedTruncatingTypeCast lc && r is CheckedTruncatingTypeCast rc:
                return lc.TypeId == rc.TypeId;

            default:
                return false;
        }
    }

    public static Int32 GetNeuType(
        this CheckedTypeCast t) {

        switch (t) {

            case CheckedFallibleTypeCast f:
                return f.TypeId;

            case CheckedInfallibleTypeCast i:
                return i.TypeId;

            case CheckedSaturatingTypeCast s:
                return s.TypeId;

            case CheckedTruncatingTypeCast tt:
                return tt.TypeId;

            default: 
                throw new Exception();
        }
    }
}

///

public partial class CheckedUnaryOperator {

    public CheckedUnaryOperator() { }
}

    public partial class CheckedPreIncrementUnaryOperator: CheckedUnaryOperator {

        public CheckedPreIncrementUnaryOperator() { }
    }
    
    public partial class CheckedPostIncrementUnaryOperator: CheckedUnaryOperator {
        
        public CheckedPostIncrementUnaryOperator() { }
    }
    
    public partial class CheckedPreDecrementUnaryOperator: CheckedUnaryOperator {

        public CheckedPreDecrementUnaryOperator() { }   
    }
    
    public partial class CheckedPostDecrementUnaryOperator: CheckedUnaryOperator {
        
        public CheckedPostDecrementUnaryOperator() { }
    }
    
    public partial class CheckedNegateUnaryOperator: CheckedUnaryOperator {
        
        public CheckedNegateUnaryOperator() { }
    }
    
    public partial class CheckedDereferenceUnaryOperator: CheckedUnaryOperator {
        
        public CheckedDereferenceUnaryOperator() { }
    }
    
    public partial class CheckedRawAddressUnaryOperator: CheckedUnaryOperator {
        
        public CheckedRawAddressUnaryOperator() { }
    }
    
    public partial class CheckedLogicalNotUnaryOperator: CheckedUnaryOperator {
        
        public CheckedLogicalNotUnaryOperator() { }
    }
    
    public partial class CheckedBitwiseNotUnaryOperator: CheckedUnaryOperator {
        
        public CheckedBitwiseNotUnaryOperator() { }
    }

    public partial class CheckedTypeCastUnaryOperator : CheckedUnaryOperator {

        public CheckedTypeCast TypeCast { get; init; }

        ///
        
        public CheckedTypeCastUnaryOperator(CheckedTypeCast typeCast) {

            this.TypeCast = typeCast;
        }
    }

    public partial class CheckedIsUnaryOperator: CheckedUnaryOperator {

        public Int32 TypeId { get; init; }

        ///

        public CheckedIsUnaryOperator(Int32 typeId) {

            this.TypeId = typeId;
        }
    }
    
public static partial class CheckedUnaryOperatorFunctions {

    public static bool Eq(CheckedUnaryOperator l, CheckedUnaryOperator r) {

        switch (true) {

            case var _ when l is CheckedPreIncrementUnaryOperator && r is CheckedPreIncrementUnaryOperator:
            case var _ when l is CheckedPostIncrementUnaryOperator && r is CheckedPostIncrementUnaryOperator:
            case var _ when l is CheckedPreDecrementUnaryOperator && r is CheckedPreDecrementUnaryOperator:
            case var _ when l is CheckedPostDecrementUnaryOperator && r is CheckedPostDecrementUnaryOperator:
            case var _ when l is CheckedNegateUnaryOperator && r is CheckedNegateUnaryOperator:
            case var _ when l is CheckedDereferenceUnaryOperator && r is CheckedDereferenceUnaryOperator:
            case var _ when l is CheckedRawAddressUnaryOperator && r is CheckedRawAddressUnaryOperator:
            case var _ when l is CheckedLogicalNotUnaryOperator && r is CheckedLogicalNotUnaryOperator:
            case var _ when l is CheckedBitwiseNotUnaryOperator && r is CheckedBitwiseNotUnaryOperator:
                return true;

            case var _ when l is CheckedTypeCastUnaryOperator lt && r is CheckedTypeCastUnaryOperator rt:
                return CheckedTypeCastFunctions.Eq(lt.TypeCast, rt.TypeCast);

            case var _ when l is CheckedIsUnaryOperator li && r is CheckedIsUnaryOperator ri:
                return li.TypeId == ri.TypeId;

            default: 
                return false;
        }
    }
}

///

public partial class CheckedWhenBody {

    public CheckedWhenBody() { }
}

    public partial class CheckedExpressionWhenBody: CheckedWhenBody {

        public CheckedExpression Expression { get; init; }

        ///

        public CheckedExpressionWhenBody(
            CheckedExpression expression) {

            this.Expression = expression;
        }
    }

    public partial class CheckedBlockWhenBody: CheckedWhenBody {

        public CheckedBlock Block { get; init; }

        ///

        public CheckedBlockWhenBody(
            CheckedBlock block) {

            this.Block = block;
        }
    }

///

public partial class CheckedWhenCase {

    public CheckedWhenCase() { }
}

    public partial class CheckedEnumVariantWhenCase: CheckedWhenCase { 

        public String VariantName { get; init; }
        
        public List<(String?, String)> VariantArguments { get; init; }
        
        public Int32 SubjectTypeId  { get; init; }
        
        public Int32 VariantIndex { get; init; }
        
        public Int32 ScopeId { get; init; }
        
        public CheckedWhenBody Body { get; init; }

        ///

        public CheckedEnumVariantWhenCase(
            String variantName,
            List<(String?, String)> variantArguments,
            Int32 subjectTypeId,
            Int32 variantIndex,
            Int32 scopeId,
            CheckedWhenBody body) {

            this.VariantName = variantName;
            this.VariantArguments = variantArguments;
            this.SubjectTypeId = subjectTypeId;
            this.VariantIndex = variantIndex;
            this.ScopeId = scopeId;
            this.Body = body;
        }
    }

///

public partial class CheckedExpression {

    public CheckedExpression() { }
}

    // Standalone

    public partial class CheckedBooleanExpression: CheckedExpression {

        public bool Value { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedBooleanExpression(
            bool value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class CheckedNumericConstantExpression: CheckedExpression {

        public NumericConstant Value { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedNumericConstantExpression(
            NumericConstant value,
            Span span,
            Int32 type) {

            this.Value = value;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedQuotedStringExpression: CheckedExpression {

        public String Value { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedQuotedStringExpression(
            String value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class CheckedCharacterConstantExpression: CheckedExpression {

        public Char Char { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedCharacterConstantExpression(
            Char c,
            Span span) {

            this.Char = c;
            this.Span = span;
        }
    }

    public partial class CheckedUnaryOpExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public CheckedUnaryOperator Operator { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedUnaryOpExpression(
            CheckedExpression expression,
            CheckedUnaryOperator op,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Operator = op;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedBinaryOpExpression: CheckedExpression {

        public CheckedExpression Lhs { get; init; }

        public BinaryOperator Operator { get; init; }

        public CheckedExpression Rhs { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedBinaryOpExpression(
            CheckedExpression lhs,
            BinaryOperator op,
            CheckedExpression rhs,
            Span span,
            Int32 type) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedTupleExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedTupleExpression(
            List<CheckedExpression> expressions,
            Span span,
            Int32 type) {

            this.Expressions = expressions;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedRangeExpression: CheckedExpression {

        public CheckedExpression Start { get; init; }

        public CheckedExpression End { get; init; }

        public Span Span { get; init; }

        public Int32 TypeId { get; init; }

        ///

        public CheckedRangeExpression(
            CheckedExpression start,
            CheckedExpression end,
            Span span,
            Int32 typeId) {
            
            this.Start = start;
            this.End = end;
            this.Span = span;
            this.TypeId = typeId;
        }
    }

    public partial class CheckedArrayExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }

        public CheckedExpression? FillSize { get; init; }

        public Span Span { get; init; }
        
        public Int32 Type { get; init; }

        ///

        public CheckedArrayExpression(
            List<CheckedExpression> expressions,
            CheckedExpression? fillSize,
            Span span,
            Int32 type) 
            : base() {

            this.Expressions = expressions;
            this.FillSize = fillSize;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedDictionaryExpression: CheckedExpression {

        public List<(CheckedExpression, CheckedExpression)> Entries { get; init; }

        public Span Span { get; init; }

        public Int32 TypeId { get; init; }

        ///

        public CheckedDictionaryExpression(
            List<(CheckedExpression, CheckedExpression)> entries,
            Span span,
            Int32 typeId) {

            this.Entries = entries;
            this.Span = span;
            this.TypeId = typeId;
        }
    }

    public partial class CheckedSetExpression: CheckedExpression {

        public List<CheckedExpression> Items { get; init; }

        public Span Span { get; init; }

        public Int32 TypeId { get; init; }

        ///

        public CheckedSetExpression(
            List<CheckedExpression> items,
            Span span,
            Int32 typeId) {

            this.Items = items;
            this.Span = span;
            this.TypeId = typeId;
        }
    }

    public partial class CheckedIndexedExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }
        
        public CheckedExpression Index { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedExpression(
            CheckedExpression expression,
            CheckedExpression index,
            Span span,
            Int32 type) 
            : base() {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedDictionaryExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }
        
        public CheckedExpression Index { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedDictionaryExpression(
            CheckedExpression expression,
            CheckedExpression index,
            Span span,
            Int32 type) 
            : base() {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedTupleExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Int64 Index { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedTupleExpression(
            CheckedExpression expression,
            Int64 index,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Index = index;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedStructExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public String Name { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedStructExpression(
            CheckedExpression expression,
            String name,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Name = name;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedWhenExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public List<CheckedWhenCase> Cases { get; init; }

        public Span Span { get; init; }

        public Int32 TypeId { get; init; }

        ///

        public CheckedWhenExpression(
            CheckedExpression expression,
            List<CheckedWhenCase> cases,
            Span span,
            Int32 typeId) {

            this.Expression = expression;
            this.Cases = cases;
            this.Span = span;
            this.TypeId = typeId;
        }
    }

    public partial class CheckedCallExpression: CheckedExpression {

        public CheckedCall Call { get; init; }
        
        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedCallExpression(
            CheckedCall call,
            Span span,
            Int32 type) {

            this.Call = call;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedMethodCallExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public CheckedCall Call { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedMethodCallExpression(
            CheckedExpression expression,
            CheckedCall call,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Call = call;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedVarExpression: CheckedExpression {
        
        public CheckedVariable Variable { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedVarExpression(
            CheckedVariable variable,
            Span span) {

            this.Variable = variable;
            this.Span = span;
        }
    }

    public partial class CheckedOptionalNoneExpression: CheckedExpression {

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedOptionalNoneExpression(
            Span span,
            Int32 type) {

            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedOptionalSomeExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedOptionalSomeExpression(
            CheckedExpression expression,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Span = span;
            this.Type = type;
        }
    }

    public partial class CheckedForceUnwrapExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Span Span { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedForceUnwrapExpression(
            CheckedExpression expression,
            Span span,
            Int32 type) {

            this.Expression = expression;
            this.Span = span;
            this.Type = type;
        }
    }

    // Parsing error

    public partial class CheckedGarbageExpression: CheckedExpression {

        public Span Span { get; init; }

        ///

        public CheckedGarbageExpression(
            Span span) { 

            this.Span = span;
        }
    }

///

public static partial class CheckedExpressionFunctions {

    public static Int32 GetNeuType(
        this CheckedExpression expr) {

        switch (expr) {

            case CheckedBooleanExpression _: {

                return Compiler.BoolTypeId;
            }

            case CheckedCallExpression e: {

                return e.Type;
            }
            
            case CheckedNumericConstantExpression ne: {

                return ne.Type;
            }

            case CheckedQuotedStringExpression _: {

                return Compiler.StringTypeId;
            }

            case CheckedCharacterConstantExpression _: {

                return Compiler.CCharTypeId; // use the C one for now
            }

            case CheckedUnaryOpExpression u: {

                return u.Type;
            }

            case CheckedBinaryOpExpression e: {

                return e.Type;
            }

            case CheckedArrayExpression vecExpr: {

                return vecExpr.Type;
            }

            case CheckedDictionaryExpression de: {

                return de.TypeId;
            }

            case CheckedSetExpression se: {

                return se.TypeId;
            }

            case CheckedTupleExpression tupleExpr: {

                return tupleExpr.Type;
            }

            case CheckedRangeExpression rangeExpr: {

                return rangeExpr.TypeId;
            }

            case CheckedIndexedDictionaryExpression ide: {

                return ide.Type;
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

            case CheckedMethodCallExpression mce: {

                return mce.Type;
            }

            case CheckedVarExpression ve: {

                return ve.Variable.Type;
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

            case CheckedWhenExpression e: {

                return e.TypeId;
            }

            case CheckedGarbageExpression _: {

                return Compiler.UnknownTypeId;
            }

            default:

                throw new Exception();
        }
    }

    public static Span GetSpan(
        this CheckedExpression expr) {

        switch (expr) {

            case CheckedBooleanExpression b:
                return b.Span;
            
            case CheckedCallExpression c:
                return c.Span;
            
            case CheckedNumericConstantExpression n:
                return n.Span;

            case CheckedQuotedStringExpression q:
                return q.Span;

            case CheckedCharacterConstantExpression c:
                return c.Span;

            case CheckedUnaryOpExpression u:
                return u.Span;
            
            case CheckedBinaryOpExpression b:
                return b.Span;

            case CheckedDictionaryExpression d:
                return d.Span;

            case CheckedSetExpression s:
                return s.Span;

            case CheckedArrayExpression a:
                return a.Span;

            case CheckedTupleExpression t:
                return t.Span;

            case CheckedRangeExpression r:
                return r.Span;

            case CheckedIndexedDictionaryExpression i:
                return i.Span;

            case CheckedIndexedExpression i:
                return i.Span;

            case CheckedIndexedTupleExpression i:
                return i.Span;

            case CheckedIndexedStructExpression i:
                return i.Span;

            case CheckedMethodCallExpression m:
                return m.Span;

            case CheckedVarExpression v:
                return v.Span;

            case CheckedOptionalNoneExpression n:
                return n.Span;

            case CheckedOptionalSomeExpression s:
                return s.Span;

            case CheckedForceUnwrapExpression f:
                return f.Span;

            case CheckedWhenExpression w:
                return w.Span;

            case CheckedGarbageExpression g:
                return g.Span;

            default:
                throw new Exception();
        }

    }

    public static IntegerConstant? ToIntegerConstant(
        this CheckedExpression e) {

        switch (e) {

            case CheckedNumericConstantExpression ne: return ne.Value.IntegerConstant();

            case CheckedUnaryOpExpression ue when 
                ue.Operator is CheckedTypeCastUnaryOperator tc
                && tc.TypeCast is CheckedInfallibleTypeCast: {

                if (!NeuTypeFunctions.IsInteger(ue.Type)) {

                    return null;
                }

                switch (ue.Expression) {

                    case CheckedNumericConstantExpression c: {

                        return c.Value.IntegerConstant();
                    }

                    default: {

                        return null;
                    }
                }
            }

            default: return null;
        }
    }

    public static bool IsMutable(
        this CheckedExpression e) {

        switch (e) {

            case CheckedVarExpression ve: return ve.Variable.Mutable;

            case CheckedIndexedStructExpression ise: return ise.Expression.IsMutable();

            case CheckedIndexedExpression ie: return ie.Expression.IsMutable();

            case CheckedIndexedTupleExpression te: return te.Expression.IsMutable();
            
            case CheckedIndexedDictionaryExpression te: return te.Expression.IsMutable();
            
            case CheckedForceUnwrapExpression fue: return fue.Expression.IsMutable();

            default: return false;
        }
    }
}

///

public partial class ResolvedNamespace {

    public String Name { get; init; }

    public List<Int32>? GenericParameters { get; set; }

    ///

    public ResolvedNamespace(
        String name,
        List<Int32>? genericParameters) {

        this.Name = name;
        this.GenericParameters = genericParameters;
    }
}

///

public partial class CheckedCall {

    public List<ResolvedNamespace> Namespace { get; init; }
    
    public String Name { get; init; }

    public bool CalleeThrows { get; init; }
    
    public List<(String, CheckedExpression)> Args { get; init; }

    public List<Int32> TypeArgs { get; init; }

    public FunctionLinkage Linkage { get; init; }
    
    public Int32 Type { get; init; }

    public DefinitionType? CalleeDefinitionType { get; init; }

    ///

    public CheckedCall(
        List<ResolvedNamespace> ns,
        String name,
        bool calleeThrows,
        List<(String, CheckedExpression)> args,
        List<Int32> typeArgs,
        FunctionLinkage linkage,
        Int32 type,
        DefinitionType? calleeDefinitionType) {

        this.Namespace = ns;
        this.Name = name;
        this.CalleeThrows = calleeThrows;
        this.Args = args;
        this.TypeArgs = typeArgs;
        this.Linkage = linkage;
        this.Type = type;
        this.CalleeDefinitionType = calleeDefinitionType;
    }
}

///

public partial class Scope {
    
    public List<CheckedVariable> Vars { get; init; }

    public List<(String, Int32)> Structs { get; init; }

    public List<(String, Int32)> Funcs { get; init; }

    public List<(String, Int32)> Enums { get; init; }

    public List<(String, Int32)> Types { get; init; }

    public Int32? Parent { get; init; }

    ///

    public Scope(
        Int32? parent) 
        : this(
            new List<CheckedVariable>(),
            new List<(String, Int32)>(),
            new List<(String, Int32)>(),
            new List<(String, Int32)>(),
            new List<(String, Int32)>(),
            parent) { }

    public Scope(
        List<CheckedVariable> vars,
        List<(String, Int32)> structs,
        List<(String, Int32)> funcs,
        List<(String, Int32)> enums,
        List<(String, Int32)> types,
        Int32? parent) {

        this.Vars = vars;
        this.Structs = structs;
        this.Funcs = funcs;
        this.Enums = enums;
        this.Parent = parent;
        this.Types = types;
    }
}

///

public static partial class TypeCheckerFunctions {

    public static Error? TypeCheckFile(
        ParsedFile parsedFile,
        Int32 scopeId,
        Project project) {

        Error? error = null;

        var projectStructLength = project.Structs.Count;

        var projectEnumLength = project.Enums.Count;

        var projectFunctionLength = project.Functions.Count;

        for (Int32 _structId = 0; _structId < parsedFile.Structs.Count; _structId++) {
            
            // Ensure we know the types ahead of time, so they can be recursive

            var structure = parsedFile.Structs.ElementAt(_structId);

            var structId = _structId + projectStructLength;

            project.Types.Add(new StructType(structId));

            var structTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(scopeId, structure.Name, structTypeId, structure.Span).Error is Error e1) {

                error = error ?? e1;
            }

            if (TypeCheckStructPredecl(structure, structTypeId, structId, scopeId, project) is Error err) {

                error = error ?? err;
            }
        }

        for (Int32 _enumId = 0; _enumId < parsedFile.Enums.Count; _enumId++) {

            var _enum = parsedFile.Enums[_enumId];

            var enumId = _enumId + projectEnumLength;

            project.Types.Add(new EnumType(enumId));

            var enumTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(scopeId, _enum.Name, enumTypeId, _enum.Span).Error is Error e) {

                error = error ?? e;
            }

            if (TypeCheckEnumPredecl(_enum, enumId, scopeId, project) is Error e2) {

                error = error ?? e2;
            }
        }

        foreach (var fun in parsedFile.Functions) {
            
            // Ensure we know the function ahead of time, so they can be recursive
            
            var chkFuncPredeclErr = TypeCheckFuncPredecl(fun, scopeId, project);

            error = error ?? chkFuncPredeclErr;
        }

        for (Int32 structId = 0; structId < parsedFile.Structs.Count; structId++) {

            var structure = parsedFile.Structs.ElementAt(structId);

            var chkStructErr = TypeCheckStruct(
                structure, 
                structId + projectStructLength,
                scopeId, 
                project);

            error = error ?? chkStructErr;
        }

        for (Int32 enumId = 0; enumId < parsedFile.Enums.Count; enumId++) {

            var _enum = parsedFile.Enums[enumId];

            var err = TypeCheckEnum(
                _enum,
                enumId + projectEnumLength,
                project.FindTypeInScope(scopeId, _enum.Name) ?? throw new Exception(),
                project.Enums[enumId + projectEnumLength].ScopeId,
                scopeId,
                project);
            
            error = error ?? err;
        }

        for (Int32 i = 0; i < parsedFile.Functions.Count; i++) {

            var func = parsedFile.Functions[i];

            project.CurrentFunctionIndex = i + projectFunctionLength;

            var err = TypeCheckFunc(func, scopeId, project);

            error = error ?? err;

            project.CurrentFunctionIndex = null;
        }

        return error;
    }

    public static Error? TypeCheckEnumPredecl(
        ParsedEnum _enum,
        Int32 enumId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var enumScopeId = project.CreateScope(parentScopeId);

        var genericParameters = new List<Int32>();

        foreach (var (genParam, paramSpan) in _enum.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var paramTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(
                enumScopeId, 
                genParam, 
                paramTypeId, 
                paramSpan).Error is Error e) {

                error = error ?? e;
            }

            genericParameters.Add(paramTypeId);
        }

        var (typeId, typeErr) = TypeCheckTypeName(_enum.UnderlyingType, enumScopeId, project);

        error = error ?? typeErr;

        Int32? underlyingType;

        if (typeId == Compiler.UnknownTypeId) {

            underlyingType = null;
        }
        else {

            underlyingType = typeId;
        }

        project.Enums.Add(
            new CheckedEnum(
                name: _enum.Name,
                genericParameters,
                variants: new List<CheckedEnumVariant>(),
                scopeId: enumScopeId,
                definitionLinkage: _enum.DefinitionLinkage,
                underlyingType,
                span: _enum.Span));
                
        switch (project.AddEnumToScope(parentScopeId, _enum.Name, enumId, _enum.Span).Error) {

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

    public static Error? TypeCheckEnum(
        ParsedEnum _enum,
        Int32 enumId,
        Int32 enumTypeId,
        Int32 enumScopeId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        // Check enum variants and resolve them if needed.

        var variants = new List<CheckedEnumVariant>();

        UInt64? nextConstantValue = 0;

        var seenNames = new HashSet<String>();

        Func<ParsedExpression, Project, (CheckedExpression, Error?)> castToUnderlying = (x, project) => {

            var span = x.GetSpan();

            var expr = new UnaryOpExpression(
                x,
                new TypeCastUnaryOperator(new InfallibleTypeCast(_enum.UnderlyingType)),
                span);

            return TypeCheckExpression(expr, enumScopeId, project, SafetyMode.Safe, null);
        };

        var underlyingType = project.Enums[enumId].UnderlyingType;

        foreach (var variant in _enum.Variants) {

            switch (variant) {

                case UntypedEnumVariant u: {

                    if (seenNames.Contains(u.Name)) {

                        error = error ?? 
                            new TypeCheckError(
                            $"Enum variant '{u.Name}' is defined more than once",
                            u.Span);
                    }
                    else {

                        seenNames.Add(u.Name);

                        if (underlyingType is not null) {

                            if (nextConstantValue == null) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Missing enum variant value, the enum underlying type is not numeric, and so all enum variants must have explicit values",
                                        u.Span);
                            }
                            else {

                                var (checkedExpr, typeErr) = castToUnderlying(
                                    new NumericConstantExpression(
                                        new UInt64Constant(nextConstantValue.Value),
                                        u.Span), 
                                    project);

                                error = error ?? typeErr;

                                variants.Add(
                                    new CheckedWithValueEnumVariant(
                                        u.Name,
                                        checkedExpr,
                                        u.Span));
                                
                                nextConstantValue = nextConstantValue.Value + 1;
                            }
                        }
                        else {

                            variants.Add(new CheckedUntypedEnumVariant(u.Name, u.Span));
                        }

                        if (project.FindFuncInScope(enumScopeId, u.Name) is null) {

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: u.Name,
                                throws: false,
                                returnType: enumTypeId,
                                parameters: new List<CheckedParameter>(),
                                genericParameters: _enum
                                    .GenericParameters
                                    .Select(x => new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId,
                                block: new CheckedBlock(),
                                linkage: FunctionLinkage.ImplicitEnumConstructor); 

                            project.Functions.Add(checkedConstructor);

                            if (project.AddFuncToScope(
                                enumScopeId, 
                                u.Name, 
                                project.Functions.Count - 1, 
                                u.Span).Error is Error e1) {

                                error = error ?? e1;
                            }
                        }
                    }

                    break;
                }

                case WithValueEnumVariant w: {

                    if (seenNames.Contains(w.Name)) {

                        error = error ?? 
                            new TypeCheckError(
                                $"Enum variant '{w.Name}' is defined more than once",
                                w.Span);
                    }
                    else {

                        seenNames.Add(w.Name);

                        var (checkedExpr, typeErr) = castToUnderlying(w.Expression, project);

                        switch (checkedExpr.ToIntegerConstant()) {

                            case IntegerConstant constant: {

                                nextConstantValue = ToUInt64(constant.ToInt64()) + 1;

                                break;
                            }

                            default: {

                                error = error ?? 
                                    new TypeCheckError(
                                        $"Enum variant '{w.Name}' in enum '{_enum.Name}' has a non-constant value: {checkedExpr}",
                                        w.Span);

                                break;
                            }
                        }

                        error = error ?? typeErr;

                        variants.Add(
                            new CheckedWithValueEnumVariant(
                                w.Name,
                                checkedExpr,
                                w.Span));

                        // FIXME: Generate a constructor
                    }

                    break;
                }

                case StructLikeEnumVariant s: {

                    if (seenNames.Contains(s.Name)) {

                        error = error ??
                            new TypeCheckError(
                                $"Enum variant '{s.Name}' is defined more than once",
                                s.Span);
                    }
                    else {

                        seenNames.Add(s.Name);

                        var memberNames = new HashSet<String>();

                        var checkedMembers = new List<CheckedVarDecl>();

                        foreach (var member in s.Declarations) {

                            if (memberNames.Contains(member.Name)) {

                                error = error ?? 
                                    new TypeCheckError(
                                        $"Enum variant '{s.Name}' has a member named '{member.Name}' more than once",
                                        s.Span);
                            }
                            else {

                                memberNames.Add(member.Name);

                                var (decl, typeErr) = TypeCheckTypeName(member.Type, enumScopeId, project);

                                error = error ?? typeErr;

                                checkedMembers.Add(
                                    new CheckedVarDecl(
                                        name: member.Name,
                                        type: decl,
                                        mutable: member.Mutable,
                                        span: member.Span));
                            }
                        }

                        variants.Add(
                            new CheckedStructLikeEnumVariant(
                                name: s.Name,
                                checkedMembers,
                                s.Span));
                                
                        if (project.FindFuncInScope(enumScopeId, s.Name) is null) {

                            // Generate a constructor

                            var constructorParams = checkedMembers
                                .Select(member => 
                                    new CheckedParameter(
                                        requiresLabel: true, 
                                        variable: new CheckedVariable(
                                            name: member.Name,
                                            type: member.Type,
                                            mutable: false)))
                                .ToList();

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: s.Name,
                                throws: false,
                                returnType: enumTypeId,
                                parameters: constructorParams,
                                genericParameters: _enum
                                    .GenericParameters
                                    .Select(x => 
                                        new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId: funcScopeId,
                                block: new CheckedBlock(),
                                linkage: FunctionLinkage.ImplicitEnumConstructor);
                            
                            project.Functions.Add(checkedConstructor);

                            if (project.AddFuncToScope(
                                enumScopeId, 
                                s.Name, 
                                project.Functions.Count - 1, 
                                s.Span).Error is Error e) {

                                error = error ?? e;
                            }
                        }
                    }

                    break;
                }

                case TypedEnumVariant t: {

                    if (!(_enum.UnderlyingType is ParsedEmptyType)) {

                        error = error ?? 
                            new TypeCheckError(
                                "Enum variants cannot have a type if the enum has an underlying type",
                                t.Span);
                    }
                    else if (seenNames.Contains(t.Name)) {

                        error = error ??
                            new TypeCheckError(
                                $"Enum variant '{t.Name}' is defined more than once",
                                t.Span);
                    }
                    else {

                        seenNames.Add(t.Name);

                        var (checkedType, typeErr) = TypeCheckTypeName(t.Type, enumScopeId, project);

                        error = error ?? typeErr;

                        variants.Add(
                            new CheckedTypedEnumVariant(
                                t.Name,
                                checkedType,
                                t.Span));

                        if (project.FindFuncInScope(enumScopeId, t.Name) == null) {

                            // Generate a constructor

                            var constructorParams = new List<CheckedParameter>(
                                new [] {
                                    new CheckedParameter(
                                        requiresLabel: false,
                                        variable: new CheckedVariable(
                                            name: "value",
                                            type: checkedType,
                                            mutable: false))
                                });

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: t.Name,
                                throws: false,
                                returnType: enumTypeId,
                                parameters: constructorParams,
                                genericParameters: 
                                    _enum
                                    .GenericParameters
                                    .Select(x => new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId,
                                block: new CheckedBlock(),
                                linkage: FunctionLinkage.ImplicitEnumConstructor);

                            project.Functions.Add(checkedConstructor);

                            if (project.AddFuncToScope(
                                enumScopeId, 
                                t.Name, 
                                project.Functions.Count - 1, 
                                t.Span).Error is Error e) {

                                error = error ?? e;
                            }    
                        }
                    }

                    break;
                }

                default: {

                    throw new Exception();
                }
            }
        }

        project.Enums[enumId].Variants = variants;

        return error;
    }

    public static Error? TypeCheckStructPredecl(
        ParsedStruct structure,
        Int32 structTypeId,
        Int32 structId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var structScopeId = project.CreateScope(parentScopeId);

        var _genericParameters = new List<Int32>();

        foreach (var (genParam, paramSpan) in structure.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var paramTypeId = project.Types.Count - 1;

            _genericParameters.Add(paramTypeId);

            if (project.AddTypeToScope(
                structScopeId, 
                genParam, 
                paramTypeId, 
                paramSpan).Error is Error err) {

                error = error ?? err;
            }
        }

        foreach (var func in structure.Methods) {

            var genericParameters = new List<FunctionGenericParameter>();

            var methodScopeId = project.CreateScope(structScopeId);

            foreach (var (genParam, paramSpan) in func.GenericParameters) {

                project.Types.Add(new TypeVariable(genParam));

                var typeVarTypeId = project.Types.Count - 1;

                genericParameters.Add(new ParameterFunctionGenericParameter(typeVarTypeId));

                if (project.AddTypeToScope(methodScopeId, genParam, typeVarTypeId, paramSpan).Error is Error e) {

                    error = error ?? e;
                }
            }

            var checkedFunction = new CheckedFunction(
                name: func.Name,
                throws: func.Throws,
                parameters: new List<CheckedParameter>(),
                genericParameters: genericParameters,
                funcScopeId: methodScopeId,
                returnType: Compiler.UnknownTypeId,
                block: new CheckedBlock(),
                linkage: func.Linkage);

            foreach (var param in func.Parameters) {

                if (param.Variable.Name == "this") {

                    var checkedVariable = new CheckedVariable(
                        name: param.Variable.Name,
                        type: structTypeId,
                        mutable: param.Variable.Mutable);

                    checkedFunction.Parameters.Add(
                        new CheckedParameter(
                            requiresLabel: param.RequiresLabel,
                            variable: checkedVariable));
                }
                else {

                    var (paramType, err) = TypeCheckTypeName(param.Variable.Type, methodScopeId, project);

                    error = error ?? err;

                    var checkedVariable = new CheckedVariable(
                        name: param.Variable.Name,
                        type: paramType,
                        mutable: param.Variable.Mutable);

                    checkedFunction.Parameters.Add(
                        new CheckedParameter(
                            requiresLabel: param.RequiresLabel,
                            variable: checkedVariable));
                }
            }

            project.Functions.Add(checkedFunction);

            if (project.AddFuncToScope(structScopeId, func.Name, project.Functions.Count - 1, structure.Span).Error is Error e1) {

                error = error ?? e1;
            }
        }

        project.Structs.Add(
            new CheckedStruct(
                name: structure.Name,
                _genericParameters,
                fields: new List<CheckedVarDecl>(),
                scopeId: structScopeId,
                definitionLinkage: structure.DefinitionLinkage,
                definitionType: structure.DefinitionType));

        if (project.AddStructToScope(parentScopeId, structure.Name, structId, structure.Span).Error is Error e2) {

            error = error ?? e2;
        }

        return error;
    }

    public static Error? TypeCheckStruct(
        ParsedStruct structure,
        Int32 structId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var fields = new List<CheckedVarDecl>();

        var chkStruct = project.Structs[structId];

        var chkStructScopeId = chkStruct.ScopeId;

        var structTypeId = project.FindOrAddTypeId(new StructType(structId));

        foreach (var uncheckedMember in structure.Fields) {

            var (checkedMemberType, checkedMemberTypeErr) = TypeCheckTypeName(uncheckedMember.Type, chkStructScopeId, project);

            error = error ?? checkedMemberTypeErr;

            fields.Add(
                new CheckedVarDecl(
                    name: uncheckedMember.Name,
                    type: checkedMemberType, 
                    mutable: uncheckedMember.Mutable, 
                    span: uncheckedMember.Span));
        }

        if (project.FindFuncInScope(chkStructScopeId, structure.Name) == null) {

            // No constructor found, so let's make one

            var constructorParams = new List<CheckedParameter>();

            foreach (var field in fields) {

                constructorParams.Add(
                    new CheckedParameter(
                        requiresLabel: true,
                        variable: new CheckedVariable(
                            name: field.Name,
                            type: field.Type,
                            mutable: field.Mutable)));
            }

            var funcScopeId = project.CreateScope(parentScopeId);

            var checkedConstructor = new CheckedFunction(
                name: structure.Name,
                throws: false,
                returnType: structTypeId,
                parameters: constructorParams,
                genericParameters: new List<FunctionGenericParameter>(),
                funcScopeId,
                block: new CheckedBlock(),
                linkage: FunctionLinkage.ImplicitConstructor);

            // Internal constructor

            project.Functions.Add(checkedConstructor);

            // Add constructor to the struct's scope

            if (project.AddFuncToScope(
                chkStructScopeId,
                structure.Name,
                project.Functions.Count - 1,
                structure.Span).Error is Error constructorErr) {

                error = error ?? constructorErr;
            }
        }

        var checkedStruct = project.Structs[structId];

        checkedStruct.Fields = fields;

        foreach (var func in structure.Methods) {

            var typeChkErr = TypeCheckMethod(func, project, structId);

            error = error ?? typeChkErr;
        }
        
        return error;
    }

    public static Error? TypeCheckFuncPredecl(
        ParsedFunction func,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var funcScopeId = project.CreateScope(parentScopeId);

        var checkedFunction = new CheckedFunction(
            name: func.Name,
            throws: func.Throws,
            returnType: Compiler.UnknownTypeId,
            parameters: new List<CheckedParameter>(),
            genericParameters: new List<FunctionGenericParameter>(),
            funcScopeId,
            block: new CheckedBlock(),
            linkage: func.Linkage);

        var checkedFuncScopeId = checkedFunction.FuncScopeId;

        var genericParams = new List<FunctionGenericParameter>();

        foreach (var (genParam, paramSpan) in func.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var typeVarTypeId = project.Types.Count - 1;

            genericParams.Add(new ParameterFunctionGenericParameter(typeVarTypeId));

            if (project.AddTypeToScope(checkedFuncScopeId, genParam, typeVarTypeId, paramSpan).Error is Error genErr) {

                error = error ?? genErr;
            }
        }

        checkedFunction.GenericParameters = genericParams;

        foreach (var param in func.Parameters) {

            var (paramType, typeCheckNameErr) = TypeCheckTypeName(param.Variable.Type, funcScopeId, project);

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

        var funcId = project.Functions.Count;

        project.Functions.Add(checkedFunction);

        if (project.AddFuncToScope(parentScopeId, func.Name, funcId, func.NameSpan).Error is Error e1) {

            error = error ?? e1;
        }

        return error;
    }

    public static Error? TypeCheckFunc(
        ParsedFunction func,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var funcId = project
            .FindFuncInScope(parentScopeId, func.Name) 
            ?? throw new Exception("Internal error: missing previously defined function");

        var checkedFunction = project.Functions[funcId];

        var functionScopeId = checkedFunction.FuncScopeId;

        var paramVars = new List<CheckedVariable>();

        foreach (var param in checkedFunction.Parameters) {

            paramVars.Add(param.Variable);
        }

        foreach (var var in paramVars) {

            if (project.AddVarToScope(functionScopeId, var, func.NameSpan).Error is Error e1) {

                error = error ?? e1;
            }
        }

        // Do this once to resolve concrete types (if any)
    
        var (_funcReturnType, returnTypeErr) = TypeCheckTypeName(func.ReturnType, functionScopeId, project);
        
        error = error ?? returnTypeErr;
        
        checkedFunction = project.Functions[funcId];

        checkedFunction.ReturnType = _funcReturnType;

        var (block, typeCheckBlockErr) = TypeCheckBlock(func.Block, functionScopeId, project, SafetyMode.Safe);

        error = error ?? typeCheckBlockErr;

        // typecheck the return type again to resolve any generics

        var (funcReturnType, typeCheckReturnTypeErr) = TypeCheckTypeName(func.ReturnType, functionScopeId, project);

        error = error ?? typeCheckReturnTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        var returnType = funcReturnType;

        if (funcReturnType == Compiler.UnknownTypeId) {

            if (block.Stmts.LastOrDefault() is CheckedReturnStatement ret) {

                returnType = ret.Expr.GetNeuType();
            }
            else {

                returnType = Compiler.VoidTypeId;
            }
        }

        checkedFunction = project.Functions[funcId];

        checkedFunction.Block = block;

        checkedFunction.ReturnType = returnType;

        return error;
    }

    public static Error? TypeCheckMethod(
        ParsedFunction func,
        Project project,
        Int32 structId) { 

        Error? error = null;

        var structure = project.Structs[structId];

        var structureScopeId = structure.ScopeId;

        var methodId = project
            .FindFuncInScope(structureScopeId, func.Name)
            ?? throw new Exception("Internal error: we just pushed the checked function, but it's not present");

        var checkedFunction = project.Functions[methodId];

        var funcScopeId = checkedFunction.FuncScopeId;

        var paramVars = new List<CheckedVariable>();

        foreach (var param in checkedFunction.Parameters) {

            paramVars.Add(param.Variable);
        }

        foreach (var variable in paramVars) {

            if (project.AddVarToScope(funcScopeId, variable, func.NameSpan).Error is Error e1) {

                error = error ?? e1;
            }
        }

        var (block, chkBlockErr) = TypeCheckBlock(func.Block, funcScopeId, project, SafetyMode.Safe);

        error = error ?? chkBlockErr;

        var (funcReturnType, chkRetTypeErr) = TypeCheckTypeName(func.ReturnType, funcScopeId, project);

        error = error ?? chkRetTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        var returnType = funcReturnType;

        if (funcReturnType == Compiler.UnknownTypeId) {

            if (block.Stmts.FirstOrDefault() is CheckedReturnStatement ret) {

                returnType = ret.Expr.GetNeuType();
            }
            else {

                returnType = Compiler.VoidTypeId;
            }
        }

        checkedFunction = project.Functions[methodId];

        checkedFunction.Block = block;

        checkedFunction.ReturnType = returnType;

        return error;       
    }

    public static (CheckedBlock, Error?) TypeCheckBlock(
        ParsedBlock block,
        Int32 parentScopeId,
        Project project,
        SafetyMode safetyMode) {

        Error? error = null;

        var checkedBlock = new CheckedBlock();

        var blockScopeId = project.CreateScope(parentScopeId);

        foreach (var stmt in block.Statements) {

            var (checkedStmt, err) = TypeCheckStatement(stmt, blockScopeId, project, safetyMode);

            error = error ?? err;

            checkedBlock.Stmts.Add(checkedStmt);
        }

        return (checkedBlock, error);
    }

    public static (CheckedStatement, Error?) TypeCheckStatement(
        ParsedStatement stmt,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode) {

        Error? error = null;

        switch (stmt) {

            case TryStatement tryStmt: {

                var (checkedStmt, err) = TypeCheckStatement(tryStmt.Statement, scopeId, project, safetyMode);

                error = error ?? err;

                var errorStructId = project
                    .FindStructInScope(0, "Error")
                    ?? throw new Exception("internal error: Error builtin definition not found");

                var errorDecl = new CheckedVariable(
                    name: tryStmt.Name,
                    type: project.FindOrAddTypeId(new StructType(errorStructId)),
                    mutable: false);

                var catchScopeId = project.CreateScope(scopeId);

                if (project.AddVarToScope(catchScopeId, errorDecl, tryStmt.Span).Error is Error e) {

                    error = error ?? e;
                }

                var (checkedCatchBlock, catchBlockErr) = TypeCheckBlock(tryStmt.Block, catchScopeId, project, safetyMode);

                error = error ?? catchBlockErr;

                return (
                    new CheckedTryStatement(
                        checkedStmt, 
                        tryStmt.Name, 
                        checkedCatchBlock),
                    error);
            }

            case ThrowStatement ts: {

                var (checkedExpr, err) = TypeCheckExpression(ts.Expr, scopeId, project, safetyMode, null);

                error = error ?? err;

                // FIXME: Verify that the expression produces an Error

                return (
                    new CheckedThrowStatement(checkedExpr),
                    error);
            }

            case ForStatement fs: {

                var (checkedExpr, err) = TypeCheckExpression(fs.Range, scopeId, project, safetyMode, null);

                error = error ?? err;

                var iteratorScopeId = project.CreateScope(scopeId);

                var rangeStructId = project
                    .FindStructInScope(0, "Range")
                    ?? throw new Exception("internal error: Range builtin definition not found");

                Int32? indexType = null;

                if (project.Types[checkedExpr.GetNeuType()] is GenericInstance gi) {

                    if (gi.StructId == rangeStructId) {

                        indexType = gi.TypeIds[0];
                    }
                    else {

                        throw new Exception("Range expression doesn't have Range type");
                    }
                }
                else {

                    throw new Exception("Range expression doesn't have Range type");
                }

                var iteratorDecl = new CheckedVariable(
                    name: fs.IteratorName,
                    type: indexType ?? throw new Exception(),
                    mutable: true);

                if (project.AddVarToScope(iteratorScopeId, iteratorDecl, fs.Range.GetSpan()).Error is Error e) {

                    error = error ?? e;
                }

                var (checkedBlock, blockErr) = TypeCheckBlock(fs.Block, iteratorScopeId, project, safetyMode);

                error = error ?? blockErr;

                return (
                    new CheckedForStatement(fs.IteratorName, checkedExpr, checkedBlock),
                    error);
            }

            case ContinueStatement cs: {

                return (
                    new CheckedContinueStatement(),
                    null);
            }

            case BreakStatement bs: {

                return (
                    new CheckedBreakStatement(),
                    null);
            }

            case ExpressionStatement es: {

                var (checkedExpr, exprErr) = TypeCheckExpression(es.Expression, scopeId, project, safetyMode, null);

                return (
                    new CheckedExpressionStatement(checkedExpr),
                    exprErr);
            }

            case DeferStatement ds: {

                var (checkedStmt, err) = TypeCheckStatement(ds.Statement, scopeId, project, safetyMode);

                return (
                    new CheckedDeferStatement(checkedStmt),
                    err);
            }

            case UnsafeBlockStatement us: {

                var (checkedBlock, blockErr) = TypeCheckBlock(us.Block, scopeId, project, SafetyMode.Unsafe);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    blockErr);
            }

            case VarDeclStatement vds: {

                var (checkedExpr, exprErr) = TypeCheckExpression(vds.Expr, scopeId, project, safetyMode, null);

                error = error ?? exprErr;

                var (checkedTypeId, chkTypeErr) = TypeCheckTypeName(vds.Decl.Type, scopeId, project);

                if (checkedTypeId == Compiler.UnknownTypeId && checkedExpr.GetNeuType() != Compiler.UnknownTypeId) {

                    checkedTypeId = checkedExpr.GetNeuType();
                }
                else {

                    error = error ?? chkTypeErr;
                }

                var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                    checkedTypeId,
                    checkedExpr,
                    vds.Expr.GetSpan());

                error = error ?? tryPromoteErr;

                if (promotedExpr is not null) {

                    checkedExpr = promotedExpr;
                }

                var checkedVarDecl = new CheckedVarDecl(
                    name: vds.Decl.Name,
                    type: checkedTypeId,
                    span: vds.Decl.Span,
                    mutable: vds.Decl.Mutable);

                if (project.AddVarToScope(
                    scopeId,
                    new CheckedVariable(
                        name: checkedVarDecl.Name, 
                        type: checkedVarDecl.Type, 
                        mutable: checkedVarDecl.Mutable),
                    checkedVarDecl.Span).Error is Error e) {

                    error = error ?? e;
                }

                return (
                    new CheckedVarDeclStatement(checkedVarDecl, checkedExpr),
                    error);
            }

            case IfStatement ifStmt: {

                var (checkedCond, exprErr) = TypeCheckExpression(ifStmt.Expr, scopeId, project, safetyMode, null);
                
                error = error ?? exprErr;

                if (checkedCond.GetNeuType() != Compiler.BoolTypeId) {
                    
                    error = error ?? 
                        new TypeCheckError(
                            "Condition must be a boolean expression",
                            checkedCond.GetSpan());
                }

                var (checkedBlock, blockErr) = TypeCheckBlock(ifStmt.Block, scopeId, project, safetyMode);
                
                error = error ?? blockErr;

                CheckedStatement? elseOutput = null;

                if (ifStmt.Trailing is ParsedStatement elseStmt) {

                    var (checkedElseStmt, checkedElseStmtErr) = TypeCheckStatement(elseStmt, scopeId, project, safetyMode);

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

            case LoopStatement ls: {

                var (checkedBlock, blockErr) = TypeCheckBlock(ls.Block, scopeId, project, safetyMode);

                error = error ?? blockErr;

                return (
                    new CheckedLoopStatement(checkedBlock),
                    error);
            }

            case WhileStatement ws: {

                var (checkedCond, exprErr) = TypeCheckExpression(ws.Expr, scopeId, project, safetyMode, null);
                
                error = error ?? exprErr;

                if (checkedCond.GetNeuType() != Compiler.BoolTypeId) {

                    error = error ?? 
                        new TypeCheckError(
                            "Condition must be a boolean expression",
                            checkedCond.GetSpan());
                }

                var (checkedBlock, blockErr) = TypeCheckBlock(ws.Block, scopeId, project, safetyMode);
                
                error = error ?? blockErr;

                return (
                    new CheckedWhileStatement(checkedCond, checkedBlock), 
                    error);
            }

            case ReturnStatement rs: {

                Int32? _retType = null;

                if (project.CurrentFunctionIndex is Int32 cfi) {

                    _retType = project.Functions[cfi].ReturnType;
                }

                var (output, outputErr) = TypeCheckExpression(
                    rs.Expr, 
                    scopeId, 
                    project, 
                    safetyMode,
                    _retType);

                return (
                    new CheckedReturnStatement(output), 
                    outputErr);
            }

            case BlockStatement bs: {

                var (checkedBlock, checkedBlockErr) = TypeCheckBlock(bs.Block, scopeId, project, safetyMode);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    checkedBlockErr);
            }

            case InlineCPPStatement i: {

                if (safetyMode == SafetyMode.Safe) {

                    return (
                        new CheckedInlineCppStatement(new List<String>()),
                        new TypeCheckError(
                            "Use of inline cpp block outside of unsafe block",
                            i.Span));
                }

                var strings = new List<String>();

                foreach (var statement in i.Block.Statements) {

                    switch (statement) {

                        case ExpressionStatement es when es.Expression is QuotedStringExpression qs: {

                            strings.Add(qs.Value);

                            break;
                        }

                        default: {

                            return (
                                new CheckedInlineCppStatement(new List<String>()),
                                new TypeCheckError(
                                    "Expected block of strings",
                                    i.Span));
                        }
                    }
                }

                return (
                    new CheckedInlineCppStatement(strings),
                    null);
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

    public static (CheckedNumericConstantExpression?, Error?) TryPromoteConstantExprToType(
        Int32 lhsTypeId,
        CheckedExpression checkedRhs,
        Span span) {

        if (!NeuTypeFunctions.IsInteger(lhsTypeId)) {

            return (null, null);
        }

        if (checkedRhs.ToIntegerConstant() is IntegerConstant rhsConstant) {

            var (_newConstant, newType) = rhsConstant.Promote(lhsTypeId);

            if (_newConstant is NumericConstant newConstant) {

                return (
                    new CheckedNumericConstantExpression(newConstant, span, newType), 
                    null);
            }
            else {

                return (
                    null, 
                    new TypeCheckError(
                        "Integer promotion failed",
                        span));
            }
        }

        return (null, null);
    }

    public static (CheckedExpression, Error?) TypeCheckExpression(
        ParsedExpression expr,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode,
        Int32? typeHint) {

        Error? error = null;

        Func<Project, Int32, Int32> unifyWithTypeHint = (project, ty) => {

            if (typeHint is Int32 hint) {

                if (hint == Compiler.UnknownTypeId) {

                    return ty;
                }

                var genericInterface = new Dictionary<Int32, Int32>();

                var err = CheckTypesForCompat(
                    ty,
                    hint,
                    genericInterface,
                    expr.GetSpan(),
                    project);

                // CHECK: is genericInterface mutated correctly here?

                if (err != null) {

                    return ty;
                }

                return SubstituteTypeVarsInType(ty, genericInterface, project);
            }

            return ty;
        };

        switch (expr) {

            case RangeExpression re: {

                var (checkedStart, startErr) = TypeCheckExpression(re.Start, scopeId, project, safetyMode, null);

                error = error ?? startErr;

                var (checkedEnd, endErr) = TypeCheckExpression(re.End, scopeId, project, safetyMode, null);

                error = error ?? endErr;

                // If the range starts or ends at a constant number, we try promoting the constant to the
                // type of the other end. This makes ranges like `0..array.size()` (as the 0 becomes 0uz).

                var (promotedEnd, promoteEndErr) = TryPromoteConstantExprToType(checkedStart.GetNeuType(), checkedEnd, re.Span);

                error = error ?? promoteEndErr;

                if (promotedEnd is not null) {

                    checkedEnd = promotedEnd;
                }

                var (promotedStart, promoteStartErr) = TryPromoteConstantExprToType(checkedEnd.GetNeuType(), checkedStart, re.Span);

                error = error ?? promoteStartErr;

                if (promotedStart is not null) {

                    checkedStart = promotedStart;
                }

                if (checkedStart.GetNeuType() != checkedEnd.GetNeuType()) {

                    error = error ?? 
                        new TypeCheckError(
                            "Range start and end must be the same type",
                            re.Span);
                }

                var rangeStructId = project
                    .FindStructInScope(0, "Range")
                    ?? throw new Exception("internal error: Range builtin definition not found");

                var _ty = new GenericInstance(rangeStructId, new List<Int32>(new [] { checkedStart.GetNeuType() }));

                var ty = project.FindOrAddTypeId(_ty);

                return (
                    new CheckedRangeExpression(
                        checkedStart, 
                        checkedEnd,
                        re.Span,
                        unifyWithTypeHint(project, ty)),
                    error);
            }

            case BinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, scopeId, project, safetyMode, null);

                error = error ?? checkedLhsErr;

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, scopeId, project, safetyMode, null);

                error = error ?? checkedRhsErr;

                var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                    checkedLhs.GetNeuType(), 
                    checkedRhs, 
                    e.Span);

                error = error ?? tryPromoteErr;

                if (promotedExpr is not null) {

                    checkedRhs = promotedExpr;
                }

                // TODO: actually do the binary operator typecheck against safe operations
                // For now, use a type we know
                
                var (ty, chkBinOpErr) = TypeCheckBinaryOperation(checkedLhs, e.Operator, checkedRhs, e.Span);

                error = error ?? chkBinOpErr;

                return (
                    new CheckedBinaryOpExpression(
                        checkedLhs, 
                        e.Operator, 
                        checkedRhs,
                        e.Span,
                        // ty),
                        unifyWithTypeHint(project, ty)),
                    error);
            }

            case UnaryOpExpression u: {

                var (checkedExpr, checkedExprErr) = TypeCheckExpression(u.Expression, scopeId, project, safetyMode, null);

                error = error ?? checkedExprErr;

                CheckedUnaryOperator? checkedOp = null;

                switch (u.Operator) {

                    case PreIncrementUnaryOperator _: {
                        
                        checkedOp = new CheckedPreIncrementUnaryOperator();

                        break;
                    }

                    case PostIncrementUnaryOperator _: {
                        
                        checkedOp = new CheckedPostIncrementUnaryOperator();

                        break;
                    }

                    case PreDecrementUnaryOperator _: {
                        
                        checkedOp = new CheckedPreDecrementUnaryOperator();

                        break;
                    }

                    case PostDecrementUnaryOperator _: {
                        
                        checkedOp = new CheckedPostDecrementUnaryOperator();

                        break;
                    }

                    case NegateUnaryOperator _: {
                        
                        checkedOp = new CheckedNegateUnaryOperator();

                        break;
                    }

                    case DereferenceUnaryOperator _: {
                        
                        checkedOp = new CheckedDereferenceUnaryOperator();

                        break;
                    }

                    case RawAddressUnaryOperator _: {
                        
                        checkedOp = new CheckedRawAddressUnaryOperator();

                        break;
                    }

                    case LogicalNotUnaryOperator _: {
                        
                        checkedOp = new CheckedLogicalNotUnaryOperator();

                        break;
                    }

                    case BitwiseNotUnaryOperator _: {
                        
                        checkedOp = new CheckedBitwiseNotUnaryOperator();

                        break;
                    }

                    case IsUnaryOperator i: {

                        var (isTypeId, isTypeErr) = TypeCheckTypeName(i.Type, scopeId, project);

                        error = error ?? isTypeErr;

                        checkedOp = new CheckedIsUnaryOperator(isTypeId);

                        break;
                    }

                    case TypeCastUnaryOperator tc: {

                        var (typeId, typeErr) = TypeCheckTypeName(tc.TypeCast.GetUncheckedType(), scopeId, project);

                        error = error ?? typeErr;

                        CheckedTypeCast? checkedCast = null;

                        switch (tc.TypeCast) {

                            case FallibleTypeCast _: {

                                checkedCast = new CheckedFallibleTypeCast(typeId);

                                break;
                            }

                            case InfallibleTypeCast _: {

                                checkedCast = new CheckedInfallibleTypeCast(typeId);

                                break;
                            }

                            case SaturatingTypeCast _: {

                                checkedCast = new CheckedSaturatingTypeCast(typeId);

                                break;
                            }

                            case TruncatingTypeCast _: {

                                checkedCast = new CheckedTruncatingTypeCast(typeId);

                                break;
                            }

                            default: {

                                break;
                            }
                        }

                        checkedOp = new CheckedTypeCastUnaryOperator(checkedCast ?? throw new Exception());

                        break;
                    }

                    default: {

                        break;
                    }
                }

                var (_checkedExpr, chkUnaryOpErr) = TypeCheckUnaryOperation(
                    checkedExpr, 
                    checkedOp ?? throw new Exception(), 
                    u.Span, 
                    project, 
                    safetyMode);

                error = error ?? chkUnaryOpErr;

                return (_checkedExpr, error);
            }

            case OptionalNoneExpression e: {

                return (
                    new CheckedOptionalNoneExpression(
                        e.Span,
                        Compiler.UnknownTypeId),
                    error);
            }

            case OptionalSomeExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode, null);

                error = error ?? ckdExprError;

                var type = ckdExpr.GetNeuType();

                return (
                    new CheckedOptionalSomeExpression(ckdExpr, e.Span, type),
                    error);
            }

            case ForcedUnwrapExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode, null);

                error = error ?? ckdExprError;

                var type = project.Types[ckdExpr.GetNeuType()];

                var optionalStructId = project
                    .FindStructInScope(0, "Optional") 
                    ?? throw new Exception("internal error: can't find builtin Optional type");

                var typeId = Compiler.UnknownTypeId;

                switch (type) {

                    case GenericInstance gi when gi.StructId == optionalStructId: {

                        typeId = gi.TypeIds[0];

                        error = null;

                        break;
                    }

                    default: {

                        error = error ?? 
                            new TypeCheckError(
                                "Forced unwrap only works on Optional",
                                e.Expression.GetSpan());

                        break;
                    }
                }

                return (
                    new CheckedForceUnwrapExpression(
                        ckdExpr, 
                        e.Span, 
                        // typeId),
                        unifyWithTypeHint(project, typeId)),
                    error);
            }

            case BooleanExpression e: {

                return (
                    new CheckedBooleanExpression(e.Value, e.Span),
                    null);
            }

            case CallExpression e: {

                var (checkedCall, checkedCallErr) = TypeCheckCall(
                    e.Call, 
                    scopeId, 
                    e.Span, 
                    project, 
                    null, 
                    null, 
                    safetyMode,
                    typeHint);

                var ty = unifyWithTypeHint(project, checkedCall.Type);

                return (
                    new CheckedCallExpression(checkedCall, e.Span, ty),
                    error ?? checkedCallErr);
            }

            case NumericConstantExpression ne: {

                return (
                    new CheckedNumericConstantExpression(
                        ne.Value, 
                        ne.Span, 
                        unifyWithTypeHint(project, ne.Value.GetNeuType())),
                    null);
            }

            case QuotedStringExpression e: {

                return (
                    new CheckedQuotedStringExpression(e.Value, e.Span),
                    null);
            }

            case CharacterLiteralExpression cle: {

                return (
                    new CheckedCharacterConstantExpression(cle.Char, cle.Span),
                    null);
            }

            case VarExpression e: {

                if (project.FindVarInScope(scopeId, e.Value) is CheckedVariable v) {

                    return (
                        new CheckedVarExpression(v, e.Span),
                        null);
                }
                else {
                    
                    return (
                        new CheckedVarExpression(
                            new CheckedVariable(
                                e.Value, 
                                type: typeHint ?? Compiler.UnknownTypeId,
                                mutable: false),
                            e.Span),
                        new TypeCheckError(
                            "variable not found",
                            e.Span));
                }
            }

            case ArrayExpression ve: {

                var innerType = Compiler.UnknownTypeId;

                var output = new List<CheckedExpression>();

                CheckedExpression? checkedFillSizeExpr = null;

                if (ve.FillSize is ParsedExpression fillSize) {

                    var (chkFillSizeExpr, chkFillSizeErr) = TypeCheckExpression(fillSize, scopeId, project, safetyMode, null);

                    checkedFillSizeExpr = chkFillSizeExpr;

                    error = error ?? chkFillSizeErr;
                }

                ///

                foreach (var v in ve.Expressions) {

                    var (checkedExpr, err) = TypeCheckExpression(v, scopeId, project, safetyMode, null);

                    error = error ?? err;

                    if (innerType is Compiler.UnknownTypeId) {

                        innerType = checkedExpr.GetNeuType();
                    }
                    else {

                        if (innerType != checkedExpr.GetNeuType()) {

                            error = error ?? 
                                new TypeCheckError(
                                    "does not match type of previous values in vector",
                                    v.GetSpan());
                        }
                    }

                    output.Add(checkedExpr);
                }

                var arrayStructId = project
                    .FindStructInScope(0, "Array")
                    ?? throw new Exception("internal error: Array builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(arrayStructId, new List<Int32>(new [] { innerType })));

                return (
                    new CheckedArrayExpression(
                        expressions: output,
                        checkedFillSizeExpr,
                        ve.Span,
                        unifyWithTypeHint(project, typeId)),
                    error);
            }

            case SetExpression se: {

                var innerTy = Compiler.UnknownTypeId;

                var output = new List<CheckedExpression>();

                foreach (var value in se.Items) {

                    var (checkedValue, err) = TypeCheckExpression(value, scopeId, project, safetyMode, null);

                    error = error ?? err;

                    if (innerTy == Compiler.UnknownTypeId) {

                        innerTy = checkedValue.GetNeuType();
                    }
                    else if (innerTy != checkedValue.GetNeuType()) {

                        error = error ?? 
                            new TypeCheckError(
                                "does not match type of previous values in set",
                                value.GetSpan());
                    }

                    output.Add(checkedValue);
                }

                var setStructId = project
                    .FindStructInScope(0, "Set")
                    ?? throw new Exception("internal error: Set builtin definition not found");

                var typeId = project
                    .FindOrAddTypeId(new GenericInstance(setStructId, new List<Int32>(new [] { innerTy })));

                return (
                    new CheckedSetExpression(output, se.Span, unifyWithTypeHint(project, typeId)),
                    error);
            }

            case DictionaryExpression de: {

                var innerTy = (Compiler.UnknownTypeId, Compiler.UnknownTypeId);

                var output = new List<(CheckedExpression, CheckedExpression)>();

                foreach (var (key, value) in de.Entries) {

                    var (checkedKey, keyErr) = TypeCheckExpression(key, scopeId, project, safetyMode, null);

                    error = error ?? keyErr;

                    var (checkedValue, valueErr) = TypeCheckExpression(value, scopeId, project, safetyMode, null);

                    error = error ?? valueErr;

                    if (innerTy.Item1 == Compiler.UnknownTypeId
                        && innerTy.Item2 == Compiler.UnknownTypeId) {

                        innerTy = (checkedKey.GetNeuType(), checkedValue.GetNeuType());
                    }
                    else {

                        if (innerTy.Item1 != checkedKey.GetNeuType()) {

                            error = error ??
                                new TypeCheckError(
                                    "does not match type of previous values in dictionary",
                                    key.GetSpan());
                        }

                        if (innerTy.Item2 != checkedValue.GetNeuType()) {

                            error = error ??
                                new TypeCheckError(
                                    "does not match type of previous values in dictionary",
                                    value.GetSpan());
                        }
                    }

                    output.Add((checkedKey, checkedValue));
                }

                var dictStructId = project
                    .FindStructInScope(0, "Dictionary")
                    ?? throw new Exception("internal error: Dictionary builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(dictStructId, new List<Int32>(new [] { innerTy.Item1, innerTy.Item2 })));

                return (
                    new CheckedDictionaryExpression(
                        output, 
                        de.Span, 
                        unifyWithTypeHint(project, typeId)),
                    error);
            }

            case TupleExpression te: {

                var checkedItems = new List<CheckedExpression>();

                var checkedTypes = new List<Int32>();

                foreach (var item in te.Expressions) {

                    var (checkedItemExpr, typeCheckItemExprErr) = TypeCheckExpression(item, scopeId, project, safetyMode, null);

                    error = error ?? typeCheckItemExprErr;

                    checkedTypes.Add(checkedItemExpr.GetNeuType());

                    checkedItems.Add(checkedItemExpr);
                }

                var tupleStructId = project
                    .FindStructInScope(0, "Tuple")
                    ?? throw new Exception("internal error: Tuple builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(tupleStructId, checkedTypes));

                return (
                    new CheckedTupleExpression(
                        checkedItems,
                        te.Span,
                        unifyWithTypeHint(project, typeId)),
                    error);
            }

            case IndexedExpression ie: {

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, scopeId, project, safetyMode, null);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, scopeId, project, safetyMode, null);
            
                error = error ?? typeCheckIdxErr;

                var exprType = Compiler.UnknownTypeId;

                var arrayStructId = project
                    .FindStructInScope(0, "Array")
                    ?? throw new Exception("internal error: Array builtin definition not found");

                var dictStructId = project
                    .FindStructInScope(0, "Dictionary")
                    ?? throw new Exception("internal error: Dictionary builtin definition not found");

                var ty = project.Types[checkedExpr.GetNeuType()];

                switch (ty) {

                    case GenericInstance ga when ga.StructId == arrayStructId: {

                        var _chkIdx = checkedIdx.GetNeuType();

                        switch (true) {

                            case var _ when NeuTypeFunctions.IsInteger(_chkIdx): {

                                exprType = ga.TypeIds[0];

                                break;
                            }

                            default: {

                                error = error ?? 
                                    new TypeCheckError(
                                        "index is not an integer",
                                        ie.Index.GetSpan());

                                break;
                            }
                        }

                        return (
                            new CheckedIndexedExpression(
                                checkedExpr,
                                checkedIdx,
                                ie.Span,
                                unifyWithTypeHint(project, exprType)),
                            error);
                    }

                    case GenericInstance gd when gd.StructId == dictStructId: {

                        var valueTy = gd.TypeIds[1];

                        var optionalStructId = project
                            .FindStructInScope(0, "Optional")
                            ?? throw new Exception("internal error: Optional builtin definition not found");

                        var innerTy = project.FindOrAddTypeId(
                            new GenericInstance(
                                optionalStructId, 
                                new List<Int32>(new [] { valueTy })));

                        exprType = innerTy;

                        return (
                            new CheckedIndexedDictionaryExpression(
                                checkedExpr, 
                                checkedIdx, 
                                ie.Span,
                                unifyWithTypeHint(project, exprType)),
                            error);
                    }

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "index used on value that can't be indexed",
                                ie.Expression.GetSpan());

                        return (
                            new CheckedIndexedExpression(
                                checkedExpr,
                                checkedIdx,
                                ie.Span,
                                unifyWithTypeHint(project, exprType)),
                            error);
                    }
                }
            }

            case IndexedTupleExpression ite: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ite.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                var ty = Compiler.UnknownTypeId;

                var tupleStructId = project
                    .FindStructInScope(0, "Tuple")
                    ?? throw new Exception("internal error: Tuple builtin definition not found");

                var checkedExprTy = project.Types[checkedExpr.GetNeuType()];

                switch (checkedExprTy) {

                    case GenericInstance gi when gi.StructId == tupleStructId: {

                        var idx = ToInt32(ite.Index);

                        switch (true) {

                            case var _ when gi.TypeIds.Count > idx: {

                                ty = gi.TypeIds[idx];

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
                    new CheckedIndexedTupleExpression(
                        checkedExpr, 
                        ite.Index, 
                        ite.Span, 
                        unifyWithTypeHint(project, ty)),
                    error);
            }

            case WhenExpression we: {

                var (checkedExpr, err) = TypeCheckExpression(we.Expression, scopeId, project, safetyMode, null);

                error = error ?? err;

                var checkedCases = new List<CheckedWhenCase>();

                var ty = project.Types[checkedExpr.GetNeuType()];

                var subjectTy = checkedExpr.GetNeuType();

                var genericParams = new Dictionary<Int32, Int32>();

                if (ty is GenericEnumInstance gei) {

                    var _enum = project.Enums[gei.EnumId];

                    if (_enum.GenericParameters.Count != gei.TypeIds.Count) {

                        throw new Exception();
                    }

                    for (var i = 0; i < _enum.GenericParameters.Count; i++) {

                        var p = _enum.GenericParameters[i];
                        var t = gei.TypeIds[i];

                        genericParams[p] = t;
                    }
                }

                // CHECK: Check genericParams is properly constructed

                Int32? finalResultType = null;

                ///

                Int32? tyEnumId = null;

                switch (ty) {

                    case EnumType e: {

                        tyEnumId = e.EnumId;

                        break;
                    }

                    case GenericEnumInstance i: {

                        tyEnumId = i.EnumId;

                        break;
                    }

                    default: {

                        break;
                    }
                }

                ///

                switch (tyEnumId) {

                    case Int32 enumId: {

                        var _enum = project.Enums[enumId];

                        var enumName = _enum.Name;

                        foreach (var c in we.Cases) {

                            switch (c) {

                                case EnumVariantWhenCase evwc: {

                                    var name = evwc.VariantName.ToList();

                                    if (name.Count == 1) {

                                        name.Insert(0, (enumName, name[0].Item2));
                                    }

                                    if (name[0].Item1 != enumName) {

                                        error = err ?? 
                                            new TypeCheckError(
                                                $"when case '{name[0].Item1}' does not match enum '{enumName}'",
                                                name[0].Item2);
                                    }
                                    else {

                                        Int32? variantIndex = null;

                                        var vars = new List<(CheckedVariable, Span)>();

                                        {
                                            var constructorName = name[1].Item1;

                                            var _variant = _enum.Variants.FirstOrDefault(x => {

                                                switch (x) {

                                                    case CheckedWithValueEnumVariant w when w.Name == constructorName:
                                                    case CheckedUntypedEnumVariant u when u.Name == constructorName:
                                                    case CheckedTypedEnumVariant t when t.Name == constructorName:
                                                    case CheckedStructLikeEnumVariant s when s.Name == constructorName:
                                                        return true;

                                                    default:
                                                        return false;
                                                }
                                            });

                                            // if (variant is null) {

                                            //     error = error ??
                                            //         new TypeCheckError(
                                            //             $"when case '{name[0].Item1}' does not match enum '{enumName}'",
                                            //             name[1].Item2);

                                            //     return (
                                            //         new CheckedWhenExpression(
                                            //             checkedExpr,
                                            //             checkedCases,
                                            //             we.Span,
                                            //             // FIXME: Figure this out
                                            //             Compiler.UnknownTypeId),
                                            //         error);
                                            // }
                                            // else {

                                            //     switch (variant) {

                                            //         case CheckedUntypedEnumVariant u: {

                                            //             if (evwc.VariantArguments.Any()) {

                                            //                 error = error ??
                                            //                     new TypeCheckError(
                                            //                         $"when case '{name}' cannot have arguments",
                                            //                         evwc.ArgumentsSpan);
                                            //             }

                                            //             break;
                                            //         }

                                            //         case CheckedTypedEnumVariant t: {

                                            //             if (evwc.VariantArguments.Any()) {

                                            //                 if (evwc.VariantArguments.Count != 1) {

                                            //                     error = error ??
                                            //                         new TypeCheckError(
                                            //                             $"when case '{name}' must have exactly one argument",
                                            //                             evwc.ArgumentsSpan);
                                            //                 }

                                            //                 var varTy = SubstituteTypeVarsInType(
                                            //                     t.TypeId,
                                            //                     genericParams,
                                            //                     project);

                                            //                 vars.Add((
                                            //                     new CheckedVariable(
                                            //                         name: evwc.VariantArguments[0].Item2,
                                            //                         varTy,
                                            //                         mutable: false),
                                            //                     t.Span));
                                            //             }

                                            //             break;
                                            //         }

                                            //         case CheckedWithValueEnumVariant w: {

                                            //             if (evwc.VariantArguments.Any()) {

                                            //                 error = error ?? 
                                            //                     new TypeCheckError(
                                            //                         $"when case '{name}' cannot have arguments",
                                            //                         evwc.ArgumentsSpan);
                                            //             }

                                            //             break;
                                            //         }

                                            //         // default: {

                                            //         //     // TODO

                                            //         //     break;
                                            //         // }

                                            //         case CheckedStructLikeEnumVariant s: {

                                            //             var variantName = s.Name;

                                            //             var fields = s.Decls.ToList();

                                            //             var namesSeen = new HashSet<String>();

                                            //             foreach (var arg in evwc.VariantArguments) {

                                            //                 var argName = arg.Item1;

                                            //                 if (IsNullOrWhiteSpace(argName)) {

                                            //                     error = error ?? 
                                            //                         new TypeCheckError(
                                            //                             $"when case argument '{arg.Item2}' for struct-like enum variant cannot be anonymous",
                                            //                             evwc.ArgumentsSpan);
                                            //                 }
                                            //                 else {

                                            //                     if (namesSeen.Contains(argName)) {

                                            //                         error = error ?? 
                                            //                             new TypeCheckError(
                                            //                                 $"when case argument '{argName}' is already defined",
                                            //                                 evwc.ArgumentsSpan);
                                            //                     }
                                            //                     else {

                                            //                         namesSeen.Add(argName);

                                            //                         var fieldType = fields
                                            //                             .FirstOrDefault(x => x.Name == argName)?
                                            //                             .Type;

                                            //                         if (fieldType is not null) {

                                            //                             fieldType = SubstituteTypeVarsInType(fieldType.Value, genericParams, project);
                                            //                         }

                                            //                         switch (fieldType) {

                                            //                             case Int32 _fieldType: {

                                            //                                 vars.Add((
                                            //                                     new CheckedVariable(
                                            //                                         name: arg.Item2,
                                            //                                         _fieldType,
                                            //                                         mutable: false),
                                            //                                     we.Span));

                                            //                                 break;
                                            //                             }

                                            //                             default: {

                                            //                                 error = error ?? 
                                            //                                     new TypeCheckError(
                                            //                                         $"when case argument '{argName}' does not exist in struct-like enum variant '{s.Name}'",
                                            //                                         evwc.ArgumentsSpan);

                                            //                                 break;
                                            //                             }
                                            //                         }
                                            //                     }
                                            //                 }
                                            //             }

                                            //             break;
                                            //         }

                                            //         default: {

                                            //             // break;

                                            //             throw new Exception();
                                            //         }

                                            //     }
                                            
                                            //     // Look these up again to appease the borrow checker

                                            //     variant = _enum
                                            //         .Variants
                                            //         .FirstOrDefault(x => {

                                            //             switch (x) {

                                            //                 case CheckedWithValueEnumVariant w when w.Name == constructorName:
                                            //                 case CheckedUntypedEnumVariant u when u.Name == constructorName:
                                            //                 case CheckedTypedEnumVariant t when t.Name == constructorName:
                                            //                 case CheckedStructLikeEnumVariant s when s.Name == constructorName:
                                            //                     return true;

                                            //                 default:
                                            //                     return false;
                                            //             }
                                            //         })
                                            //         ?? throw new Exception();

                                            //     for (var i = 0; i < _enum.Variants.Count; i++) {

                                            //         if (_enum.Variants[i] == variant) {

                                            //             variantIndex = i;

                                            //             break;
                                            //         }
                                            //     }
                                            // }

                                            switch (_variant) {

                                                case null: {

                                                    error = error ??
                                                        new TypeCheckError(
                                                            $"when case '{name[0].Item1}' does not match enum '{enumName}'",
                                                            name[1].Item2);

                                                    return (
                                                        new CheckedWhenExpression(
                                                            checkedExpr,
                                                            checkedCases,
                                                            we.Span,
                                                            // FIXME: Figure this out
                                                            Compiler.UnknownTypeId),
                                                        error);
                                                }

                                                case CheckedEnumVariant variant: {

                                                    switch (variant) {

                                                        case CheckedUntypedEnumVariant u: {

                                                            if (evwc.VariantArguments.Any()) {

                                                                error = error ??
                                                                    new TypeCheckError(
                                                                        $"when case '{name}' cannot have arguments",
                                                                        evwc.ArgumentsSpan);
                                                            }

                                                            break;
                                                        }

                                                        case CheckedTypedEnumVariant t: {

                                                            if (evwc.VariantArguments.Any()) {

                                                                if (evwc.VariantArguments.Count != 1) {

                                                                    error = error ??
                                                                        new TypeCheckError(
                                                                            $"when case '{name}' must have exactly one argument",
                                                                            evwc.ArgumentsSpan);
                                                                }

                                                                var varTy = SubstituteTypeVarsInType(
                                                                    t.TypeId,
                                                                    genericParams,
                                                                    project);

                                                                vars.Add((
                                                                    new CheckedVariable(
                                                                        name: evwc.VariantArguments[0].Item2,
                                                                        varTy,
                                                                        mutable: false),
                                                                    t.Span));
                                                            }

                                                            break;
                                                        }

                                                        case CheckedWithValueEnumVariant w: {

                                                            if (evwc.VariantArguments.Any()) {

                                                                error = error ?? 
                                                                    new TypeCheckError(
                                                                        $"when case '{name}' cannot have arguments",
                                                                        evwc.ArgumentsSpan);
                                                            }

                                                            break;
                                                        }

                                                        // default: {

                                                        //     // TODO

                                                        //     break;
                                                        // }

                                                        case CheckedStructLikeEnumVariant s: {

                                                            var variantName = s.Name;

                                                            var fields = s.Decls.ToList();

                                                            var namesSeen = new HashSet<String>();

                                                            foreach (var arg in evwc.VariantArguments) {

                                                                var argName = arg.Item1;

                                                                if (IsNullOrWhiteSpace(argName)) {

                                                                    error = error ?? 
                                                                        new TypeCheckError(
                                                                            $"when case argument '{arg.Item2}' for struct-like enum variant cannot be anonymous",
                                                                            evwc.ArgumentsSpan);
                                                                }
                                                                else {

                                                                    if (namesSeen.Contains(argName)) {

                                                                        error = error ?? 
                                                                            new TypeCheckError(
                                                                                $"when case argument '{argName}' is already defined",
                                                                                evwc.ArgumentsSpan);
                                                                    }
                                                                    else {

                                                                        namesSeen.Add(argName);

                                                                        var fieldType = fields
                                                                            .FirstOrDefault(x => x.Name == argName)?
                                                                            .Type;

                                                                        if (fieldType is not null) {

                                                                            fieldType = SubstituteTypeVarsInType(fieldType.Value, genericParams, project);
                                                                        }

                                                                        switch (fieldType) {

                                                                            case Int32 _fieldType: {

                                                                                vars.Add((
                                                                                    new CheckedVariable(
                                                                                        name: arg.Item2,
                                                                                        _fieldType,
                                                                                        mutable: false),
                                                                                    we.Span));

                                                                                break;
                                                                            }

                                                                            default: {

                                                                                error = error ?? 
                                                                                    new TypeCheckError(
                                                                                        $"when case argument '{argName}' does not exist in struct-like enum variant '{s.Name}'",
                                                                                        evwc.ArgumentsSpan);

                                                                                break;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            break;
                                                        }

                                                        default: {

                                                            // break;

                                                            throw new Exception();
                                                        }

                                                    }
                                                
                                                    // Look these up again to appease the borrow checker

                                                    variant = _enum
                                                        .Variants
                                                        .FirstOrDefault(x => {

                                                            switch (x) {

                                                                case CheckedWithValueEnumVariant w when w.Name == constructorName:
                                                                case CheckedUntypedEnumVariant u when u.Name == constructorName:
                                                                case CheckedTypedEnumVariant t when t.Name == constructorName:
                                                                case CheckedStructLikeEnumVariant s when s.Name == constructorName:
                                                                    return true;

                                                                default:
                                                                    return false;
                                                            }
                                                        })
                                                        ?? throw new Exception();

                                                    for (var i = 0; i < _enum.Variants.Count; i++) {

                                                        if (_enum.Variants[i] == variant) {

                                                            variantIndex = i;

                                                            break;
                                                        }
                                                    }

                                                    break;
                                                }

                                                default: {

                                                    throw new Exception();
                                                }
                                            }
                                        }

                                        var newScopeId = project.CreateScope(scopeId);

                                        foreach (var (v, span) in vars) {

                                            if (project.AddVarToScope(newScopeId, v, span).Error is Error e) {

                                                error = error ?? e;
                                            }
                                        }

                                        switch (evwc.Body) {

                                            case ExpressionWhenBody ewb: {

                                                var (body, exprErr) = TypeCheckExpression(
                                                    ewb.Expression,
                                                    newScopeId,
                                                    project,
                                                    safetyMode,
                                                    null);

                                                error = error ?? exprErr;

                                                switch (finalResultType) {

                                                    case Int32 _frt: {

                                                        if (CheckTypesForCompat(
                                                            body.GetNeuType(),
                                                            _frt,
                                                            genericParams,
                                                            we.Span,
                                                            project) is Error e) {

                                                            error = error ?? e;
                                                        }

                                                        break;
                                                    }

                                                    default: {

                                                        finalResultType = body.GetNeuType();

                                                        break;
                                                    }
                                                }

                                                checkedCases.Add(
                                                    new CheckedEnumVariantWhenCase(
                                                        variantName: name[1].Item1,
                                                        variantArguments: evwc.VariantArguments,
                                                        subjectTypeId: subjectTy,
                                                        variantIndex ?? throw new Exception(),
                                                        scopeId: newScopeId,
                                                        body: new CheckedExpressionWhenBody(body)));

                                                break;
                                            }

                                            case BlockWhenBody b: {

                                                var (body, blockErr) = TypeCheckBlock(
                                                    b.Block,
                                                    newScopeId,
                                                    project,
                                                    safetyMode);

                                                error = error ?? blockErr;

                                                switch (finalResultType) {

                                                    case Int32 _frt: {

                                                        if (CheckTypesForCompat(
                                                            Compiler.VoidTypeId,
                                                            _frt,
                                                            genericParams,
                                                            we.Span,
                                                            project) is Error compatErr) {

                                                            error = error ?? compatErr;
                                                        }

                                                        break;
                                                    }

                                                    default: {

                                                        finalResultType = Compiler.VoidTypeId;

                                                        break;
                                                    }
                                                }

                                                checkedCases.Add(
                                                    new CheckedEnumVariantWhenCase(
                                                        variantName: name[1].Item1,
                                                        variantArguments: evwc.VariantArguments,
                                                        subjectTypeId: subjectTy,
                                                        variantIndex ?? throw new Exception(),
                                                        scopeId: newScopeId,
                                                        body: new CheckedBlockWhenBody(body)));

                                                break;
                                            }

                                            default: {

                                                throw new Exception();
                                            }
                                        }
                                    }

                                    break;
                                }

                                default: {

                                    throw new Exception();
                                }
                            }
                        }

                        break;
                    }

                    default: {

                        error = error ?? 
                            new TypeCheckError(
                                $"when used on non-enum value (nyi: {project.Types[subjectTy]})",
                                we.Expression.GetSpan());

                        break;
                    }
                }

                if (finalResultType is Int32 frt) {

                    finalResultType = unifyWithTypeHint(project, frt);
                }

                return (
                    new CheckedWhenExpression(
                        checkedExpr,
                        checkedCases,
                        we.Span,
                        finalResultType ?? Compiler.VoidTypeId),
                    error);
            }

            case IndexedStructExpression ise: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ise.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                var ty = Compiler.UnknownTypeId;

                var checkedExprTy = project.Types[checkedExpr.GetNeuType()];

                switch (checkedExprTy) {

                    case GenericInstance gi: {

                        var structure = project.Structs[gi.StructId];

                        foreach (var member in structure.Fields) {

                            if (member.Name == ise.Name) {

                                return (
                                    new CheckedIndexedStructExpression(
                                        checkedExpr,
                                        ise.Name,
                                        ise.Span,
                                        member.Type),
                                    null);
                            }
                        }

                        error = error ?? 
                            new TypeCheckError(
                                $"unknown member of struct: {structure.Name}.{ise.Name}",
                                ise.Span);

                        break;
                    }

                    case StructType st: {

                        var structure = project.Structs[st.StructId];

                        foreach (var member in structure.Fields) {

                            if (member.Name == ise.Name) {

                                return (
                                    new CheckedIndexedStructExpression(
                                        checkedExpr,
                                        ise.Name,
                                        ise.Span,
                                        member.Type),
                                    null);
                            }
                        }

                        error = error ?? 
                            new TypeCheckError(
                                $"unknown member of struct: {structure.Name}.{ise.Name}",
                                ise.Span);

                        break;
                    }

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "member access of non-struct value",
                                ise.Span);

                        break;
                    }
                }

                return (
                    new CheckedIndexedStructExpression(
                        checkedExpr, 
                        ise.Name, 
                        ise.Span, 
                        unifyWithTypeHint(project, ty)),
                    error);
            }

            case MethodCallExpression mce: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(mce.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                if (checkedExpr.GetNeuType() == Compiler.StringTypeId) {

                    // Special-case the built-in so we don't accidentally find the user's definition

                    var stringStruct = project.FindStructInScope(0, "String");

                    switch (stringStruct) {

                        case Int32 structId: {

                            var (checkedCall, err) = TypeCheckCall(
                                mce.Call,
                                scopeId,
                                mce.Span,
                                project,
                                checkedExpr,
                                structId,
                                safetyMode,
                                typeHint);

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifyWithTypeHint(project, checkedCall.Type)),
                                error);
                        }

                        default: {

                            error = error ?? 
                                new TypeCheckError(
                                    "no methods available on value",
                                    mce.Expression.GetSpan());

                            return (
                                new CheckedGarbageExpression(mce.Span), 
                                error);
                        }
                    }
                }
                else {

                    var checkedExprTy = project.Types[checkedExpr.GetNeuType()];

                    switch (checkedExprTy) {

                        case StructType st: {

                            var (checkedCall, err) = TypeCheckCall(
                                mce.Call, 
                                scopeId, 
                                mce.Span, 
                                project,
                                checkedExpr,
                                st.StructId, 
                                safetyMode,
                                typeHint);

                            error = error ?? err;

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifyWithTypeHint(project, checkedCall.Type)),
                                error);
                        }

                        case GenericInstance gi: {

                            // ignore the inner types for now, but we'll need them in the future

                            var (checkedCall, err) = TypeCheckCall(
                                mce.Call,
                                scopeId,
                                mce.Span,
                                project,
                                checkedExpr,
                                gi.StructId,
                                safetyMode,
                                typeHint);

                            error = error ?? err;

                            var ty = checkedCall.Type;

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifyWithTypeHint(project, ty)),
                                error);
                        }

                        default: {

                            error = error ??
                                new TypeCheckError(
                                    "no methods available on value",
                                    mce.Expression.GetSpan());

                            return (
                                new CheckedGarbageExpression(mce.Span),
                                error);
                        }
                    }
                }
            }

            case OperatorExpression e: {

                return (
                    new CheckedGarbageExpression(e.Span),
                    new TypeCheckError(
                        "garbage in expression", 
                        e.Span));
            }

            case GarbageExpression e: {

                return (
                    new CheckedGarbageExpression(e.Span),
                    new TypeCheckError(
                        "garbage in expression",
                        e.Span));
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (CheckedExpression, Error?) TypeCheckUnaryOperation(
        CheckedExpression expr,
        CheckedUnaryOperator op,
        Span span,
        Project project,
        SafetyMode safetyMode) {
    
        var exprTypeId = expr.GetNeuType();

        var exprType = project.Types[exprTypeId];

        switch (op) {

            case CheckedIsUnaryOperator i: {

                return (
                    new CheckedUnaryOpExpression(expr, new CheckedIsUnaryOperator(i.TypeId), span, Compiler.BoolTypeId),
                    null);
            }
            
            case CheckedTypeCastUnaryOperator tc: {

                return (
                    new CheckedUnaryOpExpression(expr, op, span, tc.TypeCast.GetNeuType()),
                    null);
            }

            case CheckedDereferenceUnaryOperator _: {

                switch (exprType) {

                    case RawPointerType rp: {

                        if (safetyMode == SafetyMode.Unsafe) {

                            return (
                                new CheckedUnaryOpExpression(expr, op, span, rp.TypeId),
                                null);
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, op, span, rp.TypeId),
                                new TypeCheckError(
                                    "dereference of raw pointer outside of unsafe block",
                                    span));
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, span, Compiler.UnknownTypeId),
                            new TypeCheckError(
                                "dereference of a non-pointer value",
                                span));
                    }
                }
            }

            case CheckedRawAddressUnaryOperator _: {

                var typeId = project.FindOrAddTypeId(new RawPointerType(exprTypeId));

                return (
                    new CheckedUnaryOpExpression(expr, op, span, typeId),
                    null);
            }

            case CheckedLogicalNotUnaryOperator _: {

                return (
                    new CheckedUnaryOpExpression(expr, new CheckedLogicalNotUnaryOperator(), span, exprTypeId),
                    null);
            }

            case CheckedBitwiseNotUnaryOperator _: {

                return (new CheckedUnaryOpExpression(expr, new CheckedBitwiseNotUnaryOperator(), span, exprTypeId), null);
            }

            case CheckedNegateUnaryOperator _: {

                switch (exprTypeId) {

                    case Compiler.Int8TypeId:
                    case Compiler.Int16TypeId:
                    case Compiler.Int32TypeId:
                    case Compiler.Int64TypeId:
                    case Compiler.UInt8TypeId:
                    case Compiler.UInt16TypeId:
                    case Compiler.UInt32TypeId:
                    case Compiler.UInt64TypeId:
                    case Compiler.FloatTypeId:
                    case Compiler.DoubleTypeId: {

                        return (
                            new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), span, exprTypeId),
                            null);
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), span, exprTypeId),
                            new TypeCheckError(
                                "negate on non-numeric value",
                                span));
                    }
                }
            }

            case CheckedPostDecrementUnaryOperator _:
            case CheckedPostIncrementUnaryOperator _:
            case CheckedPreDecrementUnaryOperator _:
            case CheckedPreIncrementUnaryOperator _: {

                switch (exprTypeId) {

                    case Compiler.Int8TypeId:
                    case Compiler.Int16TypeId:
                    case Compiler.Int32TypeId:
                    case Compiler.Int64TypeId:
                    case Compiler.UInt8TypeId:
                    case Compiler.UInt16TypeId:
                    case Compiler.UInt32TypeId:
                    case Compiler.UInt64TypeId:
                    case Compiler.FloatTypeId:
                    case Compiler.DoubleTypeId: {

                        if (!expr.IsMutable()) {

                            return (
                                new CheckedUnaryOpExpression(expr, op, span, exprTypeId),
                                new TypeCheckError(
                                    "increment/decrement of immutable variable",
                                    span));
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, op, span, exprTypeId),
                                null);
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, span, exprTypeId),
                            new TypeCheckError(
                                "unary operation on non-numeric value",
                                span)
                        );
                    }
                }
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (Int32, Error?) TypeCheckBinaryOperation(
        CheckedExpression lhs,
        BinaryOperator op,
        CheckedExpression rhs,
        Span span) {

        var ty = lhs.GetNeuType();

        switch (op) {

            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr:
            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual: {

                ty = Compiler.BoolTypeId;

                break;
            }

            case BinaryOperator.Assign:
            case BinaryOperator.AddAssign:
            case BinaryOperator.SubtractAssign:
            case BinaryOperator.MultiplyAssign:
            case BinaryOperator.DivideAssign:    
            case BinaryOperator.BitwiseAndAssign:
            case BinaryOperator.BitwiseOrAssign:
            case BinaryOperator.BitwiseXorAssign:
            case BinaryOperator.BitwiseLeftShiftAssign:
            case BinaryOperator.BitwiseRightShiftAssign: {

                var lhsTy = lhs.GetNeuType();
                var rhsTy = rhs.GetNeuType();

                if (lhsTy != rhsTy) {

                    return (
                        lhsTy,
                        new TypeCheckError(
                            $"assignment between incompatible types ({lhsTy} and {rhsTy})",
                            span));
                }

                if (!lhs.IsMutable()) {

                    return (
                        lhsTy, 
                        new TypeCheckError(
                            "assignment to immutable variable", 
                            span));
                }

                break;
            }

            default: {

                break;
            }
        }

        return (ty, null);
    }

    public static (CheckedFunction?, DefinitionType?, Error?) ResolveCall(
        ParsedCall call,
        List<ResolvedNamespace> namespaces,
        Span span,
        Int32 scopeId,
        Project project) {

        CheckedFunction? callee = null;

        DefinitionType? definitionType = null;
        
        Error? error = null;

        if (call.Namespace.FirstOrDefault() is String ns) {

            // For now, assume class is our namespace
            // In the future, we'll have real namespaces

            if (project.FindStructInScope(scopeId, ns) is Int32 structId) {

                var structure = project.Structs[structId];

                definitionType = structure.DefinitionType;

                // Look for the constructor

                if (project.FindStructInScope(structure.ScopeId, call.Name) is Int32 _structId) {

                    var _structure = project.Structs[_structId];

                    if (project.FindFuncInScope(_structure.ScopeId, call.Name) is Int32 _funcId) {

                        callee = project.Functions[_funcId];
                    }
                }
                else if (project.FindFuncInScope(structure.ScopeId, call.Name) is Int32 funcId1) {

                    callee = project.Functions[funcId1];
                }

                if (structure.GenericParameters.Any()) {

                    namespaces[0].GenericParameters = structure.GenericParameters;
                }

                return (callee, definitionType, error);
            }
            else if (project.FindEnumInScope(scopeId, ns) is Int32 enumId) {
                
                var _enum = project.Enums[enumId];

                if (project.FindFuncInScope(_enum.ScopeId, call.Name) is Int32 funcId) {

                    callee = project.Functions[funcId];
                }

                if (_enum.GenericParameters.Any()) {

                    namespaces[0].GenericParameters = _enum.GenericParameters;
                }

                return (callee, definitionType, error);
            }
            else if (project.FindFuncInScope(scopeId, call.Name) is Int32 funcId2) {

                callee = project.Functions[funcId2];

                definitionType = DefinitionType.Struct;

                return (callee, definitionType, error);
            }
            else {

                error = error ?? 
                    new TypeCheckError(
                        $"unknown namespace or class: {ns}",
                        span);

                return (callee, definitionType, error);
            }
        }
        else {

            // FIXME: Support function overloading.

            // Look for the constructor

            if (project.FindStructInScope(scopeId, call.Name) is Int32 _structId) {

                var _structure = project.Structs[_structId];

                if (project.FindFuncInScope(_structure.ScopeId, call.Name) is Int32 _funcId) {

                    callee = project.Functions[_funcId];
                }
            }
            else if (project.FindFuncInScope(scopeId, call.Name) is Int32 funcId3) {

                callee = project.Functions[funcId3];
            }
            
            if (callee == null) {

                error = error ?? 
                    new TypeCheckError(
                        $"call to unknown function: {call.Name}", 
                        span);
            }

            return (callee, definitionType, error);
        }
    }

    public static (CheckedCall, Error?) TypeCheckCall(
        ParsedCall call, 
        Int32 callerScopeId,
        Span span,
        Project project,
        CheckedExpression? thisExpr,
        Int32? structId,
        SafetyMode safetyMode,
        Int32? typeHint) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        DefinitionType? calleDefType = null;

        var returnType = Compiler.UnknownTypeId;

        var linkage = FunctionLinkage.Internal;

        var genericSubstitutions = new Dictionary<Int32, Int32>();

        var typeArgs = new List<Int32>();

        var calleeThrows = false;

        var resolvedNamespaces = call
            .Namespace
            .Select(x => 
                new ResolvedNamespace(
                    name: x, 
                    genericParameters: null))
            .ToList();

        var calleeScopeId = structId switch {
            
            Int32 s => project.Structs[s].ScopeId,
            _ => callerScopeId
        };

        switch (call.Name) {

            case "printLine" when structId == null:
            case "warnLine" when structId == null: {

                // FIXME: This is a hack since printLine() and warnLine() are hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, callerScopeId, project, safetyMode, null);

                    error = error ?? checkedArgErr;

                    var resultTy = SubstituteTypeVarsInType(checkedArg.GetNeuType(), genericSubstitutions, project);

                    if (resultTy == Compiler.VoidTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "printLine/warnLine can't take void values",
                                span);
                    }

                    returnType = Compiler.VoidTypeId;

                    checkedArgs.Add((arg.Item1, checkedArg));
                }

                break;
            }

            ///

            default: {

                var (callee, _calleDefType, resolveErr) = ResolveCall(
                    call,
                    resolvedNamespaces,
                    span,
                    calleeScopeId, 
                    project);

                error = error ?? resolveErr;

                calleDefType = _calleDefType;

                if (callee != null) {

                    calleeThrows = callee.Throws;

                    returnType = callee.ReturnType;
                    
                    linkage = callee.Linkage;

                    // If the user gave us explicit type arguments, let's use them in our substitutions

                    for (var idx = 0; idx < call.TypeArgs.Count; idx++) {

                        var typeArg = call.TypeArgs[idx];

                        var (checkedTypeArg, argErr) = TypeCheckTypeName(typeArg, callerScopeId, project);

                        error = error ?? argErr;

                        // Find the associated type variable for this parameter, we'll use it in substitution

                        Int32? _typeVarTypeId = null;

                        switch (callee.GenericParameters[idx]) {

                            case InferenceGuideFunctionGenericParameter i: {

                                _typeVarTypeId = i.TypeId;

                                break;
                            }

                            case ParameterFunctionGenericParameter p: {

                                _typeVarTypeId = p.TypeId;

                                break;
                            }

                            default: {

                                throw new Exception();
                            }
                        }

                        var typeVarTypeId = _typeVarTypeId ?? throw new Exception();

                        genericSubstitutions[typeVarTypeId] = checkedTypeArg;
                    }

                    // If this is a method, let's also add the types we know from our 'this' pointer

                    if (thisExpr is CheckedExpression _thisExpr) {

                        var typeId = _thisExpr.GetNeuType();

                        var paramType = project.Types[typeId];

                        if (paramType is GenericInstance gi) {

                            var structure = project.Structs[gi.StructId];

                            var idx = 0;

                            while (idx < structure.GenericParameters.Count) {

                                genericSubstitutions[structure.GenericParameters[idx]] = gi.TypeIds[idx];

                                idx += 1;
                            }
                        }
                    }

                    // Make sure that our call doesn't have a 'this' pointer to a static callee

                    if (thisExpr is not null && callee.IsStatic()) {

                        error = error ?? 
                            new TypeCheckError(
                                "Cannot call static method on an instance of an object",
                                span);
                    }

                    // This will be 0 for functions or 1 for instance methods, because of the
                    // 'this' ptr

                    var argOffset = thisExpr != null ? 1 : 0;

                    // Check that we have the right number of arguments

                    if (callee.Parameters.Count != (call.Args.Count + argOffset)) {

                        error = error ?? new ParserError(
                            "wrong number of arguments", 
                            span);
                    }
                    else {

                        var idx = 0;

                        while (idx < call.Args.Count) {

                            var (checkedArg, checkedArgErr) = TypeCheckExpression(
                                call.Args[idx].Item2, 
                                callerScopeId, 
                                project, 
                                safetyMode,
                                null);

                            error = error ?? checkedArgErr;

                            var (_callee, _, _) = ResolveCall(
                                call, 
                                resolvedNamespaces,
                                span, 
                                calleeScopeId, 
                                project); // need to do something with defType here?

                            callee = _callee ??
                                throw new Exception("internal error: previously resolved call is now unresolved");

                            if (call.Args[idx].Item2 is VarExpression ve) {

                                if (ve.Value != callee.Parameters[idx + argOffset].Variable.Name
                                    && callee.Parameters[idx + argOffset].RequiresLabel
                                    && call.Args[idx].Item1 != callee.Parameters[idx + argOffset].Variable.Name) {

                                    error = error ?? 
                                        new TypeCheckError(
                                            "Wrong parameter name in argument label",
                                            call.Args[idx].Item2.GetSpan());
                                }
                            }
                            else if (callee.Parameters[idx + argOffset].RequiresLabel
                                && call.Args[idx].Item1 != callee.Parameters[idx + argOffset].Variable.Name) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Wrong parameter name in argument label",
                                        call.Args[idx].Item2.GetSpan());
                            }

                            var lhsTypeId = callee.Parameters[idx + argOffset].Variable.Type;

                            var (promoted, promoteErr) = TryPromoteConstantExprToType(lhsTypeId, checkedArg, span);

                            error = error ?? promoteErr;

                            lhsTypeId = callee.Parameters[idx + argOffset].Variable.Type;

                            if (promoted is not null) {

                                checkedArg = promoted;
                            }

                            var rhsTypeId = checkedArg.GetNeuType();

                            if (CheckTypesForCompat(
                                lhsTypeId, 
                                rhsTypeId,
                                genericSubstitutions, 
                                call.Args[idx].Item2.GetSpan(), 
                                project) is Error compatErr1) {

                                error = error ?? compatErr1;
                            }

                            checkedArgs.Add((call.Args[idx].Item1, checkedArg));

                            idx += 1;
                        }
                    }

                    // We've now seen all the arguments and should be able to substitute the return type, if it's contains a
                    // type variable. For the moment, we'll just checked to see if it's a type variable.

                    if (typeHint is Int32 th) {

                        CheckTypesForCompat(
                            returnType,
                            th,
                            genericSubstitutions,
                            span,
                            project);
                    }

                    returnType = SubstituteTypeVarsInType(returnType, genericSubstitutions, project);

                    resolvedNamespaces = resolvedNamespaces
                        .Select(n =>
                            new ResolvedNamespace(
                                name: n.Name,
                                genericParameters: 
                                    n
                                    .GenericParameters?
                                    .Select(ty => {
                                        return SubstituteTypeVarsInType(ty, genericSubstitutions, project);
                                    })
                                    .ToList()))
                        .ToList();

                    foreach (var genericTypeVar in callee.GenericParameters) {

                        if (genericTypeVar is ParameterFunctionGenericParameter p) {

                            if (genericSubstitutions.ContainsKey(p.TypeId)) {

                                typeArgs.Add(genericSubstitutions[p.TypeId]);
                            }
                            else {

                                error = error ??
                                    new TypeCheckError(
                                        "not all generic parameters have known types",
                                        span);
                            }
                        }
                    }
                }

                break;
            }
        }

        return (
            new CheckedCall(
                ns: resolvedNamespaces,
                call.Name, 
                calleeThrows,
                checkedArgs,
                typeArgs,
                linkage,
                returnType,
                calleDefType),
            error);
    }

    public static Int32 SubstituteTypeVarsInType(
        Int32 typeId,
        Dictionary<Int32, Int32> genericInferences,
        Project project) {

        var result = SubstituteTypeVarsInTypeHelper(typeId, genericInferences, project);

        var cont = true;

        while (cont) {

            var fixedPoint = SubstituteTypeVarsInTypeHelper(typeId, genericInferences, project);

            if (fixedPoint == result) {

                cont = false;

                break;
            }
            else {

                result = fixedPoint;
            }
        }

        return result;
    }

    public static Int32 SubstituteTypeVarsInTypeHelper(
        Int32 typeId,
        Dictionary<Int32, Int32> genericInferences,
        Project project) {

        var ty = project.Types[typeId];

        switch (ty) {

            case TypeVariable _: {

                if (genericInferences.ContainsKey(typeId)) {

                    return genericInferences[typeId];
                }

                break;
            }

            case GenericInstance gi: {

                var newArgs = gi.TypeIds.ToList();

                for (var i = 0; i < newArgs.Count; i++) {

                    newArgs[i] = SubstituteTypeVarsInType(newArgs[i], genericInferences, project);
                }

                return project.FindOrAddTypeId(new GenericInstance(gi.StructId, newArgs));
            }

            case GenericEnumInstance gei: {

                var newArgs = gei.TypeIds.ToList();

                for (var i = 0; i < newArgs.Count; i++) {

                    newArgs[i] = SubstituteTypeVarsInType(newArgs[i], genericInferences, project);
                }

                return project.FindOrAddTypeId(new GenericEnumInstance(gei.EnumId, newArgs));
            }

            case StructType st: {

                var structure = project.Structs[st.StructId];

                if (structure.GenericParameters.Any()) {

                    var newArgs = structure.GenericParameters.ToList();

                    for (var i = 0; i < newArgs.Count; i++) {

                        newArgs[i] = SubstituteTypeVarsInType(newArgs[i], genericInferences, project);
                    }

                    return project.FindOrAddTypeId(new GenericInstance(st.StructId, newArgs));
                }

                break;
            }

            case EnumType e: {

                var _enum = project.Enums[e.EnumId];

                if (_enum.GenericParameters.Any()) {

                    var newArgs = _enum.GenericParameters.ToList();

                    for (var i = 0; i < newArgs.Count; i++) {

                        newArgs[i] = SubstituteTypeVarsInType(newArgs[i], genericInferences, project);
                    }

                    return project.FindOrAddTypeId(new GenericEnumInstance(e.EnumId, newArgs));
                }

                break;
            }

            default: {

                break;
            }
        }

        return typeId;
    }

    public static Error? CheckTypesForCompat(
        Int32 lhsTypeId,
        Int32 rhsTypeId,
        Dictionary<Int32, Int32> genericInferences,
        Span span,
        Project project) {

        Error? error = null;

        var lhsType = project.Types[lhsTypeId];

        switch (lhsType) {

            case TypeVariable _: {

                // If the call expects a generic type variable, let's see if we've already seen it
                
                if (genericInferences.ContainsKey(lhsTypeId)) {

                    // We've seen this type variable assigned something before
                    // we should error if it's incompatible.

                    if (rhsTypeId != genericInferences[lhsTypeId]) {

                        error = error ?? 
                            new TypeCheckError(
                                $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(genericInferences[lhsTypeId], project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                                span);   
                    }
                }
                else {

                    // We haven't seen this type variable before, so go ahead
                    // and give it an actual type during this call

                    genericInferences[lhsTypeId] = rhsTypeId;
                }

                break;
            }

            case GenericEnumInstance gei: {

                var lhsArgs = gei.TypeIds.ToList();

                var rhsType = project.Types[rhsTypeId];

                switch (rhsType) {

                    case GenericEnumInstance innerEnumInstance: {

                        var rhsArgs = innerEnumInstance.TypeIds.ToList();

                        if (gei.EnumId == innerEnumInstance.EnumId) {

                            // Same enum, so check the generic arguments

                            var lhsEnum = project.Enums[gei.EnumId];

                            if (rhsArgs.Count != rhsArgs.Count) {

                                return new TypeCheckError(
                                    $"mismatched number of generic parameters for {lhsEnum.Name}",
                                    span);
                            }

                            var idx = 0;

                            while (idx < lhsArgs.Count) {

                                var lhsArgTypeId = lhsArgs[idx];
                                var rhsArgTypeId = rhsArgs[idx];

                                if (CheckTypesForCompat(
                                    lhsArgTypeId,
                                    rhsArgTypeId,
                                    genericInferences,
                                    span,
                                    project) is Error err) {

                                    return err;
                                }

                                idx += 1;
                            }
                        }

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {

                            // They're the same type, might be okay to just leave now

                            error = error ?? 
                                new TypeCheckError(
                                    $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(lhsTypeId, project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                                    span);
                        }

                        break;
                    }
                }

                break;
            }

            case GenericInstance gi: {

                var lhsArgs = gi.TypeIds.ToList();

                var rhsType = project.Types[rhsTypeId];

                switch (rhsType) {

                    case GenericInstance rhsGi: {

                        if (gi.StructId == rhsGi.StructId) {

                            var rhsArgs = rhsGi.TypeIds.ToList();

                            // Same struct, perhaps this is an instantiation of it

                            var lhsStruct = project.Structs[gi.StructId];

                            if (rhsArgs.Count != gi.TypeIds.Count) {

                                return new TypeCheckError(
                                    $"mismatched number of generic parameters for {lhsStruct.Name}",
                                    span);
                            }

                            var idx = 0;

                            while (idx < gi.TypeIds.Count) {

                                var lhsArgTypeId = lhsArgs[idx];
                                var rhsArgTypeId = rhsArgs[idx];

                                if (CheckTypesForCompat(
                                    lhsArgTypeId, 
                                    rhsArgTypeId, 
                                    genericInferences, 
                                    span, 
                                    project) is Error e2) {

                                    return e2;
                                }

                                idx += 1;
                            }
                        }

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {

                            // They're the same type, might be okay to just leave now
                            
                            error = error ??
                                new TypeCheckError(
                                    $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(lhsTypeId, project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                                    span);
                        }

                        break;
                    }
                }

                break;
            }

            case EnumType enumType: {

                if (rhsTypeId == lhsTypeId) {

                    // They're the same type, might be okay to just leave now
                    
                    return null;
                }

                var rhsType = project.Types[rhsTypeId];

                switch (rhsType) {

                    case GenericEnumInstance gei: {

                        var rhsArgs = gei.TypeIds.ToList();

                        if (enumType.EnumId == gei.EnumId) {

                            var lhsEnum = project.Enums[enumType.EnumId];

                            if (rhsArgs.Count != lhsEnum.GenericParameters.Count) {

                                return new TypeCheckError(
                                    $"mismatched number of generic parameters for {lhsEnum.Name}",
                                    span);
                            }

                            var lhsEnumGenericParams = lhsEnum.GenericParameters.ToList();

                            var idx = 0;

                            while (idx < rhsArgs.Count) {

                                var lhsArgTypeId = lhsEnumGenericParams[idx];

                                var rhsArgTypeId = rhsArgs[idx];

                                if (CheckTypesForCompat(
                                    lhsArgTypeId,
                                    rhsArgTypeId,
                                    genericInferences,
                                    span,
                                    project) is Error e) {

                                    return e;
                                }

                                idx += 1;
                            }
                        }

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {

                            // They're the same type, might be okay to just leave now
                            
                            error = error ??
                                new TypeCheckError(
                                    $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(lhsTypeId, project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                                    span);
                        }

                        break;
                    }
                }

                break;
            }

            case StructType st: {

                if (rhsTypeId == lhsTypeId) {

                    // They're the same type, might be okay to just leave now

                    return null;
                }

                var rhsType = project.Types[rhsTypeId];

                switch (rhsType) {

                    case GenericInstance gi: {

                        if (st.StructId == gi.StructId) {

                            var args = gi.TypeIds.ToList();

                            // Same struct, perhaps this is an instantiation of it

                            var lhsStruct = project.Structs[st.StructId];

                            if (args.Count != lhsStruct.GenericParameters.Count) {

                                return new TypeCheckError(
                                    $"mismatched number of generic parameters for {lhsStruct.Name}",
                                    span);
                            }

                            var idx = 0;

                            var lhsArgTypeId = lhsStruct.GenericParameters[idx];
                            var rhsArgTypeId = args[idx];

                            while (idx < args.Count) {

                                if (CheckTypesForCompat(lhsArgTypeId, rhsArgTypeId, genericInferences, span, project) is Error e3) {

                                    return e3;
                                }

                                idx += 1;
                            }
                        }

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {
                            
                            // They're the same type, might be okay to just leave now
                            
                            error = error ??
                                new TypeCheckError(
                                    $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(lhsTypeId, project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                                    span);
                        }

                        break;
                    }
                }

                break;
            }

            default: {

                if (rhsTypeId != lhsTypeId) {

                    error = error ?? 
                        new TypeCheckError(
                            $"Parameter type mismatch: {CodeGenFunctions.CodeGenType(lhsTypeId, project)} vs {CodeGenFunctions.CodeGenType(rhsTypeId, project)}",
                            span);
                }

                break;
            }
        }

        return error;
    }

    public static (Int32, Error?) TypeCheckTypeName(
        ParsedType uncheckedType,
        Int32 scopeId,
        Project project) {

        Error? error = null;

        switch (uncheckedType) {

            case ParsedNameType nt: {

                switch (nt.Name) {

                    case "Int8": {

                        return (Compiler.Int8TypeId, null);
                    }
                    
                    case "Int16": {

                        return (Compiler.Int16TypeId, null);
                    }

                    case "Int32": {

                        return (Compiler.Int32TypeId, null);
                    }

                    case "Int64": {

                        return (Compiler.Int64TypeId, null);
                    }

                    case "UInt8": {

                        return (Compiler.UInt8TypeId, null);
                    }
                    
                    case "UInt16": {

                        return (Compiler.UInt16TypeId, null);
                    }

                    case "UInt32": {

                        return (Compiler.UInt32TypeId, null);
                    }
                    
                    case "UInt64": {

                        return (Compiler.UInt64TypeId, null);
                    }

                    case "Float": {

                        return (Compiler.FloatTypeId, null);
                    }
                    
                    case "Double": {

                        return (Compiler.DoubleTypeId, null);
                    }

                    case "CChar": {

                        return (Compiler.CCharTypeId, null);
                    }

                    case "CInt": {

                        return (Compiler.CIntTypeId, null);
                    }

                    case "Int": {

                        return (Compiler.IntTypeId, null);
                    }

                    case "UInt": {

                        return (Compiler.UIntTypeId, null);
                    }

                    case "String": {

                        return (Compiler.StringTypeId, null);
                    }

                    case "Bool": {

                        return (Compiler.BoolTypeId, null);
                    }

                    case "Void": {

                        return (Compiler.VoidTypeId, null);
                    }

                    case var x: {

                        var typeId = project.FindTypeInScope(scopeId, x);

                        switch (typeId) {

                            case Int32 _typeId: {

                                return (_typeId, null);
                            }

                            default: {

                                return (
                                    Compiler.UnknownTypeId, 
                                    new TypeCheckError(
                                        "unknown type",
                                        nt.Span));
                            }
                        }
                    }
                }
            }

            case ParsedEmptyType _: {

                return (Compiler.UnknownTypeId, null);
            }

            case ParsedArrayType vt: {

                var (innerType, innerTypeErr) = TypeCheckTypeName(vt.Type, scopeId, project);

                error = error ?? innerTypeErr;

                var vectorStructId = project
                    .FindStructInScope(0, "Array")
                    ?? throw new Exception("internal error: Array builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(vectorStructId, new List<Int32>(new [] { innerType })));

                return (
                    typeId,
                    error);
            }

            case ParsedSetType st: {

                var (innerTy, err) = TypeCheckTypeName(st.Type, scopeId, project);

                error = error ?? err;

                var setStructId = project
                    .FindStructInScope(0, "Set")
                    ?? throw new Exception("internal error: Set builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(setStructId, new List<Int32>(new [] { innerTy })));

                return (typeId, error);
            }

            case ParsedOptionalType opt: {

                var (innerType, err) = TypeCheckTypeName(opt.Type, scopeId, project);

                error = error ?? err;

                var optionalStructId = project
                    .FindStructInScope(0, "Optional")
                    ?? throw new Exception("internal error: Optional builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(optionalStructId, new List<Int32>(new [] { innerType })));

                return (
                    typeId,
                    error);
            }

            case ParsedRawPointerType rp: {

                var (innerType, err) = TypeCheckTypeName(rp.Type, scopeId, project);

                error = error ?? err;

                var typeId = project.FindOrAddTypeId(new RawPointerType(innerType));

                return (
                    typeId,
                    error);
            }

            case ParsedGenericType gt: {

                var checkedInnerTypes = new List<Int32>();

                foreach (var innerType in gt.Types) {

                    var (innerTy, innerTypeNameErr) = TypeCheckTypeName(innerType, scopeId, project);

                    error = error ?? innerTypeNameErr;

                    checkedInnerTypes.Add(innerTy);
                }

                var structId = project.FindStructInScope(scopeId, gt.Name);

                if (structId is Int32 _structId) {

                    return (
                        project.FindOrAddTypeId(new GenericInstance(_structId, checkedInnerTypes)),
                        error);
                }
                else {

                    var _enumId = project.FindEnumInScope(scopeId, gt.Name);

                    if (_enumId is Int32 enumId) {

                        return (
                            project.FindOrAddTypeId(
                                new GenericEnumInstance(
                                    enumId, 
                                    checkedInnerTypes)),
                            error);
                    }
                    else {

                        return (
                            Compiler.UnknownTypeId,
                            new TypeCheckError(
                                $"could not find {gt.Name}",
                                gt.Span));
                    }
                }
            }

            default: {

                throw new Exception();
            }
        }
    }
}
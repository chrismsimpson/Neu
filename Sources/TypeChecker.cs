
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

public partial class StructType : NeuType {

    public Int32 StructId { get; init; }

    ///

    public StructType(
        Int32 structId) {

        this.StructId = structId;
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

        switch (typeId) {

            case Compiler.Int8TypeId:
            case Compiler.Int16TypeId:
            case Compiler.Int32TypeId:
            case Compiler.Int64TypeId:
            case Compiler.UInt8TypeId:
            case Compiler.UInt16TypeId:
            case Compiler.UInt32TypeId:
            case Compiler.UInt64TypeId:
                return true;
            
            default: 
                return false;
        }
    }

    public static bool CanFitInteger(
        Int32 typeId,
        IntegerConstant value) {

        switch (value) {

            case SignedIntegerConstant si: {

                switch (typeId) {

                    case Compiler.Int8TypeId: return si.Value >= sbyte.MinValue && si.Value <= sbyte.MaxValue;
                    case Compiler.Int16TypeId: return si.Value >= short.MinValue && si.Value <= short.MaxValue;
                    case Compiler.Int32TypeId: return si.Value >= int.MinValue && si.Value <= int.MaxValue;
                    case Compiler.Int64TypeId: return true;
                    case Compiler.UInt8TypeId: return si.Value >= 0 && si.Value <= byte.MaxValue;
                    case Compiler.UInt16TypeId: return si.Value >= 0 && si.Value <= ushort.MaxValue;
                    case Compiler.UInt32TypeId: return si.Value >= 0 && si.Value <= uint.MaxValue;
                    case Compiler.UInt64TypeId: return si.Value >= 0;
                    default: return false;
                }
            }

            case UnsignedIntegerConstant ui: {

                switch (typeId) {

                    case Compiler.Int8TypeId: return ui.Value <= ToUInt64(sbyte.MaxValue);
                    case Compiler.Int16TypeId: return ui.Value <= ToUInt64(short.MaxValue);
                    case Compiler.Int32TypeId: return ui.Value <= ToUInt64(int.MaxValue);
                    case Compiler.Int64TypeId: return ui.Value <= ToUInt64(long.MaxValue);
                    case Compiler.UInt8TypeId: return ui.Value <= ToUInt64(byte.MaxValue);
                    case Compiler.UInt16TypeId: return ui.Value <= ToUInt64(ushort.MaxValue);
                    case Compiler.UInt32TypeId: return ui.Value <= ToUInt64(uint.MaxValue);
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

    public List<Scope> Scopes { get; init; }

    public List<NeuType> Types { get; init; }

    ///

    public Project() {

        // Top-level (project-global) scope has no parent scope
        // and is the parent scope of all file scopes
        // var projectGlobalScope = new Scope()

        var projectGlobalScope = new Scope(null);

        this.Functions = new List<CheckedFunction>();
        this.Structs = new List<CheckedStruct>();
        this.Scopes = new List<Scope>(new [] { projectGlobalScope });
        this.Types = new List<NeuType>();
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
                        $"redefinition of {var.Name}",
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
                        $"redefinition of {name}",
                        span));
            }
        }

        scope.Structs.Add((name, structId));

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
                        $"redefinition of {name}",
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
                        $"redefinition of {typeName}",
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
    
    public Int32 ReturnType { get; set; }
    
    public List<CheckedParameter> Parameters { get; init; }

    public List<Int32> GenericParameters { get; set; }

    public Int32 FuncScopeId { get; init; }
    
    public CheckedBlock Block { get; set; }

    public FunctionLinkage Linkage { get; init; }

    ///

    public CheckedFunction(
        String name,
        Int32 returnType,
        List<CheckedParameter> parameters,
        List<Int32> genericParameters,
        Int32 funcScopeId,
        CheckedBlock block,
        FunctionLinkage linkage) { 

        this.Name = name;
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

        NumericConstant newConstant = i switch {
            SignedIntegerConstant si => typeId switch {      
                Compiler.Int8TypeId => new Int8Constant(ToSByte(si.Value)),
                Compiler.Int16TypeId => new Int16Constant(ToInt16(si.Value)),
                Compiler.Int32TypeId => new Int32Constant(ToInt32(si.Value)),
                Compiler.Int64TypeId => new Int64Constant(si.Value),
                Compiler.UInt8TypeId => new UInt8Constant(ToByte(si.Value)),
                Compiler.UInt16TypeId => new UInt16Constant(ToUInt16(si.Value)),
                Compiler.UInt32TypeId => new UInt32Constant(ToUInt32(si.Value)),
                Compiler.UInt64TypeId => new UInt64Constant(ToUInt64(si.Value)),
                _ => throw new Exception("Bogus state in IntegerConstant.promote")
            },
            UnsignedIntegerConstant ui => typeId switch {
                Compiler.Int8TypeId => new Int8Constant(ToSByte(ui.Value)),
                Compiler.Int16TypeId => new Int16Constant(ToInt16(ui.Value)),
                Compiler.Int32TypeId => new Int32Constant(ToInt32(ui.Value)),
                Compiler.Int64TypeId => new Int64Constant(System.Convert.ToInt64(ui.Value)),
                Compiler.UInt8TypeId => new UInt8Constant(ToByte(ui.Value)),
                Compiler.UInt16TypeId => new UInt16Constant(ToUInt16(ui.Value)),
                Compiler.UInt32TypeId => new UInt32Constant(ToUInt32(ui.Value)),
                Compiler.UInt64TypeId => new UInt64Constant(ui.Value),
                _ => throw new Exception("Bogus state in IntegerConstant.promote")
            },
            _ => throw new Exception()
        };

        return (newConstant, typeId);
    }
}

///

public partial class NumericConstant {

    public NumericConstant() { }
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

public static partial class NumericConstantFunctions {

    public static bool Eq(NumericConstant l, NumericConstant r) {

        switch (true) {

            case var _ when l is Int8Constant li8 && r is Int8Constant ri8:             return li8 == ri8;
            case var _ when l is Int16Constant li16 && r is Int16Constant ri16:         return li16 == ri16;
            case var _ when l is Int32Constant li32 && r is Int32Constant ri32:         return li32 == ri32;
            case var _ when l is Int64Constant li64 && r is Int64Constant ri64:         return li64 == ri64;
            case var _ when l is UInt8Constant lu8 && r is UInt8Constant ru8:           return lu8 == ru8;
            case var _ when l is UInt16Constant lu16 && r is UInt16Constant ru16:       return lu16 == ru16;
            case var _ when l is UInt32Constant lu32 && r is UInt32Constant ru32:       return lu32 == ru32;
            case var _ when l is UInt64Constant lu64 && r is UInt64Constant ru64:       return lu64 == ru64;
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
            case UInt8Constant u8: return new UnsignedIntegerConstant(ToUInt64(u8.Value));
            case UInt16Constant u16: return new UnsignedIntegerConstant(ToUInt64(u16.Value));
            case UInt32Constant u32: return new UnsignedIntegerConstant(ToUInt64(u32.Value));
            case UInt64Constant u64: return new UnsignedIntegerConstant(u64.Value);
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
            case UInt8Constant u8: return Compiler.UInt8TypeId;
            case UInt16Constant u16: return Compiler.UInt16TypeId;
            case UInt32Constant u32: return Compiler.UInt32TypeId;
            case UInt64Constant u64: return Compiler.UInt64TypeId;
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

    public partial class CheckedNumericConstantExpression: CheckedExpression {

        public NumericConstant Value { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedNumericConstantExpression(
            NumericConstant value,
            Int32 type) {

            this.Value = value;
            this.Type = type;
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

    public partial class CheckedCharacterConstantExpression: CheckedExpression {

        public Char Char { get; init; }

        ///

        public CheckedCharacterConstantExpression(
            Char c) {

            this.Char = c;
        }
    }

    public partial class CheckedUnaryOpExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public CheckedUnaryOperator Operator { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedUnaryOpExpression(
            CheckedExpression expression,
            CheckedUnaryOperator op,
            Int32 type) {

            this.Expression = expression;
            this.Operator = op;
            this.Type = type;
        }
    }

    public partial class CheckedBinaryOpExpression: CheckedExpression {

        public CheckedExpression Lhs { get; init; }

        public BinaryOperator Operator { get; init; }

        public CheckedExpression Rhs { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedBinaryOpExpression(
            CheckedExpression lhs,
            BinaryOperator op,
            CheckedExpression rhs,
            Int32 type) {

            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
            this.Type = type;
        }
    }

    public partial class CheckedTupleExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedTupleExpression(
            List<CheckedExpression> expressions,
            Int32 type) {

            this.Expressions = expressions;
            this.Type = type;
        }
    }

    public partial class CheckedRangeExpression: CheckedExpression {

        public CheckedExpression Start { get; init; }

        public CheckedExpression End { get; init; }

        public Int32 TypeId { get; init; }

        ///

        public CheckedRangeExpression(
            CheckedExpression start,
            CheckedExpression end,
            Int32 typeId) {
            
            this.Start = start;
            this.End = end;
            this.TypeId = typeId;
        }
    }

    public partial class CheckedVectorExpression: CheckedExpression {

        public List<CheckedExpression> Expressions { get; init; }

        public CheckedExpression? FillSize { get; init; }
        
        public Int32 Type { get; init; }

        ///

        public CheckedVectorExpression(
            List<CheckedExpression> expressions,
            CheckedExpression? fillSize,
            Int32 type) 
            : base() {

            this.Expressions = expressions;
            this.FillSize = fillSize;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }
        
        public CheckedExpression Index { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedExpression(
            CheckedExpression expression,
            CheckedExpression index,
            Int32 type) 
            : base() {

            this.Expression = expression;
            this.Index = index;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedTupleExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Int64 Index { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedTupleExpression(
            CheckedExpression expression,
            Int64 index,
            Int32 type) {

            this.Expression = expression;
            this.Index = index;
            this.Type = type;
        }
    }

    public partial class CheckedIndexedStructExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public String Name { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedIndexedStructExpression(
            CheckedExpression expression,
            String name,
            Int32 type) {

            this.Expression = expression;
            this.Name = name;
            this.Type = type;
        }
    }


    public partial class CheckedCallExpression: CheckedExpression {

        public CheckedCall Call { get; init; }
        
        public Int32 Type { get; init; }

        ///

        public CheckedCallExpression(
            CheckedCall call,
            Int32 type) {

            this.Call = call;
            this.Type = type;
        }
    }

    public partial class CheckedMethodCallExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public CheckedCall Call { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedMethodCallExpression(
            CheckedExpression expression,
            CheckedCall call,
            Int32 type) {

            this.Expression = expression;
            this.Call = call;
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

        public Int32 Type { get; init; }

        ///

        public CheckedOptionalNoneExpression(
            Int32 type) {

            this.Type = type;
        }
    }

    public partial class CheckedOptionalSomeExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedOptionalSomeExpression(
            CheckedExpression expression,
            Int32 type) {

            this.Expression = expression;
            this.Type = type;
        }
    }

    public partial class CheckedForceUnwrapExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public Int32 Type { get; init; }

        ///

        public CheckedForceUnwrapExpression(
            CheckedExpression expression,
            Int32 type) {

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

            case CheckedVectorExpression vecExpr: {

                return vecExpr.Type;
            }

            case CheckedTupleExpression tupleExpr: {

                return tupleExpr.Type;
            }

            case CheckedRangeExpression rangeExpr: {

                return rangeExpr.TypeId;
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

            case CheckedGarbageExpression _: {

                return Compiler.UnknownTypeId;
            }

            default:

                throw new Exception();
        }
    }

    public static IntegerConstant? ToIntegerConstant(
        this CheckedExpression e) {

        switch (e) {

            case CheckedNumericConstantExpression ne: return ne.Value.IntegerConstant();

            default: return null;
        }
    }

    public static bool IsMutable(
        this CheckedExpression e) {

        switch (e) {

            case CheckedVarExpression ve: return ve.Variable.Mutable;

            case CheckedIndexedStructExpression ise: return ise.Expression.IsMutable();

            case CheckedIndexedExpression ie: return ie.Expression.IsMutable();

            default: return false;
        }
    }
}

///

public partial class CheckedCall {

    public List<String> Namespace { get; init; }
    
    public String Name { get; init; }
    
    public List<(String, CheckedExpression)> Args { get; init; }

    public FunctionLinkage Linkage { get; init; }
    
    public Int32 Type { get; init; }

    public DefinitionType? CalleeDefinitionType { get; init; }

    ///

    public CheckedCall(
        List<String> ns,
        String name,
        List<(String, CheckedExpression)> args,
        FunctionLinkage linkage,
        Int32 type,
        DefinitionType? calleeDefinitionType) {

        this.Namespace = ns;
        this.Name = name;
        this.Args = args;
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
            parent) { }

    public Scope(
        List<CheckedVariable> vars,
        List<(String, Int32)> structs,
        List<(String, Int32)> funcs,
        List<(String, Int32)> types,
        Int32? parent) {

        this.Vars = vars;
        this.Structs = structs;
        this.Funcs = funcs;
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

        for (Int32 _structId = 0; _structId < parsedFile.Structs.Count; _structId++) {
            
            // Ensure we know the types ahead of time, so they can be recursive

            var structure = parsedFile.Structs.ElementAt(_structId);

            var structId = _structId + projectStructLength;

            project.Types.Add(new StructType(structId));

            var structTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(scopeId, structure.Name, structTypeId, structure.Span).Error is Error e1) {

                error = error ?? e1;
            }

            TypeCheckStructPredecl(structure, structTypeId, structId, scopeId, project);
        }

        foreach (var fun in parsedFile.Functions) {
            
            // Ensure we know the function ahead of time, so they can be recursive
            
            error = error ?? TypeCheckFuncPredecl(fun, scopeId, project);
        }

        for (Int32 structId = 0; structId < parsedFile.Structs.Count; structId++) {

            var structure = parsedFile.Structs.ElementAt(structId);

            error = error ?? TypeCheckStruct(
                structure, 
                structId + projectStructLength,
                scopeId, 
                project);
        }

        foreach (var fun in parsedFile.Functions) {

            error = error ?? TypeCheckFunc(fun, scopeId, project);
        }

        return error;
    }

    public static Error? TypeCheckStructPredecl(
        Struct structure,
        Int32 structTypeId,
        Int32 structId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var structScopeId = project.CreateScope(parentScopeId);

        foreach (var func in structure.Methods) {

            var methodScopeId = project.CreateScope(structScopeId);

            var checkedFunction = new CheckedFunction(
                name: func.Name,
                parameters: new List<CheckedParameter>(),
                genericParameters: new List<Int32>(),
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

                    var (paramType, err) = TypeCheckTypeName(param.Variable.Type, structScopeId, project);

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
                genericParameters: new List<Int32>(),
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
        Struct structure,
        Int32 structId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var fields = new List<CheckedVarDecl>();

        foreach (var (genParam, paramSpan) in structure.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var paramTypeId = project.Types.Count - 1;

            var _checkedStruct = project.Structs[structId];

            var _checkedStructScopeId = _checkedStruct.ScopeId;

            _checkedStruct.GenericParameters.Add(paramTypeId);

            if (project.AddTypeToScope(_checkedStructScopeId, genParam, paramTypeId, paramSpan).Error is Error genErr) {

                error = error ?? genErr;
            }
        }

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

        var constructorParams = new List<CheckedParameter>();

        foreach (var field in fields) {

            constructorParams.Add(
                new CheckedParameter(
                    requiresLabel: true,
                    variable: 
                        new CheckedVariable(
                            name: field.Name,
                            type: field.Type,
                            mutable: field.Mutable)));
        }

        var funcScopeId = project.CreateScope(parentScopeId);

        var checkedStruct = project.Structs.ElementAt(structId);

        checkedStruct.Fields = fields;

        var checkedConstructor = new CheckedFunction(
            name: structure.Name,
            returnType: structTypeId,
            parameters: constructorParams,
            genericParameters: new List<Int32>(),
            funcScopeId,
            block: new CheckedBlock(),
            linkage: FunctionLinkage.ImplicitConstructor);

        // Internal constructor

        project.Functions.Add(checkedConstructor);

        var checkedStructScopeId = checkedStruct.ScopeId;

        // Add constructor to the struct's scope

        if (project.AddFuncToScope(checkedStructScopeId, structure.Name, project.Functions.Count - 1, structure.Span).Error is Error e1) {

            error = error ?? e1;
        }

        // Add helper function for constructor to the parent scope

        if (project.AddFuncToScope(parentScopeId, structure.Name, project.Functions.Count - 1, structure.Span).Error is Error e2) {

            error = error ?? e2;
        }
        
        foreach (var func in structure.Methods) {

            error = error ?? TypeCheckMethod(func, checkedStructScopeId, project, structId);
        }
        
        return error;
    }

    public static Error? TypeCheckFuncPredecl(
        Function func,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var funcScopeId = project.CreateScope(parentScopeId);

        var checkedFunction = new CheckedFunction(
            name: func.Name,
            returnType: Compiler.UnknownTypeId,
            parameters: new List<CheckedParameter>(),
            genericParameters: new List<Int32>(),
            funcScopeId,
            block: new CheckedBlock(),
            linkage: func.Linkage);

        var checkedFuncScopeId = checkedFunction.FuncScopeId;

        var genericParams = new List<Int32>();

        foreach (var (genParam, paramSpan) in func.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var typeVarTypeId = project.Types.Count - 1;

            genericParams.Add(typeVarTypeId);

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
        Function func,
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

        var (block, typeCheckBlockErr) = TypeCheckBlock(func.Block, functionScopeId, project, SafetyMode.Safe);

        error = error ?? typeCheckBlockErr;

        var (funcReturnType, typeCheckReturnTypeErr) = TypeCheckTypeName(func.ReturnType, functionScopeId, project);

        error = error ?? typeCheckReturnTypeErr;

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

        checkedFunction = project.Functions[funcId];

        checkedFunction.Block = block;

        checkedFunction.ReturnType = returnType;

        return error;
    }

    public static Error? TypeCheckMethod(
        Function func,
        Int32 parentScopeId,
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

        var (funcReturnType, chkRetTypeErr) = TypeCheckTypeName(func.ReturnType, parentScopeId, project);

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
        Block block,
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
        Statement stmt,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode) {

        Error? error = null;

        switch (stmt) {

            case ForStatement fs: {

                var (checkedExpr, err) = TypeCheckExpression(fs.Range, scopeId, project, safetyMode);

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

                var (checkedBlock, blockErr) = TypeCheckBlock(fs.Block, iteratorScopeId, project, SafetyMode.Unsafe);

                error = error ?? blockErr;

                return (
                    new CheckedForStatement(fs.IteratorName, checkedExpr, checkedBlock),
                    error);
            }

            case Expression e: {

                var (checkedExpr, exprErr) = TypeCheckExpression(e, scopeId, project, safetyMode);

                return (
                    checkedExpr,
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

                var (checkedExpr, exprErr) = TypeCheckExpression(vds.Expr, scopeId, project, safetyMode);

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

                var (checkedCond, exprErr) = TypeCheckExpression(ifStmt.Expr, scopeId, project, safetyMode);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ifStmt.Block, scopeId, project, safetyMode);
                
                error = error ?? blockErr;

                CheckedStatement? elseOutput = null;

                if (ifStmt.Trailing is Statement elseStmt) {

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

            case WhileStatement ws: {

                var (checkedCond, exprErr) = TypeCheckExpression(ws.Expr, scopeId, project, safetyMode);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ws.Block, scopeId, project, safetyMode);
                
                error = error ?? blockErr;

                return (
                    new CheckedWhileStatement(checkedCond, checkedBlock), 
                    error);
            }

            case ReturnStatement rs: {

                var (output, outputErr) = TypeCheckExpression(rs.Expr, scopeId, project, safetyMode);

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
                    new CheckedNumericConstantExpression(newConstant, newType), 
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
        Expression expr,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode) {

        Error? error = null;

        switch (expr) {

            case RangeExpression re: {

                var (checkedStart, startErr) = TypeCheckExpression(re.Start, scopeId, project, safetyMode);

                error = error ?? startErr;

                var (checkedEnd, endErr) = TypeCheckExpression(re.End, scopeId, project, safetyMode);

                error = error ?? endErr;

                if (checkedStart.GetNeuType() != checkedEnd.GetNeuType()) {

                    error = error ?? 
                        new TypeCheckError(
                            "Range start and end must be the same type",
                            re.Span);
                }

                var rangeStructId = project
                    .FindStructInScope(0, "Range")
                    ?? throw new Exception("internal error: Range builtin definition not found");

                var ty = new GenericInstance(rangeStructId, new List<Int32>(new [] { checkedStart.GetNeuType() }));

                return (
                    new CheckedRangeExpression(
                        checkedStart, 
                        checkedEnd, 
                        project.FindOrAddTypeId(ty)),
                    error);
            }

            case BinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, scopeId, project, safetyMode);

                error = error ?? checkedLhsErr;

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, scopeId, project, safetyMode);

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
                        ty),
                    error);
            }

            case UnaryOpExpression u: {

                var (checkedExpr, checkedExprErr) = TypeCheckExpression(u.Expression, scopeId, project, safetyMode);

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
                        Compiler.UnknownTypeId),
                    error);
            }

            case OptionalSomeExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode);

                error = error ?? ckdExprError;

                var type = ckdExpr.GetNeuType();

                return (
                    new CheckedOptionalSomeExpression(ckdExpr, type),
                    error);
            }

            case ForcedUnwrapExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode);

                error = error ?? ckdExprError;

                var type = project.Types[ckdExpr.GetNeuType()];

                var typeId = Compiler.UnknownTypeId;

                var optionalStructId = project
                    .FindStructInScope(0, "Optional") 
                    ?? throw new Exception("internal error: can't find builtin Optional type");

                switch (type) {

                    case GenericInstance gi when gi.StructId == optionalStructId: {

                        typeId = gi.TypeIds[0];

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
                    new CheckedForceUnwrapExpression(ckdExpr, typeId),
                    error);
            }

            case BooleanExpression e: {

                return (
                    new CheckedBooleanExpression(e.Value),
                    null);
            }

            case CallExpression e: {

                var (checkedCall, checkedCallErr) = TypeCheckCall(e.Call, scopeId, e.Span, project, safetyMode);

                var ty = checkedCall.Type;

                return (
                    new CheckedCallExpression(checkedCall, ty),
                    error ?? checkedCallErr);
            }

            case NumericConstantExpression ne: {

                return (
                    new CheckedNumericConstantExpression(ne.Value, ne.Value.GetNeuType()),
                    null);
            }

            case QuotedStringExpression e: {

                return (
                    new CheckedQuotedStringExpression(e.Value),
                    null);
            }

            case CharacterLiteralExpression cle: {

                return (
                    new CheckedCharacterConstantExpression(cle.Char),
                    null);
            }

            case VarExpression e: {

                if (project.FindVarInScope(scopeId, e.Value) is CheckedVariable v) {

                    return (
                        new CheckedVarExpression(v),
                        null);
                }
                else {
                    
                    return (
                        new CheckedVarExpression(
                            new CheckedVariable(
                                e.Value, 
                                type: Compiler.UnknownTypeId, 
                                mutable: false)
                        ),
                        new TypeCheckError(
                            "variable not found",
                            e.Span));
                }
            }

            case VectorExpression ve: {

                var innerType = Compiler.UnknownTypeId;

                var output = new List<CheckedExpression>();

                CheckedExpression? checkedFillSizeExpr = null;

                if (ve.FillSize is Expression fillSize) {

                    var (chkFillSizeExpr, chkFillSizeErr) = TypeCheckExpression(fillSize, scopeId, project, safetyMode);

                    checkedFillSizeExpr = chkFillSizeExpr;

                    error = error ?? chkFillSizeErr;
                }

                ///

                foreach (var v in ve.Expressions) {

                    var (checkedExpr, err) = TypeCheckExpression(v, scopeId, project, safetyMode);

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

                var vectorStructId = project
                    .FindStructInScope(0, "RefVector")
                    ?? throw new Exception("internal error: RefVector builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(vectorStructId, new List<Int32>(new [] { innerType })));

                return (
                    new CheckedVectorExpression(
                        expressions: output,
                        checkedFillSizeExpr,
                        typeId),
                    error);
            }

            case TupleExpression te: {

                var checkedItems = new List<CheckedExpression>();

                var checkedTypes = new List<Int32>();

                foreach (var item in te.Expressions) {

                    var (checkedItemExpr, typeCheckItemExprErr) = TypeCheckExpression(item, scopeId, project, safetyMode);

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
                        typeId),
                    error);
            }

            case IndexedExpression ie: {

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, scopeId, project, safetyMode);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, scopeId, project, safetyMode);
            
                error = error ?? typeCheckIdxErr;

                var exprType = Compiler.UnknownTypeId;

                var vectorStructId = project
                    .FindStructInScope(0, "RefVector")
                    ?? throw new Exception("internal error: RefVector builtin definition not found");

                var ty = project.Types[checkedExpr.GetNeuType()];

                switch (ty) {

                    case GenericInstance gi when gi.StructId == vectorStructId: {

                        var _chkIdx = checkedIdx.GetNeuType();

                        switch (true) {

                            case var _ when NeuTypeFunctions.IsInteger(_chkIdx): {

                                exprType = gi.TypeIds[0];

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

                        break;
                    }

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "index used on value that can't be indexed",
                                ie.Expression.GetSpan());

                        break;
                    }
                }

                return (
                    new CheckedIndexedExpression(
                        checkedExpr,
                        checkedIdx,
                        exprType),
                    error);
            }

            case IndexedTupleExpression ite: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ite.Expression, scopeId, project, safetyMode);

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
                    new CheckedIndexedTupleExpression(checkedExpr, ite.Index, ty),
                    error);
            }

            case IndexedStructExpression ise: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ise.Expression, scopeId, project, safetyMode);

                error = error ?? chkExprErr;

                var ty = Compiler.UnknownTypeId;

                var checkedExprTy = project.Types[checkedExpr.GetNeuType()];

                switch (checkedExprTy) {

                    case StructType st: {

                        var structure = project.Structs[st.StructId];

                        foreach (var member in structure.Fields) {

                            if (member.Name == ise.Name) {

                                return (
                                    new CheckedIndexedStructExpression(
                                        checkedExpr,
                                        ise.Name,
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
                    new CheckedIndexedStructExpression(checkedExpr, ise.Name, ty),
                    error);
            }

            case MethodCallExpression mce: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(mce.Expression, scopeId, project, safetyMode);

                error = error ?? chkExprErr;

                if (checkedExpr.GetNeuType() == Compiler.StringTypeId) {

                    // Special-case the built-in so we don't accidentally find the user's definition

                    var stringStruct = project.FindStructInScope(0, "String");

                    switch (stringStruct) {

                        case Int32 structId: {

                            var (checkedCall, err) = TypeCheckMethodCall(
                                mce.Call,
                                scopeId,
                                mce.Span,
                                project,
                                checkedExpr,
                                structId,
                                safetyMode);

                            return (
                                new CheckedMethodCallExpression(checkedExpr, checkedCall, checkedCall.Type),
                                error);
                        }

                        default: {

                            error = error ?? 
                                new TypeCheckError(
                                    "no methods available on value",
                                    mce.Expression.GetSpan());

                            return (
                                new CheckedGarbageExpression(), 
                                error);
                        }
                    }
                }
                else {

                    var checkedExprTy = project.Types[checkedExpr.GetNeuType()];

                    switch (checkedExprTy) {

                        case StructType st: {

                            var (checkedCall, err) = TypeCheckMethodCall(
                                mce.Call, 
                                scopeId, 
                                mce.Span, 
                                project,
                                checkedExpr,
                                st.StructId, 
                                safetyMode);

                            error = error ?? err;

                            return (
                                new CheckedMethodCallExpression(checkedExpr, checkedCall, checkedCall.Type),
                                error);
                        }

                        case GenericInstance gi: {

                            // ignore the inner types for now, but we'll need them in the future

                            var (checkedCall, err) = TypeCheckMethodCall(
                                mce.Call,
                                scopeId,
                                mce.Span,
                                project,
                                checkedExpr,
                                gi.StructId,
                                safetyMode);

                            error = error ?? err;

                            var ty = checkedCall.Type;

                            return (
                                new CheckedMethodCallExpression(checkedExpr, checkedCall, ty),
                                error);
                        }

                        default: {

                            error = error ??
                                new TypeCheckError(
                                    "no methods available on value",
                                    mce.Expression.GetSpan());

                            return (
                                new CheckedGarbageExpression(),
                                error);
                        }
                    }
                }
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
                    new CheckedUnaryOpExpression(expr, new CheckedIsUnaryOperator(i.TypeId), Compiler.BoolTypeId),
                    null);
            }
            
            case CheckedTypeCastUnaryOperator tc: {

                return (
                    new CheckedUnaryOpExpression(expr, op, tc.TypeCast.GetNeuType()),
                    null);
            }

            case CheckedDereferenceUnaryOperator _: {

                switch (exprType) {

                    case RawPointerType rp: {

                        if (safetyMode == SafetyMode.Unsafe) {

                            return (
                                new CheckedUnaryOpExpression(expr, op, rp.TypeId),
                                null);
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, op, rp.TypeId),
                                new TypeCheckError(
                                    "dereference of raw pointer outside of unsafe block",
                                    span));
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, Compiler.UnknownTypeId),
                            new TypeCheckError(
                                "dereference of a non-pointer value",
                                span));
                    }
                }
            }

            case CheckedRawAddressUnaryOperator _: {

                var typeId = project.FindOrAddTypeId(new RawPointerType(exprTypeId));

                return (
                    new CheckedUnaryOpExpression(expr, op, typeId),
                    null);
            }

            case CheckedLogicalNotUnaryOperator _: {

                return (
                    new CheckedUnaryOpExpression(expr, new CheckedLogicalNotUnaryOperator(), exprTypeId),
                    null);
            }

            case CheckedBitwiseNotUnaryOperator _: {

                return (new CheckedUnaryOpExpression(expr, new CheckedBitwiseNotUnaryOperator(), exprTypeId), null);
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
                            new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), exprTypeId),
                            null);
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), exprTypeId),
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
                                new CheckedUnaryOpExpression(expr, op, exprTypeId),
                                new TypeCheckError(
                                    "increment/decrement of immutable variable",
                                    span));
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, op, exprTypeId),
                                null);
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, exprTypeId),
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
            case BinaryOperator.LogicalOr: {

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
        Call call,
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

                if (project.FindFuncInScope(structure.ScopeId, call.Name) is Int32 funcId1) {

                    callee = project.Functions[funcId1];
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

            if (project.FindFuncInScope(scopeId, call.Name) is Int32 funcId3) {

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
        Call call, 
        Int32 scopeId,
        Span span,
        Project project,
        SafetyMode safetyMode) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        DefinitionType? calleDefType = null;

        var returnType = Compiler.UnknownTypeId;

        var linkage = FunctionLinkage.Internal;

        switch (call.Name) {

            case "printLine":
            case "warnLine": {

                // FIXME: This is a hack since printLine() and warnLine() are hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, scopeId, project, safetyMode);

                    error = error ?? checkedArgErr;

                    if (checkedArg.GetNeuType() == Compiler.VoidTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "printLine/warnLine can't take void value",
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
                    span,
                    scopeId, 
                    project);

                error = error ?? resolveErr;

                calleDefType = _calleDefType;

                if (callee != null) {

                    returnType = callee.ReturnType;

                    linkage = callee.Linkage;

                    var genericInferences = new Dictionary<Int32, Int32>();

                    // Check that we have the right number of arguments

                    if (callee.Parameters.Count != call.Args.Count) {

                        error = error ?? new ParserError(
                            "wrong number of arguments", 
                            span);
                    }
                    else {

                        var idx = 0;

                        while (idx < call.Args.Count) {

                            var (checkedArg, checkedArgErr) = TypeCheckExpression(call.Args[idx].Item2, scopeId, project, safetyMode);

                            error = error ?? checkedArgErr;

                            var (_callee, _, _) = ResolveCall(call, span, scopeId, project); // need to do something with defType here?

                            callee = _callee ??
                                throw new Exception("internal error: previously resolved call is now unresolved");

                            if (call.Args[idx].Item2 is VarExpression ve) {

                                if (ve.Value != callee.Parameters[idx].Variable.Name
                                    && callee.Parameters[idx].RequiresLabel
                                    && call.Args[idx].Item1 != callee.Parameters[idx].Variable.Name) {

                                    error = error ?? 
                                        new TypeCheckError(
                                            "Wrong parameter name in argument label",
                                            call.Args[idx].Item2.GetSpan());
                                }
                            }
                            else if (callee.Parameters[idx].RequiresLabel
                                && call.Args[idx].Item1 != callee.Parameters[idx].Variable.Name) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Wrong parameter name in argument label",
                                        call.Args[idx].Item2.GetSpan());
                            }

                            var lhsTypeId = callee.Parameters[idx].Variable.Type;

                            var (promoted, promoteErr) = TryPromoteConstantExprToType(lhsTypeId, checkedArg, span);

                            error = error ?? promoteErr;

                            if (promoted is not null) {

                                checkedArg = promoted;
                            }

                            var rhsTypeId = checkedArg.GetNeuType();

                            if (CheckTypesForCompat(
                                callee.Parameters[idx].Variable.Type, 
                                rhsTypeId,
                                genericInferences, 
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

                    // switch (project.Types[returnType]) {

                    //     case TypeVariable _: {

                    //         if (genericInferences.ContainsKey(returnType)) {

                    //             returnType = genericInferences[returnType];
                    //         }

                    //         break;
                    //     }

                    //     default: {

                    //         break;
                    //     }
                    // }

                    returnType = SubstituteTypeVarsInType(returnType, genericInferences, project);
                }

                break;
            }
        }

        return (
            new CheckedCall(
                ns: call.Namespace,
                call.Name, 
                checkedArgs, 
                linkage,
                returnType,
                calleDefType),
            error);
    }

    public static (CheckedCall, Error?) TypeCheckMethodCall(
        Call call,
        Int32 scopeId,
        Span span,
        Project project,
        CheckedExpression thisExpr,
        Int32 structId,
        SafetyMode safetyMode) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        var returnType = Compiler.UnknownTypeId;

        var linkage = FunctionLinkage.Internal;

        var (_callee, calleeDefType, resolveCallErr) = ResolveCall(call, span, project.Structs[structId].ScopeId, project);

        error = error ?? resolveCallErr;

        if (_callee is CheckedFunction callee) {

            returnType = callee.ReturnType;

            linkage = callee.Linkage;

            var genericInferences = new Dictionary<Int32, Int32>();




            // Before we check the method, let's go ahead and make sure we know any instantiated generic types
            // This will make it easier later to know how to create the proper return type


            var typeId = thisExpr.GetNeuType();

            var paramType = project.Types[typeId];

            switch (paramType) {

                case GenericInstance gi: {

                    var structure = project.Structs[gi.StructId];

                    var idx = 0;

                    while (idx < structure.GenericParameters.Count) {

                        genericInferences[structure.GenericParameters[idx]] = gi.TypeIds[idx];

                        idx += 1;
                    }

                    break;
                }

                default: {
                    
                    break;
                }
            }








            // Check that we have the right number of arguments.

            if (callee.Parameters.Count != (call.Args.Count + 1)) {
                
                error = error ??
                    new TypeCheckError(
                        "wrong number of arguments",
                        span);
            }
            else {

                var idx = 0;

                while (idx < call.Args.Count) {

                    var (checkedArg, chkExprErr) = TypeCheckExpression(call.Args[idx].Item2, scopeId, project, safetyMode);

                    error = error ?? chkExprErr;

                    var (_callee2, _, _) = ResolveCall(call, span, project.Structs[structId].ScopeId, project); // do something with defType here?

                    callee = _callee2
                        ?? throw new Exception("internal error: previously resolved call is now unresolved");

                    if (call.Args[idx].Item2 is VarExpression ve) {

                        if (ve.Value != callee.Parameters[idx + 1].Variable.Name
                            && callee.Parameters[idx + 1].RequiresLabel
                            && call.Args[idx].Item1 != callee.Parameters[idx + 1].Variable.Name) {

                            error = error ?? 
                                new TypeCheckError(
                                    "Wrong parameter name in argument label",
                                    call.Args[idx].Item2.GetSpan());
                        }
                    }
                    else if (callee.Parameters[idx + 1].RequiresLabel
                        && call.Args[idx].Item1 != callee.Parameters[idx + 1].Variable.Name) {

                        error = error ??
                            new TypeCheckError(
                                "Wrong parameter name in argument label",
                                call.Args[idx].Item2.GetSpan());
                    }

                    var lhsTypeId = callee.Parameters[idx + 1].Variable.Type;

                    var (promoted, promoteErr) = TryPromoteConstantExprToType(lhsTypeId, checkedArg, span);

                    error = error ?? promoteErr;

                    if (promoted is not null) {

                        checkedArg = promoted;
                    }

                    var rhsTypeId = checkedArg.GetNeuType();

                    if (CheckTypesForCompat(
                        lhsTypeId, 
                        rhsTypeId, 
                        genericInferences, 
                        call.Args[idx].Item2.GetSpan(), 
                        project) is Error compatErr) {

                        error = error ?? compatErr;
                    }

                    checkedArgs.Add((call.Args[idx].Item1, checkedArg));

                    idx += 1;
                }
            }

            // We've now seen all the arguments and should be able to substitute the return type, if it's contains a
            // type variable. For the moment, we'll just checked to see if it's a type variable.

            // switch (project.Types[returnType]) {

            //     case TypeVariable _: {

            //         if (genericInferences.ContainsKey(returnType)) {

            //             returnType = genericInferences[returnType];
            //         }

            //         break;
            //     }

            //     default: {

            //         break;
            //     }
            // }

            returnType = SubstituteTypeVarsInType(returnType, genericInferences, project);
        }

        return (
            new CheckedCall(
                ns: new List<String>(),
                name: call.Name,
                args: checkedArgs,
                linkage,
                type: returnType,
                calleeDefType),
            error);
    }

    public static Int32 SubstituteTypeVarsInType(
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
                                "Parameter type mismatch",
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

                            var lhsArgTypeId = gi.TypeIds[idx];
                            var rhsArgTypeId = rhsArgs[idx];

                            while (idx < gi.TypeIds.Count) {

                                if (CheckTypesForCompat(lhsArgTypeId, rhsArgTypeId, genericInferences, span, project) is Error e2) {

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
                                    "Parameter type mismatch",
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
                                    "Parameter type mismatch",
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
                            "Parameter type mismatch",
                            span);
                }

                break;
            }
        }

        return error;
    }

    public static (Int32, Error?) TypeCheckTypeName(
        UncheckedType uncheckedType,
        Int32 scopeId,
        Project project) {

        Error? error = null;

        switch (uncheckedType) {

            case UncheckedNameType nt: {

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

            case UncheckedEmptyType _: {

                return (Compiler.UnknownTypeId, null);
            }

            case UncheckedVectorType vt: {

                var (innerType, innerTypeErr) = TypeCheckTypeName(vt.Type, scopeId, project);

                error = error ?? innerTypeErr;

                var vectorStructId = project
                    .FindStructInScope(0, "RefVector")
                    ?? throw new Exception("internal error: RefVector builtin definition not found");

                var typeId = project.FindOrAddTypeId(new GenericInstance(vectorStructId, new List<Int32>(new [] { innerType })));

                return (
                    typeId,
                    error);
            }

            case UncheckedOptionalType opt: {

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

            case UncheckedRawPointerType rp: {

                var (innerType, err) = TypeCheckTypeName(rp.Type, scopeId, project);

                error = error ?? err;

                var typeId = project.FindOrAddTypeId(new RawPointerType(innerType));

                return (
                    typeId,
                    error);
            }

            default: {

                throw new Exception();
            }
        }
    }
}
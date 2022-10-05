
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

            case var _ when l is BoolType && r is BoolType:                         return true;
            case var _ when l is StringType && r is StringType:                     return true;
            case var _ when l is Int8Type && r is Int8Type:                         return true;
            case var _ when l is Int16Type && r is Int16Type:                       return true;
            case var _ when l is Int32Type && r is Int32Type:                       return true;
            case var _ when l is Int64Type && r is Int64Type:                       return true;
            case var _ when l is UInt8Type && r is UInt8Type:                       return true;
            case var _ when l is UInt16Type && r is UInt16Type:                     return true;
            case var _ when l is UInt32Type && r is UInt32Type:                     return true;
            case var _ when l is UInt64Type && r is UInt64Type:                     return true;
            case var _ when l is FloatType && r is FloatType:                       return true;
            case var _ when l is DoubleType && r is DoubleType:                     return true;
            case var _ when l is VoidType && r is VoidType:                         return true;
            case var _ when l is VectorType lv && r is VectorType rv:               return Eq(lv.Type, rv.Type);

            case var _ when l is TupleType lt && r is TupleType rt: {

                if (lt.Types.Count != rt.Types.Count) {

                    return false;
                }

                for (var i = 0; i < lt.Types.Count; i++) {

                    if (!Eq(lt.Types[i], rt.Types[i])) {

                        return false;
                    }
                }

                return true;
            }

            case var _ when l is OptionalType lo && r is OptionalType ro:           return Eq(lo.Type, ro.Type);

            case var _ when l is StructType ls && r is StructType rs:               return ls.StructId == rs.StructId;
            
            case var _ when l is RawPointerType lp && r is RawPointerType rp:       return Eq(lp.Type, rp.Type);

            case var _ when l is UnknownType && r is UnknownType:                   return true;

            // C interop

            case var _ when l is CCharType && r is CCharType:                       return true;
            case var _ when l is CIntType && r is CIntType:                         return true;

            default:                                                                return false;
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

    public Int32 StructId { get; init; }

    ///

    public StructType(
        Int32 structId) {

        this.StructId = structId;
    }
}

public partial class RawPointerType: NeuType {

    public NeuType Type { get; init; }

    ///

    public RawPointerType(
        NeuType type) {

        this.Type = type;
    }
}

public partial class UnknownType : NeuType {

    public UnknownType()
        : base() { }
}

// C interop

public partial class CCharType: NeuType {

    public CCharType()
        : base() { }
}

public partial class CIntType: NeuType {

    public CIntType()
        : base() { }
}

public static partial class NeuTypeFunctions {

    public static bool IsInteger(
        this NeuType ty) {

        switch (ty) {

            case Int8Type _:        return true;
            case Int16Type _:       return true;
            case Int32Type _:       return true;
            case Int64Type _:       return true;
            case UInt8Type _:       return true;
            case UInt16Type _:      return true;
            case UInt32Type _:      return true;
            case UInt64Type _:      return true;
            default:                return false;
        }
    }

    public static bool CanFitInteger(
        this NeuType ty,
        IntegerConstant value) {

        switch (value) {

            case SignedIntegerConstant si: {

                switch (ty) {

                    case Int8Type _: return si.Value >= sbyte.MinValue && si.Value <= sbyte.MaxValue;
                    case Int16Type _: return si.Value >= short.MinValue && si.Value <= short.MaxValue;
                    case Int32Type _: return si.Value >= int.MinValue && si.Value <= int.MaxValue;
                    case Int64Type _: return true;
                    case UInt8Type _: return si.Value >= 0 && si.Value <= byte.MaxValue;
                    case UInt16Type _: return si.Value >= 0 && si.Value <= ushort.MaxValue;
                    case UInt32Type _: return si.Value >= 0 && si.Value <= uint.MaxValue;
                    case UInt64Type _: return si.Value >= 0;
                    default: return false;
                }
            }

            case UnsignedIntegerConstant ui: {

                switch (ty) {

                    case Int8Type _: return ui.Value <= ToUInt64(sbyte.MaxValue);
                    case Int16Type _: return ui.Value <= ToUInt64(short.MaxValue);
                    case Int32Type _: return ui.Value <= ToUInt64(int.MaxValue);
                    case Int64Type _: return ui.Value <= ToUInt64(long.MaxValue);
                    case UInt8Type _: return ui.Value <= ToUInt64(byte.MaxValue);
                    case UInt16Type _: return ui.Value <= ToUInt64(ushort.MaxValue);
                    case UInt32Type _: return ui.Value <= ToUInt64(uint.MaxValue);
                    case UInt64Type _: return true;
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

    public static Int32? FindStruct(
        this CheckedFile file,
        String name) {

        for (Int32 idx = 0; idx < file.Structs.Count; idx++) {

            var structure = file.Structs[idx];

            if (structure.Name == name) {

                return idx;
            }
        }

        return null;
    }

    public static (CheckedStruct, Int32)? GetStruct(
        this CheckedFile checkedFile,
        String name) {
        
        for (Int32 idx = 0; idx < checkedFile.Structs.Count; idx++) {

            var structure = checkedFile.Structs[idx];

            if (structure.Name == name) {

                return (structure, idx);
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

    public List<CheckedVarDecl> Fields { get; set; }

    public List<CheckedFunction> Methods { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; }

    ///

    public CheckedStruct(
        String name,
        List<CheckedVarDecl> fields,
        List<CheckedFunction> methods,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType) {

        this.Name = name;
        this.Fields = fields;
        this.Methods = methods;
        this.DefinitionLinkage = definitionLinkage;
        this.DefinitionType = definitionType;
    }
}

public static partial class CheckedStructFunctions {

    public static CheckedFunction? GetMethod(
        this CheckedStruct structure,
        String name) {

        foreach (var func in structure.Methods) {

            if (func.Name == name) {

                return func;
            }
        }

        return null;
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

        // public CheckedBlock Block { get; init; }
        public CheckedStatement Statement { get; init; }

        ///

        public CheckedDeferStatement(
            // CheckedBlock block) {
            CheckedStatement statement) {

            // this.Block = block;
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

        public static (NumericConstant?, NeuType) Promote(
            this IntegerConstant i,
            NeuType ty) {

            if (!ty.CanFitInteger(i)) {

                return (null, new UnknownType());
            }

            NumericConstant newConstant = i switch {
                SignedIntegerConstant si => ty switch {      
                    Int8Type => new Int8Constant(ToSByte(si.Value)),
                    Int16Type => new Int16Constant(ToInt16(si.Value)),
                    Int32Type => new Int32Constant(ToInt32(si.Value)),
                    Int64Type => new Int64Constant(si.Value),
                    UInt8Type => new UInt8Constant(ToByte(si.Value)),
                    UInt16Type => new UInt16Constant(ToUInt16(si.Value)),
                    UInt32Type => new UInt32Constant(ToUInt32(si.Value)),
                    UInt64Type => new UInt64Constant(ToUInt64(si.Value)),
                    _ => throw new Exception("Bogus state in IntegerConstant.promote")
                },

                UnsignedIntegerConstant ui => ty switch {
                    Int8Type => new Int8Constant(ToSByte(ui.Value)),
                    Int16Type => new Int16Constant(ToInt16(ui.Value)),
                    Int32Type => new Int32Constant(ToInt32(ui.Value)),
                    Int64Type => new Int64Constant(ToInt64(ui.Value)),
                    UInt8Type => new UInt8Constant(ToByte(ui.Value)),
                    UInt16Type => new UInt16Constant(ToUInt16(ui.Value)),
                    UInt32Type => new UInt32Constant(ToUInt32(ui.Value)),
                    UInt64Type => new UInt64Constant(ui.Value),
                    _ => throw new Exception("Bogus state in IntegerConstant.promote")
                },
                _ => throw new Exception()
            };

            return (newConstant, ty);
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

    public static NeuType GetNeuType(
        this NumericConstant n) {

        switch (n) {
            case Int8Constant i8: return new Int8Type();
            case Int16Constant i16: return new Int16Type();
            case Int32Constant i32: return new Int32Type();
            case Int64Constant i64: return new Int64Type();
            case UInt8Constant u8: return new UInt8Type();
            case UInt16Constant u16: return new UInt16Type();
            case UInt32Constant u32: return new UInt32Type();
            case UInt64Constant u64: return new UInt64Type();
            default: throw new Exception();
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

        public NeuType Type { get; init; }

        ///

        public CheckedNumericConstantExpression(
            NumericConstant value,
            NeuType type) {

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

        public Int64 Index { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedIndexedTupleExpression(
            CheckedExpression expression,
            Int64 index,
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

    public partial class CheckedMethodCallExpression: CheckedExpression {

        public CheckedExpression Expression { get; init; }

        public CheckedCall Call { get; init; }

        public NeuType Type { get; init; }

        ///

        public CheckedMethodCallExpression(
            CheckedExpression expression,
            CheckedCall call,
            NeuType type) {

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
            
            case CheckedNumericConstantExpression ne: {

                return ne.Type;
            }

            case CheckedQuotedStringExpression _: {

                return new StringType();   
            }

            case CheckedCharacterConstantExpression _: {

                return new CCharType(); // use the C one for now
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

                return new UnknownType();
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
    
    public NeuType Type { get; init; }

    public DefinitionType? CalleeDefinitionType { get; init; }

    ///

    public CheckedCall(
        List<String> ns,
        String name,
        List<(String, CheckedExpression)> args,
        NeuType type,
        DefinitionType? calleeDefinitionType) {

        this.Namespace = ns;
        this.Name = name;
        this.Args = args;
        this.Type = type;
        this.CalleeDefinitionType = calleeDefinitionType;
    }
}

public partial class Stack {
    
    public List<StackFrame> Frames { get; init; }

    ///

    public Stack()
        // : this(new List<StackFrame>()) { }
        : this(new List<StackFrame>(new [] { new StackFrame() })) { }

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
        Int32 structId,
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

    public static Int32? FindStruct(
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

    public List<(String, Int32)> Structs { get; init; }

    ///

    public StackFrame() 
        : this(
            new List<CheckedVariable>(),
            new List<(String, Int32)>()) { }

    public StackFrame(
        List<CheckedVariable> vars,
        List<(String, Int32)> structs) {

        this.Vars = vars;
        this.Structs = structs;
    }
}

///

public static partial class TypeCheckerFunctions {

    public static (CheckedFile, Error?) TypeCheckFile(
        ParsedFile file,
        CheckedFile prelude) {

        var stack = new Stack();

        return TypeCheckFileHelper(file, stack, prelude);
    }

    public static (CheckedFile, Error?) TypeCheckFileHelper(
        ParsedFile parsedFile,
        Stack stack,
        CheckedFile prelude) {

        var file = new CheckedFile();

        Error? error = null;

        foreach (var structure in prelude.Structs) {
            
            file.Structs.Add(structure);

            var _ = stack.AddStruct(
                structure.Name, 
                file.Structs.Count - 1,
                new Span(0, 0, 0));
        }

        foreach (var func in prelude.Functions) {

            file.Functions.Add(func);
        }

        for (Int32 structId = 0; structId < parsedFile.Structs.Count; structId++) {
            
            // Ensure we know the types ahead of time, so they can be recursive

            var structure = parsedFile.Structs.ElementAt(structId);

            TypeCheckStructPredecl(
                structure, 
                structId + prelude.Structs.Count,
                stack, 
                file);
        }

        foreach (var fun in parsedFile.Functions) {
            
            // Ensure we know the function ahead of time, so they can be recursive
            
            error = error ?? TypeCheckFuncPredecl(fun, stack, file);
        }

        for (Int32 structId = 0; structId < parsedFile.Structs.Count; structId++) {

            var structure = parsedFile.Structs.ElementAt(structId);

            error = error ?? TypeCheckStruct(
                structure, 
                structId + prelude.Structs.Count,
                stack, 
                file);
        }

        foreach (var fun in parsedFile.Functions) {

            error = error ?? TypeCheckFunc(fun, stack, file);
        }

        return (file, error);
    }

    public static Error? TypeCheckStructPredecl(
        Struct structure,
        Int32 structId,
        Stack stack, 
        CheckedFile file) {

        Error? error = null;

        var methods = new List<CheckedFunction>();

        foreach (var func in structure.Methods) {

            var checkedFunction = new CheckedFunction(
                name: func.Name,
                parameters: new List<CheckedParameter>(),
                returnType: new UnknownType(),
                block: new CheckedBlock(),
                linkage: func.Linkage);

            foreach (var param in func.Parameters) {

                if (param.Variable.Name == "this") {

                    var checkedVariable = new CheckedVariable(
                        name: param.Variable.Name,
                        type: new StructType(structId),
                        mutable: param.Variable.Mutable);

                    checkedFunction.Parameters.Add(
                        new CheckedParameter(
                            requiresLabel: param.RequiresLabel,
                            variable: checkedVariable));
                }
                else {

                    // var (paramType, err) = TypeCheckTypeName(param.Variable.Type, stack, file, structId);
                    var (paramType, err) = TypeCheckTypeName(param.Variable.Type, stack);

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

            methods.Add(checkedFunction);
        }

        file.Structs.Add(
            new CheckedStruct(
                name: structure.Name,
                fields: new List<CheckedVarDecl>(),
                methods: methods,
                definitionLinkage: structure.DefinitionLinkage,
                definitionType: structure.DefinitionType));

        if (stack.AddStruct(structure.Name, structId, structure.Span).Error is Error e) {

            error = error ?? e;
        }

        return error;
    }

    public static Error? TypeCheckStruct(
        Struct structure,
        Int32 structId,
        Stack stack,
        CheckedFile file) {

        Error? error = null;

        var fields = new List<CheckedVarDecl>();

        foreach (var uncheckedMember in structure.Fields) {

            // var (checkedMemberType, checkedMemberTypeErr) = TypeCheckTypeName(uncheckedMember.Type, stack, file, structId);
            var (checkedMemberType, checkedMemberTypeErr) = TypeCheckTypeName(uncheckedMember.Type, stack);

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

        var checkedStruct = file.Structs.ElementAt(structId);

        checkedStruct.Fields = fields;

        var (_checkedStruct, _structId) = file
            .GetStruct(structure.Name) 
            ?? throw new Exception("Internal error: we previously defined the struct but it's now missing");

        var checkedConstructor = new CheckedFunction(
            name: structure.Name,
            block: new CheckedBlock(),
            linkage: FunctionLinkage.ImplicitConstructor,
            parameters: constructorParams,
            returnType: new StructType(_structId));

        
        _checkedStruct.Methods.Add(checkedConstructor);

        file.Functions.Add(checkedConstructor);

        foreach (var func in structure.Methods) {

            error = error ?? TypeCheckMethod(func, stack, file, _structId);
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

            // var (paramType, typeCheckNameErr) = TypeCheckTypeName(param.Variable.Type, stack, file, null);
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

        var (block, typeCheckBlockErr) = TypeCheckBlock(func.Block, stack, file, SafetyMode.Safe);

        error = error ?? typeCheckBlockErr;

        stack.PopFrame();

        // var (funcReturnType, typeCheckReturnTypeErr) = TypeCheckTypeName(func.ReturnType, stack, file, null);
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

    public static Error? TypeCheckMethod(
        Function func,
        Stack stack,
        CheckedFile file,
        Int32 structId) { 

        Error? error = null;

        stack.PushFrame();

        var structure = file.Structs.ElementAt(structId);

        var checkedFunction = structure
            .GetMethod(func.Name) 
            ?? throw new Exception("Internal error: we just pushed the checked function, but it's not present");

        foreach (var param in checkedFunction.Parameters) {

            if (stack.AddVar(param.Variable, func.NameSpan) is Error e) {

                error = error ?? e;
            }
        }

        var (block, chkBlockErr) = TypeCheckBlock(func.Block, stack, file, SafetyMode.Safe);

        error = error ?? chkBlockErr;

        stack.PopFrame();

        // var (funcReturnType, chkRetTypeErr) = TypeCheckTypeName(func.ReturnType, stack, file, structId);
        var (funcReturnType, chkRetTypeErr) = TypeCheckTypeName(func.ReturnType, stack);

        error = error ?? chkRetTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        NeuType? returnType = null;

        if (funcReturnType is UnknownType) {

            if (block.Stmts.FirstOrDefault() is CheckedReturnStatement rs) {

                returnType = rs.Expr.GetNeuType();
            }
            else {

                returnType = new VoidType();
            }
        }
        else {

            returnType = funcReturnType;
        }

        structure = file.Structs.ElementAt(ToInt32(structId));

        checkedFunction = structure
            .GetMethod(func.Name) 
            ?? throw new Exception("Internal error: we just pushed the checked function, but it's not present");

        checkedFunction.Block = block;

        checkedFunction.ReturnType = returnType;

        return error;       
    }

    public static (CheckedBlock, Error?) TypeCheckBlock(
        Block block,
        Stack stack,
        CheckedFile file,
        SafetyMode safetyMode) {

        Error? error = null;

        var checkedBlock = new CheckedBlock();

        stack.PushFrame();

        foreach (var stmt in block.Statements) {

            var (checkedStmt, err) = TypeCheckStatement(stmt, stack, file, safetyMode);

            error = error ?? err;

            checkedBlock.Stmts.Add(checkedStmt);
        }

        stack.PopFrame();

        return (checkedBlock, error);
    }

    public static (CheckedStatement, Error?) TypeCheckStatement(
        Statement stmt,
        Stack stack,
        CheckedFile file,
        SafetyMode safetyMode) {

        Error? error = null;

        switch (stmt) {

            case Expression e: {

                var (checkedExpr, exprErr) = TypeCheckExpression(e, stack, file, safetyMode);

                return (
                    checkedExpr,
                    exprErr);
            }

            case DeferStatement ds: {

                var (checkedStmt, err) = TypeCheckStatement(ds.Statement, stack, file, safetyMode);

                return (
                    new CheckedDeferStatement(checkedStmt),
                    err);
            }

            case UnsafeBlockStatement us: {

                var (checkedBlock, blockErr) = TypeCheckBlock(us.Block, stack, file, SafetyMode.Unsafe);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    blockErr);
            }

            case VarDeclStatement vds: {

                var (checkedExpr, exprErr) = TypeCheckExpression(vds.Expr, stack, file, safetyMode);

                error = error ?? exprErr;

                // var (checkedType, chkTypeErr) = TypeCheckTypeName(vds.Decl.Type, stack, file, null);
                var (checkedType, chkTypeErr) = TypeCheckTypeName(vds.Decl.Type, stack);

                if (checkedType is UnknownType && checkedExpr.GetNeuType() is not UnknownType) {

                    checkedType = checkedExpr.GetNeuType();
                }
                else {

                    error = error ?? chkTypeErr;
                }

                // var tryPromoteErr = TryPromoteConstantExprToType(
                //     checkedType, 
                //     checkedExpr, 
                //     vds.Expr.GetSpan());

                var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                    checkedType, 
                    checkedExpr, 
                    vds.Expr.GetSpan());

                error = error ?? tryPromoteErr;

                if (promotedExpr is not null) {

                    checkedExpr = promotedExpr;
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

                var (checkedCond, exprErr) = TypeCheckExpression(ifStmt.Expr, stack, file, safetyMode);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ifStmt.Block, stack, file, safetyMode);
                
                error = error ?? blockErr;

                CheckedStatement? elseOutput = null;

                if (ifStmt.Trailing is Statement elseStmt) {

                    var (checkedElseStmt, checkedElseStmtErr) = TypeCheckStatement(elseStmt, stack, file, safetyMode);

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

                var (checkedCond, exprErr) = TypeCheckExpression(ws.Expr, stack, file, safetyMode);
                
                error = error ?? exprErr;

                var (checkedBlock, blockErr) = TypeCheckBlock(ws.Block, stack, file, safetyMode);
                
                error = error ?? blockErr;

                return (
                    new CheckedWhileStatement(checkedCond, checkedBlock), 
                    error);
            }

            case ReturnStatement rs: {

                var (output, outputErr) = TypeCheckExpression(rs.Expr, stack, file, safetyMode);

                return (
                    new CheckedReturnStatement(output), 
                    outputErr);
            }

            case BlockStatement bs: {

                var (checkedBlock, checkedBlockErr) = TypeCheckBlock(bs.Block, stack, file, safetyMode);

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

    // public static Error? TryPromoteConstantExprToType(
    //     NeuType lhsType,
    //     CheckedExpression checkedRhs,
    //     Span span) {

    //     if (!lhsType.IsInteger()) {

    //         return null;
    //     }

    //     if (checkedRhs.ToIntegerConstant() is IntegerConstant rhsConstant) {

    //         var (_newConstant, newType) = rhsConstant.Promote(lhsType);

    //         if (_newConstant is NumericConstant newConstant) {

    //             checkedRhs = new CheckedNumericConstantExpression(newConstant, newType);
    //         }
    //         else {

    //             return new TypeCheckError(
    //                 "Integer promotion failed",
    //                 span);
    //         }
    //     }

    //     return null;
    // }

    public static (CheckedNumericConstantExpression?, Error?) TryPromoteConstantExprToType(
        NeuType lhsType,
        CheckedExpression checkedRhs,
        Span span) {

        if (!lhsType.IsInteger()) {

            return (null, null);
        }

        if (checkedRhs.ToIntegerConstant() is IntegerConstant rhsConstant) {

            var (_newConstant, newType) = rhsConstant.Promote(lhsType);

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
        Stack stack,
        CheckedFile file,
        SafetyMode safetyMode) {

        Error? error = null;

        switch (expr) {

            case BinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, stack, file, safetyMode);

                error = error ?? checkedLhsErr;

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, stack, file, safetyMode);

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

                var (checkedExpr, checkedExprErr) = TypeCheckExpression(u.Expression, stack, file, safetyMode);

                error = error ?? checkedExprErr;

                var (_checkedExpr, chkUnaryOpErr) = TypeCheckUnaryOperation(checkedExpr, u.Operator, u.Span, safetyMode);

                error = error ?? chkUnaryOpErr;

                return (_checkedExpr, error);
            }

            case OptionalNoneExpression e: {

                return (
                    new CheckedOptionalNoneExpression(
                        new UnknownType()),
                    error);
            }

            case OptionalSomeExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, stack, file, safetyMode);

                error = error ?? ckdExprError;

                var type = ckdExpr.GetNeuType();

                return (
                    new CheckedOptionalSomeExpression(ckdExpr, type),
                    error);
            }

            case ForcedUnwrapExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, stack, file, safetyMode);

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

                var (checkedCall, checkedCallErr) = TypeCheckCall(e.Call, stack, e.Span, file, safetyMode);

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

                    var (checkedExpr, err) = TypeCheckExpression(v, stack, file, safetyMode);

                    error = error ?? err;

                    if (innerType is UnknownType) {

                        innerType = checkedExpr.GetNeuType();
                    }
                    else {

                        if (!NeuTypeFunctions.Eq(innerType, checkedExpr.GetNeuType())) {

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

                    var (checkedItemExpr, typeCheckItemExprErr) = TypeCheckExpression(item, stack, file, safetyMode);

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

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, stack, file, safetyMode);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, stack, file, safetyMode);
            
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

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ite.Expression, stack, file, safetyMode);

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

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ise.Expression, stack, file, safetyMode);

                error = error ?? chkExprErr;

                NeuType ty = new UnknownType();

                switch (checkedExpr.GetNeuType()) {

                    case StructType st: {

                        var structure = file.Structs[st.StructId];

                        foreach (var member in structure.Fields) {

                            if (member.Name == ise.Name) {

                                return (
                                    new CheckedIndexedStructExpression(
                                        checkedExpr,
                                        ise.Name,
                                        member.Type),
                                    error);
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

                var (checkedExpr, chkExprErr) = TypeCheckExpression(mce.Expression, stack, file, safetyMode);

                error = error ?? chkExprErr;

                switch (checkedExpr.GetNeuType()) {

                    case StructType st: {

                        var (checkedCall, chkMethodCallErr) = TypeCheckMethodCall(mce.Call, stack, mce.Span, file, st.StructId, safetyMode);

                        error = error ?? chkMethodCallErr;

                        var ty = checkedCall.Type;

                        return (
                            new CheckedMethodCallExpression(checkedExpr, checkedCall, ty),
                            error);
                    }

                    case StringType _: {

                        var stringStruct = file.FindStruct("String");

                        switch (stringStruct) {

                            case Int32 structId: {

                                var (checkedCall, err) = TypeCheckMethodCall(mce.Call, stack, mce.Span, file, structId, safetyMode);

                                error = error ?? err;

                                var ty = checkedCall.Type;

                                return (
                                    new CheckedMethodCallExpression(
                                        checkedExpr,
                                        checkedCall,
                                        ty),
                                    error);
                            }

                            default: {

                                error = error ??
                                    new TypeCheckError(
                                        "no methods available on value",
                                        mce.Span);

                                return (
                                    new CheckedGarbageExpression(),
                                    error);
                            }
                        }
                    }

                    case VectorType _: {

                        var vectorStruct = file.FindStruct("Vector");

                        switch (vectorStruct) {

                            case Int32 structId: {

                                var (checkedCall, chkMethCallErr) = TypeCheckMethodCall(mce.Call, stack, mce.Span, file, structId, safetyMode);

                                error = error ?? chkMethCallErr;

                                var ty = checkedCall.Type;

                                return (
                                    new CheckedMethodCallExpression(
                                        checkedExpr,
                                        checkedCall,
                                        ty),
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
        UnaryOperator op,
        Span span,
        SafetyMode safetyMode) {
    
        var exprType = expr.GetNeuType();

        switch (op) {

            case UnaryOperator.Dereference: {

                switch (exprType) {

                    case RawPointerType rp: {

                        if (safetyMode == SafetyMode.Unsafe) {

                            return (
                                new CheckedUnaryOpExpression(expr, op, rp.Type),
                                null);
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, op, rp.Type),
                                new TypeCheckError(
                                    "dereference of raw pointer outside of unsafe block",
                                    span));
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, new UnknownType()),
                            new TypeCheckError(
                                "dereference of a non-pointer value",
                                span));
                    }
                }
            }

            case UnaryOperator.RawAddress: {

                return (
                    new CheckedUnaryOpExpression(expr, op, new RawPointerType(exprType)),
                    null);
            }

            case UnaryOperator.LogicalNot: {

                return (
                    new CheckedUnaryOpExpression(expr, UnaryOperator.LogicalNot, exprType),
                    null);
            }

            case UnaryOperator.BitwiseNot: {

                return (new CheckedUnaryOpExpression(expr, UnaryOperator.BitwiseNot, exprType), null);
            }

            case UnaryOperator.Negate: {

                switch (exprType) {

                    case Int8Type _:
                    case Int16Type _:
                    case Int32Type _:
                    case Int64Type _:
                    case UInt8Type _:
                    case UInt16Type _:
                    case UInt32Type _:
                    case UInt64Type _:
                    case FloatType _:
                    case DoubleType _: {

                        return (
                            new CheckedUnaryOpExpression(expr, UnaryOperator.Negate, exprType),
                            null);
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, UnaryOperator.Negate, exprType),
                            new TypeCheckError(
                                "negate on non-numeric value",
                                span));
                    }
                }
            }

            case UnaryOperator.PostDecrement:
            case UnaryOperator.PostIncrement:
            case UnaryOperator.PreDecrement:
            case UnaryOperator.PreIncrement: {

                switch (exprType) {

                    case Int8Type _:
                    case Int16Type _:
                    case Int32Type _:
                    case Int64Type _:
                    case UInt8Type _:
                    case UInt16Type _:
                    case UInt32Type _:
                    case UInt64Type _:
                    case FloatType _:
                    case DoubleType _: {

                        switch (expr) {

                            case CheckedVarExpression ve: {

                                if (!ve.Variable.Mutable) {

                                    return (
                                        new CheckedUnaryOpExpression(expr, op, exprType),
                                        new TypeCheckError(
                                            "increment on immutable variable",
                                            span));
                                }
                                else {

                                    return (
                                        new CheckedUnaryOpExpression(expr, op, exprType),
                                        null);
                                }
                            }

                            default: {

                                return (
                                    new CheckedUnaryOpExpression(expr, op, exprType),
                                    null);
                            }
                        }
                    }

                    default: {

                        return (
                            new CheckedUnaryOpExpression(expr, op, exprType),
                            new TypeCheckError(
                                "unary operation on non-numeric value",
                                span));
                    }
                }
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (NeuType, Error?) TypeCheckBinaryOperation(
        CheckedExpression lhs,
        BinaryOperator op,
        CheckedExpression rhs,
        Span span) {

        var ty = lhs.GetNeuType();

        switch (op) {

            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr: {

                ty = new BoolType();

                break;
            }

            case BinaryOperator.Assign:
            case BinaryOperator.AddAssign:
            case BinaryOperator.SubtractAssign:
            case BinaryOperator.MultiplyAssign:
            case BinaryOperator.DivideAssign: {

                var lhsTy = lhs.GetNeuType();
                var rhsTy = rhs.GetNeuType();

                if (!NeuTypeFunctions.Eq(lhsTy, rhsTy)) {

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
        Stack stack,
        List<CheckedFunction> functions,
        CheckedFile file) {

        CheckedFunction? callee = null;

        DefinitionType? definitionType = null;
        
        Error? error = null;

        if (call.Namespace.FirstOrDefault() is String ns) {

            if (file.FindStruct(ns) is Int32 structId) {

                var structure = file.Structs[structId];

                definitionType = structure.DefinitionType;

                foreach (var func in structure.Methods) {

                    if (func.Name == call.Name) {

                        callee = func;

                        break;
                    }
                }

                return (callee, definitionType, error);
            }
            else {

                var v = stack.FindVar(ns);

                if (v?.Type is StructType st) {

                    var structure = file.Structs[st.StructId];

                    definitionType = structure.DefinitionType;

                    foreach (var func in structure.Methods) {

                        if (func.Name == call.Name) {

                            callee = func;

                            break;
                        }
                    }

                    return (callee, definitionType, error);
                }
                else if (v is CheckedVariable cv
                    && cv.Type is StringType
                    && file.FindStruct("String") is int stringStructId) {

                    var structure = file.Structs[stringStructId];

                    definitionType = DefinitionType.Struct; // probably has to be more contextual

                    foreach (var func in structure.Methods) {

                        if (func.Name == call.Name) {

                            callee = func;

                            break;
                        }
                    }

                    return (callee, definitionType, error);
                }
                else if (v is CheckedVariable cv2
                    && file.FindStruct("Vector") is int vectorStructId) {

                    var structure = file.Structs[vectorStructId];

                    definitionType = DefinitionType.Struct; // probably has to be more contextual

                    foreach (var func in structure.Methods) {

                        if (func.Name == call.Name) {

                            callee = func;

                            break;
                        }
                    }

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
        }
        else {

            // FIXME: Support function overloading.

            foreach (var func in functions) {

                if (func.Name == call.Name) {

                    callee = func;

                    break;
                }
            }

            if (callee == null) {

                error = error ?? new TypeCheckError(
                    $"call to unknown function: {call.Name}",
                    span);
            }

            return (callee, definitionType, error);
        }
    }

    public static (CheckedCall, Error?) TypeCheckCall(
        Call call, 
        Stack stack,
        Span span,
        CheckedFile file,
        SafetyMode safetyMode) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        DefinitionType? calleDefType = null;

        NeuType returnType = new UnknownType();

        switch (call.Name) {

            case "printLine":
            case "warnLine": {

                // FIXME: This is a hack since printLine() and warnLine() are hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, stack, file, safetyMode);

                    error = error ?? checkedArgErr;

                    returnType = new VoidType();

                    checkedArgs.Add((arg.Item1, checkedArg));
                }

                break;
            }

            ///

            default: {

                var (callee, _calleDefType, resolveErr) = ResolveCall(call, span, stack, file.Functions, file);

                error = error ?? resolveErr;

                calleDefType = _calleDefType;

                if (callee != null) {

                    returnType = callee.ReturnType;

                    // Check that we have the right number of arguments

                    var paramCount = callee.Parameters.Count;

                    var thisFirstArgument = callee.Parameters.FirstOrDefault()?.Variable.Name == "this";

                    if (thisFirstArgument) {

                        paramCount -= 1;
                    }

                    if (paramCount != call.Args.Count) {

                        error = error ?? new ParserError(
                            "wrong number of arguments", 
                            span);
                    }
                    else {

                        var idx = 0;

                        while (idx < call.Args.Count) {

                            // var (checkedArg, checkedArgErr) = TypeCheckExpression(call.Args[idx].Item2, stack, file, safetyMode);

                            // error = error ?? checkedArgErr;

                            // if (call.Args[idx].Item2 is VarExpression ve) {

                            //     if (ve.Value != callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Name
                            //         && callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].RequiresLabel
                            //         && call.Args[idx].Item1 != callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Name) {

                            //         error = error ??
                            //             new TypeCheckError(
                            //                 "Wrong parameter name in argument label".to_string(),
                            //                 call.Args[idx].Item2.GetSpan());
                            //     }
                            // }
                            // else if (callee.Parameters[idx].RequiresLabel
                            //     && call.Args[idx].Item1 != callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Name) {

                            //     error = error ?? 
                            //         new TypeCheckError(
                            //             "Wrong parameter name in argument label",
                            //             call.Args[idx].Item2.GetSpan());
                            // }

                            // // var tryPromoteErr = TryPromoteConstantExprToType(
                            // //     callee.Parameters[idx].Variable.Type,
                            // //     checkedArg,
                            // //     call.Args[idx].Item2.GetSpan());

                            // var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                            //     callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Type,
                            //     checkedArg,
                            //     call.Args[idx].Item2.GetSpan());

                            // error = error ?? tryPromoteErr;

                            // if (promotedExpr is not null) {

                            //     checkedArg = promotedExpr;
                            // }

                            // if (!NeuTypeFunctions.Eq(checkedArg.GetNeuType(), callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Type)) {

                            //     error = error ?? new TypeCheckError(
                            //         "Parameter type mismatch",
                            //         call.Args[idx].Item2.GetSpan());
                            // }

                            // checkedArgs.Add((call.Args[idx].Item1, checkedArg));

                            // idx += 1;


                            var (checkedArg, checkedArgErr) = TypeCheckExpression(call.Args[idx].Item2, stack, file, safetyMode);

                            error = error ?? checkedArgErr;

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

                            var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                                callee.Parameters[idx + (thisFirstArgument ? 1 : 0)].Variable.Type,
                                checkedArg,
                                call.Args[idx].Item2.GetSpan());

                            error = error ?? tryPromoteErr;

                            if (promotedExpr is not null) {

                                checkedArg = promotedExpr;
                            }

                            if (!NeuTypeFunctions.Eq(checkedArg.GetNeuType(), callee.Parameters[idx].Variable.Type)) {

                                error = error ??
                                    new TypeCheckError(
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
                ns: call.Namespace,
                call.Name, 
                checkedArgs, 
                returnType,
                calleDefType),
            error);
    }

    public static (CheckedCall, Error?) TypeCheckMethodCall(
        Call call,
        Stack stack,
        Span span,
        CheckedFile file,
        Int32 structId,
        SafetyMode safetyMode) {

        var checkedArgs = new List<(String, CheckedExpression)>();

        Error? error = null;

        NeuType returnType = new UnknownType();

        var (_callee, calleeDefType, resolveCallErr) = ResolveCall(call, span, stack, file.Structs[structId].Methods, file);

        error = error ?? resolveCallErr;

        if (_callee is CheckedFunction callee) {

            returnType = callee.ReturnType;

            // Check that we have the right number of arguments.

            if (callee.Parameters.Count != (call.Args.Count + 1)) {
                
                error = error ??
                    new TypeCheckError(
                        "wrong number of arguments",
                        span);
            }
            else {

                var idx = 0;

                // The first index should be the 'this'

                while (idx < call.Args.Count) {

                    var (checkedArg, chkExprErr) = TypeCheckExpression(call.Args[idx].Item2, stack, file, safetyMode);

                    error = error ?? chkExprErr;

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

                    var (_checkedArg, promoteErr) = TryPromoteConstantExprToType(
                        callee.Parameters[idx + 1].Variable.Type, 
                        checkedArg, 
                        call.Args[idx].Item2.GetSpan());

                    error = error ?? promoteErr;

                    checkedArg = _checkedArg ?? checkedArg;

                    if (!NeuTypeFunctions.Eq(checkedArg.GetNeuType(), callee.Parameters[idx + 1].Variable.Type)) {

                        error = error ?? 
                            new TypeCheckError(
                                "Parameter type mismatch",
                                call.Args[idx].Item2.GetSpan());
                    }

                    checkedArgs.Add((call.Args[idx].Item1, checkedArg));

                    idx += 1;
                }
            }
        }

        return (
            new CheckedCall(
                ns: new List<String>(),
                name: call.Name,
                args: checkedArgs,
                type: returnType,
                calleeDefType),
            error);
    }

    public static (NeuType, Error?) TypeCheckTypeName(
        UncheckedType uncheckedType,
        // Int32? possibleSelfStructId,
        Stack stack
        // ,
        // CheckedFile file,
        // Int32? structId
        ) {

        Error? error = null;

        switch (uncheckedType) {

            case UncheckedNameType nt: {

                switch (nt.Name) {

                    case "Int8": {

                        return (new Int8Type(), null);
                    }
                    
                    case "Int16": {

                        return (new Int16Type(), null);
                    }

                    case "Int32": {

                        return (new Int32Type(), null);
                    }

                    case "Int64": {

                        return (new Int64Type(), null);
                    }

                    case "UInt8": {

                        return (new UInt8Type(), null);
                    }
                    
                    case "UInt16": {

                        return (new UInt16Type(), null);
                    }

                    case "UInt32": {

                        return (new UInt32Type(), null);
                    }
                    
                    case "UInt64": {

                        return (new UInt64Type(), null);
                    }

                    case "Float": {

                        return (new FloatType(), null);
                    }
                    
                    case "Double": {

                        return (new DoubleType(), null);
                    }

                    case "CChar": {

                        return (new CCharType(), null);
                    }

                    case "CInt": {

                        return (new CIntType(), null);
                    }

                    case "String": {

                        return (new StringType(), null);
                    }

                    case "Bool": {

                        return (new BoolType(), null);
                    }

                    case "Void": {

                        return (new VoidType(), null);
                    }

                    case var x: {

                        var stackStruct = stack.FindStruct(x);
                        
                        // var fileStruct = file.FindStruct(x);

                        // switch (stackStruct ?? fileStruct) {
                        switch (stackStruct) {

                            case Int32 _structId: {

                                return (new StructType(_structId), null);
                            }

                            // case var _ when structId is Int32 sid: {

                            //     return (new StructType(sid), null);
                            // }

                            default: {

                                // Trace("ERROR: unknown type");

                                return (
                                    new UnknownType(),
                                    new TypeCheckError(
                                        "unknown type",
                                        nt.Span));
                            }
                        }
                    }
                }
            }

            case UncheckedEmptyType _: {

                return (new UnknownType(), null);
            }

            case UncheckedVectorType vt: {

                // var (innerType, innerTypeErr) = TypeCheckTypeName(vt.Type, stack, file, structId);
                var (innerType, innerTypeErr) = TypeCheckTypeName(vt.Type, stack);

                error = error ?? innerTypeErr;

                return (
                    new VectorType(innerType),
                    error);
            }

            case UncheckedOptionalType opt: {

                // var (innerType, err) = TypeCheckTypeName(opt.Type, stack, file, structId);
                var (innerType, err) = TypeCheckTypeName(opt.Type, stack);

                error = error ?? err;

                return (
                    new OptionalType(innerType),
                    error);
            }

            case UncheckedRawPointerType rp: {

                // var (innerType, err) = TypeCheckTypeName(rp.Type, stack, file, structId);
                var (innerType, err) = TypeCheckTypeName(rp.Type, stack);

                error = error ?? err;

                return (
                    new RawPointerType(innerType),
                    error);
            }

            default: {

                throw new Exception();
            }
        }
    }
}
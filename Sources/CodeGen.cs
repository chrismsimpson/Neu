
namespace Neu;

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String CodeGen(
        this Compiler compiler,
        Project project,
        Scope scope) {

        var output = new StringBuilder();

        output.Append("#include \"../../../Runtime/lib.h\"\n");

        foreach (var (_, structId) in scope.Structs) {

            var structure = project.Structs[structId];

            var structOutput = compiler.CodeGenStructPredecl(structure);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }
        
        output.Append('\n');

        foreach (var (_, structId) in scope.Structs) {

            var structure = project.Structs[structId];

            var structOutput = compiler.CodeGenStruct(structure, project);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, funcId) in scope.Funcs) {

            var func = project.Functions[funcId];

            var funcOutput = compiler.CodeGenFuncPredecl(func, project);
            
            if (func.Linkage != FunctionLinkage.ImplicitConstructor && func.Name != "main") {

                output.Append(funcOutput);

                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, funcId) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.External) {

                continue;
            }
            else if (func.Linkage == FunctionLinkage.ImplicitConstructor) {

                continue;
            }
            else {
                
                var funOutput = compiler.CodeGenFunc(func, project);

                output.Append(funOutput);

                output.Append("\n");
            }
        }

        return output.ToString();
    }

    public static String CodeGenStructPredecl(
        this Compiler compiler,
        CheckedStruct structure) {

        if (structure.DefinitionLinkage == DefinitionLinkage.External) {

            return String.Empty;
        }
        else {

            switch (structure.DefinitionType) {

                case DefinitionType.Class: {

                    return $"class {structure.Name};";
                }

                case DefinitionType.Struct: {

                    return $"struct {structure.Name};";
                }

                default: {

                    throw new Exception();
                }
            }
        }
    }

    public static String CodeGenStruct(
        this Compiler compiler,
        CheckedStruct structure,
        Project project) {

        if (structure.DefinitionLinkage == DefinitionLinkage.External) {

            return String.Empty;
        }

        var output = new StringBuilder();

        switch (structure.DefinitionType) {

            case DefinitionType.Class: {

                output.Append($"class {structure.Name} {{\n");
                
                // As we should test the visibility before codegen, we take a simple
                // approach to codegen
                
                output.Append("  public:\n");

                break;
            }

            case DefinitionType.Struct: {

                output.Append($"struct {structure.Name} {{\n");
                output.Append("  public:\n");

                break;
            }

            default: {

                throw new Exception();
            }
        }

        foreach (var field in structure.Fields) {

            output.Append(new String(' ', INDENT_SIZE));

            output.Append(compiler.CodeGenType(field.Type, project));
            
            output.Append(' ');
            
            output.Append(field.Name);
            
            output.Append(";\n");
        }

        var scope = project.Scopes[structure.ScopeId];

        foreach (var (_, funcId) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.ImplicitConstructor) {

                var funcOutput = compiler.CodeGenConstructor(func, project);

                output.Append(new String(' ', INDENT_SIZE));
                output.Append(funcOutput);
                output.Append('\n');
            }
            else {

                output.Append(new String(' ', INDENT_SIZE));

                if (func.IsStatic()) {

                    output.Append("static ");
                }

                var methodOutput = compiler.CodeGenFunc(func, project);

                output.Append(methodOutput);
            }
        }

        output.Append("};");

        return output.ToString();
    }

    public static String CodeGenFuncPredecl(
        this Compiler compiler,
        CheckedFunction fun,
        Project project) {
        
        var output = new StringBuilder();

        if (fun.Linkage == FunctionLinkage.External) {

            output.Append("extern ");
        }

        if (fun.Name == "main") {

            output.Append("int");
        }
        else {

            output.Append(compiler.CodeGenType(fun.ReturnType, project));
        }

        output.Append(' ');

        output.Append(fun.Name);

        output.Append('(');

        var first = true;

        foreach (var p in fun.Parameters) {

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }

            if (!p.Variable.Mutable) {

                output.Append("const ");
            }

            var ty = compiler.CodeGenType(p.Variable.Type, project);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Variable.Name);
        }

        output.Append(");");

        return output.ToString();
    }

    public static String CodeGenFunc(
        this Compiler compiler,
        CheckedFunction fun,
        Project project) {

        var output = new StringBuilder();

        if (fun.Name == "main") {

            output.Append("int");
        }
        else {

            output.Append(compiler.CodeGenType(fun.ReturnType, project));
        }

        output.Append(' ');

        if (fun.Name == "main") {

            output.Append("__neu_main");
        }
        else {

            output.Append(fun.Name);
        }

        output.Append('(');

        if (fun.Name == "main" && !fun.Parameters.Any()) {
        
            output.Append("RefVector<String>");
        }

        var first = true;

        var constFunc = false;

        foreach (var p in fun.Parameters) {

            if (p.Variable.Name == "this") {

                constFunc = !p.Variable.Mutable;

                continue;
            }

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }
            
            if (!p.Variable.Mutable) {

                output.Append("const ");
            }

            var ty = compiler.CodeGenType(p.Variable.Type, project);

            output.Append(ty);

            output.Append(' ');

            output.Append(p.Variable.Name);
        }

        output.Append(')');

        if (constFunc) {

            output.Append(" const");
        }

        if (fun.Name == "main") {
            
            output.Append("\n");
            output.Append("{\n");
            output.Append(new String(' ', INDENT_SIZE));
        }

        var block = compiler.CodeGenBlock(INDENT_SIZE, fun.Block, project);

        output.Append(block);

        if (fun.Name == "main") {
            
            output.Append(new String(' ', INDENT_SIZE));
            output.Append("return 0;\n}");
        }

        return output.ToString();
    }

    public static String CodeGenConstructor(
        this Compiler compiler,
        CheckedFunction func,
        Project project) {

        var output = new StringBuilder();

        output.Append(func.Name);
        
        output.Append('(');

        var first = true;
        
        foreach (var p in func.Parameters) {
            
            if (!first) {
                
                output.Append(", ");
            }
            else {

                first = false;
            }

            var ty = compiler.CodeGenType(p.Variable.Type, project);
            output.Append(ty);
            output.Append(" a_");
            output.Append(p.Variable.Name);
        }

        output.Append("): ");

        first = true;
        
        foreach (var p in func.Parameters) {

            if (!first) {
                
                output.Append(", ");
            } 
            else {
                
                first = false;
            }

            output.Append(p.Variable.Name);
            output.Append("(a_");
            output.Append(p.Variable.Name);
            output.Append(')');
        }

        output.Append("{}\n");

        return output.ToString();
    }

    public static String CodeGenType(
        this Compiler compiler,
        Int32 typeId,
        Project project) {

        var ty = project.Types[typeId];

        switch (ty) {

            case RawPointerType pt: {

                return $"{compiler.CodeGenType(pt.TypeId, project)}*";
            }

            case GenericType gt: {

                var output = new StringBuilder(project.Structs[gt.ParentStructId].Name);

                output.Append('<');

                var first = true;

                foreach (var t in gt.InnerTypeIds) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append(compiler.CodeGenType(t, project));
                }

                output.Append('>');

                return output.ToString();
            }

            case StructType st: return project.Structs[st.StructId].Name;

            case Builtin _: {

                switch (typeId) {
                    case Compiler.BoolTypeId: return "bool";
                    case Compiler.StringTypeId: return "String";
                    case Compiler.CCharTypeId: return "char";
                    case Compiler.CIntTypeId: return "int";
                    case Compiler.Int8TypeId: return "Int8";
                    case Compiler.Int16TypeId: return "Int16";
                    case Compiler.Int32TypeId: return "Int32";
                    case Compiler.Int64TypeId: return "Int64";
                    case Compiler.UInt8TypeId: return "UInt8";
                    case Compiler.UInt16TypeId: return "UInt16";
                    case Compiler.UInt32TypeId: return "UInt32";
                    case Compiler.UInt64TypeId: return "UInt64";
                    case Compiler.FloatTypeId: return "Float";
                    case Compiler.DoubleTypeId: return "Double";
                    case Compiler.VoidTypeId: return "void";
                    default: return "auto";
                }
            }

            default:    
                throw new Exception();
        }
    }

    public static String CodeGenBlock(
        this Compiler compiler,
        int indent,
        CheckedBlock block,
        Project project) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Stmts) {

            var stmtStr = compiler.CodeGenStatement(indent + INDENT_SIZE, stmt, project);

            output.Append(stmtStr);
        }

        output.Append(new String(' ', indent));

        output.Append("}\n");

        return output.ToString();
    }

    public static String CodeGenStatement(
        this Compiler compiler,
        int indent,
        CheckedStatement stmt,
        Project project) {

        var output = new StringBuilder();

        output.Append(new String(' ', indent));

        ///

        switch (stmt) {

            case CheckedExpression expr: {

                var exprStr = compiler.CodeGenExpr(indent, expr, project);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            ///

            case CheckedDeferStatement defer: {

                // NOTE: We let the preprocessor generate a unique name for the RAII helper.
                output.Append("#define __SCOPE_GUARD_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("ScopeGuard __SCOPE_GUARD_NAME  ([&] \n");
                output.Append("#undef __SCOPE_GUARD_NAME\n{");
                output.Append(compiler.CodeGenStatement(indent, defer.Statement, project));
                output.Append("});\n");

                break;
            }

            ///

            case CheckedReturnStatement rs: {

                var exprStr = compiler.CodeGenExpr(indent, rs.Expr, project);

                output.Append("return (");
                
                output.Append(exprStr);
                
                output.Append(");\n");

                break;
            }

            ///

            case CheckedIfStatement ifStmt: {

                var exprStr = compiler.CodeGenExpr(indent, ifStmt.Expr, project);

                output.Append("if (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.CodeGenBlock(indent, ifStmt.Block, project);
                
                output.Append(blockStr);

                if (ifStmt.Trailing is CheckedStatement e) {

                    output.Append(new String(' ', indent));

                    output.Append("else ");

                    var elseStr = compiler.CodeGenStatement(indent, e, project);

                    output.Append(elseStr);
                }

                break;
            }

            ///

            case CheckedWhileStatement whileStmt: {

                var exprStr = compiler.CodeGenExpr(indent, whileStmt.Expression, project);

                output.Append("while (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.CodeGenBlock(indent, whileStmt.Block, project);
                
                output.Append(blockStr);

                break;
            }

            ///

            case CheckedVarDeclStatement vd: {

                if (!vd.VarDecl.Mutable) {

                    output.Append("const ");
                }

                output.Append(compiler.CodeGenType(vd.VarDecl.Type, project));
                output.Append(" ");
                output.Append(vd.VarDecl.Name);
                output.Append(" = ");
                output.Append(compiler.CodeGenExpr(indent, vd.Expr, project));
                output.Append(";\n");

                break;
            }

            ///

            case CheckedBlockStatement chBlockStmt: {

                var blockStr = compiler.CodeGenBlock(indent, chBlockStmt.Block, project);

                output.Append(blockStr);

                break;
            }

            ///

            case CheckedGarbageStatement _: {

                // Incorrect parse/typecheck
                // Probably shouldn't be able to get to this point?

                break;
            }

            ///

            default: {

                throw new Exception();
            }
        }

        ///

        return output.ToString();
    }

    public static String CodeGenExpr(
        this Compiler compiler,
        int indent,
        CheckedExpression expr,
        Project project) {

        var output = new StringBuilder();

        switch (expr) {

            case CheckedOptionalNoneExpression _: {

                output.Append("{ }");

                break;
            }

            case CheckedOptionalSomeExpression o: {

                output.Append('(');
                output.Append(compiler.CodeGenExpr(indent, o.Expression, project));
                output.Append(')');

                break;
            }

            case CheckedForceUnwrapExpression f: {

                output.Append('(');
                output.Append(compiler.CodeGenExpr(indent, f.Expression, project));
                output.Append(".value())");

                break;
            }

            case CheckedQuotedStringExpression qs: {
            
                output.Append("String(\"");        
                output.Append(qs.Value);
                output.Append("\")");
            
                break;
            }

            case CheckedCharacterConstantExpression cce: {

                output.Append('\'');
                output.Append(cce.Char);
                output.Append('\'');

                break;
            }

            case CheckedNumericConstantExpression ne: {

                switch (ne.Value) {

                    case Int8Constant i8: {

                        output.Append("static_cast<Int8>(");
                        output.Append(i8.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case Int16Constant i16: {

                        output.Append("static_cast<Int16>(");
                        output.Append(i16.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case Int32Constant i32: {

                        output.Append("static_cast<Int32>(");
                        output.Append(i32.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case Int64Constant i64: {

                        output.Append("static_cast<Int64>(");
                        output.Append(i64.Value.ToString());
                        output.Append("LL)");

                        break;
                    }

                    case UInt8Constant u8: {

                        output.Append("static_cast<UInt8>(");
                        output.Append(u8.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case UInt16Constant u16: {

                        output.Append("static_cast<UInt16>(");
                        output.Append(u16.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case UInt32Constant u32: {

                        output.Append("static_cast<UInt32>(");
                        output.Append(u32.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case UInt64Constant u64: {

                        output.Append("static_cast<UInt64>(");
                        output.Append(u64.Value.ToString());
                        output.Append("ULL)");

                        break;
                    }

                    default: { 
                        
                        break;
                    }
                }

                break;
            }

            case CheckedVarExpression v: {

                output.Append(v.Variable.Name);

                break;
            }

            case CheckedBooleanExpression b: {

                if (b.Value) {

                    output.Append("true");
                }
                else {

                    output.Append("false");
                }

                break;
            }

            case CheckedCallExpression ce: {

                switch (ce.Call.Name) {

                    case "printLine": {

                        output.Append("outLine(\"{}\", ");
                        
                        foreach (var param in ce.Call.Args) {

                            output.Append(compiler.CodeGenExpr(indent, param.Item2, project));
                        }
                        
                        output.Append(")");

                        break;
                    }

                    case "warnLine": {

                        output.Append("warnLine(\"{}\", ");
                        
                        foreach (var param in ce.Call.Args) {

                            output.Append(compiler.CodeGenExpr(indent, param.Item2, project));
                        }
                        
                        output.Append(")");

                        break;
                    }

                    ///

                    default: {

                        foreach (var ns in ce.Call.Namespace) {

                            output.Append(ns);

                            if (ce.Call.CalleeDefinitionType == DefinitionType.Struct) {

                                output.Append(".");
                            }
                            else {

                                output.Append("::");
                            }
                        }

                        output.Append(ce.Call.Name);

                        output.Append("(");

                        var first = true;

                        foreach (var parameter in ce.Call.Args) {

                            if (!first) {

                                output.Append(", ");
                            }
                            else {

                                first = false;
                            }

                            output.Append(compiler.CodeGenExpr(indent, parameter.Item2, project));
                        }

                        output.Append(")");

                        break;
                    }
                }

                break;
            }

            case CheckedMethodCallExpression mce: {

                output.Append('(');

                output.Append('(');
                output.Append(compiler.CodeGenExpr(indent, mce.Expression, project));
                output.Append(")");

                switch (mce.Expression) {

                    case CheckedVarExpression ve when ve.Variable.Name == "this": {

                        output.Append("->");

                        break;
                    }

                    default: {

                        output.Append('.');

                        break;
                    }
                }

                output.Append(mce.Call.Name);
                output.Append('(');
                
                var first = true;

                foreach (var param in mce.Call.Args) {
                    
                    if (!first) {

                        output.Append(", ");
                    } 
                    else {

                        first = false;
                    }

                    output.Append(compiler.CodeGenExpr(indent, param.Item2, project));
                }

                output.Append("))");

                break;
            }

            case CheckedUnaryOpExpression unaryOp: {

                output.Append('(');

                switch (unaryOp.Operator) {

                    case PreIncrementUnaryOperator _: {

                        output.Append("++");

                        break;
                    }

                    case PreDecrementUnaryOperator _: {

                        output.Append("--");

                        break;
                    }

                    case NegateUnaryOperator _: {

                        output.Append('-');

                        break;
                    }

                    case DereferenceUnaryOperator _: {

                        output.Append('*');

                        break;
                    }

                    case RawAddressUnaryOperator _: {

                        output.Append('&');

                        break;
                    }

                    case LogicalNotUnaryOperator _: {

                        output.Append('!');

                        break;
                    }

                    case BitwiseNotUnaryOperator _: {

                        output.Append('~');

                        break;
                    }

                    case TypeCastUnaryOperator tc: {

                        switch (tc.TypeCast) {

                            case FallibleTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {
                            
                                    output.Append("fallibleIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case InfallibleTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {

                                    output.Append("infallibleIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case SaturatingTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {

                                    output.Append("saturatingIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case TruncatingTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {

                                    output.Append("truncatingIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            default: {

                                break;
                            }
                        }

                        output.Append('<');
                        output.Append(compiler.CodeGenType(unaryOp.Type, project));
                        output.Append(">(");

                        break;
                    }

                    default: {

                        break;
                    }
                }

                output.Append(compiler.CodeGenExpr(indent, unaryOp.Expression, project));

                switch (unaryOp.Operator) {

                    case PostIncrementUnaryOperator _: {

                        output.Append("++");

                        break;
                    }

                    case PostDecrementUnaryOperator _: {

                        output.Append("--");

                        break;
                    }

                    case TypeCastUnaryOperator _: {

                        output.Append(')');

                        break;
                    }

                    default: {

                        break;
                    }
                }
                
                output.Append(')');

                break;
            }

            case CheckedBinaryOpExpression binOp: {

                output.Append("(");

                output.Append(compiler.CodeGenExpr(indent, binOp.Lhs, project));

                switch (binOp.Operator) {

                    case BinaryOperator.Add: {

                        output.Append(" + ");

                        break;
                    }

                    case BinaryOperator.Subtract: {

                        output.Append(" - ");

                        break;
                    }

                    case BinaryOperator.Multiply: {

                        output.Append(" * ");

                        break;
                    }

                    case BinaryOperator.Modulo: {

                        output.Append(" % ");

                        break;
                    }

                    case BinaryOperator.Divide: {

                        output.Append(" / ");

                        break;
                    }

                    case BinaryOperator.Assign: {

                        output.Append(" = ");

                        break;
                    }
                    
                    case BinaryOperator.AddAssign: {

                        output.Append(" += ");

                        break;
                    }

                    case BinaryOperator.SubtractAssign: {

                        output.Append(" -= ");

                        break;
                    }

                    case BinaryOperator.MultiplyAssign: {

                        output.Append(" *= ");

                        break;
                    }

                    case BinaryOperator.ModuloAssign: {

                        output.Append(" %= ");

                        break;
                    }

                    case BinaryOperator.DivideAssign: {

                        output.Append(" /= ");

                        break;
                    }

                    case BinaryOperator.BitwiseAndAssign: { 
                        
                        output.Append(" &= "); 

                        break;
                    }

                    case BinaryOperator.BitwiseOrAssign: { 
                        
                        output.Append(" |= "); 

                        break;
                    }
                    
                    case BinaryOperator.BitwiseXorAssign: { 
                        
                        output.Append(" ^= ");

                        break;
                    }
                    
                    case BinaryOperator.BitwiseLeftShiftAssign: { 
                        
                        output.Append(" <<= "); 

                        break;
                    }
                    
                    case BinaryOperator.BitwiseRightShiftAssign: { 
                        
                        output.Append(" >>= ");

                        break;
                    }

                    case BinaryOperator.Equal: {

                        output.Append(" == ");

                        break;
                    }

                    case BinaryOperator.NotEqual: {

                        output.Append(" != ");

                        break;
                    }

                    case BinaryOperator.LessThan: {

                        output.Append(" < ");

                        break;
                    }

                    case BinaryOperator.LessThanOrEqual: {

                        output.Append(" <= ");

                        break;
                    }

                    case BinaryOperator.GreaterThan: {

                        output.Append(" > ");

                        break;
                    }

                    case BinaryOperator.GreaterThanOrEqual: {

                        output.Append(" >= ");

                        break;
                    }

                    case BinaryOperator.LogicalAnd: {

                        output.Append(" && ");

                        break;
                    }

                    case BinaryOperator.LogicalOr: {

                        output.Append(" || ");

                        break;
                    }

                    case BinaryOperator.BitwiseAnd: {

                        output.Append(" & ");

                        break;
                    }

                    case BinaryOperator.BitwiseOr: {
                        
                        output.Append(" | ");

                        break;
                    }

                    case BinaryOperator.BitwiseXor: {

                        output.Append(" ^ ");

                        break;
                    }

                    case BinaryOperator.BitwiseLeftShift: {

                        output.Append(" << ");

                        break;
                    }

                    case BinaryOperator.BitwiseRightShift: {

                        output.Append(" >> ");

                        break;
                    }

                    default: {
                        
                        throw new Exception("Cannot codegen garbage operator");
                    }
                }

                output.Append(compiler.CodeGenExpr(indent, binOp.Rhs, project));

                output.Append(")");

                break;
            }

            case CheckedVectorExpression ve: {

                if (ve.FillSize is CheckedExpression fillSize) {

                    output.Append("(RefVector<");
                    output.Append(compiler.CodeGenType(ve.Expressions.First().GetNeuType(), project));
                    output.Append(">::filled(");
                    output.Append(compiler.CodeGenExpr(indent, fillSize, project));
                    output.Append(", ");
                    output.Append(compiler.CodeGenExpr(indent, ve.Expressions.First(), project));
                    output.Append("))");
                }
                else {

                    // (RefVector({1, 2, 3}))

                    output.Append("(RefVector({");

                    var first = true;

                    foreach (var val in ve.Expressions) {
                        
                        if (!first) {
                            
                            output.Append(", ");
                        } 
                        else {
                            
                            first = false;
                        }

                        output.Append(compiler.CodeGenExpr(indent, val, project));
                    }

                    output.Append("}))");
                }

                break;
            }

            case CheckedTupleExpression te: {

                // (Tuple{1, 2, 3})

                output.Append("(Tuple{");
                
                var first = true;

                foreach (var val in te.Expressions) {

                    if (!first) {
                        output.Append(", ");
                    } 
                    else {

                        first = false;
                    }

                    output.Append(compiler.CodeGenExpr(indent, val, project));
                }

                output.Append("})");

                break;
            }

            case CheckedIndexedExpression ie: {

                output.Append("((");
            
                output.Append(compiler.CodeGenExpr(indent, ie.Expression, project));
            
                output.Append(")[");
            
                output.Append(compiler.CodeGenExpr(indent, ie.Index, project));
            
                output.Append("])");

                break;
            }

            case CheckedIndexedTupleExpression ite: {

                // x.get<1>()
                
                output.Append("((");
                output.Append(compiler.CodeGenExpr(indent, ite.Expression, project));
                output.Append($").get<{ite.Index}>())");

                break;
            }

            case CheckedIndexedStructExpression ise: {

                // x.someName
                
                output.Append("((");
                output.Append(compiler.CodeGenExpr(indent, ise.Expression, project));
                output.Append(')');

                switch (ise.Expression) {

                    case CheckedVarExpression ve when ve.Variable.Name == "this": {

                        output.Append("->");

                        break;
                    }

                    default: {

                        output.Append('.');

                        break;
                    }
                }
            
                output.Append($"{ise.Name})");

                break;
            }
            
            case CheckedGarbageExpression _: {

                // Incorrect parse/typecheck
                // Probably shouldn't be able to get to this point?

                break;
            }

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }
}
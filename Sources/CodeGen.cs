
namespace Neu;

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String Translate(
        this Compiler compiler,
        CheckedFile file) {

        var output = new StringBuilder();

        output.Append("#include \"../../../Runtime/lib.h\"\n");

        foreach (var structure in file.Structs) {

            var structOutput = compiler.TranslateStructPredecl(structure);

            output.Append(structOutput);

            output.Append('\n');
        }
        
        output.Append('\n');

        foreach (var structure in file.Structs) {

            var structOutput = compiler.TranslateStruct(structure, file);

            output.Append(structOutput);
            
            output.Append('\n');
        }

        output.Append('\n');

        foreach (var func in file.Functions) {

            var funcOutput = compiler.TranslateFuncPredecl(func, file);
            
            if (func.Linkage != FunctionLinkage.ImplicitConstructor) {

                output.Append(funcOutput);

                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var func in file.Functions) {

            if (func.Linkage == FunctionLinkage.External) {

                continue;
            }
            else if (func.Linkage == FunctionLinkage.ImplicitConstructor) {

                var funcOutput = compiler.TranslateConstructor(func, file);

                output.Append(funcOutput);

                output.Append("\n");
            }
            else {
                
                var funOutput = compiler.TranslateFunc(func, file);

                output.Append(funOutput);

                output.Append("\n");
            }
        }

        return output.ToString();
    }

    public static String TranslateStructPredecl(
        this Compiler compiler,
        CheckedStruct structure) {

        return $"struct {structure.Name};";
    }

    public static String TranslateStruct(
        this Compiler compiler,
        CheckedStruct structure,
        CheckedFile file) {

        var output = new StringBuilder($"struct {structure.Name} {{\n");

        foreach (var member in structure.Members) {

            output.Append(new String(' ', INDENT_SIZE));

            output.Append(compiler.TranslateType(member.Type, file));
            
            output.Append(' ');
            
            output.Append(member.Name);
            
            output.Append(";\n");
        }

        // Put together our own constructor
        // eg) Person(String name, i64 age);

        output.Append(new String(' ', INDENT_SIZE));
        output.Append(structure.Name);
        output.Append('(');

        var first = true;

        foreach (var member in structure.Members) {

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }
            
            output.Append(compiler.TranslateType(member.Type, file));
            output.Append(' ');
            output.Append(member.Name);
        }

        output.Append(");\n");

        output.Append("};");

        return output.ToString();
    }

    public static String TranslateFuncPredecl(
        this Compiler compiler,
        CheckedFunction fun,
        CheckedFile file) {
        
        var output = new StringBuilder();

        if (fun.Linkage == FunctionLinkage.External) {

            output.Append("extern ");
        }

        if (fun.Name == "main") {

            output.Append("int");
        }
        else {

            output.Append(compiler.TranslateType(fun.ReturnType, file));
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

            var ty = compiler.TranslateType(p.Variable.Type, file);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Variable.Name);
        }

        output.Append(");");

        return output.ToString();
    }

    public static String TranslateFunc(
        this Compiler compiler,
        CheckedFunction fun,
        CheckedFile file) {

        var output = new StringBuilder();

        if (fun.Name == "main") {

            output.Append("int");
        }
        else {

            output.Append(compiler.TranslateType(fun.ReturnType, file));
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
                
            var ty = compiler.TranslateType(p.Variable.Type, file);

            output.Append(ty);

            output.Append(' ');

            output.Append(p.Variable.Name);
        }

        output.Append(')');

        var block = compiler.TranslateBlock(0, fun.Block, file);

        output.Append(block);

        return output.ToString();
    }

    public static String TranslateConstructor(
        this Compiler compiler,
        CheckedFunction func,
        CheckedFile file) {

        var output = new StringBuilder();

        output.Append(compiler.TranslateType(func.ReturnType, file));

        output.Append("::");

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

            var ty = compiler.TranslateType(p.Variable.Type, file);
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

    public static String TranslateType(
        this Compiler compiler,
        NeuType ty,
        CheckedFile file) {

        switch (ty) {

            case BoolType _: {

                return "bool";
            }

            case StringType _: {

                return "String";
            }

            case Int8Type _: {

                return "Int8";
            }

            case Int16Type _: {

                return "Int16";
            }

            case Int32Type _: {

                return "Int32";
            }

            case Int64Type _: {

                return "Int64";
            }

            case UInt8Type _: {

                return "UInt8";
            }

            case UInt16Type _: {

                return "UInt16";
            }

            case UInt32Type _: {

                return "UInt32";
            }

            case UInt64Type _: {

                return "UInt64";
            }

            case FloatType _: {

                return "Float";
            }

            case DoubleType _: {

                return "Double";
            }

            case VoidType _: {
                
                return "void";
            }

            case VectorType vt: {

                return $"Vector<{compiler.TranslateType(vt.Type, file)}>";
            }

            case TupleType tt: {

                var output = new StringBuilder("Tuple<");

                var first = true;

                foreach (var t in tt.Types) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append(compiler.TranslateType(t, file));
                }

                output.Append('>');

                return output.ToString();
            }

            case OptionalType ot: {

                return $"Optional<{compiler.TranslateType(ot.Type, file)}>";
            }

            case StructType st: {

                return file.Structs[ToInt32(st.StructId)].Name;
            }

            case UnknownType _: {

                return "auto";
            }

            ///

            default: {

                throw new Exception();
            }
        }
    }

    public static String TranslateBlock(
        this Compiler compiler,
        int indent,
        CheckedBlock block,
        CheckedFile file) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Stmts) {

            var stmtStr = compiler.TranslateStmt(indent + INDENT_SIZE, stmt, file);

            output.Append(stmtStr);
        }

        output.Append(new String(' ', indent));

        output.Append("}\n");

        return output.ToString();
    }

    public static String TranslateStmt(
        this Compiler compiler,
        int indent,
        CheckedStatement stmt,
        CheckedFile file) {

        var output = new StringBuilder();

        output.Append(new String(' ', indent));

        ///

        switch (stmt) {

            case CheckedExpression expr: {

                var exprStr = compiler.TranslateExpr(indent, expr, file);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            ///

            case CheckedDeferStatement defer: {

                output.Append("#define __SCOPE_GUARD_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("ScopeGuard __SCOPE_GUARD_NAME  ([&] \n");
                output.Append("#undef __SCOPE_GUARD_NAME\n");
                output.Append(compiler.TranslateBlock(indent, defer.Block, file));
                output.Append(");\n");

                break;
            }

            ///

            case CheckedReturnStatement rs: {

                var exprStr = compiler.TranslateExpr(indent, rs.Expr, file);

                output.Append("return (");
                
                output.Append(exprStr);
                
                output.Append(");\n");

                break;
            }

            ///

            case CheckedIfStatement ifStmt: {

                var exprStr = compiler.TranslateExpr(indent, ifStmt.Expr, file);

                output.Append("if (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, ifStmt.Block, file);
                
                output.Append(blockStr);

                if (ifStmt.Trailing is CheckedStatement e) {

                    output.Append(new String(' ', indent));

                    output.Append("else ");

                    var elseStr = compiler.TranslateStmt(indent, e, file);

                    output.Append(elseStr);
                }

                break;
            }

            ///

            case CheckedWhileStatement whileStmt: {

                var exprStr = compiler.TranslateExpr(indent, whileStmt.Expression, file);

                output.Append("while (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, whileStmt.Block, file);
                
                output.Append(blockStr);

                break;
            }

            ///

            case CheckedVarDeclStatement vd: {

                if (!vd.VarDecl.Mutable) {

                    output.Append("const ");
                }

                output.Append(compiler.TranslateType(vd.VarDecl.Type, file));
                output.Append(" ");
                output.Append(vd.VarDecl.Name);
                output.Append(" = ");
                output.Append(compiler.TranslateExpr(indent, vd.Expr, file));
                output.Append(";\n");

                break;
            }

            ///

            case CheckedBlockStatement chBlockStmt: {

                var blockStr = compiler.TranslateBlock(indent, chBlockStmt.Block, file);

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

    public static String TranslateExpr(
        this Compiler compiler,
        int indent,
        CheckedExpression expr,
        CheckedFile file) {

        var output = new StringBuilder();

        switch (expr) {

            case CheckedOptionalNoneExpression _: {

                output.Append("{ }");

                break;
            }

            ///

            case CheckedOptionalSomeExpression o: {

                output.Append('(');
                output.Append(compiler.TranslateExpr(indent, o.Expression, file));
                output.Append(')');

                break;
            }

            case CheckedForceUnwrapExpression f: {

                output.Append('(');
                output.Append(compiler.TranslateExpr(indent, f.Expression, file));
                output.Append(".value())");

                break;
            }

            ///

            case CheckedQuotedStringExpression qs: {
            
                output.Append("String(\"");        
                output.Append(qs.Value);
                output.Append("\")");
            
                break;
            }

            ///

            case CheckedInt64Expression i: {

                output.Append("static_cast<Int64>(");
                output.Append($"{i.Value}");
                output.Append("LL)");

                break;
            }

            ///

            case CheckedVarExpression v: {

                output.Append(v.Variable.Name);

                break;
            }

            ///

            case CheckedBooleanExpression b: {

                if (b.Value) {

                    output.Append("true");
                }
                else {

                    output.Append("false");
                }

                break;
            }

            ///

            case CheckedCallExpression ce: {

                switch (ce.Call.Name) {

                    case "print":

                        output.Append("outLine(\"{}\", ");
                        
                        foreach (var param in ce.Call.Args) {

                            output.Append(compiler.TranslateExpr(indent, param.Item2, file));
                        }
                        
                        output.Append(")");

                        break;

                    ///

                    default:

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

                            output.Append(compiler.TranslateExpr(indent, parameter.Item2, file));
                        }

                        output.Append(")");

                        break;
                }

                break;
            }

            ///

            case CheckedUnaryOpExpression unaryOp: {

                output.Append('(');

                switch (unaryOp.Operator) {

                    case UnaryOperator.PreIncrement: {

                        output.Append("++");

                        break;
                    }

                    case UnaryOperator.PreDecrement: {

                        output.Append("--");

                        break;
                    }

                    default: {

                        break;
                    }
                }

                output.Append(compiler.TranslateExpr(indent, unaryOp.Expression, file));

                switch (unaryOp.Operator) {

                    case UnaryOperator.PostIncrement: {

                        output.Append("++");

                        break;
                    }

                    case UnaryOperator.PostDecrement: {

                        output.Append("--");

                        break;
                    }

                    default: {

                        break;
                    }
                }
                
                output.Append(')');

                break;
            }

            ///

            case CheckedBinaryOpExpression binOp: {

                output.Append("(");

                output.Append(compiler.TranslateExpr(indent, binOp.Lhs, file));

                switch (binOp.Operator) {

                    case BinaryOperator.Add: {

                        output.Append(" + ");

                        break;
                    }

                    ///

                    case BinaryOperator.Subtract: {

                        output.Append(" - ");

                        break;
                    }
                    
                    ///

                    case BinaryOperator.Multiply: {

                        output.Append(" * ");

                        break;
                    }

                    ///

                    case BinaryOperator.Modulo: {

                        output.Append(" % ");

                        break;
                    }

                    ///

                    case BinaryOperator.Divide: {

                        output.Append(" / ");

                        break;
                    }

                    ///

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

                    ///

                    case BinaryOperator.Equal: {

                        output.Append(" == ");

                        break;
                    }

                    ///

                    case BinaryOperator.NotEqual: {

                        output.Append(" != ");

                        break;
                    }

                    ///

                    case BinaryOperator.LessThan: {

                        output.Append(" < ");

                        break;
                    }

                    ///

                    case BinaryOperator.LessThanOrEqual: {

                        output.Append(" <= ");

                        break;
                    }

                    ///

                    case BinaryOperator.GreaterThan: {

                        output.Append(" > ");

                        break;
                    }

                    ///

                    case BinaryOperator.GreaterThanOrEqual: {

                        output.Append(" >= ");

                        break;
                    }

                    ///

                    default: {
                        
                        throw new Exception("Cannot codegen garbage operator");
                    }
                }

                output.Append(compiler.TranslateExpr(indent, binOp.Rhs, file));

                output.Append(")");

                break;
            }

            ///

            case CheckedVectorExpression ve: {

                // (Vector({1, 2, 3}))

                output.Append("(Vector({");

                var first = true;

                foreach (var val in ve.Expressions) {
                    
                    if (!first) {
                        
                        output.Append(", ");
                    } 
                    else {
                        
                        first = false;
                    }

                    output.Append(compiler.TranslateExpr(indent, val, file));
                }

                output.Append("}))");

                break;
            }

            ///

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

                    output.Append(compiler.TranslateExpr(indent, val, file));
                }

                output.Append("})");

                break;
            }

            ///

            case CheckedIndexedExpression ie: {

                output.Append("((");
            
                output.Append(compiler.TranslateExpr(indent, ie.Expression, file));
            
                output.Append(")[");
            
                output.Append(compiler.TranslateExpr(indent, ie.Index, file));
            
                output.Append("])");

                break;
            }

            ///

            case CheckedIndexedTupleExpression ite: {

                // x.get<1>()
                
                output.Append("((");
                output.Append(compiler.TranslateExpr(indent, ite.Expression, file));
                output.Append($").get<{ite.Index}>())");

                break;
            }

            ///

            case CheckedIndexedStructExpression ise: {

                // x.someName
                
                output.Append("((");
                output.Append(compiler.TranslateExpr(indent, ise.Expression, file));
                output.Append($").{ise.Name})");

                break;
            }

            ///
            
            case CheckedGarbageExpression _: {

                // Incorrect parse/typecheck
                // Probably shouldn't be able to get to this point?

                break;
            }

            ///

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }
}
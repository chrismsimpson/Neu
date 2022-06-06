
namespace Neu;

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String Translate(
        this Compiler compiler,
        CheckedFile file) {

        var output = new StringBuilder();

        output.Append("#include <iostream>\n");

        output.Append("#include \"../../Runtime/lib.h\"\n");

        foreach (var fun in file.CheckedFunctions) {

            var funOutput = compiler.TranslateFunctionPredeclaration(fun);

            output.Append(funOutput);

            output.Append("\n");
        }

        foreach (var fun in file.CheckedFunctions) {

            var funOutput = compiler.TranslateFunction(fun);

            output.Append(funOutput);

            output.Append("\n");
        }

        return output.ToString();

    }

    public static String TranslateFunction(
        this Compiler compiler,
        CheckedFunction fun) {

        var output = new StringBuilder();

        if (fun.Name == "main") {

            output.Append("int");
        }
        else {

            output.Append(compiler.TranslateType(fun.ReturnType));
        }

        output.Append(" ");

        output.Append(fun.Name);

        output.Append("(");

        var first = true;

        foreach (var p in fun.Parameters) {

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }
                
            var ty = compiler.TranslateType(p.Item2);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Item1);
        }

        output.Append(")");

        var block = compiler.TranslateBlock(0, fun.Block, returnZero: fun.Name == "main");

        output.Append(block);

        return output.ToString();
    }

    public static String TranslateFunctionPredeclaration(
        this Compiler compiler,
        CheckedFunction fun) {

        if (fun.Name == "main") {

            return String.Empty;
        }

        var output = new StringBuilder();

        output.Append(compiler.TranslateType(fun.ReturnType));

        output.Append(" ");

        output.Append(fun.Name);

        output.Append("(");

        var first = true;

        foreach (var p in fun.Parameters) {

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }

            var ty = compiler.TranslateType(p.Item2);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Item1);
        }

        output.Append(");");

        return output.ToString();
    }

    public static String TranslateType(
        this Compiler compiler,
        NeuType ty) {

        switch (ty) {

            case BoolType _: {

                return "bool";
            }

            case StringType _: {

                return "char const*";
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

            ///

            case VoidType _: {
                
                return "void";
            }

            ///

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
        bool returnZero = false) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Stmts) {

            var stmtStr = compiler.TranslateStmt(indent + INDENT_SIZE, stmt);

            output.Append(stmtStr);
        }

        if (returnZero) {

            output.Append("\n");

            output.Append(new String(' ', indent + INDENT_SIZE));

            output.Append("return 0;\n");
        }

        output.Append(new String(' ', indent));

        output.Append("}\n");

        return output.ToString();
    }

    public static String TranslateStmt(
        this Compiler compiler,
        int indent,
        CheckedStatement stmt) {

        var output = new StringBuilder();

        output.Append(new String(' ', indent));

        ///

        switch (stmt) {

            case CheckedExpression expr: {

                var exprStr = compiler.TranslateExpr(indent, expr);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            ///

            case CheckedDeferStatement defer: {

                output.Append("#define __DEFER_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("Defer __DEFER_NAME  ([&] \n");
                output.Append("#undef __DEFER_NAME\n");
                output.Append(compiler.TranslateBlock(indent, defer.Block));
                output.Append(");\n");

                break;
            }

            ///

            case CheckedReturnStatement rs: {

                var exprStr = compiler.TranslateExpr(indent, rs.Expr);

                output.Append("return (");
                
                output.Append(exprStr);
                
                output.Append(");\n");

                break;
            }

            ///

            case CheckedIfStatement ifStmt: {

                var exprStr = compiler.TranslateExpr(indent, ifStmt.Expr);

                output.Append("if (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, ifStmt.Block);
                
                output.Append(blockStr);

                break;
            }

            ///

            case CheckedWhileStatement whileStmt: {

                var exprStr = compiler.TranslateExpr(indent, whileStmt.Expression);

                output.Append("while (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, whileStmt.Block);
                
                output.Append(blockStr);

                break;
            }

            ///

            case CheckedVarDeclStatement vd: {

                if (!vd.VarDecl.Mutable) {

                    output.Append("const ");
                }

                output.Append(compiler.TranslateType(vd.VarDecl.Type));
                output.Append(" ");
                output.Append(vd.VarDecl.Name);
                output.Append(" = ");
                output.Append(compiler.TranslateExpr(indent, vd.Expr));
                output.Append(";\n");

                break;
            }

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
        CheckedExpression expr) {

        var output = new StringBuilder();

        switch (expr) {

            case CheckedQuotedStringExpression qs: {
                
                output.Append("\"");
                output.Append(qs.Value);
                output.Append("\"");

                break;
            }

            ///

            case CheckedInt64Expression i: {

                output.Append($"{i.Value}");

                break;
            }

            ///

            case CheckedVarExpression v: {

                output.Append(v.Name);

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

                        output.Append("std::cout << ");
                        
                        output.Append("(");

                        foreach (var param in ce.Call.Args) {

                            output.Append(compiler.TranslateExpr(indent, param.Item2));
                        }
                        
                        output.Append(") << std::endl");

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

                            output.Append(compiler.TranslateExpr(indent, parameter.Item2));
                        }

                        output.Append(")");

                        break;
                }

                break;
            }

            ///

            case CheckedBinaryOpExpression binOp: {

                output.Append("(");

                output.Append(compiler.TranslateExpr(indent, binOp.Lhs));

                switch (binOp.Operator) {

                    case Operator.Add: {

                        output.Append(" + ");

                        break;
                    }

                    ///

                    case Operator.Subtract: {

                        output.Append(" - ");

                        break;
                    }
                    
                    ///

                    case Operator.Multiply: {

                        output.Append(" * ");

                        break;
                    }

                    ///

                    case Operator.Divide: {

                        output.Append(" / ");

                        break;
                    }

                    ///

                    case Operator.Assign: {

                        output.Append(" = ");

                        break;
                    }
                    
                    case Operator.AddAssign: {

                        output.Append(" += ");

                        break;
                    }

                    case Operator.SubtractAssign: {

                        output.Append(" -= ");

                        break;
                    }

                    case Operator.MultiplyAssign: {

                        output.Append(" *= ");

                        break;
                    }

                    case Operator.DivideAssign: {

                        output.Append(" /= ");

                        break;
                    }

                    ///

                    case Operator.Equal: {

                        output.Append(" == ");

                        break;
                    }

                    ///

                    case Operator.NotEqual: {

                        output.Append(" != ");

                        break;
                    }

                    ///

                    case Operator.LessThan: {

                        output.Append(" < ");

                        break;
                    }

                    ///

                    case Operator.LessThanOrEqual: {

                        output.Append(" <= ");

                        break;
                    }

                    ///

                    case Operator.GreaterThan: {

                        output.Append(" > ");

                        break;
                    }

                    ///

                    case Operator.GreaterThanOrEqual: {

                        output.Append(" >= ");

                        break;
                    }

                    ///

                    default: {
                        
                        throw new Exception("Cannot codegen garbage operator");
                    }
                }

                output.Append(compiler.TranslateExpr(indent, binOp.Rhs));

                output.Append(")");

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

namespace Neu;

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String Translate(
        this Compiler compiler,
        ParsedFile file) {

        var output = new StringBuilder();

        output.Append("#include <iostream>\n");

        output.Append("#include \"../../Runtime/lib.h\"\n");

        foreach (var fun in file.Functions) {

            var funOutput = compiler.TranslateFunctionPredeclaration(fun);

            output.Append(funOutput);

            output.Append("\n");
        }

        foreach (var fun in file.Functions) {

            var funOutput = compiler.TranslateFunction(fun);

            output.Append(funOutput);

            output.Append("\n");
        }

        return output.ToString();
    }

    public static String TranslateFunction(
        this Compiler compiler,
        Function fun) {

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
        Function fun) {

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

                return "char*";
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

            default: {

                throw new Exception();
            }
        }
    }

    public static String TranslateBlock(
        this Compiler compiler,
        int indent,
        Block block,
        bool returnZero = false) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Statements) {

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
        Statement stmt) {

        var output = new StringBuilder();

        output.Append(new String(' ', indent));

        ///

        switch (stmt) {

            case Expression expr: {

                var exprStr = compiler.TranslateExpr(indent, expr);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            ///

            case DeferStatement defer: {

                output.Append("#define __DEFER_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("Defer __DEFER_NAME  ([&] \n");
                output.Append("#undef __DEFER_NAME\n");
                output.Append(compiler.TranslateBlock(indent, defer.Block));
                output.Append(");\n");

                break;
            }

            ///

            case ReturnStatement rs: {

                var exprStr = compiler.TranslateExpr(indent, rs.Expr);

                output.Append("return (");
                
                output.Append(exprStr);
                
                output.Append(");\n");

                break;
            }

            ///

            case IfStatement ifStmt: {

                var exprStr = compiler.TranslateExpr(indent, ifStmt.Expr);

                output.Append("if (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, ifStmt.Block);
                
                output.Append(blockStr);

                break;
            }

            ///

            case WhileStatement whileStmt: {

                var exprStr = compiler.TranslateExpr(indent, whileStmt.Expr);

                output.Append("while (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = compiler.TranslateBlock(indent, whileStmt.Block);
                
                output.Append(blockStr);

                break;
            }

            ///

            case VarDeclStatement vd: {

                if (!vd.Decl.Mutable) {

                    output.Append("const ");
                }

                output.Append(compiler.TranslateType(vd.Decl.Type));
                output.Append(" ");
                output.Append(vd.Decl.Name);
                output.Append(" = ");
                output.Append(compiler.TranslateExpr(indent, vd.Expr));
                output.Append(";\n");

                break;
            }

            case GarbageStatement _: {

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
        Expression expr) {

        var output = new StringBuilder();

        switch (expr) {

            case QuotedStringExpression qs: {
                
                output.Append("\"");
                output.Append(qs.Value);
                output.Append("\"");

                break;
            }

            ///

            case Int64Expression i: {

                output.Append($"{i.Value}");

                break;
            }

            ///

            case VarExpression v: {

                output.Append(v.Value);

                break;
            }

            ///

            case BooleanExpression b: {

                if (b.Value) {

                    output.Append("true");
                }
                else {

                    output.Append("false");
                }

                break;
            }

            ///

            case CallExpression ce: {

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

            case BinaryOpExpression binOp: {

                output.Append("(");

                output.Append(compiler.TranslateExpr(indent, binOp.Lhs));

                switch (binOp.Op) {

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Add: {

                        output.Append(" + ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Subtract: {

                        output.Append(" - ");

                        break;
                    }
                    
                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Multiply: {

                        output.Append(" * ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Divide: {

                        output.Append(" / ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Assign: {

                        output.Append(" = ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.Equal: {

                        output.Append(" == ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.NotEqual: {

                        output.Append(" != ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.LessThan: {

                        output.Append(" < ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.LessThanOrEqual: {

                        output.Append(" <= ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.GreaterThan: {

                        output.Append(" > ");

                        break;
                    }

                    ///

                    case OperatorExpression opExpr when opExpr.Operator == Operator.GreaterThanOrEqual: {

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

            case GarbageExpression _: {

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
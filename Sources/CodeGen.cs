
namespace Neu;

public static partial class CodeGenFunctions {

    public static String Translate(
        this Compiler compiler,
        ParsedFile file) {

        var output = new StringBuilder();

        output.Append("#include <stdio.h>\n");
        output.Append("#include \"../../Runtime/lib.h\"\n");

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

        output.Append("void ");

        output.Append(fun.Name);

        output.Append("()");

        var block = compiler.TranslateBlock(fun.Block);

        output.Append(block);

        return output.ToString();
    }

    public static String TranslateBlock(
        this Compiler compiler,
        Block block) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Statements) {

            var stmtStr = compiler.TranslateStmt(stmt);

            output.Append(stmtStr);
        }

        output.Append("}\n");

        return output.ToString();
    }

    public static String TranslateStmt(
        this Compiler compiler,
        Statement stmt) {

        var output = new StringBuilder();

        ///

        switch (stmt) {

            case Expression expr:

                var exprStr = compiler.TranslateExpr(expr);

                output.Append(exprStr);

                break;

            ///

            case DeferStatement defer:

                output.Append("#define __DEFER_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("Defer __DEFER_NAME  ([&] \n");
                output.Append("#undef __DEFER_NAME\n");
                output.Append(compiler.TranslateBlock(defer.Block));
                output.Append(")");

                break;

            ///

            default:

                throw new Exception();
        }

        ///

        output.Append(";\n");

        ///

        return output.ToString();
    }

    public static String TranslateExpr(
        this Compiler compiler,
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

            case CallExpression ce: {

                switch (ce.Call.Name) {

                    case "print":

                        output.Append("printf");

                        break;

                    ///

                    default:

                        output.Append(ce.Call.Name);

                        break;
                }

                output.Append("(");

                foreach (var parameter in ce.Call.Args) {

                    output.Append(compiler.TranslateExpr(parameter.Item2));
                }

                output.Append(")");

                break;
            }

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }

}

namespace Neu;

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String CodeGen(
        Project project,
        Scope scope) {

        var output = new StringBuilder();

        output.Append("#include \"../../../Runtime/lib.h\"\n");

        foreach (var (_, structId) in scope.Structs) {

            var structure = project.Structs[structId];

            var structOutput = CodeGenStructPredecl(structure, project);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }
        
        output.Append('\n');

        foreach (var (_, structId) in scope.Structs) {

            var structure = project.Structs[structId];

            var structOutput = CodeGenStruct(structure, project);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, funcId) in scope.Funcs) {

            var func = project.Functions[funcId];

            var funcOutput = CodeGenFuncPredecl(func, project);
            
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
                
                var funOutput = CodeGenFunc(func, project);

                output.Append(funOutput);

                output.Append("\n");
            }
        }

        return output.ToString();
    }

    public static String CodeGenStructPredecl(
        CheckedStruct structure,
        Project project) {

        if (structure.DefinitionLinkage == DefinitionLinkage.External) {

            return String.Empty;
        }
        else {

            var output = new StringBuilder();

            if (structure.GenericParameters.Any()) {

                output.Append("template <");
            }

            var first = true;

            foreach (var genParam in structure.GenericParameters) {

                if (!first) {

                    output.Append(", ");
                }
                else {

                    first = false;
                }

                output.Append("typename ");
                output.Append(CodeGenType(genParam, project));
            }

            if (structure.GenericParameters.Any()) {

                output.Append(">\n");
            }
            
            switch (structure.DefinitionType) {

                case DefinitionType.Class: {

                    output.Append($"class {structure.Name};");

                    break;
                }

                case DefinitionType.Struct: {

                    output.Append($"struct {structure.Name};");

                    break;
                }

                default: {

                    throw new Exception();
                }
            }

            return output.ToString();
        }
    }

    public static String CodeGenStruct(
        CheckedStruct structure,
        Project project) {

        if (structure.DefinitionLinkage == DefinitionLinkage.External) {

            return String.Empty;
        }

        var output = new StringBuilder();

        if (structure.GenericParameters.Any()) {

            output.Append("template <");
        }

        var first = true;

        foreach (var genParam in structure.GenericParameters) {

            if (!first) {

                output.Append(", ");
            }
            else {

                first = false;
            }

            output.Append("typename ");
            output.Append(CodeGenType(genParam, project));
        }

        if (structure.GenericParameters.Any()) {

            output.Append(">\n");
        }

        switch (structure.DefinitionType) {

            case DefinitionType.Class: {

                output.Append($"class {structure.Name} : public RefCounted<{structure.Name}> {{\n");
                
                // As we should test the visibility before codegen, we take a simple
                // approach to codegen
                
                output.Append("  public:\n");

                // Make sure emitted classes always have a vtable.
            
                output.Append($"    virtual ~{structure.Name}() = default;");

                break;
            }

            case DefinitionType.Struct: {

                output.Append($"struct {structure.Name}");
                output.Append(" {\n");
                output.Append("  public:\n");

                break;
            }

            default: {

                throw new Exception();
            }
        }

        foreach (var field in structure.Fields) {

            output.Append(new String(' ', INDENT_SIZE));

            output.Append(CodeGenType(field.Type, project));
            
            output.Append(' ');
            
            output.Append(field.Name);
            
            output.Append(";\n");
        }

        var scope = project.Scopes[structure.ScopeId];

        foreach (var (_, funcId) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.ImplicitConstructor) {

                var funcOutput = CodeGenConstructor(func, project);

                output.Append(new String(' ', INDENT_SIZE));
                output.Append(funcOutput);
                output.Append('\n');
            }
            else {

                output.Append(new String(' ', INDENT_SIZE));

                if (func.IsStatic()) {

                    output.Append("static ");
                }

                var methodOutput = CodeGenFunc(func, project);

                output.Append(methodOutput);
            }
        }

        output.Append("};");

        return output.ToString();
    }

    public static String CodeGenFuncPredecl(
        CheckedFunction fun,
        Project project) {
        
        var output = new StringBuilder();

        if (fun.Linkage == FunctionLinkage.External) {

            output.Append("extern ");
        }

        if (fun.GenericParameters.Any()) {

            output.Append("template <");
        }

        var firstGenParam = true;

        foreach (var genParam in fun.GenericParameters) {

            if (!firstGenParam) {

                output.Append(", ");
            }
            else {

                firstGenParam = false;
            }

            output.Append("typename ");
            output.Append(CodeGenType(genParam, project));
        }

        if (fun.GenericParameters.Any()) {

            output.Append(">\n");
        }

        if (fun.Name == "main") {

            output.Append("ErrorOr<int>");
        }
        else {

            String returnType;

            if (fun.Throws) {

                returnType = $"ErrorOr<{CodeGenType(fun.ReturnType, project)}>";
            }
            else {

                returnType = CodeGenType(fun.ReturnType, project);
            }

            output.Append(returnType);
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

            var ty = CodeGenType(p.Variable.Type, project);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Variable.Name);
        }

        output.Append(");");

        return output.ToString();
    }

    public static String CodeGenFunc(
        CheckedFunction fun,
        Project project) {

        var output = new StringBuilder();

        if (fun.GenericParameters.Any()) {
            
            output.Append("template <");
        }
        
        var firstGenParam = true;
        
        foreach (var genParam in fun.GenericParameters) {
            
            if (!firstGenParam) {
                
                output.Append(", ");
            } 
            else {
                
                firstGenParam = false;
            }
            
            output.Append("typename ");
            output.Append(CodeGenType(genParam, project));
        }

        if (fun.GenericParameters.Any()) {
            
            output.Append(">\n");
        }

        if (fun.Name == "main") {

            output.Append("ErrorOr<int>");
        }
        else {

            String returnType;

            if (fun.Throws) {

                returnType = $"ErrorOr<{CodeGenType(fun.ReturnType, project)}>";
            }
            else {

                returnType = CodeGenType(fun.ReturnType, project);
            }

            output.Append(returnType);
        }

        output.Append(' ');

        if (fun.Name == "main") {

            output.Append("_neu_main");
        }
        else {

            output.Append(fun.Name);
        }

        output.Append('(');

        if (fun.Name == "main" && !fun.Parameters.Any()) {
        
            output.Append("Array<String>");
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

            var ty = CodeGenType(p.Variable.Type, project);

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

        var block = CodeGenBlock(INDENT_SIZE, fun.Block, project);

        output.Append(block);

        if (fun.Name == "main") {
            
            output.Append(new String(' ', INDENT_SIZE));
            output.Append("return 0;\n}");
        }

        return output.ToString();
    }

    public static String CodeGenConstructor(
        CheckedFunction func,
        Project project) {

        var typeId = func.ReturnType;

        var ty = project.Types[typeId];

        switch (ty) {

            case StructType st: {

                var structure = project.Structs[st.StructId];

                if (structure.DefinitionType == DefinitionType.Class) {

                    var output = new StringBuilder($"static NonNullRefPointer<{func.Name}> create");

                    output.Append('(');

                    var first = true;

                    foreach (var param in func.Parameters) {

                        if (!first) {

                            output.Append(", ");
                        }
                        else {

                            first = false;
                        }

                        var tyStr = CodeGenType(param.Variable.Type, project);

                        output.Append(tyStr);
                        output.Append(' ');
                        output.Append(param.Variable.Name);
                    }

                    output.Append($") {{ auto o = adoptRef(*new {func.Name}); ");

                    foreach (var param in func.Parameters) {

                        output.Append("o->");
                        output.Append(param.Variable.Name);
                        output.Append(" = ");
                        output.Append(param.Variable.Name);
                        output.Append("; ");
                    }

                    output.Append("return o; }");

                    return output.ToString();
                }
                else {

                    var output = new StringBuilder();

                    output.Append(func.Name);
                    output.Append('(');

                    var first = true;

                    foreach (var param in func.Parameters) {

                        if (!first) {

                            output.Append(", ");
                        }
                        else {

                            first = false;
                        }

                        var tyStr = CodeGenType(param.Variable.Type, project);
                        output.Append(tyStr);
                        output.Append(" a_");
                        output.Append(param.Variable.Name);
                    }

                    output.Append(") ");

                    if (func.Parameters.Any()) {

                        output.Append(':');
                    }

                    first = true;

                    foreach (var param in func.Parameters) {

                        if (!first) {

                            output.Append(", ");
                        }
                        else {

                            first = false;
                        }

                        output.Append(param.Variable.Name);
                        output.Append("(a_");
                        output.Append(param.Variable.Name);
                        output.Append(')');
                    }

                    output.Append("{ }\n");

                    return output.ToString();
                }
            }

            default: {

                throw new Exception("internal error: call to a constructor, but not a struct/class type");
            }
        }
    }

    public static String CodeGenStructType(
        Int32 typeId,
        Project project) {

        var ty = project.Types[typeId];

        switch (ty) {

            case StructType st: {

                return project.Structs[st.StructId].Name;
            }

            default: {

                throw new Exception("codegen_struct_type on non-struct");
            }
        }
    }

    public static String CodeGenType(
        Int32 typeId,
        Project project) {

        var ty = project.Types[typeId];

        switch (ty) {

            case RawPointerType pt: {

                return $"{CodeGenType(pt.TypeId, project)}*";
            }

            case GenericInstance gi: {

                var output = new StringBuilder(project.Structs[gi.StructId].Name);

                output.Append('<');

                var first = true;

                foreach (var t in gi.TypeIds) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append(CodeGenType(t, project));
                }

                output.Append('>');

                return output.ToString();
            }

            case StructType st: {

                var inner = project.Structs[st.StructId];

                if (inner.DefinitionType == DefinitionType.Class) {

                    return $"NonNullRefPointer<{inner.Name}>";
                }
                else {

                    return inner.Name;
                }
            }

            case Builtin _: {

                switch (typeId) {
                    case Compiler.IntTypeId: return "ssize_t";
                    case Compiler.UIntTypeId: return "size_t";
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

            case TypeVariable t: {

                return t.Name;
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static String CodeGenBlock(
        int indent,
        CheckedBlock block,
        Project project) {

        var output = new StringBuilder();

        output.Append("{\n");

        foreach (var stmt in block.Stmts) {

            var stmtStr = CodeGenStatement(indent + INDENT_SIZE, stmt, project);

            output.Append(stmtStr);
        }

        output.Append(new String(' ', indent));

        output.Append("}\n");

        return output.ToString();
    }

    public static String CodeGenStatement(
        int indent,
        CheckedStatement stmt,
        Project project) {

        var output = new StringBuilder();

        output.Append(new String(' ', indent));

        ///

        switch (stmt) {

            case CheckedTryStatement ts: {

                output.Append('{');
                output.Append("auto _neu_tryResult = [&]() -> ErrorOr<void> {");
                output.Append(CodeGenStatement(indent, ts.Statement, project));
                output.Append(';');
                output.Append("return {};");
                output.Append("}();");
                output.Append("if (_neu_tryResult.isError()) {");
                output.Append("auto ");
                output.Append(ts.Name);
                output.Append(" = _neu_tryResult.releaseError();");
                output.Append(CodeGenBlock(indent, ts.Block, project));
                output.Append("}");
                output.Append('}');

                break;
            }

            case CheckedThrowStatement t: {

                output.Append("return ");
                output.Append(CodeGenExpr(indent, t.Expression, project));
                output.Append(";");
                
                break;
            }

            case CheckedContinueStatement _: {

                output.Append("continue;");
                
                break;
            }

            case CheckedBreakStatement _: {

                output.Append("break;");
                
                break;
            }

            case CheckedForStatement f: {
                
                output.Append("{ auto&& _range = ");
                output.Append(CodeGenExpr(indent, f.Range, project));
                output.Append("; for(auto ");
                output.Append(f.IteratorName);
                output.Append(' ');
                output.Append(" = _range.start;");
                output.Append(f.IteratorName);
                output.Append("!= _range.end;");
                output.Append(f.IteratorName);
                output.Append("++");
                output.Append(") {");
                output.Append(CodeGenBlock(indent, f.Block, project));
                output.Append("}}");

                break;
            }

            case CheckedExpressionStatement es: {

                var exprStr = CodeGenExpr(indent, es.Expression, project);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            case CheckedDeferStatement defer: {

                // NOTE: We let the preprocessor generate a unique name for the RAII helper.
                output.Append("#define __SCOPE_GUARD_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("ScopeGuard __SCOPE_GUARD_NAME  ([&] \n");
                output.Append("#undef __SCOPE_GUARD_NAME\n{");
                output.Append(CodeGenStatement(indent, defer.Statement, project));
                output.Append("});\n");

                break;
            }

            case CheckedReturnStatement rs: {

                var exprStr = CodeGenExpr(indent, rs.Expr, project);

                output.Append("return (");
                
                output.Append(exprStr);
                
                output.Append(");\n");

                break;
            }

            case CheckedIfStatement ifStmt: {

                var exprStr = CodeGenExpr(indent, ifStmt.Expr, project);

                output.Append("if (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = CodeGenBlock(indent, ifStmt.Block, project);
                
                output.Append(blockStr);

                if (ifStmt.Trailing is CheckedStatement e) {

                    output.Append(new String(' ', indent));

                    output.Append("else ");

                    var elseStr = CodeGenStatement(indent, e, project);

                    output.Append(elseStr);
                }

                break;
            }

            case CheckedLoopStatement loopStmt: {

                output.Append("for (;;) {");
                var block = CodeGenBlock(indent, loopStmt.Block, project);
                output.Append(block);
                output.Append("}");
                
                break;
            }

            case CheckedWhileStatement whileStmt: {

                var exprStr = CodeGenExpr(indent, whileStmt.Expression, project);

                output.Append("while (");
                
                output.Append(exprStr);
                
                output.Append(") ");

                var blockStr = CodeGenBlock(indent, whileStmt.Block, project);
                
                output.Append(blockStr);

                break;
            }

            case CheckedVarDeclStatement vd: {

                if (!vd.VarDecl.Mutable) {

                    output.Append("const ");
                }

                output.Append(CodeGenType(vd.VarDecl.Type, project));
                output.Append(' ');
                output.Append(vd.VarDecl.Name);
                output.Append(" = ");
                output.Append(CodeGenExpr(indent, vd.Expr, project));
                output.Append(";\n");

                break;
            }

            case CheckedBlockStatement chBlockStmt: {

                var blockStr = CodeGenBlock(indent, chBlockStmt.Block, project);

                output.Append(blockStr);

                break;
            }

            case CheckedGarbageStatement _: {

                // Incorrect parse/typecheck
                // Probably shouldn't be able to get to this point?

                break;
            }

            default: {

                throw new Exception();
            }
        }

        ///

        return output.ToString();
    }

    public static String CodeGenExpr(
        int indent,
        CheckedExpression expr,
        Project project) {

        var output = new StringBuilder();

        switch (expr) {

            case CheckedRangeExpression r: {

                Int32? _indexType = null;

                var ty = project.Types[r.TypeId];

                switch (ty) {

                    case GenericInstance gi: {

                        _indexType = gi.TypeIds[0];

                        break;
                    }

                    default: {

                        throw new Exception("Interal error: range expression doesn't have Range type");
                    }
                }

                Int32 indexType = _indexType ?? throw new Exception();

                output.Append("(");
                output.Append(CodeGenType(r.TypeId, project));
                output.Append("{");
                output.Append("static_cast<");
                output.Append(CodeGenType(indexType, project));
                output.Append(">(");
                output.Append(CodeGenExpr(indent, r.Start, project));
                output.Append("),static_cast<");
                output.Append(CodeGenType(indexType, project));
                output.Append(">(");
                output.Append(CodeGenExpr(indent, r.End, project));
                output.Append(")})");

                break;
            }

            case CheckedOptionalNoneExpression _: {

                output.Append("{ }");

                break;
            }

            case CheckedOptionalSomeExpression o: {

                output.Append('(');
                output.Append(CodeGenExpr(indent, o.Expression, project));
                output.Append(')');

                break;
            }

            case CheckedForceUnwrapExpression f: {

                output.Append('(');
                output.Append(CodeGenExpr(indent, f.Expression, project));
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

                    case IntConstant i: {

                        output.Append("static_cast<ssize_t>(");
                        output.Append(i.Value.ToString());
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

                    case UIntConstant u: {

                        output.Append("static_cast<size_t>(");
                        output.Append(u.Value.ToString());
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

                if (ce.Call.CalleeThrows) {

                    output.Append("TRY(");
                }

                switch (ce.Call.Name) {

                    case "printLine": {

                        output.Append("outLine(");
                        
                        for (var i = 0; i < ce.Call.Args.Count; i++) {

                            var param = ce.Call.Args[i];

                            output.Append(CodeGenExpr(indent, param.Item2, project));

                            if (i != ce.Call.Args.Count - 1) {

                                output.Append(',');
                            }
                        }
                        
                        output.Append(")");

                        break;
                    }

                    case "warnLine": {

                        output.Append("warnLine(");

                        for (var i = 0; i < ce.Call.Args.Count; i++) {

                            var param = ce.Call.Args[i];

                            output.Append(CodeGenExpr(indent, param.Item2, project));

                            if (i != ce.Call.Args.Count - 1) {

                                output.Append(',');
                            }
                        }
                        
                        output.Append(")");

                        break;
                    }

                    ///

                    default: {

                        for (var idx = 0; idx < ce.Call.Namespace.Count; idx++) {

                            var ns = ce.Call.Namespace[idx];

                            // hack warning: this is to get around C++'s limitation that a constructor
                            // can't be called like other static methods
                            if (idx == ce.Call.Namespace.Count - 1 && ns == ce.Call.Name) {
                                
                                break;
                            }

                            output.Append(ns);
                            output.Append("::");
                        }

                        if (ce.Call.Linkage == FunctionLinkage.ImplicitConstructor) {

                            var typeId = ce.Call.Type;

                            var ty = project.Types[typeId];

                            switch (ty) {

                                case GenericInstance gi: {

                                    var structure = project.Structs[gi.StructId];

                                    if (structure.DefinitionType == DefinitionType.Class) {
                                        
                                        output.Append(ce.Call.Name);
                                        output.Append("::");
                                        output.Append("create");
                                    }
                                    else {

                                        output.Append(ce.Call.Name);
                                    }

                                    break;
                                }

                                case StructType st: {

                                    var structure = project.Structs[st.StructId];

                                    if (structure.DefinitionType == DefinitionType.Class) {
                                        
                                        output.Append(ce.Call.Name);
                                        output.Append("::");
                                        output.Append("create");
                                    }
                                    else {

                                        output.Append(ce.Call.Name);
                                    }

                                    break;
                                }

                                default: {

                                    throw new Exception("internal error: constructor expected class or struct type");
                                }
                            }
                        }
                        else {

                            output.Append(ce.Call.Name);
                        }

                        if (ce.Call.TypeArgs.Any()) {

                            output.Append('<');

                            var firstTypeArg = true;

                            foreach (var typeArg in ce.Call.TypeArgs) {

                                if (!firstTypeArg) {

                                    output.Append(", ");
                                }
                                else {

                                    firstTypeArg = false;
                                }

                                output.Append(CodeGenType(typeArg, project));
                            }

                            output.Append('>');
                        }
                    
                        output.Append("(");

                        var first = true;

                        foreach (var parameter in ce.Call.Args) {

                            if (!first) {

                                output.Append(", ");
                            }
                            else {

                                first = false;
                            }

                            output.Append(CodeGenExpr(indent, parameter.Item2, project));
                        }

                        output.Append(")");

                        break;
                    }
                }

                if (ce.Call.CalleeThrows) {

                    output.Append(")");
                }

                break;
            }

            case CheckedMethodCallExpression mce: {

                if (mce.Call.CalleeThrows) {

                    output.Append("TRY(");
                }

                output.Append('(');

                output.Append('(');
                output.Append(CodeGenExpr(indent, mce.Expression, project));
                output.Append(")");

                switch (mce.Expression) {

                    case CheckedVarExpression ve when ve.Variable.Name == "this": {

                        output.Append("->");

                        break;
                    }

                    case var x: {

                        switch (project.Types[x.GetNeuType()]) {

                            case RawPointerType p: {

                                output.Append("->");

                                break;
                            }

                            case StructType s: {

                                var structure = project.Structs[s.StructId];

                                if (structure.DefinitionType == DefinitionType.Class) {

                                    output.Append("->");
                                }
                                else {

                                    output.Append('.');
                                }

                                break;
                            }

                            default: {

                                output.Append('.');

                                break;
                            }
                        }

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

                    output.Append(CodeGenExpr(indent, param.Item2, project));
                }

                output.Append("))");

                if (mce.Call.CalleeThrows) {

                    output.Append(")");
                }

                break;
            }

            case CheckedUnaryOpExpression unaryOp: {

                output.Append('(');

                switch (unaryOp.Operator) {

                    case CheckedPreIncrementUnaryOperator _: {

                        output.Append("++");

                        break;
                    }

                    case CheckedPreDecrementUnaryOperator _: {

                        output.Append("--");

                        break;
                    }

                    case CheckedNegateUnaryOperator _: {

                        output.Append('-');

                        break;
                    }

                    case CheckedDereferenceUnaryOperator _: {

                        output.Append('*');

                        break;
                    }

                    case CheckedRawAddressUnaryOperator _: {

                        output.Append('&');

                        break;
                    }

                    case CheckedLogicalNotUnaryOperator _: {

                        output.Append('!');

                        break;
                    }

                    case CheckedBitwiseNotUnaryOperator _: {

                        output.Append('~');

                        break;
                    }

                    case CheckedIsUnaryOperator i: {

                        output.Append("is<");
                        output.Append(CodeGenStructType(i.TypeId, project));
                        output.Append(">(");

                        break;
                    }

                    case CheckedTypeCastUnaryOperator tc: {

                        switch (tc.TypeCast) {

                            case CheckedFallibleTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {
                            
                                    output.Append("fallibleIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case CheckedInfallibleTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {

                                    output.Append("infallibleIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case CheckedSaturatingTypeCast _: {

                                if (NeuTypeFunctions.IsInteger(unaryOp.Type)) {

                                    output.Append("saturatingIntegerCast");
                                }
                                else {

                                    output.Append("dynamic_cast");
                                }

                                break;
                            }

                            case CheckedTruncatingTypeCast _: {

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
                        output.Append(CodeGenType(unaryOp.Type, project));
                        output.Append(">(");

                        break;
                    }

                    default: {

                        break;
                    }
                }

                output.Append(CodeGenExpr(indent, unaryOp.Expression, project));

                switch (unaryOp.Operator) {

                    case CheckedPostIncrementUnaryOperator _: {

                        output.Append("++");

                        break;
                    }

                    case CheckedPostDecrementUnaryOperator _: {

                        output.Append("--");

                        break;
                    }

                    case CheckedTypeCastUnaryOperator _:
                    case CheckedIsUnaryOperator _: {

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

                switch (binOp.Operator) {

                    case BinaryOperator.NoneCoalescing: {

                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));
                        output.Append(".valueOrLazyEvaluated([&] { return ");
                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));
                        output.Append("; })");

                        break;
                    }

                    case BinaryOperator.ArithmeticRightShift: {

                        output.Append("__arithmeticShiftRight(");
                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));
                        output.Append(", ");
                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));
                        output.Append(')');

                        break;
                    }

                    default: {

                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));

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

                            case BinaryOperator.ArithmeticLeftShift: {

                                output.Append(" << ");

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
                                
                                break;
                            }
                        }

                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));

                        break;
                    }
                }

                output.Append(")");

                break;
            }

            case CheckedArrayExpression ve: {

                if (ve.FillSize is CheckedExpression fillSize) {

                    output.Append("(TRY(Array<");
                    output.Append(CodeGenType(ve.Expressions.First().GetNeuType(), project));
                    output.Append(">::filled(");
                    output.Append(CodeGenExpr(indent, fillSize, project));
                    output.Append(", ");
                    output.Append(CodeGenExpr(indent, ve.Expressions.First(), project));
                    output.Append(")))");
                }
                else {

                    // (Array({1, 2, 3}))

                    output.Append("(Array({");

                    var first = true;

                    foreach (var val in ve.Expressions) {
                        
                        if (!first) {
                            
                            output.Append(", ");
                        } 
                        else {
                            
                            first = false;
                        }

                        output.Append(CodeGenExpr(indent, val, project));
                    }

                    output.Append("}))");
                }

                break;
            }

            case CheckedDictionaryExpression de: {

                // (Dictionary({1, 2, 3}))

                var keyTypeId = de.Entries[0].Item1.GetNeuType();
                var valueTypeId = de.Entries[0].Item2.GetNeuType();

                output.Append($"(Dictionary<{CodeGenType(keyTypeId, project)}, {CodeGenType(valueTypeId, project)}>({{");

                var first = true;

                foreach (var (key, value) in de.Entries) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append('{');
                    output.Append(CodeGenExpr(indent, key, project));
                    output.Append(", ");
                    output.Append(CodeGenExpr(indent, value, project));
                    output.Append('}');
                }

                output.Append("}))");

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

                    output.Append(CodeGenExpr(indent, val, project));
                }

                output.Append("})");

                break;
            }

            case CheckedIndexedExpression ie: {

                output.Append("((");
            
                output.Append(CodeGenExpr(indent, ie.Expression, project));
            
                output.Append(")[");
            
                output.Append(CodeGenExpr(indent, ie.Index, project));
            
                output.Append("])");

                break;
            }

            case CheckedIndexedDictionaryExpression ide: {

                output.Append("((");
                output.Append(CodeGenExpr(indent, ide.Expression, project));
                output.Append(").get(");
                output.Append(CodeGenExpr(indent, ide.Index, project));
                output.Append("))");

                break;
            }

            case CheckedIndexedTupleExpression ite: {

                // x.get<1>()
                
                output.Append("((");
                output.Append(CodeGenExpr(indent, ite.Expression, project));
                output.Append($").get<{ite.Index}>())");

                break;
            }

            case CheckedIndexedStructExpression ise: {

                // x.foo or x->foo
                
                output.Append("((");
                output.Append(CodeGenExpr(indent, ise.Expression, project));
                output.Append(')');

                switch (ise.Expression) {

                    case CheckedVarExpression ve when ve.Variable.Name == "this": {

                        output.Append("->");

                        break;
                    }

                    case var x: {

                        switch (project.Types[x.GetNeuType()]) {

                            case RawPointerType p: {

                                output.Append("->");

                                break;
                            }

                            case StructType s: {

                                var structure = project.Structs[s.StructId];

                                if (structure.DefinitionType == DefinitionType.Class) {

                                    output.Append("->");
                                }
                                else {

                                    output.Append('.');
                                }

                                break;
                            }

                            default: {

                                output.Append('.');

                                break;
                            }
                        }

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
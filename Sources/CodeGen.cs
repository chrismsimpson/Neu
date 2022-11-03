
namespace Neu;

public partial class CodeGenContext {

    public List<String> NamespaceStack { get; init; }

    public StringBuilder DeferredOutput { get; init; }

    ///

    public CodeGenContext(
        List<String> namespaceStack,
        StringBuilder deferredOutput) {

        this.NamespaceStack = namespaceStack;
        this.DeferredOutput = deferredOutput;
    }
}

public static partial class CodeGenFunctions {

    public static readonly int INDENT_SIZE = 4;

    public static String CodeGen(
        Project project,
        Scope scope) {
        
        var output = new StringBuilder();

        output.Append("#include <lib.h>\n");

        var context = new CodeGenContext(
            namespaceStack: new List<String>(),
            deferredOutput: new StringBuilder());

        output.Append(CodeGenNamespace(project, scope, context));

        output.Append(context.DeferredOutput.ToString());

        return output.ToString();
    }

    public static String CodeGenNamespace(
        Project project,
        Scope scope,
        CodeGenContext context) {
            
        var output = new StringBuilder();

        // Output the namespaces (and their children)

        foreach (var childScopeId in scope.Children) {

            // For now, require children of the current namespace to have names before being emitted

            var childScope = project.Scopes[childScopeId];

            if (childScope.NamespaceName is String name) {

                context.NamespaceStack.Add(name);
                output.Append($"namespace {name} {{\n");
                output.Append(CodeGenNamespace(project, childScope, context));
                output.Append("}\n");
                context.NamespaceStack.Pop();
            }
        }

        foreach (var (_, structId, _) in scope.Structs) {

            var structure = project.Structs[structId];

            var structOutput = CodeGenStructPredecl(structure, project);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, enumId, _) in scope.Enums) {

            var _enum = project.Enums[enumId];

            var enumOutput = CodeGenEnumPredecl(_enum, project);

            if (!IsNullOrWhiteSpace(enumOutput)) {

                output.Append(enumOutput);
                output.Append('\n');
            }
        }
        
        output.Append('\n');

        // Figure out the right order to output the structs
        // This is necessary as C++ requires a type to be defined before it's used as a value type,
        // we can ignore the generic types as they are only resolved when used by other non-generic code.

        var typeDependencyGraph = ProduceTypeDependencyGraph(project, scope);

        var seenTypes = new HashSet<Int32>();

        foreach (var typeId in typeDependencyGraph.Keys) {

            var traversal = new List<Int32>();

            PostOrderTraversal(
                project,
                typeId,
                seenTypes,
                typeDependencyGraph,
                traversal);
                
            foreach (var _typeId in traversal) {

                var _type = project.Types[_typeId];

                seenTypes.Add(_typeId);

                switch (_type) {

                    case EnumType et: {

                        var _enum = project.Enums[et.EnumId];
                        var enumOutput = CodeGenEnum(_enum, project, context);

                        if (!IsNullOrWhiteSpace(enumOutput)) {
                            
                            output.Append(enumOutput);
                            output.Append('\n');
                        }

                        break;
                    }

                    case StructType st: {

                        var structure = project.Structs[st.StructId];
                        var structOutput = CodeGenStruct(structure, project, context);

                        if (!IsNullOrWhiteSpace(structOutput)) {
                            
                            output.Append(structOutput);
                            output.Append('\n');
                        }

                        break;
                    }

                    default: {

                        throw new Exception($"Unexpected type in dependency graph: {_type}");
                    }
                }
            }
        }

        foreach (var (_, structId, _) in scope.Structs) {

            var structure = project.Structs[structId];

            if (seenTypes.Contains(structure.TypeId)) {

                continue;
            }

            var structOutput = CodeGenStruct(structure, project, context);

            if (!IsNullOrWhiteSpace(structOutput)) {

                output.Append(structOutput);
                output.Append('\n');
            }
        }

        output.Append('\n');
        
        foreach (var (_, enumId, _) in scope.Enums) {

            var _enum = project.Enums[enumId];

            if (seenTypes.Contains(_enum.TypeId)) {

                continue;
            }

            var enumOutput = CodeGenEnum(_enum, project, context);

            if (!IsNullOrWhiteSpace(enumOutput)) {

                output.Append(enumOutput);

                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, funcId, _) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.ImplicitEnumConstructor) {

                continue;
            }

            var funcOutput = CodeGenFuncPredecl(func, project);
            
            if (func.Linkage != FunctionLinkage.ImplicitConstructor && func.Name != "main") {

                output.Append(funcOutput);

                output.Append('\n');
            }
        }

        output.Append('\n');

        foreach (var (_, funcId, _) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.External
                || func.Linkage == FunctionLinkage.ImplicitConstructor
                || func.Linkage == FunctionLinkage.ImplicitEnumConstructor) {

                continue;
            }
            else {
                
                var funOutput = CodeGenFunc(func, project);

                output.Append(funOutput);

                output.Append("\n");
            }
        }

        foreach (var (_, structId, _) in scope.Structs) {

            var structure = project.Structs[structId];

            if (structure.DefinitionLinkage == DefinitionLinkage.External) {

                continue;
            }

            if (structure.GenericParameters.Any()) {

                continue;
            }

            var _scope = project.Scopes[structure.ScopeId];

            foreach (var (_, scopeFuncId, _) in _scope.Funcs) {

                var scopeFunc = project.Functions[scopeFuncId];

                if (scopeFunc.Linkage != FunctionLinkage.ImplicitConstructor) {

                    var funcOutput = CodeGenFuncInNamespace(scopeFunc, structure.TypeId, project);

                    output.Append(funcOutput);

                    output.Append('\n');
                }
            }
        }

        return output.ToString();
    }

    public static String CodeGenEnumPredecl(
        CheckedEnum _enum,
        Project project) {

        if (_enum.DefinitionType == DefinitionType.Class) {

            return CodeGenRecursiveEnumPredecl(_enum, project);
        }
        else {

            return CodeGenNonRecursiveEnumPredecl(_enum, project);
        }
    }

    public static String CodeGenEnum(
        CheckedEnum _enum,
        Project project,
        CodeGenContext context) {

        if (_enum.DefinitionType == DefinitionType.Class) {

            return CodeGenRecursiveEnum(_enum, project, context);
        }
        else {

            return CodeGenNonRecursiveEnum(_enum, project, context);
        }
    }

    public static String CodeGenNonRecursiveEnum(
        CheckedEnum _enum,
        Project project,
        CodeGenContext context) {

        if (_enum.UnderlyingTypeId is Int32 typeId) {

            if (NeuTypeFunctions.IsInteger(typeId)) {

                var _output = new StringBuilder();

                _output.Append("enum class ");
                _output.Append(_enum.Name);
                _output.Append(": ");
                _output.Append(CodeGenType(typeId, project));
                _output.Append(" {\n");

                foreach (var variant in _enum.Variants) {

                    switch (variant) {

                        case CheckedUntypedEnumVariant uev: {

                            _output.Append("    ");
                            _output.Append(uev.Name);
                            _output.Append(",\n");

                            break;
                        }

                        case CheckedWithValueEnumVariant wvev: {

                            _output.Append("    ");
                            _output.Append(wvev.Name);
                            _output.Append(" = ");
                            _output.Append(CodeGenExpr(0, wvev.Expression, project));
                            _output.Append(",\n");

                            break;
                        }

                        default: {

                            throw new Exception();
                        }
                    }
                }

                _output.Append("};\n");

                return _output.ToString();
            }
            else {

                throw new Exception("TODO: Enums with a non-integer underlying type");
            }
        }

        // These are all Variant<Ts...>, make a new namespace and define the variant types first.

        var isGeneric = _enum.GenericParameters.Any();

        var genericParamNames = _enum
            .GenericParameters
            .Select(x => {

                switch (project.Types[x]) {

                    case TypeVariable t: {

                        return t.Name;
                    }

                    default: {

                        throw new Exception();
                    }
                }
            })
            .ToList();

        var templateArgs = Join(
            ", ", 
            genericParamNames
                .Select(x => $"typename {x}")
                .ToList());

        var output = new StringBuilder();
        
        output.Append("namespace ");
        
        output.Append(_enum.Name);
        
        output.Append("_Details {\n");

        foreach (var variant in _enum.Variants) {

            switch (variant) {

                case CheckedStructLikeEnumVariant s: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(s.Name);
                    output.Append(" {\n");

                    foreach (var member in s.Decls) {

                        output.Append("        ");
                        output.Append(CodeGenType(member.TypeId, project));
                        output.Append(" ");
                        output.Append(member.Name);
                        output.Append(";\n");
                    }

                    output.Append("        template<");

                    for (var i = 0; i < s.Decls.Count; i++) {

                        if (i > 0) {

                            output.Append(", ");
                        }

                        output.Append("typename _MemberT");
                        output.Append($"{i}");
                    }
                    output.Append(">\n");
                    output.Append("        ");
                    output.Append(s.Name);
                    output.Append("(");

                    for (var i = 0; i < s.Decls.Count; i++) {
                        
                        if (i > 0) {
                        
                            output.Append(", ");
                        }
                        
                        output.Append("_MemberT");
                        output.Append($"{i}");
                        output.Append("&& member_");
                        output.Append($"{i}");
                    }
                    output.Append("):\n");

                    for (var i = 0; i < s.Decls.Count; i++) {

                        var member = s.Decls[i];

                        output.Append("            ");
                        output.Append(member.Name);
                        output.Append("{ forward<_MemberT");
                        output.Append($"{i}");
                        output.Append(">(member_");
                        output.Append($"{i}");
                        output.Append(")}");

                        if (i < s.Decls.Count - 1) {
                            
                            output.Append(",\n");
                        } 
                        else {
                            
                            output.Append("\n");
                        }
                    }

                    output.Append("    { }\n");
                    output.Append("};\n");

                    break;
                }

                case CheckedUntypedEnumVariant u: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(u.Name);
                    output.Append(" { };\n");

                    break;
                }

                case CheckedTypedEnumVariant t: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(t.Name);
                    output.Append(" {\n");
                    output.Append("        ");
                    output.Append(CodeGenType(t.TypeId, project));
                    output.Append(" value;\n");
                    output.Append("\n");
                    output.Append("        template<typename... Args>\n");
                    output.Append("        ");
                    output.Append(t.Name);
                    output.Append("(Args&&... args): ");
                    output.Append(" value { forward<Args>(args)... } { }\n");
                    output.Append("    };\n");

                    break;
                }

                default: {

                    break;
                }
            }
        }

        output.Append("}\n");

        // Now define the variant itself

        if (isGeneric) {

            output.Append("template<");
            output.Append(templateArgs);
            output.Append(">\n");
        }

        output.Append("struct ");
        output.Append(_enum.Name);
        output.Append(" : public Variant<");

        var variantArgs = new StringBuilder();

        var first = true;

        var variantNames = new List<String>();

        foreach (var variant in _enum.Variants) {

            if (first) {

                first = false;
            }
            else {

                variantArgs.Append(", ");
            }

            var variantName = String.Empty;

            switch (variant) {

                case CheckedStructLikeEnumVariant s: {

                    variantName = s.Name;

                    break;
                }

                case CheckedUntypedEnumVariant u: {

                    variantName = u.Name;

                    break;
                }

                case CheckedTypedEnumVariant t: {

                    variantName = t.Name;

                    break;
                }

                default: {

                    throw new Exception();
                }
            }

            if (!IsNullOrWhiteSpace(variantName)) {

                variantArgs.Append(_enum.Name);
                variantArgs.Append("_Details::");
                variantArgs.Append(variantName);
                
                variantNames.Add(variantName);
                
                if (isGeneric) {

                    variantArgs.Append("<");
                    variantArgs.Append(Join(", ", genericParamNames));
                    variantArgs.Append(">");
                }
            }
        }

        output.Append(variantArgs);
        output.Append("> {\n");

        output.Append("    using Variant<");
        output.Append(variantArgs);
        output.Append(">::Variant;\n");

        foreach (var name in variantNames) {

            output.Append("    using ");
            output.Append(name);
            output.Append(" = ");
            output.Append(_enum.Name);
            output.Append("_Details::");
            output.Append(name);

            if (isGeneric) {

                output.Append("<");
                output.Append(Join(", ", genericParamNames));
                output.Append(">");
            }
            
            output.Append(";\n");
        }

        output.Append(CodeGenEnumDebugDescriptionGetter(_enum));

        output.Append("};\n");

        context.DeferredOutput.Append(
            CodeGenCoreFormatter(
                _enum.Name,
                genericParamNames,
                context));

        return output.ToString();
    }

    public static String CodeGenNonRecursiveEnumPredecl(
        CheckedEnum _enum,
        Project project) {

        if (_enum.UnderlyingTypeId is Int32 typeId) {

            if (NeuTypeFunctions.IsInteger(typeId)) {

                var _output = new StringBuilder();

                _output.Append("enum class ");
                _output.Append(_enum.Name);
                _output.Append(": ");
                _output.Append(CodeGenType(typeId, project));
                _output.Append(";\n");
                
                return _output.ToString();
            }
            else {

                throw new Exception("TODO: Enums with a non-integer underlying type");
            }
        }

        // These are all Variant<Ts...>, make a new namespace and define the variant types first.

        var isGeneric = _enum.GenericParameters.Any();

        var genericParameterNames = _enum
            .GenericParameters
            .Select(p => project.Types[p] switch {
                TypeVariable tv => tv.Name,
                _ => throw new Exception()
            })
            .ToList();

        var templateArgs = Join(", ", genericParameterNames.Select(p => $"typename {p}"));

        var output = new StringBuilder();
        output.Append("namespace ");
        output.Append(_enum.Name);
        output.Append("_Details {\n");

        foreach (var variant in _enum.Variants) {

            switch (variant) {
                
                case CheckedStructLikeEnumVariant s: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(s.Name);
                    output.Append(";\n");

                    break;
                }

                case CheckedUntypedEnumVariant u: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(u.Name);
                    output.Append(";\n");

                    break;
                }

                case CheckedTypedEnumVariant t: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(t.Name);
                    output.Append(";\n");

                    break;
                }

                default: {

                    break;
                }
            }
        }

        output.Append("}\n");

        // Now declare the variant itself.

        if (isGeneric) {

            output.Append("template<");
            output.Append(templateArgs);
            output.Append(">\n");
        }

        output.Append("struct ");
        output.Append(_enum.Name);
        output.Append(';');

        return output.ToString();
    }

    public static String CodeGenRecursiveEnumPredecl(
        CheckedEnum _enum,
        Project project) {

        var output = new StringBuilder();

        if (_enum.UnderlyingTypeId is Int32 typeId) {

            if (NeuTypeFunctions.IsInteger(typeId)) {

                output.Append("enum class ");
                output.Append(_enum.Name);
                output.Append(": ");
                output.Append(CodeGenType(typeId, project));
                output.Append(";\n");
                return output.ToString();
            }
            else {

                throw new Exception("TODO: Enums with a non-integer underlying type");
            }
        }

        // These are all Variant<Ts...>, make a new namespace and define the variant types first.

        var isGeneric = _enum.GenericParameters.Any();

        var genericParameterNames = _enum
            .GenericParameters
            .Select(p => {

                switch (project.Types[p]) {

                    case TypeVariable t:
                        return t.Name;

                    default:
                        throw new Exception("Unreachable");
                }
            })
            .ToList();

        var templateArgs = String.Join(", ", genericParameterNames.Select(p => $"typename {p}"));

        output.Append("namespace ");
        output.Append(_enum.Name);
        output.Append("_Details {\n");

        foreach (var variant in _enum.Variants) {

            switch (variant) {

                case CheckedStructLikeEnumVariant s: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(s.Name);
                    output.Append(";\n");

                    break;
                }

                case CheckedUntypedEnumVariant u: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(u.Name);
                    output.Append(";\n");

                    break;
                }

                case CheckedTypedEnumVariant t: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(t.Name);
                    output.Append(";\n");

                    break;
                }

                default: {

                    break;
                }
            }
        }

        output.Append("}\n");

        // Now declare the variant itself.
        if (isGeneric) {

            output.Append("template<");
            output.Append(templateArgs);
            output.Append(">\n");
        }

        output.Append("struct ");
        output.Append(_enum.Name);
        output.Append(';');

        return output.ToString();
    }

    public static String CodeGenRecursiveEnum(
        CheckedEnum _enum,
        Project project,
        CodeGenContext context) {

        var output = new StringBuilder();

        if (_enum.UnderlyingTypeId is Int32 typeId) {

            if (NeuTypeFunctions.IsInteger(typeId)) {

                output.Append("enum class ");
                output.Append(_enum.Name);
                output.Append(": ");
                output.Append(CodeGenType(typeId, project));
                output.Append(" {\n");

                foreach (var variant in _enum.Variants) {

                    switch (variant) {

                        case CheckedUntypedEnumVariant u: {

                            output.Append("    ");
                            output.Append(u.Name);
                            output.Append(",\n");

                            break;
                        }

                        case CheckedWithValueEnumVariant v: {
                            
                            output.Append("    ");
                            output.Append(v.Name);
                            output.Append(" = ");
                            output.Append(CodeGenExpr(0, v.Expression, project));
                            output.Append(",\n");

                            break;
                        }

                        default: {

                            throw new Exception("Unreachable");
                        }
                    }
                }

                output.Append("};\n");

                return output.ToString();
            }
            else {

                throw new Exception($"TODO: Enums with a non-integer underlying type");
            }
        }

        // These are all Variant<Ts...>, make a new namespace and define the variant types first.

        var isGeneric = _enum.GenericParameters.Any();

        var genericParameterNames = _enum
            .GenericParameters
            .Select(p => {

                switch (project.Types[p]) {

                    case TypeVariable t:
                        return t.Name;

                    default:
                        throw new Exception("Unreachable");
                }
            })
            .ToList();

        var templateArgs = String.Join(", ", genericParameterNames.Select(p => $"typename {p}"));

        output.Append("namespace ");
        output.Append(_enum.Name);
        output.Append("_Details {\n");

        foreach (var variant in _enum.Variants) {

            switch (variant) {

                case CheckedStructLikeEnumVariant s: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(s.Name);
                    output.Append(" {\n");

                    foreach (var member in s.Decls) {

                        output.Append("        ");
                        output.Append(CodeGenType(member.TypeId, project));
                        output.Append(' ');
                        output.Append(member.Name);
                        output.Append(";\n");
                    }

                    output.Append('\n');
                    output.Append("        template<");

                    for (var i = 0; i < s.Decls.Count; i++) {

                        if (i > 0) {

                            output.Append(", ");
                        }

                        output.Append("typename _MemberT");
                        output.Append($"{i}");
                    }

                    output.Append(">\n");
                    output.Append("        ");
                    output.Append(s.Name);
                    output.Append('(');

                    for (var i = 0; i < s.Decls.Count; i++) {

                        if (i > 0) {

                            output.Append(", ");
                        }

                        output.Append("_MemberT");
                        output.Append($"{i}");
                        output.Append("&& member_");
                        output.Append($"{i}");
                    }

                    output.Append("):\n");

                    for (var i = 0; i < s.Decls.Count; i++) {

                        var member = s.Decls[i];
                    
                        output.Append("            ");
                        output.Append(member.Name);
                        output.Append("{ forward<_MemberT");
                        output.Append($"{i}");
                        output.Append(">(member_");
                        output.Append($"{i}");
                        output.Append(")}");

                        if (i < s.Decls.Count - 1) {

                            output.Append(",\n");
                        } 
                        else {

                            output.Append('\n');
                        }
                    }

                    output.Append("    {}\n");

                    output.Append("};\n");

                    break;
                }

                case CheckedUntypedEnumVariant v: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(v.Name);
                    output.Append(" {};\n");

                    break;
                }

                case CheckedTypedEnumVariant t: {

                    if (isGeneric) {

                        output.Append("    template<");
                        output.Append(templateArgs);
                        output.Append(">\n");
                    }

                    output.Append("    struct ");
                    output.Append(t.Name);
                    output.Append(" {\n");
                    output.Append("        ");
                    output.Append(CodeGenType(t.TypeId, project));
                    output.Append(" value;\n");
                    output.Append('\n');
                    output.Append("        template<typename... Args>\n");
                    output.Append("        ");
                    output.Append(t.Name);
                    output.Append("(Args&&... args): ");
                    output.Append(" value { forward<Args>(args)... } {}\n");
                    output.Append("    };\n");

                    break;
                }

                default: {

                    break;
                }
            }
        }

        output.Append("}\n");

        // Now define the variant itself.

        if (isGeneric) {

            output.Append("template<");
            output.Append(templateArgs);
            output.Append(">\n");
        }

        output.Append("struct ");
        output.Append(_enum.Name);
        output.Append(" : public Variant<");

        var variantArgs = new StringBuilder();

        var first = true;

        var variantNames = new List<String>();

        foreach (var variant in _enum.Variants) {

            if (first) {

                first = false;
            }
            else {

                variantArgs.Append(", ");
            }

            var variantName = variant switch {

                CheckedStructLikeEnumVariant s => s.Name,
                CheckedUntypedEnumVariant u => u.Name,
                CheckedTypedEnumVariant t => t.Name,
                _ => throw new Exception("Unreachable")
            };

            variantArgs.Append(_enum.Name);
            variantArgs.Append("_Details::");
            variantArgs.Append(variantName);

            variantNames.Add(variantName);

            if (isGeneric) {

                variantArgs.Append('<');
                variantArgs.Append(Join(", ", genericParameterNames));
                variantArgs.Append('>');
            }
        }

        output.Append(variantArgs);
        output.Append("> ");

        if (_enum.DefinitionType == DefinitionType.Class) {

            output.Append(", public RefCounted<");
            output.Append(_enum.Name);

            if (isGeneric) {

                output.Append('<');
                output.Append(Join(", ", genericParameterNames));
                output.Append('>');
            }

            output.Append('>');
        }

        output.Append(" {\n");

        output.Append("    using Variant<");
        output.Append(variantArgs);
        output.Append(">::Variant;\n");

        foreach (var name in variantNames) {

            output.Append("    using ");
            output.Append(name);
            output.Append(" = ");
            output.Append(_enum.Name);
            output.Append("_Details::");
            output.Append(name);

            if (isGeneric) {

                output.Append('<');
                output.Append(Join(", ", genericParameterNames));
                output.Append('>');
            }

            output.Append(";\n");
        }

        if (_enum.DefinitionType == DefinitionType.Class) {

            var fullyInstantiatedName = new StringBuilder(_enum.Name);

            if (isGeneric) {

                fullyInstantiatedName.Append('<');
                fullyInstantiatedName.Append(Join(", ", genericParameterNames));
                fullyInstantiatedName.Append('>');
            }

            output.Append(
                "    template<typename V, typename... Args> static auto create(Args&&... args) {\n");
            output.Append($"        return adoptNonNullRefOrErrorNomem(new (nothrow) {fullyInstantiatedName}(V(forward<Args>(args)...)));\n");
            output.Append("    }\n");
        }

        output.Append(CodeGenEnumDebugDescriptionGetter(_enum));

        output.Append("};\n");

        context.DeferredOutput.Append(
            CodeGenCoreFormatter(
                _enum.Name,
                genericParameterNames,
                context));

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



















    public static String CodeGenEnumDebugDescriptionGetter(
        CheckedEnum _enum) {

        var output = new StringBuilder();

        output.Append("String debugDescription() const { ");

        output.Append("StringBuilder builder;");

        output.Append("this->visit(");

        for (var i = 0; i < _enum.Variants.Count; i++) {

            var variant = _enum.Variants[i];

            var name = variant switch {

                CheckedUntypedEnumVariant u => u.Name,
                CheckedTypedEnumVariant t => t.Name,
                CheckedWithValueEnumVariant v => v.Name,
                CheckedStructLikeEnumVariant s => s.Name,
                _ => throw new Exception()
            };

            output.Append($"[&]([[maybe_unused]] {name} const& that) {{\n");
        
            output.Append($"builder.append(\"{_enum.Name}.{name}\");");

            switch (variant) {

                case CheckedStructLikeEnumVariant sv: {

                    output.Append("builder.append(\"(\");");

                    for (var j = 0; j < sv.Decls.Count; j++) {

                        var field = sv.Decls[j];

                        output.Append($"builder.append(\"{field.Name}: \");");

                        if (field.TypeId == Compiler.StringTypeId) {
                            
                            output.Append("builder.append(\"\\\"\");");
                        }

                        output.Append($"builder.appendff(\"{{}}\", that.{field.Name});");

                        if (field.TypeId == Compiler.StringTypeId) {

                            output.Append("builder.append(\"\\\"\");");
                        }

                        if (j != (sv.Decls.Count - 1)) {

                            output.Append("builder.append(\", \");");
                        }
                    }

                    output.Append("builder.append(\")\");");

                    break;
                }

                case CheckedTypedEnumVariant tv: {

                    output.Append("builder.append(\"(\");");

                    if (tv.TypeId == Compiler.StringTypeId) {

                        output.Append("builder.append(\"\\\"\");");
                    }

                    output.Append("builder.appendff(\"{}\", that.value);");

                    if (tv.TypeId == Compiler.StringTypeId) {

                        output.Append("builder.append(\"\\\"\");");
                    }

                    output.Append("builder.append(\")\");");

                    break;
                }

                default: {

                    break;
                }
            }

            output.Append('}');

            if (i != (_enum.Variants.Count - 1)) {
                
                output.Append(',');
            }
        }

        output.Append(");");
        output.Append("return builder.toString();");
        output.Append(" }");

        return output.ToString();
    }

    public static String CodeGenDebugDescriptionGetter(
        CheckedStruct structure,
        Project project) {

        var output = new StringBuilder();

        output.Append("String debugDescription() const { ");

        output.Append("StringBuilder builder;");

        output.Append($"builder.append(\"{structure.Name}(\");");

        for (var i = 0; i < structure.Fields.Count; i++) {

            var field = structure.Fields[i];

            output.Append(CodeGenIndent(INDENT_SIZE));

            output.Append("builder.appendff(\"");
            output.Append(field.Name);
            output.Append(": {}");

            if (i != (structure.Fields.Count - 1)) {

                output.Append(", ");
            }

            output.Append("\", ");

            switch (project.Types[field.TypeId]) {

                case StructType st when project.Structs[st.StructId].DefinitionType == DefinitionType.Class: {

                    output.Append('*');

                    break;
                }

                default: {

                    break;
                }
            }

            output.Append(field.Name);

            output.Append(");\n");
        }

        output.Append("builder.append(\")\");");
        output.Append("return builder.toString();");
        output.Append(" }");

        return output.ToString();
    }

    public static String CodeGenCoreFormatter(
        String name,
        List<String> genericParameterNames,
        CodeGenContext context) {

        var templateArgs = Join(", ", genericParameterNames.Select(p => $"typename {p}"));

        var genericTypeArgs = Join(", ", genericParameterNames);

        var qualifiedNameBuilder = new StringBuilder();

        foreach (var ns in context.NamespaceStack) {

            qualifiedNameBuilder.Append(ns);
            qualifiedNameBuilder.Append("::");
        }

        qualifiedNameBuilder.Append(name);

        if (genericParameterNames.Any()) {

            qualifiedNameBuilder.Append('<');
            qualifiedNameBuilder.Append(genericTypeArgs);
            qualifiedNameBuilder.Append(">\n");
        }

        var qualifiedName = qualifiedNameBuilder.ToString();

        var output = new StringBuilder();

        // Begin Core namespace

        output.Append("template<");
        output.Append(templateArgs);
        output.Append('>');
        output.Append(
            $"struct Formatter<{qualifiedName}> : Formatter<StringView> {{\n");
        output.Append(
            $"    ErrorOr<void> format(FormatBuilder& builder, {qualifiedName} const& value)\n");
        output.Append("{ ");
        output.Append("return Formatter<StringView>::format(builder, value.debugDescription()); }");

        output.Append("};");

        // End Core namespace

        return output.ToString();
    }












    public static String CodeGenStruct(
        CheckedStruct structure,
        Project project,
        CodeGenContext context) {

        var output = new StringBuilder();

        if (structure.DefinitionLinkage == DefinitionLinkage.External) {

            return String.Empty;
        }

        if (structure.GenericParameters.Any()) {

            output.Append("template <");
        }

        var genericParameterNames = structure
            .GenericParameters
            .Select(p => {
                switch (project.Types[p]) {

                    case TypeVariable tv:
                        return tv.Name;

                    default:
                        throw new Exception("Unreachable");
                }
            })
            .ToList();

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

                output.Append($"class {structure.Name} : public RefCounted<{structure.Name}>, public Weakable<{structure.Name}> {{\n");
                
                // As we should test the visibility before codegen, we take a simple
                // approach to codegen
                
                output.Append("  public:\n");

                // Make sure emitted classes always have a vtable.
            
                output.Append($"    virtual ~{structure.Name}() = default;\n");

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

            output.Append(CodeGenIndent(INDENT_SIZE));

            output.Append(CodeGenType(field.TypeId, project));
            
            output.Append(' ');
            
            output.Append(field.Name);
            
            output.Append(";\n");
        }

        var scope = project.Scopes[structure.ScopeId];

        foreach (var (_, funcId, _) in scope.Funcs) {

            var func = project.Functions[funcId];

            if (func.Linkage == FunctionLinkage.ImplicitConstructor) {

                var funcOutput = CodeGenConstructor(func, project);

                output.Append(CodeGenIndent(INDENT_SIZE));
                output.Append(funcOutput);
                output.Append('\n');
            }
            else {

                output.Append(CodeGenIndent(INDENT_SIZE));

                String methodOutput;

                if (!structure.GenericParameters.Any()) {

                    methodOutput = CodeGenFuncPredecl(func, project);
                }
                else {

                    methodOutput = CodeGenFunc(func, project);
                }

                output.Append(methodOutput);
            }
        }

        output.Append(CodeGenDebugDescriptionGetter(structure, project));

        output.Append("};");

        context.DeferredOutput.Append(
            CodeGenCoreFormatter(
                structure.Name,
                genericParameterNames,
                context));
        
        return output.ToString();
    }

    public static String CodeGenFuncPredecl(
        CheckedFunction fun,
        Project project) {
        
        var output = new StringBuilder();

        // Extern generics need to be in the header anyways, so we can't codegen for them.

        if ((new [] { 
                FunctionLinkage.External,
                FunctionLinkage.ExternalClassConstructor
            }).Contains(fun.Linkage)
            && fun.GenericParameters.Any() ) {
        
            return output.ToString();
        }

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
            
            Int32? _id = null;

            switch (genParam) {

                case InferenceGuideFunctionGenericParameter i: {

                    _id = i.TypeId;

                    break;
                }

                case ParameterFunctionGenericParameter p: {

                    _id = p.TypeId;

                    break;
                }

                default: {

                    break;
                }
            }

            var id = _id ?? throw new Exception();

            output.Append(CodeGenType(id, project));
        }

        if (fun.GenericParameters.Any()) {

            output.Append(">\n");
        }

        if (fun.Name == "main") {

            output.Append("ErrorOr<int>");
        }
        else {

            if (fun.IsStatic() && fun.Linkage != FunctionLinkage.External) {

                output.Append("static ");
            }

            String returnType;

            if (fun.Throws) {

                returnType = $"ErrorOr<{CodeGenType(fun.ReturnTypeId, project)}>";
            }
            else {

                returnType = CodeGenType(fun.ReturnTypeId, project);
            }

            output.Append(returnType);
        }

        output.Append(' ');

        output.Append(fun.Name);

        output.Append('(');

        var first = true;

        foreach (var p in fun.Parameters) {

            if (first && p.Variable.Name == "this") {

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

            var ty = CodeGenType(p.Variable.TypeId, project);

            output.Append(ty);

            output.Append(" ");

            output.Append(p.Variable.Name);
        }

        if (!fun.IsStatic() && !fun.IsMutating()) {

            output.Append(") const;");
        }
        else {

            output.Append(");");
        }

        return output.ToString();
    }

    public static String CodeGenFunc(
        CheckedFunction fun,
        Project project) {

        return CodeGenFuncInNamespace(fun, null, project);
    }

    public static String CodeGenFuncInNamespace(
        CheckedFunction fun,
        Int32? containingStruct,
        Project project) {

        // Extern generics need to be in the header anyways, so we can't codegen for them.

        if ((new [] { 
                FunctionLinkage.External,
                FunctionLinkage.ExternalClassConstructor
            }).Contains(fun.Linkage)
            && fun.GenericParameters.Any()) {

            return String.Empty;
        }

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

            Int32? _id = null;

            switch (genParam) {

                case InferenceGuideFunctionGenericParameter i: {

                    _id = i.TypeId;

                    break;
                }

                case ParameterFunctionGenericParameter p: {

                    _id = p.TypeId;

                    break;
                }

                default: {

                    break;
                }
            }

            var id = _id ?? throw new Exception();

            output.Append(CodeGenType(id, project));
        }

        if (fun.GenericParameters.Any()) {
            
            output.Append(">\n");
        }

        if (fun.Name == "main") {

            output.Append("ErrorOr<int>");
        }
        else {

            if (fun.IsStatic() && containingStruct == null) {

                output.Append("static ");
            }

            String returnType;

            if (fun.Throws) {

                returnType = $"ErrorOr<{CodeGenType(fun.ReturnTypeId, project)}>";
            }
            else {

                returnType = CodeGenType(fun.ReturnTypeId, project);
            }

            output.Append(returnType);
        }

        output.Append(' ');

        var isMain = fun.Name == "main" && containingStruct == null;

        if (isMain) {

            output.Append("NeuInternal::main");
        }
        else {

            String? _qualifier = null;

            if (containingStruct is Int32 _typeId) {

                _qualifier = CodeGenTypePossiblyAsNamespace(_typeId, project, true);
            }

            if (_qualifier is String qualifier) {

                if (!IsNullOrWhiteSpace(qualifier)) {

                    output.Append(qualifier);
                    output.Append("::");
                }
            }

            output.Append(fun.Name);
        }

        output.Append('(');

        if (isMain && !fun.Parameters.Any()) {
        
            output.Append("Array<String>");
        }

        var first = true;

        foreach (var p in fun.Parameters) {

            if (p.Variable.Name == "this") {

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

            var ty = CodeGenType(p.Variable.TypeId, project);

            output.Append(ty);

            output.Append(' ');

            output.Append(p.Variable.Name);
        }

        output.Append(')');

        if (!fun.IsStatic() && !fun.IsMutating()) {

            output.Append(" const");
        }

        output.Append("\n{\n");
        output.Append(CodeGenIndent(INDENT_SIZE));
        
        // Put the return type in scope

        if (isMain) {

            output.Append("using _NeuCurrentFunctionReturnType = ErrorOr<int>;\n");
        }
        else {

            if (fun.ReturnTypeId == Compiler.UnknownTypeId) {

                throw new Exception($"Function type unknown at codegen time in {fun.Name}");
            }

            if (fun.Throws) {

                output.Append($"using _NeuCurrentFunctionReturnType = ErrorOr<{CodeGenType(fun.ReturnTypeId, project)}>;\n");
            }
            else {

                output.Append($"using _NeuCurrentFunctionReturnType = {CodeGenType(fun.ReturnTypeId, project)};\n");
            }
        }

        var block = CodeGenBlock(
            INDENT_SIZE, 
            fun.Block
                ?? throw new Exception("Function being generated must be checked"),
            project);

        output.Append(block);

        if (isMain) {
            
            output.Append(CodeGenIndent(INDENT_SIZE));
            output.Append("return 0;\n");
        }
        else if (fun.Throws && fun.ReturnTypeId == Compiler.VoidTypeId) {
            
            output.Append(CodeGenIndent(INDENT_SIZE));
            output.Append("return {};\n");
        }
        
        output.Append("}\n");

        return output.ToString();
    }

    public static String CodeGenConstructor(
        CheckedFunction func,
        Project project) {

        var typeId = func.ReturnTypeId;

        var ty = project.Types[typeId];

        switch (ty) {

            case StructType st: {

                var structure = project.Structs[st.StructId];

                if (structure.DefinitionType == DefinitionType.Class) {

                    var output = new StringBuilder();

                    // First, generate a private constructor:

                    output.Append("private:\n");

                    output.Append($"explicit {func.Name}(");
                    
                    var first1 = true;

                    foreach (var param in func.Parameters) {

                        if (!first1) {
                            
                            output.Append(", ");
                        } 
                        else {
                            
                            first1 = false;
                        }

                        output.Append(CodeGenType(param.Variable.TypeId, project));
                        output.Append("&& a_");
                        output.Append(param.Variable.Name);
                    }

                    output.Append(')');

                    if (func.Parameters.Any()) {

                        output.Append(": ");

                        var first2 = true;

                        foreach (var param in func.Parameters) {

                            if (!first2) {
                                
                                output.Append(", ");
                            } 
                            else {
                                
                                first2 = false;
                            }

                            output.Append(param.Variable.Name);
                            output.Append("(move(a_");
                            output.Append(param.Variable.Name);
                            output.Append("))");
                        }
                    }

                    output.Append("{}\n");

                    output.Append("public:\n");
                    output.Append(
                        $"static ErrorOr<NonNullRefPointer<{func.Name}>> create");









                    output.Append('(');

                    var first3 = true;

                    foreach (var param in func.Parameters) {

                        if (!first3) {

                            output.Append(", ");
                        }
                        else {

                            first3 = false;
                        }

                        var tyStr = CodeGenType(param.Variable.TypeId, project);

                        output.Append(tyStr);
                        output.Append(' ');
                        output.Append(param.Variable.Name);
                    }

                    output.Append($") {{ auto o = TRY(adoptNonNullRefOrErrorNomem(new (nothrow) {func.Name} (");

                    var first4 = true;

                    foreach (var param in func.Parameters) {

                        if (!first4) {
                            
                            output.Append(", ");
                        } 
                        else {
                            
                            first4 = false;
                        }

                        output.Append("move(");
                        output.Append(param.Variable.Name);
                        output.Append(')');
                    }

                    output.Append("))); return o; }");

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

                        var tyStr = CodeGenType(param.Variable.TypeId, project);
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

    public static String CodeGenNamespaceQualifier(
        Int32 scopeId,
        Project project) {

        var output = new StringBuilder();

        Int32? currentScopeId = project.Scopes[scopeId].Parent;

        while (currentScopeId is not null) {

            // Walk backward, prepending the parents with names to the current output

            if (project.Scopes[currentScopeId.Value].NamespaceName is String namespaceName) {

                output.Insert(0, $"{namespaceName}::");
            }

            currentScopeId = project.Scopes[currentScopeId.Value].Parent;
        }

        return output.ToString();
    }

    public static String CodeGenType(
        Int32 typeId,
        Project project) {

        return CodeGenTypePossiblyAsNamespace(typeId, project, false);
    }

    public static String CodeGenTypePossiblyAsNamespace(
        Int32 typeId,
        Project project,
        bool asNamespace) {

        var output = new StringBuilder();

        var ty = project.Types[typeId];

        var weakPointerStructId = project
            .FindStructInScope(0, "WeakPointer") 
            ?? throw new Exception("internal error: can't find builtin WeakPointer type");

        switch (ty) {

            case RawPointerType pt: {

                return $"{CodeGenType(pt.TypeId, project)}*";
            }

            case GenericInstance gi when gi.StructId == weakPointerStructId: {

                var innerTy = gi.TypeIds[0];

                if (project.Types[innerTy] is StructType st) {
                    output.Append("WeakPointer<");
                    output.Append(CodeGenNamespaceQualifier(
                        project.Structs[st.StructId].ScopeId,
                        project));
                    output.Append(project.Structs[st.StructId].Name);
                    output.Append('>');
                }

                return output.ToString();
            }

            case GenericInstance gi: {

                output.Append(CodeGenNamespaceQualifier(project.Structs[gi.StructId].ScopeId, project));

                output.Append(project.Structs[gi.StructId].Name);

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

            case GenericEnumInstance gei: {

                var closeTag = false;

                if (!asNamespace && project.Enums[gei.EnumId].DefinitionType == DefinitionType.Class) {

                    output.Append("NonNullRefPointer<");
                    
                    var qualifier = CodeGenNamespaceQualifier(project.Enums[gei.EnumId].ScopeId, project);

                    if (!IsNullOrWhiteSpace(qualifier)) {

                        output.Append("typename ");
                        output.Append(qualifier);
                    }

                    output.Append(project.Enums[gei.EnumId].Name);

                    closeTag = true;
                }
                else {

                    var qualifier = CodeGenNamespaceQualifier(project.Enums[gei.EnumId].ScopeId, project);

                    if (!IsNullOrWhiteSpace(qualifier)) {

                        output.Append("typename ");
                        output.Append(qualifier);
                    }

                    output.Append(project.Enums[gei.EnumId].Name);
                }

                output.Append('<');

                var first = true;

                foreach (var innerTy in gei.TypeIds) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append(CodeGenType(innerTy, project));
                }

                output.Append('>');

                if (closeTag) {

                    output.Append('>');
                }

                return output.ToString();
            }

            case StructType st: {

                var inner = project.Structs[st.StructId];

                if (!asNamespace && inner.DefinitionType == DefinitionType.Class) {

                    output.Append("NonNullRefPointer<");
                    output.Append(CodeGenNamespaceQualifier(inner.ScopeId, project));
                    output.Append(inner.Name);
                    output.Append('>');

                }
                else {

                    output.Append(CodeGenNamespaceQualifier(inner.ScopeId, project));
                    output.Append(inner.Name);
                }

                return output.ToString();
            }

            case EnumType e: {

                if (!asNamespace && project.Enums[e.EnumId].DefinitionType == DefinitionType.Class) {

                    output.Append("NonNullRefPointer<");

                    var qualifier = CodeGenNamespaceQualifier(project.Enums[e.EnumId].ScopeId, project);

                    if (!IsNullOrWhiteSpace(qualifier)) {

                        output.Append("typename ");
                        output.Append(qualifier);
                    }

                    output.Append(project.Enums[e.EnumId].Name);
                    output.Append('>');

                }
                else {

                    var qualifier = CodeGenNamespaceQualifier(project.Enums[e.EnumId].ScopeId, project);

                    if (!IsNullOrWhiteSpace(qualifier)) {

                        output.Append("typename ");
                        output.Append(qualifier);
                    }

                    output.Append(project.Enums[e.EnumId].Name);
                }

                return output.ToString();
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

        output.Append(CodeGenIndent(indent));

        output.Append("{\n");

        foreach (var stmt in block.Stmts) {

            var stmtStr = CodeGenStatement(indent + INDENT_SIZE, stmt, project);

            output.Append(stmtStr);
        }

        output.Append(CodeGenIndent(indent));

        output.Append("}\n");

        return output.ToString();
    }

    public static String CodeGenStatement(
        int indent,
        CheckedStatement stmt,
        Project project) {

        var output = new StringBuilder();

        output.Append(CodeGenIndent(indent));

        ///

        switch (stmt) {

            case CheckedTryStatement ts: {

                output.Append('{');
                output.Append("auto _neu_tryResult = [&]() -> ErrorOr<void> {");
                output.Append(CodeGenStatement(indent, ts.Statement, project));
                output.Append(';');
                output.Append("return { };");
                output.Append("}();");
                output.Append("if (_neu_tryResult.isError()) {");

                if (!IsNullOrWhiteSpace(ts.Name)) {
                
                    output.Append("auto ");
                    output.Append(ts.Name);
                    output.Append(" = _neu_tryResult.releaseError();");
                }

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

            case CheckedExpressionStatement es: {

                var exprStr = CodeGenExpr(indent, es.Expression, project);

                output.Append(exprStr);

                output.Append(";\n");

                break;
            }

            case CheckedDeferStatement defer: {

                // NOTE: We let the preprocessor generate a unique name for the RAII helper.
                output.Append("#define __SCOPE_GUARD_NAME __scope_guard_ ## __COUNTER__\n");
                output.Append("ScopeGuard __SCOPE_GUARD_NAME ([&] \n");
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

                    output.Append(CodeGenIndent(indent));

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

                output.Append(CodeGenType(vd.VarDecl.TypeId, project));
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

            case CheckedInlineCppStatement i: {

                var first = true;

                foreach (var str in i.Lines) {

                    if (!first) {

                        output.Append(CodeGenIndent(indent));
                    }
                    else {

                        first = false;
                    }

                    output.Append(str.Replace("\\\"", "\"").Replace("\\\\", "\\"));

                    output.Append("\n");
                }

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

    public static String CodeGenCheckedBinaryOp(
        int indent,
        CheckedExpression lhs,
        CheckedExpression rhs,
        Int32 typeId,
        BinaryOperator op,
        Project project) {

        var output = new StringBuilder();

        output.Append("NeuInternal::");

        switch (op) {

            case BinaryOperator.Add: 
                output.Append("checkedAdd");
                break;

            case BinaryOperator.Subtract:
                output.Append("checkedSub");
                break;

            case BinaryOperator.Multiply:
                output.Append("checkedMul");
                break;

            case BinaryOperator.Divide: 
                output.Append("checkedDiv");
                break;

            case BinaryOperator.Modulo: 
                output.Append("checkedMod");
                break;

            default:
                throw new Exception($"Checked binary operation codegen is not supported for BinaryOperator::{op}");
        }

        output.Append('<');
        output.Append(CodeGenType(typeId, project));
        output.Append(">(");
        output.Append(CodeGenExpr(indent, lhs, project));
        output.Append(", ");
        output.Append(CodeGenExpr(indent, rhs, project));
        output.Append(')');

        return output.ToString();
    }

    public static String CodeGenCheckedBinaryOpAssign(
        int indent,
        CheckedExpression lhs,
        CheckedExpression rhs,
        Int32 typeId,
        BinaryOperator op,
        Project project) {

        var output = new StringBuilder();

        output.Append('{');
        output.Append("auto& _neu_ref = ");
        output.Append(CodeGenExpr(indent, lhs, project));
        output.Append(';');
        output.Append("_neu_ref = NeuInternal::");

        switch (op) {

            case BinaryOperator.AddAssign:
                output.Append("checkedAdd");
                break;

            case BinaryOperator.SubtractAssign:
                output.Append("checkedSub");
                break;

            case BinaryOperator.MultiplyAssign:
                output.Append("checkedMul");
                break;

            case BinaryOperator.DivideAssign:
                output.Append("checkedDiv");
                break;

            case BinaryOperator.ModuloAssign:
                output.Append("checkedMod");
                break;

            default:
                throw new Exception($"Checked binary operation assignment codegen is not supported for BinaryOperator::{op}");
        };

        output.Append('<');
        output.Append(CodeGenType(typeId, project));
        output.Append(">(_neu_ref, ");
        output.Append(CodeGenExpr(indent, rhs, project));
        output.Append(");");
        output.Append('}');

        return output.ToString();
    }

    public static String CodeGenWhenBody(
        int indent,
        CheckedWhenBody body,
        Project project,
        Int32 returnTypeId) {

        var output = new StringBuilder();

        switch (body) {

            case CheckedExpressionWhenBody expr: {

                if (expr.Expression.GetTypeIdOrTypeVar() == Compiler.VoidTypeId) {

                    output.Append("   return (");
                    output.Append(CodeGenExpr(indent + 1, expr.Expression, project));
                    output.Append("), NeuInternal::ExplicitValue<void>();\n");
                }
                else {

                    output.Append("   return NeuInternal::ExplicitValue(");
                    output.Append(CodeGenExpr(indent + 1, expr.Expression, project));
                    output.Append(");\n");
                }

                break;
            }

            case CheckedBlockWhenBody block: {

                output.Append(CodeGenBlock(indent, block.Block, project));

                if (returnTypeId == Compiler.VoidTypeId) {

                    output.Append("return NeuInternal::ExplicitValue<void>();\n");
                }

                break;
            }

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }

    public static String CodeGenEnumMatch(
        CheckedEnum _enum,
        CheckedExpression expr,
        List<CheckedWhenCase> cases,
        Int32 returnTypeId,
        bool matchValuesAreAllConstant,
        Int32 indent,
        Project project) {

        var output = new StringBuilder();

        var needsDeref = _enum.DefinitionType == DefinitionType.Class;

        switch (_enum.UnderlyingTypeId) {

            case Int32 _: {

                if (matchValuesAreAllConstant) {

                    // Use a switch statement instead of if-else chains
                    
                    output.Append("NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(([&]() -> NeuInternal::ExplicitValueOrReturn<");
                    output.Append(CodeGenType(returnTypeId, project));
                    output.Append(", ");
                    output.Append("_NeuCurrentFunctionReturnType");
                    output.Append("> { \n");
                    output.Append("switch (");
                    
                    if (needsDeref) {
                        
                        output.Append('*');
                    }
                    
                    output.Append(CodeGenExpr(indent, expr, project));
                    output.Append(") {\n");
                }
                else {

                    output.Append("NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(([&]() -> NeuInternal::ExplicitValueOrReturn<");
                    output.Append(CodeGenType(returnTypeId, project));
                    output.Append(", ");
                    output.Append("_NeuCurrentFunctionReturnType");
                    output.Append("> { \n");
                    output.Append("auto __neu_enum_value = ");

                    if (needsDeref) {

                        output.Append('*');
                    }

                    output.Append(CodeGenExpr(indent, expr, project));
                    output.Append(";\n");
                }

                var first1 = true;

                foreach (var _case in cases) {

                    var thisIsTheFirstCase = first1;

                    if (first1) {

                        first1 = false;
                    }

                    switch (_case) {

                        case CheckedEnumVariantWhenCase evwc: {

                            if (matchValuesAreAllConstant) {
                                output.Append("case ");
                            } 
                            else {
                                
                                if (!thisIsTheFirstCase) {
                                    
                                    output.Append("else ");
                                }

                                output.Append("if (__neu_enum_value == ");
                            }

                            output.Append(
                                CodeGenTypePossiblyAsNamespace(
                                    expr.GetTypeIdOrTypeVar(),
                                    project,
                                    true));

                            output.Append("::");
                            output.Append(evwc.VariantName);
                            
                            if (matchValuesAreAllConstant) {

                                output.Append(":\n");
                            } 
                            else {
                                output.Append(") {\n");
                            }

                            output.Append(CodeGenIndent(indent));
                            output.Append(
                                CodeGenWhenBody(
                                    indent + 1,
                                    evwc.Body,
                                    project,
                                    returnTypeId));

                            if (matchValuesAreAllConstant) {

                                output.Append("break;\n");
                            } 
                            else {

                                output.Append('}');
                            }

                            break;
                        }

                        case CheckedCatchAllWhenCase cawc: {

                            if (matchValuesAreAllConstant) {
                                
                                output.Append("default:\n");
                            } 
                            else if (thisIsTheFirstCase) {
                                
                                output.Append('{');
                            } 
                            else {
                                
                                output.Append("else {\n");
                            }
                            
                            output.Append(CodeGenIndent(indent + 1));
                            output.Append(
                                CodeGenWhenBody(
                                    indent + 1,
                                    cawc.Body,
                                    project,
                                    returnTypeId));
                            
                            if (matchValuesAreAllConstant) {

                                output.Append("break;\n");
                            } 
                            else {

                                output.Append('}');
                            }

                            break;
                        }

                        case CheckedExpressionWhenCase ewc: {

                            if (matchValuesAreAllConstant) {

                                output.Append("case ");
                            } 
                            else {
                                
                                if (!thisIsTheFirstCase) {
                                    
                                    output.Append("else ");
                                }

                                output.Append("if (__neu_enum_value == ");
                            }

                            output.Append(CodeGenExpr(indent, ewc.Expression, project));
                            
                            if (matchValuesAreAllConstant) {

                                output.Append(":\n");
                            } 
                            else {

                                output.Append(") {\n");
                            }

                            output.Append(CodeGenIndent(indent + 1));
                            
                            output.Append(
                                CodeGenWhenBody(
                                    indent + 1,
                                    ewc.Body,
                                    project,
                                    returnTypeId));

                            if (matchValuesAreAllConstant) {

                                output.Append("break;\n");
                            } 
                            else {

                                output.Append('}');
                            }

                            break;
                        }

                        default: {

                            throw new Exception();
                        }
                    }
                    
                    output.Append('\n');
                }

                if (matchValuesAreAllConstant) {

                    output.Append("}\n");
                }

                output.Append("}()))");

                break;
            }

            default: {

                output.Append("NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN((");
                output.Append(CodeGenExpr(indent, expr, project));
                output.Append(')');

                if (needsDeref) {

                    output.Append("->");
                }
                else {

                    output.Append('.');
                }

                output.Append("visit(");

                var first = true;

                foreach (var _case in cases) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append("[&] (");

                    switch (_case) {

                        case CheckedEnumVariantWhenCase evwc: {

                            var _type = project.Types[evwc.SubjectTypeId];

                            var enumId = _type switch {
                                GenericEnumInstance gei => gei.EnumId,
                                EnumType et => et.EnumId,
                                _ => throw new Exception("Expected enum type")
                            };

                            var _enum2 = project.Enums[enumId];

                            var variant = _enum2.Variants[evwc.VariantIndex];

                            switch (variant) {

                                case CheckedTypedEnumVariant t: {

                                    output.Append("typename ");
                                    
                                    output.Append(CodeGenTypePossiblyAsNamespace(
                                        evwc.SubjectTypeId,
                                        project,
                                        true));
                                    
                                    output.Append("::");
                                    output.Append(t.Name);
                                    output.Append(
                                        " const& __neu_match_value) -> NeuInternal::ExplicitValueOrReturn<");
                                    
                                    output.Append(CodeGenType(returnTypeId, project));
                                    output.Append(", ");
                                    output.Append("_NeuCurrentFunctionReturnType");
                                    output.Append('>');
                                    output.Append(" {\n");
                                    output.Append("   ");

                                    if (evwc.VariantArguments.Any()) {

                                        var _var = project
                                            .FindVarInScope(evwc.ScopeId, evwc.VariantArguments[0].Item2)
                                            ?? throw new Exception();
                                            
                                        output.Append(CodeGenType(_var.TypeId, project));
                                        output.Append(" const& ");
                                        output.Append(evwc.VariantArguments[0].Item2);
                                        output.Append(" = __neu_match_value.value;\n");
                                    }

                                    break;
                                }

                                case CheckedUntypedEnumVariant u: {

                                    output.Append("typename ");
                                    output.Append(CodeGenTypePossiblyAsNamespace(
                                        evwc.SubjectTypeId,
                                        project,
                                        true));
                                    output.Append("::");
                                    output.Append(u.Name);
                                    output.Append(
                                        " const& __neu_match_value) -> NeuInternal::ExplicitValueOrReturn<");
                                    output.Append(CodeGenType(returnTypeId, project));
                                    output.Append(", ");
                                    output.Append("_NeuCurrentFunctionReturnType");
                                    output.Append('>');
                                    output.Append(" {\n");

                                    break;
                                }

                                case CheckedStructLikeEnumVariant s: {

                                    output.Append("typename ");

                                    output.Append(CodeGenTypePossiblyAsNamespace(
                                        evwc.SubjectTypeId,
                                        project,
                                        true));

                                    output.Append("::");
                                    output.Append(s.Name);
                                    output.Append(
                                        " const& __neu_match_value) -> NeuInternal::ExplicitValueOrReturn<");

                                    output.Append(CodeGenType(returnTypeId, project));
                                    output.Append(", ");
                                    output.Append("_NeuCurrentFunctionReturnType");
                                    output.Append('>');
                                    output.Append(" {\n");

                                    if (evwc.VariantArguments.Any()) {

                                        foreach (var arg in evwc.VariantArguments) {

                                            var _var = project.FindVarInScope(evwc.ScopeId, arg.Item2) ?? throw new Exception();

                                            output.Append(CodeGenType(_var.TypeId, project));
                                            output.Append(" const& ");
                                            output.Append(arg.Item2);
                                            output.Append(" = __neu_match_value.");
                                            output.Append(arg.Item1 ?? throw new Exception());
                                            output.Append(";\n");
                                        }
                                    }

                                    break;
                                }

                                case CheckedWithValueEnumVariant v: {

                                    throw new Exception("Unreachable");
                                }

                                default: {

                                    throw new Exception();
                                }
                            }

                            output.Append(
                                CodeGenWhenBody(
                                    indent + 1, 
                                    evwc.Body, 
                                    project, 
                                    returnTypeId));

                            break;
                        }

                        case CheckedCatchAllWhenCase cawc: {

                            output.Append(
                                "auto const& __neu_match_value) -> NeuInternal::ExplicitValueOrReturn<");

                            output.Append(CodeGenType(returnTypeId, project));
                            output.Append(", ");
                            output.Append("_NeuCurrentFunctionReturnType");
                            output.Append('>');
                            output.Append(" {\n");

                            output.Append(
                                CodeGenWhenBody(
                                    indent + 1,
                                    cawc.Body,
                                    project,
                                    returnTypeId));

                            break;
                        }

                        default: {

                            throw new Exception("Matching enum subject with non-enum value");
                        }
                    }

                    output.Append("}\n");
                }

                output.Append(')');
                output.Append(')');

                break;
            }
        }

        return output.ToString();

        throw new Exception();
    }

    public static String CodeGenGenericMatch(
        CheckedExpression expr,
        List<CheckedWhenCase> cases,
        Int32 returnTypeId,
        bool matchValuesAreAllConstant,
        Int32 indent,
        Project project) {

        var output = new StringBuilder();

        var isGenericEnum = cases.Any(c => c is CheckedEnumVariantWhenCase);

        var _matchValuesAreAllConstant = matchValuesAreAllConstant && !isGenericEnum;

        if (_matchValuesAreAllConstant && !isGenericEnum) {
    
            // Use a switch statement instead of if-else chains

            output.Append(
                "NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(([&]() -> NeuInternal::ExplicitValueOrReturn<");
            
            output.Append(CodeGenType(returnTypeId, project));
            output.Append(", ");
            output.Append("_NeuCurrentFunctionReturnType");
            output.Append("> { \n");
            output.Append("switch (");
            output.Append(CodeGenExpr(indent, expr, project));
            output.Append(") {\n");
        } 
        else {

            output.Append(
                "NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(([&]() -> NeuInternal::ExplicitValueOrReturn<");
            
            output.Append(CodeGenType(returnTypeId, project));
            output.Append(", ");
            output.Append("_NeuCurrentFunctionReturnType");
            output.Append("> { \n");

            if (isGenericEnum) {
                
                output.Append("auto&& __neu_enum_value = NeuInternal::derefIfRefPointer(");
            } 
            else {
                
                output.Append("auto __neu_enum_value = (");
            }

            output.Append(CodeGenExpr(indent, expr, project));

            output.Append(");\n");
        }

        var first = true;

        foreach (var _case in cases) {

            var thisIsTheFirstCase = first;

            if (first) {

                first = false;
            }

            switch (_case) {

                case CheckedEnumVariantWhenCase evwc: {

                    output.Append("if (__neu_enum_value.template has<");

                    var variantTypeBuilder = new StringBuilder();

                    var variantTypeQualifier = CodeGenType(expr.GetTypeId(evwc.ScopeId, project), project);

                    if (!IsNullOrWhiteSpace(variantTypeQualifier)) {

                        variantTypeBuilder.Append("typename NeuInternal::RemoveRefPointer<");
                        variantTypeBuilder.Append(variantTypeQualifier);
                        variantTypeBuilder.Append(">::");
                    }

                    variantTypeBuilder.Append(evwc.VariantName);

                    var variantTypeName = variantTypeBuilder.ToString();

                    output.Append(variantTypeName);
                    output.Append(">()) {\n");
                    output.Append("auto& __neu_match_value = __neu_enum_value.template get<");
                    output.Append(variantTypeName);
                    output.Append(">();\n");

                    foreach (var arg in evwc.VariantArguments) {

                        output.Append("auto& ");
                        output.Append(arg.Item2);
                        output.Append(" = __neu_match_value.");
                        output.Append(arg.Item1 ?? "value");
                        output.Append(";\n");
                    }

                    output.Append(
                        CodeGenWhenBody(
                            indent + 1,
                            evwc.Body,
                            project,
                            returnTypeId));

                    output.Append("}\n");

                    break;
                }

                case CheckedCatchAllWhenCase cawc: {

                    if (_matchValuesAreAllConstant) {
                        
                        output.Append("default:\n");
                    } 
                    else if (thisIsTheFirstCase) {
                        
                        output.Append('{');
                    } 
                    else {
                        
                        output.Append("else {\n");
                    }
                    
                    output.Append(CodeGenIndent(indent + 1));
                    output.Append(
                        CodeGenWhenBody(
                            indent + 1,
                            cawc.Body,
                            project,
                            returnTypeId));
                            
                    if (_matchValuesAreAllConstant) {

                        output.Append("break;\n");
                    } 
                    else {
                        
                        output.Append('}');
                    }

                    break;
                }

                case CheckedExpressionWhenCase ewc: {

                    if (_matchValuesAreAllConstant) {
                        
                        output.Append("case ");
                    } 
                    else {
                        
                        if (!thisIsTheFirstCase) {
                            
                            output.Append("else ");
                        }
                        
                        output.Append("if (__neu_enum_value == ");
                    }

                    output.Append(CodeGenExpr(indent, ewc.Expression, project));
                    
                    if (_matchValuesAreAllConstant) {
                        
                        output.Append(":\n");
                    } 
                    else {

                        output.Append(") {\n");
                    }
                    
                    output.Append(CodeGenIndent(indent + 1));
                    output.Append(
                        CodeGenWhenBody(
                            indent + 1,
                            ewc.Body,
                            project,
                            returnTypeId));
                    
                    if (_matchValuesAreAllConstant) {
                        
                        output.Append("break;\n");
                    } 
                    else {
                        
                        output.Append('}');
                    }

                    break;
                }

                default: {

                    throw new Exception();
                }
            }

            output.Append('\n');
        }

        if (_matchValuesAreAllConstant) {
            
            output.Append("}\n");
        }

        if (returnTypeId == Compiler.VoidTypeId) {
            
            output.Append("return NeuInternal::ExplicitValue<void>();\n");
        }
        
        output.Append("}()))");

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

                output.Append("NeuInternal::OptionalNone()");

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

            case CheckedByteConstantExpression b: {

                output.Append('\'');
                output.Append(b.Value);
                output.Append('\'');

                break;
            }

            case CheckedCharacterConstantExpression cce: {

                output.Append('\'');
                output.Append(cce.Value);
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

                    case FloatConstant f: {

                        output.Append("static_cast<Float>(");
                        output.Append(f.Value.ToString());
                        output.Append(")");

                        break;
                    }

                    case DoubleConstant d: {

                        output.Append("static_cast<Double>(");
                        output.Append(d.Value.ToString());
                        output.Append(')');

                        break;
                    }

                    default: { 
                        
                        break;
                    }
                }

                break;
            }

            case CheckedNamespacedVarExpression n: {

                foreach (var ns in n.Namespace) {
                    
                    if (!IsNullOrWhiteSpace(ns.Name)) {
                        
                        output.Append(ns.Name);
                        output.Append("::");
                    }
                }

                output.Append(n.Variable.Name);

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

                    case "print": {

                        output.Append("out(");

                        for (var i = 0; i < ce.Call.Args.Count; i++) {

                            output.Append(CodeGenExpr(indent, ce.Call.Args[i].Item2, project));

                            if (i != ce.Call.Args.Count - 1) {

                                output.Append(',');
                            }
                        }

                        output.Append(')');

                        break;
                    }

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

                    case "format": {

                        output.Append("String::formatted(");

                        for (var i = 0; i < ce.Call.Args.Count; i++) {

                            var param = ce.Call.Args[i];
                        
                            output.Append(CodeGenExpr(indent, param.Item2, project));

                            if (i != ce.Call.Args.Count - 1) {

                                output.Append(',');
                            }
                        }

                        output.Append(')');

                        break;
                    }

                    ///

                    default: {

                        if (ce.Call.Linkage == FunctionLinkage.ImplicitConstructor
                            || ce.Call.Linkage == FunctionLinkage.ExternalClassConstructor) {

                            var typeId = ce.Call.TypeId;

                            var ty = project.Types[typeId];

                            output.Append(CodeGenNamespacePath(ce.Call, project));

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
                        else if (ce.Call.Linkage == FunctionLinkage.ImplicitEnumConstructor) {

                            var typeId = ce.Call.TypeId;

                            var type = project.Types[typeId];

                            var enumId = type switch {
                                EnumType e => e.EnumId,
                                GenericEnumInstance i => i.EnumId,
                                _ => throw new Exception("internal error: constructor expected enum type")
                            };

                            var _enum = project.Enums[enumId];

                            if (_enum.DefinitionType == DefinitionType.Struct) {

                                output.Append("typename ");
                                output.Append(CodeGenNamespacePath(ce.Call, project));
                                output.Append(ce.Call.Name);
                            } 
                            else {

                                output.Append(CodeGenNamespacePath(ce.Call, project));
                                output.Append("template create<");
                                output.Append("typename ");
                                output.Append(
                                    CodeGenTypePossiblyAsNamespace(
                                        typeId, project, true));
                                output.Append("::");
                                output.Append(ce.Call.Name);
                                output.Append('>');
                            }
                        }
                        else {

                            output.Append(CodeGenNamespacePath(ce.Call, project));

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

                        switch (project.Types[x.GetTypeIdOrTypeVar()]) {

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

            case CheckedWhenExpression we: {

                var exprType = project.Types[we.Expression.GetTypeIdOrTypeVar()];

                Int32? _enumId = exprType switch {

                    GenericEnumInstance gei => gei.EnumId,
                    EnumType et => et.EnumId,
                    _ => null
                };

                switch (_enumId) {

                    case Int32 enumId: {

                        var _enum = project.Enums[enumId];

                        output.Append(
                            CodeGenEnumMatch(
                                _enum,
                                we.Expression,
                                we.Cases,
                                we.TypeId,
                                we.ValuesAreAllConstant,
                                indent,
                                project));

                        break;
                    }

                    default: {

                        output.Append(
                            CodeGenGenericMatch(
                                we.Expression,
                                we.Cases,
                                we.TypeId,
                                we.ValuesAreAllConstant,
                                indent,
                                project));

                        break;
                    }
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

                var exprTy = expr.GetTypeIdOrTypeVar();

                var exprIsInt = NeuTypeFunctions.IsInteger(exprTy);

                switch (binOp.Operator) {

                    case BinaryOperator.NoneCoalescing: {

                        var rhsType = project.Types[binOp.Rhs.GetTypeIdOrTypeVar()];

                        var optionalStructId = project.CachedOptionalStructId ?? throw new Exception();

                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));

                        switch (rhsType) {

                            case GenericInstance gi when gi.StructId == optionalStructId: {

                                output.Append(".valueOrLazyEvaluatedOptional([&] { return ");

                                break;
                            }

                            default: {

                                output.Append(".valueOrLazyEvaluated([&] { return ");

                                break;
                            }
                        }
                        
                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));
                        output.Append("; })");

                        break;
                    }

                    case BinaryOperator.NoneCoalescingAssign: {

                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));
                        output.Append(".lazyEmplace([&] { return ");
                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));
                        output.Append("; })");

                        break;
                    }

                    case BinaryOperator.ArithmeticRightShift: {

                        output.Append("NeuInternal::arithmeticShiftRight(");
                        output.Append(CodeGenExpr(indent, binOp.Lhs, project));
                        output.Append(", ");
                        output.Append(CodeGenExpr(indent, binOp.Rhs, project));
                        output.Append(')');

                        break;
                    }

                    case BinaryOperator.Add when exprIsInt:
                    case BinaryOperator.Subtract when exprIsInt:
                    case BinaryOperator.Multiply when exprIsInt:
                    case BinaryOperator.Divide when exprIsInt:
                    case BinaryOperator.Modulo when exprIsInt: {

                        output.Append(
                            CodeGenCheckedBinaryOp(
                                indent,
                                binOp.Lhs,
                                binOp.Rhs,
                                exprTy,
                                binOp.Operator,
                                project));

                        break;
                    }

                    case BinaryOperator.AddAssign when exprIsInt:
                    case BinaryOperator.SubtractAssign when exprIsInt:
                    case BinaryOperator.MultiplyAssign when exprIsInt:
                    case BinaryOperator.DivideAssign when exprIsInt:
                    case BinaryOperator.ModuloAssign when exprIsInt: {

                        output.Append(
                            CodeGenCheckedBinaryOpAssign(
                                indent,
                                binOp.Lhs,
                                binOp.Rhs,
                                exprTy,
                                binOp.Operator,
                                project));

                        break;
                    }

                    default: {

                        if (binOp.Operator == BinaryOperator.Assign) {

                            if (binOp.Lhs is CheckedIndexedDictionaryExpression id) {

                                output.Append(CodeGenExpr(0, id.Expression, project));
                                output.Append(".set(");
                                output.Append(CodeGenExpr(0, id.Index, project));
                                output.Append(", ");
                                output.Append(CodeGenExpr(0, binOp.Rhs, project));
                                output.Append("))");
                                return output.ToString();
                            }
                        }

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

                var valueTypeId = project.Types[ve.Type] switch {

                    GenericInstance gi => gi.TypeIds[0],
                    _ => throw new Exception("Internal error: Array doesn't have inner type")
                };

                if (ve.FillSize is CheckedExpression fillSize) {

                    output.Append("(TRY(Array<");
                    output.Append(CodeGenType(valueTypeId, project));
                    output.Append(">::filled(");
                    output.Append(CodeGenExpr(indent, fillSize, project));
                    output.Append(", ");
                    output.Append(CodeGenExpr(indent, ve.Expressions.First(), project));
                    output.Append(")))");
                }
                else {

                    // (Array({1, 2, 3}))

                    output.Append("(Array<");
                    output.Append(CodeGenType(valueTypeId, project));
                    output.Append(">({");

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

                var (keyTypeId, valueTypeId) = project.Types[de.TypeId] switch {

                    GenericInstance gi => (gi.TypeIds[0], gi.TypeIds[1]),
                    _ => throw new Exception("Internal error: Dictionary doesn't have inner type")
                };

                output.Append(
                    $"(TRY(Dictionary<{CodeGenType(keyTypeId, project)}, {CodeGenType(valueTypeId, project)}>::createWithEntries({{");

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

                output.Append("})))");

                break;
            }

            case CheckedSetExpression se: {

                // (Set({1, 2, 3}))

                var valueTypeId = project.Types[se.TypeId] switch {

                    GenericInstance gi => gi.TypeIds[0],
                    _ => throw new Exception("Internal error: Set doesn't have inner type")
                };

                output.Append($"(TRY(Set<{CodeGenType(valueTypeId, project)}>::createWithValues({{");

                var first = true;

                foreach (var value in se.Items) {

                    if (!first) {

                        output.Append(", ");
                    }
                    else {

                        first = false;
                    }

                    output.Append(CodeGenExpr(indent, value, project));
                }
                
                output.Append("})))");

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
                output.Append(")[");
                output.Append(CodeGenExpr(indent, ide.Index, project));
                output.Append("])");

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

                        switch (project.Types[x.GetTypeIdOrTypeVar()]) {

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

                throw new Exception("Garbage statement in codegen");

                // break;
            }

            default: {

                throw new Exception();
            }
        }

        return output.ToString();
    }

    public static String CodeGenNamespacePath(CheckedCall call, Project project) {

        var output = new StringBuilder();

        for (var idx = 0; idx < call.Namespace.Count; idx++) {

            var ns = call.Namespace[idx];

            // hack warning: this is to get around C++'s limitation that a constructor
            // can't be called like other static methods

            if (idx == call.Namespace.Count - 1 && ns.Name == call.Name) {
                
                break;
            }

            output.Append(ns.Name);

            if (ns.GenericParameters is List<Int32> _params && _params.Any()) {

                output.Append('<');

                for (var i = 0; i < _params.Count; i++) {

                    var param = _params[i];

                    output.Append(CodeGenType(param, project));

                    if (i != _params.Count - 1) {

                        output.Append(',');
                    }
                }

                output.Append('>');
            }

            output.Append("::");
        }

        return output.ToString();
    }

    public static String CodeGenIndent(
        int indent) {

        return new String(' ', indent);
    }

    public static void ExtractDependenciesFrom(
        Project project,
        Int32 typeId,
        HashSet<Int32> deps,
        Dictionary<Int32, List<Int32>> graph,
        bool topLevel) {

        if (graph.ContainsKey(typeId)) {

            foreach (var dep in graph[typeId]) {

                deps.Add(dep);
            }

            return;
        }

        var _type = project.Types[typeId];

        Int32? _structId = null;

        Int32? _enumId = null;

        switch (_type) {

            case GenericInstance gi: {

                _structId = gi.StructId;

                break;
            }

            case StructType st: {

                _structId = st.StructId;

                break;
            }

            case GenericEnumInstance gei: {

                _enumId = gei.EnumId;

                break;
            }

            case EnumType et: {

                _enumId = et.EnumId;

                break;
            }

            default: {

                break;
            }
        }

        switch (_type) {

            case GenericInstance _:
            case StructType _: {

                var structId = _structId ?? throw new Exception();

                var _struct = project.Structs[structId];

                if (_struct.DefinitionLinkage == DefinitionLinkage.External) {

                    // This type is defined somewhere else,
                    // so we can skip marking it as a dependency.
                    return;
                }

                if (_struct.DefinitionType == DefinitionType.Class && !topLevel) {

                    // We store and pass these as pointers, so we don't need to
                    // include them in the dependency graph.
                    return;
                }

                deps.Add(_struct.TypeId);

                // The struct's fields are also dependencies.

                foreach (var field in _struct.Fields) {

                    ExtractDependenciesFrom(project, field.TypeId, deps, graph, false);
                }

                break;
            }

            case GenericEnumInstance _:
            case EnumType _: {

                var enumId = _enumId ?? throw new Exception();

                var _enum = project.Enums[enumId];

                if (_enum.DefinitionLinkage == DefinitionLinkage.External) {
    
                    // This type is defined somewhere else,
                    // so we can skip marking it as a dependency.
                    return;
                }

                if (_enum.DefinitionType == DefinitionType.Class && !topLevel) {

                    // We store and pass these as pointers, so we don't need to
                    // include them in the dependency graph.
                    return;
                }

                deps.Add(_enum.TypeId);

                if (_enum.UnderlyingTypeId is Int32 _typeId) {

                    ExtractDependenciesFrom(project, _typeId, deps, graph, false);
                }

                // The enum variants' types are also dependencies

                foreach (var variant in _enum.Variants) {

                    switch (variant) {

                        case CheckedTypedEnumVariant t: {

                            ExtractDependenciesFrom(project, t.TypeId, deps, graph, false);

                            break;
                        }

                        case CheckedStructLikeEnumVariant s: {

                            foreach (var field in s.Decls) {

                                ExtractDependenciesFrom(project, field.TypeId, deps, graph, false);
                            }

                            break;
                        }

                        default: {

                            break;
                        }
                    }
                }

                break;
            }

            case Builtin _: {

                break;
            }

            case TypeVariable _: {

                break;
            }

            case RawPointerType _: {

                break;
            }

            default: {

                throw new Exception();
            }
        }

    }

    public static Dictionary<Int32, List<Int32>> ProduceTypeDependencyGraph(
        Project project,
        Scope scope) {

        var graph = new Dictionary<Int32, List<Int32>>();

        foreach (var (_, typeId, _) in scope.Types) {

            var deps = new HashSet<Int32>();

            ExtractDependenciesFrom(project, typeId, deps, graph, true);

            graph[typeId] = deps.ToList();

        }

        return graph;
    }

    public static void PostOrderTraversal(
        Project project,
        Int32 typeId,
        HashSet<Int32> visited,
        Dictionary<Int32, List<Int32>> graph,
        List<Int32> output) {

        if (visited.Contains(typeId)) {

            return;
        }

        visited.Add(typeId);

        if (graph.ContainsKey(typeId)) {

            foreach (var dep in graph[typeId]) {

                PostOrderTraversal(project, dep, visited, graph, output);
            }
        }

        output.Add(typeId);
    }
}
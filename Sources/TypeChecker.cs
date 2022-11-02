
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

        return (new [] { Compiler.CCharTypeId, Compiler.CIntTypeId, Compiler.Int8TypeId, Compiler.Int16TypeId, Compiler.Int32TypeId, Compiler.Int64TypeId, Compiler.IntTypeId, Compiler.UInt8TypeId, Compiler.UInt16TypeId, Compiler.UInt32TypeId, Compiler.UInt64TypeId, Compiler.UIntTypeId }).Contains(typeId);
    }

    public static bool IsFloating(
        Int32 typeId) {

        switch (typeId) {

            case Compiler.FloatTypeId:
            case Compiler.DoubleTypeId:
                return true;

            default:
                return false;
        }
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

    public static Int32? FlipSignedness(
        Int32 typeId) {

        switch (typeId) {

            case Compiler.Int8TypeId:       return Compiler.UInt8TypeId;
            case Compiler.Int16TypeId:      return Compiler.UInt16TypeId;
            case Compiler.Int32TypeId:      return Compiler.UInt32TypeId;
            case Compiler.Int64TypeId:      return Compiler.UInt64TypeId;
            case Compiler.UInt8TypeId:      return Compiler.Int8TypeId;
            case Compiler.UInt16TypeId:     return Compiler.Int16TypeId;
            case Compiler.UInt32TypeId:     return Compiler.Int32TypeId;
            case Compiler.UInt64TypeId:     return Compiler.Int64TypeId;
            case Compiler.IntTypeId:        return Compiler.UIntTypeId;
            case Compiler.UIntTypeId:       return Compiler.IntTypeId;

            default:                        return null;
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

    public static bool CanFitNumber(
        Int32 typeId,
        NumberConstant value) {

        switch (value) {

            case SignedNumberConstant si: {

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

            case UnsignedNumberConstant ui: {

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

            case FloatingNumberConstant fc: {

                switch (typeId) {

                    case Compiler.FloatTypeId: return fc.Value >= ToDouble(float.MinValue) && fc.Value <= ToDouble(float.MaxValue);
                    case Compiler.DoubleTypeId: return true;
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

    public Int32? CurrentStructTypeId { get; set; }

    public bool InsideDefer { get; set; }


    public Int32? CachedArrayStructId { get; set; }
    
    public Int32? CachedDictionaryStructId { get; set; }
    
    public Int32? CachedErrorStructId { get; set; }
    
    public Int32? CachedOptionalStructId { get; set; }
    
    public Int32? CachedRangeStructId { get; set; }
    
    public Int32? CachedSetStructId { get; set; }
    
    public Int32? CachedTupleStructId { get; set; }
    
    public Int32? CachedWeakPointerStructId { get; set; }

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
        this.CurrentStructTypeId = null;
        this.InsideDefer = false;

        this.CachedArrayStructId = null;
        this.CachedDictionaryStructId = null;
        this.CachedErrorStructId = null;
        this.CachedOptionalStructId = null;
        this.CachedRangeStructId = null;
        this.CachedSetStructId = null;
        this.CachedTupleStructId = null;
        this.CachedWeakPointerStructId = null;
    }
}

public static partial class ProjectFunctions {

    public static Int32 FindOrAddTypeId(
        this Project project,
        NeuType type) {

        for (var idx = 0; idx < project.Types.Count; idx++) {

            var t = project.Types[idx];

            if (NeuTypeFunctions.Eq(t, type)) {

                return idx;
            }
        }

        // in the future, we may want to group related types (like instantiations of the same generic)

        project.Types.Add(type);

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
                    new TypecheckErrorWithHint(
                        $"redefinition of variable {var.Name}",
                        span,
                        $"variable {var.Name} was first defined here",
                        existingVar.DefinitionSpan));
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

        foreach (var (existingStruct, _, defSpan) in scope.Structs) {

            if (name == existingStruct) {

                return new ErrorOrVoid(
                    new TypecheckErrorWithHint(
                        $"redefinition of struct/class {name}",
                        span,
                        $"struct/class {name} was first defined here",
                        defSpan));
            }
        }

        scope.Structs.Add((name, structId, span));

        return new ErrorOrVoid();
    }

    public static ErrorOrVoid AddEnumToScope(
        this Project project,
        Int32 scopeId,
        String name,
        Int32 enumId,
        Span span) {

        var scope = project.Scopes[scopeId];

        foreach (var (existingEnum, _, defSpan) in scope.Enums) {

            if (name == existingEnum) {

                return new ErrorOrVoid(
                    new TypecheckErrorWithHint(
                        $"redefinition of enum {name}",
                        span,
                        $"enum {name} was first defined here",
                        defSpan));
            }
        }

        scope.Enums.Add((name, enumId, span));

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

    // Find the namespace in the current scope, or one of its parents

    public static Int32? FindNamespaceInScope(
        this Project project,
        Int32 scopeId,
        String namespaceName) {

        Int32? currentId = scopeId;

        while (currentId is not null) {

            var scope = project.Scopes[currentId.Value];

            foreach (var childScopeId in scope.Children) {

                var childScope = project.Scopes[childScopeId];

                if (childScope.NamespaceName is String name) {

                    if (name == namespaceName) {

                        return childScopeId;
                    }
                }
            }

            currentId = scope.Parent;
        }

        return null;
    }

    // Find namespace in the current scope, but not any of its parents (strictly in the current scope)

    public static Int32? FindNamespaceInScopeStrict(
        this Project project,
        Int32 scopeId,
        String namespaceName) {

        var scope = project.Scopes[scopeId];

        foreach (var childScopeId in scope.Children) {

            var childScope = project.Scopes[childScopeId];

            if (childScope.NamespaceName is String name) {

                if (name == namespaceName) {

                    return childScopeId;
                }
            }
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

        foreach (var (existingFunc, _, defSpan) in scope.Funcs) {

            if (name == existingFunc) {

                return new ErrorOrVoid(
                    new TypecheckErrorWithHint(
                        $"redefinition of function {name}",
                        span,
                        $"function {name} was first defined here",
                        defSpan));
            }
        }

        scope.Funcs.Add((name, funcId, span));

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

        foreach (var (existingType, _, defSpan) in scope.Types) {

            if (typeName == existingType) {

                return new ErrorOrVoid(
                    new TypecheckErrorWithHint(
                        $"redefinition of type {typeName}",
                        span,
                        $"type {typeName} was first defined here",
                        defSpan));
            }
        }

        scope.Types.Add((typeName, typeId, span));

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

    public static String TypeNameForTypeId(
        this Project project,
        Int32 typeId) {

        // NOTE: Can't use get_*_struct_id here since it needs a Span.
        
        var optionalStructId = project.CachedOptionalStructId ?? throw new Exception();
        
        var weakPointerStructId = project.CachedWeakPointerStructId ?? throw new Exception();

        switch (project.Types[typeId]) {

            case Builtin b: {

                switch (typeId) {

                    case Compiler.VoidTypeId:
                        return "Void";

                    case Compiler.Int8TypeId:
                        return "Int8";
                    
                    case Compiler.Int16TypeId:
                        return "Int16";
                    
                    case Compiler.Int32TypeId:
                        return "Int32";
                    
                    case Compiler.Int64TypeId:
                        return "Int64";

                    case Compiler.UInt8TypeId:
                        return "UInt8";
                    
                    case Compiler.UInt16TypeId:
                        return "UInt16";

                    case Compiler.UInt32TypeId:
                        return "UInt32";

                    case Compiler.UInt64TypeId:
                        return "UInt64";

                    case Compiler.FloatTypeId:
                        return "Float";

                    case Compiler.DoubleTypeId:
                        return "Double";

                    case Compiler.IntTypeId:
                        return "Int";

                    case Compiler.UIntTypeId:
                        return "UInt";

                    case Compiler.CCharTypeId:
                        return "CChar";

                    case Compiler.CIntTypeId:
                        return "CInt";

                    case Compiler.StringTypeId:
                        return "String";

                    case Compiler.BoolTypeId:
                        return "Bool";

                    default:
                        return "Unknown";
                }
            }

            case EnumType e: {

                return $"enum {project.Enums[e.EnumId].Name}";
            }

            case StructType s: {

                if (project.Structs[s.StructId].DefinitionType == DefinitionType.Class) {

                    return $"class {project.Structs[s.StructId].Name}";
                }
                else {

                    return $"struct {project.Structs[s.StructId].Name}";
                }
            }

            case GenericEnumInstance gei: {

                var output = new StringBuilder($"enum {project.Enums[gei.EnumId].Name}");

                output.Append('<');
                
                var first = true;

                foreach (var arg in gei.TypeIds) {

                    if (!first) {
                        
                        output.Append(", ");
                    } 
                    else {
                        first = false;
                    }
                    
                    output.Append(project.TypeNameForTypeId(arg));
                }

                output.Append('>');

                return output.ToString();
            }

            case GenericInstance gi when gi.StructId == optionalStructId: {

                return $"{project.TypeNameForTypeId(gi.TypeIds[0])}?";
            }

            case GenericInstance gi when gi.StructId == weakPointerStructId: {

                return $"weak {project.TypeNameForTypeId(gi.TypeIds[0])}?";
            }

            case GenericInstance gi: {

                var output = new StringBuilder();

                if (project.Structs[gi.StructId].DefinitionType == DefinitionType.Class) {

                    output.Append($"class {project.Structs[gi.StructId].Name}");
                } 
                else {
                    
                    output.Append($"struct {project.Structs[gi.StructId].Name}");
                }

                output.Append('<');
                
                var first = true;
                
                foreach (var arg in gi.TypeIds) {

                    if (!first) {
                        
                        output.Append(", ");
                    } 
                    else {
                        
                        first = false;
                    }

                    output.Append(project.TypeNameForTypeId(arg));
                }

                output.Append('>');

                return output.ToString();
            }

            case TypeVariable v: {

                return v.Name;
            }

            case RawPointerType p: {

                return $"raw {project.TypeNameForTypeId(p.TypeId)}";
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static Int32 GetCachedStructId(
        this Project project,
        Int32? cachedStructId,
        String name,
        Span span) {

        // FIXME: Currently not being able to get an internally used struct ID
        //        causes a panic. In the future it would be nice to report it
        //        as an internal compiler error instead.

        switch (cachedStructId) {

            case Int32 id:
                return id;

            default:
                throw new Exception($"can't find builtin {name} type");
        }
    }

    public static Int32 GetArrayStructId(
        this Project project,
        Span span) {

        return project.GetCachedStructId(project.CachedArrayStructId, "Array", span);
    }

    public static Int32 GetDictionaryStrutId(
        this Project project,
        Span span) {

        return project.GetCachedStructId(project.CachedDictionaryStructId, "Dictionary", span);
    }

    public static Int32 GetErrorStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedErrorStructId, "Error", span);
    }

    public static Int32 GetOptionalStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedOptionalStructId, "Optional", span);
    }

    public static Int32 GetRangeStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedRangeStructId, "Range", span);
    }

    public static Int32 GetSetStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedSetStructId, "Set", span);
    }

    public static Int32 GetTupleStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedTupleStructId, "Tuple", span);
    }

    public static Int32 GetWeakPointerStructId(
        this Project project,
        Span span) {
        
        return project.GetCachedStructId(project.CachedWeakPointerStructId, "WeakPtr", span);
    }
}

///

public partial class CheckedStruct {

    public String Name { get; init; }

    public List<Int32> GenericParameters { get; set; }

    public List<CheckedVarDecl> Fields { get; set; }

    public Int32 ScopeId { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; }

    public Int32 TypeId { get; init; }

    ///

    public CheckedStruct(
        String name,
        List<Int32> genericParameters,
        List<CheckedVarDecl> fields,
        Int32 scopeId,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType,
        Int32 typeId) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Fields = fields;
        this.ScopeId = scopeId;
        this.DefinitionLinkage = definitionLinkage;
        this.DefinitionType = definitionType;
        this.TypeId = typeId;
    }
}

///

public partial class CheckedEnum {

    public String Name { get; init; }

    public List<Int32> GenericParameters { get; init; }

    public List<CheckedEnumVariant> Variants { get; set; }

    public Int32 ScopeId { get; init; }

    public DefinitionLinkage DefinitionLinkage { get; init; }

    public DefinitionType DefinitionType { get; init; }

    public Int32? UnderlyingTypeId { get; init; }

    public Span Span { get; init; }

    public Int32 TypeId { get; init; }

    ///

    public CheckedEnum(
        String name,
        List<Int32> genericParameters,
        List<CheckedEnumVariant> variants,
        Int32 scopeId,
        DefinitionLinkage definitionLinkage,
        DefinitionType definitionType,
        Int32? underlyingTypeId,
        Span span,
        Int32 typeId) {

        this.Name = name;
        this.GenericParameters = genericParameters;
        this.Variants = variants;
        this.ScopeId = scopeId;
        this.DefinitionLinkage = definitionLinkage;
        this.DefinitionType = definitionType;
        this.UnderlyingTypeId = underlyingTypeId;
        this.Span = span;
        this.TypeId = typeId;
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

public partial class CheckedNamespace {

    public String? Name { get; init; }

    public Int32 ScopeId { get; init; }

    ///

    public CheckedNamespace(
        String? name,
        Int32 scopeId) {

        this.Name = name;
        this.ScopeId = scopeId;
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

    public Visibility Visibility { get; init; }

    public bool Throws { get; init; }
    
    public Int32 ReturnTypeId { get; set; }
    
    public List<CheckedParameter> Parameters { get; init; }

    public List<FunctionGenericParameter> GenericParameters { get; set; }

    public Int32 FuncScopeId { get; set; }
    
    // public CheckedBlock Block { get; set; }
    public CheckedBlock? Block { get; set; }
    
    public ParsedFunction? Function { get; set; }

    public FunctionLinkage Linkage { get; set; }

    public bool IsInstantiated { get; set; }

    ///

    public CheckedFunction(
        String name,
        Visibility visibility,
        bool throws,
        Int32 returnTypeId,
        List<CheckedParameter> parameters,
        List<FunctionGenericParameter> genericParameters,
        Int32 funcScopeId,
        // CheckedBlock block,
        CheckedBlock? block,
        ParsedFunction? func,
        FunctionLinkage linkage,
        bool isInstantiated) {

        this.Name = name;
        this.Visibility = visibility;
        this.Throws = throws;
        this.ReturnTypeId = returnTypeId;
        this.Parameters = parameters;
        this.GenericParameters = genericParameters;
        this.FuncScopeId = funcScopeId;
        this.Block = block;
        this.Function = func;
        this.Linkage = linkage;
        this.IsInstantiated = isInstantiated;
    }
}

public static partial class CheckedFunctionFunctions {

    public static ParsedFunction GetParsedFunction(
        this CheckedFunction func) {

        return func.Function
            ?? throw new Exception();
    }

    public static bool IsStatic(
        this CheckedFunction func) {

        if (func.Parameters.FirstOrDefault() is CheckedParameter p) {

            return p.Variable.Name != "this";
        }

        return true;
    }

    public static bool IsMutating(
        this CheckedFunction func) {

        if (!func.IsStatic()) {

            return func.Parameters.First().Variable.Mutable;
        }

        return false;
    }
}

///

public partial class CheckedBlock {

    public List<CheckedStatement> Stmts { get; init; }

    public bool DefinitelyReturns { get; set; }

    ///

    public CheckedBlock() 
        : this(new List<CheckedStatement>(), false) { }

    public CheckedBlock(
        List<CheckedStatement> stmts,
        bool definitelyReturns) { 

        this.Stmts = stmts;
        this.DefinitelyReturns = definitelyReturns;
    }
}

///

public partial class CheckedVarDecl { 

    public String Name { get; init; }

    public Int32 TypeId { get; init; }

    public bool Mutable { get; init; }

    public Span Span { get; init; }

    public Visibility Visibility { get; init; }

    ///

    public CheckedVarDecl(
        String name,
        Int32 typeId,
        bool mutable,
        Span span,
        Visibility visibility) {

        this.Name = name;
        this.TypeId = typeId;
        this.Mutable = mutable;
        this.Span = span;
        this.Visibility = visibility;
    }
}

///

public partial class CheckedVariable { 

    public String Name { get; init; }

    public Int32 TypeId { get; init; }

    public bool Mutable { get; init; }

    public Visibility Visibility { get; init; }

    public Span DefinitionSpan { get; init; }

    ///

    public CheckedVariable(
        String name,
        Int32 typeId,
        bool mutable,
        Visibility visibility,
        Span definitionSpan) {

        this.Name = name;
        this.TypeId = typeId;
        this.Mutable = mutable;
        this.Visibility = visibility;
        this.DefinitionSpan = definitionSpan;
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

public partial class NumberConstant {

    public NumberConstant() { }

    public override string ToString() {
        
        switch (this) {

            case SignedNumberConstant si:
                return $"Signed({si.Value})";

            case UnsignedNumberConstant ui:
                return $"Unsigned({ui.Value})";

            default:
                throw new Exception();
        }
    }
}

    public partial class SignedNumberConstant: NumberConstant {

        public Int64 Value { get; init; }

        ///

        public SignedNumberConstant(
            Int64 value) {

            this.Value = value;
        }
    }

    public partial class UnsignedNumberConstant: NumberConstant {

        public UInt64 Value { get; init; }

        ///

        public UnsignedNumberConstant(
            UInt64 value) {

            this.Value = value;
        }
    }

    public partial class FloatingNumberConstant: NumberConstant {

        public double Value { get; init; }

        public FloatingNumberConstant(
            double value) {

            this.Value = value;
        }
    }

public static partial class NumberConstantFunctions {

    public static Int64 ToInt64(
        this NumberConstant i) {

        switch (i) {

            case SignedNumberConstant s: {

                return s.Value;
            }

            case UnsignedNumberConstant u: {

                return System.Convert.ToInt64(u.Value);
            }

            default: {

                throw new Exception();
            }
        }
    }

    public static (NumericConstant?, Int32) Promote(
        this NumberConstant i,
        Int32 typeId) {

        if (!NeuTypeFunctions.CanFitNumber(typeId, i)) {

            return (null, Compiler.UnknownTypeId);
        }

        var bits = NeuTypeFunctions.GetBits(typeId);

        var signed = NeuTypeFunctions.IsSigned(typeId);

        NumericConstant newConstant = i switch {

            SignedNumberConstant si => (bits, signed) switch {

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

            UnsignedNumberConstant ui => (bits, signed) switch {

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

            FloatingNumberConstant f => (bits, signed) switch {

                (32, true) => new FloatConstant((float) f.Value),
                (64, true) => new DoubleConstant((double) f.Value),
                _ => throw new Exception("Floating numeric constants can only be 32, or 64 bits long")
            },

            _ => throw new Exception()
        };

        return (newConstant, typeId);
    }

    public static BigInteger ToBigInteger(
        this NumberConstant integer) {

        switch (integer) {

            case SignedNumberConstant si: 
                return new BigInteger(si.Value);

            case UnsignedNumberConstant ui:
                return new BigInteger(ui.Value);

            default:
                throw new Exception("Not supported for floating numbers");
        }
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

    public partial class FloatConstant: NumericConstant {

        public float Value { get; init; }

        ///

        public FloatConstant(
            float value) {

            this.Value = value;
        }
    }

    public partial class DoubleConstant: NumericConstant {

        public double Value { get; init; }

        ///

        public DoubleConstant(
            double value) {

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
            case var _ when l is FloatConstant lf && r is FloatConstant rf:             return lf.Value == rf.Value;
            case var _ when l is DoubleConstant ld && r is DoubleConstant rd:           return ld.Value == rd.Value;
            default:                                                                    return false;
        }
    }

    public static NumberConstant? NumberConstant(
        this NumericConstant n) {

        switch (n) {
            case Int8Constant i8: return new SignedNumberConstant(ToInt64(i8.Value));
            case Int16Constant i16: return new SignedNumberConstant(ToInt64(i16.Value));
            case Int32Constant i32: return new SignedNumberConstant(ToInt64(i32.Value));
            case Int64Constant i64: return new SignedNumberConstant(i64.Value);
            case IntConstant i: return new SignedNumberConstant(i.Value);
            case UInt8Constant u8: return new UnsignedNumberConstant(ToUInt64(u8.Value));
            case UInt16Constant u16: return new UnsignedNumberConstant(ToUInt64(u16.Value));
            case UInt32Constant u32: return new UnsignedNumberConstant(ToUInt64(u32.Value));
            case UInt64Constant u64: return new UnsignedNumberConstant(u64.Value);
            case UIntConstant u: return new UnsignedNumberConstant(u.Value);
            case FloatConstant f: return new FloatingNumberConstant((double) f.Value);
            case DoubleConstant d: return new FloatingNumberConstant(d.Value);
            default: throw new Exception();
        }
    }

    public static Int32 GetTypeId(
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
            case FloatConstant f: return Compiler.FloatTypeId;
            case DoubleConstant d: return Compiler.DoubleTypeId;
            default: throw new Exception();
        }
    }
}

///

public static partial class TypeCheckerFunctions {

    public static Int32 ResolveTypeVar(
        Int32 typeVar,
        Int32 scopeId,
        Project project) {

        var typeId = typeVar;

        while (true) {

            if (project.Types[typeId] is TypeVariable tv) {

                switch (project.FindTypeInScope(scopeId, tv.Name)) {

                    case Int32 id: {

                        if (typeId == id) {

                            return typeId;
                        }

                        typeId = id;

                        break;
                    }

                    default: {

                        return typeId;
                    }
                }
            }
            else {

                return typeId;
            }
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

public static partial class CheckedTypeCastFunctions {

    public static bool Eq(CheckedTypeCast l, CheckedTypeCast r) {

        switch (true) {

            case var _ when l is CheckedFallibleTypeCast lc && r is CheckedFallibleTypeCast rc:
                return lc.TypeId == rc.TypeId;

            case var _ when l is CheckedInfallibleTypeCast lc && r is CheckedInfallibleTypeCast rc:
                return lc.TypeId == rc.TypeId;

            default:
                return false;
        }
    }

    public static Int32 GetTypeIdOrTypeVar(
        this CheckedTypeCast t) {

        switch (t) {

            case CheckedFallibleTypeCast f:
                return f.TypeId;

            case CheckedInfallibleTypeCast i:
                return i.TypeId;

            default: 
                throw new Exception();
        }
    }

    public static Int32 GetTypeId(
        this CheckedTypeCast t,
        Int32 scopeId,
        Project project) {

        return TypeCheckerFunctions.ResolveTypeVar(t.GetTypeIdOrTypeVar(), scopeId, project);
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

    public partial class CheckedExpressionWhenCase: CheckedWhenCase {

        public CheckedExpression Expression { get; init; }

        public CheckedWhenBody Body { get; init; }

        ///

        public CheckedExpressionWhenCase(
            CheckedExpression expression,
            CheckedWhenBody body) {

            this.Expression = expression;
            this.Body = body;
        }
    }

    public partial class CheckedCatchAllWhenCase: CheckedWhenCase {

        public CheckedWhenBody Body { get; init; }
        
        ///

        public CheckedCatchAllWhenCase(
            CheckedWhenBody body) {

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

    public partial class CheckedByteConstantExpression: CheckedExpression {

        public String Value { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedByteConstantExpression(
            String value,
            Span span) {

            this.Value = value;
            this.Span = span;
        }
    }

    public partial class CheckedCharacterConstantExpression: CheckedExpression {

        public String Value { get; init; }

        public Span Span { get; init; }

        ///

        public CheckedCharacterConstantExpression(
            String value,
            Span span) {

            this.Value = value;
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

        public bool ValuesAreAllConstant { get; init; }

        ///

        public CheckedWhenExpression(
            CheckedExpression expression,
            List<CheckedWhenCase> cases,
            Span span,
            Int32 typeId,
            bool valuesAreAllConstant) {

            this.Expression = expression;
            this.Cases = cases;
            this.Span = span;
            this.TypeId = typeId;
            this.ValuesAreAllConstant = valuesAreAllConstant;
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

    public partial class CheckedNamespacedVarExpression: CheckedExpression {

        public List<CheckedNamespace> Namespace { get; init; }

        public CheckedVariable Variable { get; init; }

        public Span Span { get; init; }

        ///
        
        public CheckedNamespacedVarExpression(
            List<CheckedNamespace> ns,
            CheckedVariable variable,
            Span span) {

            this.Namespace = ns;
            this.Variable = variable;
            this.Span = span;
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

    public static Int32 GetTypeIdOrTypeVar(
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

            case CheckedByteConstantExpression _: {

                return Compiler.UInt8TypeId;
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

                return ve.Variable.TypeId;
            }

            case CheckedNamespacedVarExpression ne: {

                return ne.Variable.TypeId;
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

    public static Int32 GetTypeId(
        this CheckedExpression expr,
        Int32 scopeId,
        Project project) {

        return TypeCheckerFunctions.ResolveTypeVar(expr.GetTypeIdOrTypeVar(), scopeId, project);
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

            case CheckedByteConstantExpression b:
                return b.Span;

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

            case CheckedNamespacedVarExpression n:
                return n.Span;

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

    public static NumberConstant? ToNumberConstant(
        this CheckedExpression e) {

        switch (e) {

            case CheckedNumericConstantExpression ne: return ne.Value.NumberConstant();

            case CheckedUnaryOpExpression ue when 
                ue.Operator is CheckedTypeCastUnaryOperator tc
                && tc.TypeCast is CheckedInfallibleTypeCast: {

                if (!NeuTypeFunctions.IsInteger(ue.Type) && !NeuTypeFunctions.IsFloating(ue.Type)) {

                    return null;
                }

                switch (ue.Expression) {

                    case CheckedNumericConstantExpression c: {

                        return c.Value.NumberConstant();
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
    
    public Int32 TypeId { get; init; }

    ///

    public CheckedCall(
        List<ResolvedNamespace> ns,
        String name,
        bool calleeThrows,
        List<(String, CheckedExpression)> args,
        List<Int32> typeArgs,
        FunctionLinkage linkage,
        Int32 typeId) {

        this.Namespace = ns;
        this.Name = name;
        this.CalleeThrows = calleeThrows;
        this.Args = args;
        this.TypeArgs = typeArgs;
        this.Linkage = linkage;
        this.TypeId = typeId;
    }
}

///

public partial class Scope {

    public String? NamespaceName { get; set; }
    
    public List<CheckedVariable> Vars { get; init; }

    public List<(String, Int32, Span)> Structs { get; init; }

    public List<(String, Int32, Span)> Funcs { get; init; }

    public List<(String, Int32, Span)> Enums { get; init; }

    public List<(String, Int32, Span)> Types { get; init; }

    public Int32? Parent { get; init; }

    // Namespaces may also have children that are also namespaces

    public List<Int32> Children { get; init; }

    ///

    public Scope(
        Int32? parent) 
        : this(
            namespaceName: null,
            new List<CheckedVariable>(),
            new List<(String, Int32, Span)>(),
            new List<(String, Int32, Span)>(),
            new List<(String, Int32, Span)>(),
            new List<(String, Int32, Span)>(),
            parent,
            children: new List<Int32>()) { }

    public Scope(
        String? namespaceName,
        List<CheckedVariable> vars,
        List<(String, Int32, Span)> structs,
        List<(String, Int32, Span)> funcs,
        List<(String, Int32, Span)> enums,
        List<(String, Int32, Span)> types,
        Int32? parent,
        List<Int32> children) {

        this.NamespaceName = namespaceName;
        this.Vars = vars;
        this.Structs = structs;
        this.Funcs = funcs;
        this.Enums = enums;
        this.Parent = parent;
        this.Types = types;
        this.Children = children;
    }

    public static bool CanAccess(
        Int32 ownScopeId,
        Int32 otherScopeId,
        Project project) {

        // We can access another scope if we're either the same scope, or we are a direct children scope of the other scope

        if (ownScopeId == otherScopeId) {

            return true;
        }
        else {

            var ownScope = project.Scopes[ownScopeId];

            while (ownScope.Parent is Int32 parent) {

                if (parent == otherScopeId) {

                    return true;
                } 

                ownScope = project.Scopes[parent];
            }

            return false;
        }
    }
}

///

public interface INamespaceMember {

    Visibility GetVisibility();
    String GetName();
    String GetKind();
}

public partial class CheckedVarDecl: INamespaceMember {

    public Visibility GetVisibility() {

        return this.Visibility;
    }

    public String GetName() {

        return this.Name;
    }

    public String GetKind() {

        return "variable";
    }
}

public partial class CheckedVariable: INamespaceMember {

    public Visibility GetVisibility() {

        return this.Visibility;
    }

    public String GetName() {

        return this.Name;
    }

    public String GetKind() {

        return "variable";
    }
}


public partial class CheckedFunction: INamespaceMember {

    public Visibility GetVisibility() {

        return this.Visibility;
    }

    public String GetName() {

        return this.Name;
    }

    public String GetKind() {

        return "function";
    }
}

///

public static partial class TypeCheckerFunctions {

    public static Error? CheckAccessibility(
        Int32 ownScopeId,
        Int32 memberScopeId,
        INamespaceMember member,
        Span span,
        Project project) {

        switch (member.GetVisibility()) {

            case PrivateVisibility _ when !Scope.CanAccess(ownScopeId, memberScopeId, project): {

                return new TypeCheckError(
                    $"Can't access {member.GetKind()} '{member.GetName()}' from scope {project.Scopes[ownScopeId].NamespaceName ?? "none"}, because it is marked private",
                    span);
            }

            case RestrictedVisibility rv: {

                switch (project.CurrentStructTypeId) {

                    case Int32 ownTypeId: {

                        // Only structs/classes can be listed in `restricted()`.

                        if (project.Types[ownTypeId] is StructType st) {

                            foreach (var whitelistedType in rv.Types) {

                                var (typeId, err) = TypeCheckTypeName(whitelistedType, memberScopeId, project);

                                if (err is not null) {

                                    return err;
                                }

                                if (typeId == ownTypeId) {

                                    return null;
                                }
                            }

                            return new TypecheckErrorWithHint(
                                $"Can't access {member.GetKind()} '{member.GetName()}' from '{project.Structs[st.StructId].Name}', because '{project.Structs[st.StructId].Name}' is not in the restricted whitelist",
                                span,
                                "Whitelist declared here",
                                rv.Span);
                        }
                        else {

                            return new TypecheckErrorWithHint(
                                $"Can't access {member.GetKind()} '{member.GetName()}' from scope '{project.Scopes[ownScopeId].NamespaceName}', because it is not in the restricted whitelist",
                                span,
                                "Whitelist declared here",
                                rv.Span);
                        }
                    }

                    default: {

                        return new TypecheckErrorWithHint(
                            $"Can't access {member.GetKind()} '{member.GetName()}' from scope '{project.Scopes[ownScopeId].NamespaceName}', because it is marked restricted",
                            span,
                            "Whitelist declared here",
                            rv.Span);
                    }
                }
            }            

            default: {

                return null;
            }
        }
    }

    public static Error? TypeCheckNamespace(
        ParsedNamespace parsedNamespace,
        Int32 scopeId,
        Project project) {

        Error? error = null;

        var projectStructLength = project.Structs.Count;

        var projectEnumLength = project.Enums.Count;

        foreach (var ns in parsedNamespace.Namespaces) {

            // Do full typechecks of all the namespaces that are children of this namespace

            var namespaceScopeId = project.CreateScope(scopeId);

            project.Scopes[namespaceScopeId].NamespaceName = ns.Name;

            project.Scopes[scopeId].Children.Add(namespaceScopeId);
        
            TypeCheckNamespace(ns, namespaceScopeId, project);
        }

        for (Int32 _structId = 0; _structId < parsedNamespace.Structs.Count; _structId++) {
            
            // Bring the struct names into scope for future typechecking

            var structure = parsedNamespace.Structs.ElementAt(_structId);

            var structId = _structId + projectStructLength;

            project.Types.Add(new StructType(structId));

            var structTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(
                scopeId, 
                structure.Name, 
                structTypeId, 
                structure.Span).Error is Error e1) {

                error = error ?? e1;
            }

        }

        for (Int32 _enumId = 0; _enumId < parsedNamespace.Enums.Count; _enumId++) {

            // Bring the enum names into scope for future typechecking

            var _enum = parsedNamespace.Enums[_enumId];

            var enumId = _enumId + projectEnumLength;

            project.Types.Add(new EnumType(enumId));

            var enumTypeId = project.Types.Count - 1;

            if (project.AddTypeToScope(scopeId, _enum.Name, enumTypeId, _enum.Span).Error is Error e) {

                error = error ?? e;
            }
        }

        for (Int32 _structId = 0; _structId < parsedNamespace.Structs.Count; _structId++) {

            // Typecheck the protype of the struct

            var structure = parsedNamespace.Structs.ElementAt(_structId);

            var structId = _structId + projectStructLength;

            if (TypeCheckStructPredecl(structure, structId, scopeId, project) is Error err) {

                error = error ?? err;
            }
        }

        for (Int32 _enumId = 0; _enumId < parsedNamespace.Enums.Count; _enumId++) {

            // Typecheck the protype of the enum

            var _enum = parsedNamespace.Enums[_enumId];

            var enumId = _enumId + projectEnumLength;

            if (TypeCheckEnumPredecl(_enum, enumId, scopeId, project) is Error e2) {

                error = error ?? e2;
            }
        }

        for (Int32 enumId = 0; enumId < parsedNamespace.Enums.Count; enumId++) {

            // Finish typechecking the full enum

            var _enum = parsedNamespace.Enums[enumId];

            var err = TypeCheckEnum(
                _enum,
                enumId + projectEnumLength,
                project.FindTypeInScope(scopeId, _enum.Name) ?? throw new Exception(),
                project.Enums[enumId + projectEnumLength].ScopeId,
                scopeId,
                project);
            
            error = error ?? err;
        }

        var projectFunctionLength = project.Functions.Count;

        foreach (var fun in parsedNamespace.Functions) {
            
            // Ensure we know the function prototypes ahead of time, so that
            // and calls can find and resolve to them
            
            var chkFuncPredeclErr = TypeCheckFuncPredecl(
                fun, scopeId, null, project);

            error = error ?? chkFuncPredeclErr;
        }

        for (Int32 structId = 0; structId < parsedNamespace.Structs.Count; structId++) {

            // Finish typechecking the full struct (including methods)

            var structure = parsedNamespace.Structs.ElementAt(structId);

            var chkStructErr = TypeCheckStruct(
                structure, 
                structId + projectStructLength,
                scopeId, 
                project);

            error = error ?? chkStructErr;
        }

        for (Int32 i = 0; i < parsedNamespace.Functions.Count; i++) {

            var func = parsedNamespace.Functions[i];

            // Typecheck the function bodies

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

        Int32? underlyingTypeId;

        if (typeId == Compiler.UnknownTypeId) {

            underlyingTypeId = null;
        }
        else {

            underlyingTypeId = typeId;
        }

        project.Enums.Add(
            new CheckedEnum(
                name: _enum.Name,
                genericParameters,
                variants: new List<CheckedEnumVariant>(),
                scopeId: enumScopeId,
                definitionLinkage: _enum.DefinitionLinkage,
                definitionType: 
                    _enum.IsRecursive 
                        ? DefinitionType.Class 
                        : DefinitionType.Struct,
                underlyingTypeId,
                span: _enum.Span,
                typeId: project
                    .FindTypeInScope(parentScopeId, _enum.Name)
                    ?? throw new Exception("Enum must exist before predeclaration")));
                
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

            var expr = new ParsedUnaryOpExpression(
                x,
                new TypeCastUnaryOperator(new InfallibleTypeCast(_enum.UnderlyingType)),
                span);

            return TypeCheckExpression(expr, enumScopeId, project, SafetyMode.Safe, null);
        };

        var underlyingTypeId = project.Enums[enumId].UnderlyingTypeId;

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

                        if (underlyingTypeId is not null) {

                            if (nextConstantValue == null) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Missing enum variant value, the enum underlying type is not numeric, and so all enum variants must have explicit values",
                                        u.Span);
                            }
                            else {

                                var (checkedExpr, typeErr) = castToUnderlying(
                                    new ParsedNumericConstantExpression(
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

                                // This has a value, so generate a "variable" for it

                                var varErr = project.AddVarToScope(
                                    enumScopeId,
                                    new CheckedVariable(
                                        name: u.Name,
                                        typeId: enumTypeId,
                                        mutable: false,
                                        visibility: new PublicVisibility(),
                                        u.Span),
                                    u.Span);

                                error = error ?? varErr.Error;
                            }
                        }
                        else {

                            variants.Add(new CheckedUntypedEnumVariant(u.Name, u.Span));
                        }

                        if (project.FindFuncInScope(enumScopeId, u.Name) is null) {

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: u.Name,
                                // Enum variant constructors are always visible.
                                visibility: new PublicVisibility(),
                                throws: _enum.IsRecursive,
                                returnTypeId: enumTypeId,
                                parameters: new List<CheckedParameter>(),
                                genericParameters: _enum
                                    .GenericParameters
                                    .Select(x => new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId,
                                block: new CheckedBlock(),
                                func: null,
                                linkage: FunctionLinkage.ImplicitEnumConstructor,
                                isInstantiated: true); 

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

                        switch (checkedExpr.ToNumberConstant()) {

                            case NumberConstant constant: {

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

                        // This has a value, so generate a "variable" for it

                        var varErr = project.AddVarToScope(
                            enumScopeId,
                            new CheckedVariable(
                                name: w.Name,
                                typeId: enumTypeId,
                                mutable: false,
                                visibility: new PublicVisibility(),
                                w.Span),
                            w.Span);
                        
                        error = error ?? varErr.Error;
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
                                        typeId: decl,
                                        mutable: member.Mutable,
                                        span: member.Span,
                                        visibility: member.Visibility));
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
                                            typeId: member.TypeId,
                                            mutable: false,
                                            visibility: member.Visibility,
                                            s.Span)))
                                .ToList();

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: s.Name,
                                visibility: new PublicVisibility(),
                                throws: _enum.IsRecursive,
                                returnTypeId: enumTypeId,
                                parameters: constructorParams,
                                genericParameters: _enum
                                    .GenericParameters
                                    .Select(x => 
                                        new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId: funcScopeId,
                                block: new CheckedBlock(),
                                func: null,
                                linkage: FunctionLinkage.ImplicitEnumConstructor,
                                isInstantiated: true);
                            
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
                                            typeId: checkedType,
                                            mutable: false,
                                            visibility: new PublicVisibility(),
                                            t.Span))
                                });

                            var funcScopeId = project.CreateScope(parentScopeId);

                            var checkedConstructor = new CheckedFunction(
                                name: t.Name,
                                visibility: new PublicVisibility(),
                                throws: _enum.IsRecursive,
                                returnTypeId: enumTypeId,
                                parameters: constructorParams,
                                genericParameters: 
                                    _enum
                                    .GenericParameters
                                    .Select(x => new InferenceGuideFunctionGenericParameter(project.FindTypeInScope(enumScopeId, x.Item1)!.Value) as FunctionGenericParameter)
                                    .ToList(),
                                funcScopeId,
                                block: new CheckedBlock(),
                                func: null,
                                linkage: FunctionLinkage.ImplicitEnumConstructor,
                                isInstantiated: true);

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
        Int32 structId,
        Int32 parentScopeId,
        Project project) {

        Error? error = null;

        var structTypeId = project.FindOrAddTypeId(new StructType(structId));

        project.CurrentStructTypeId = structTypeId;

        var structScopeId = project.CreateScope(parentScopeId);

        var _genericParameters = new List<Int32>();

        var isExtern = structure.DefinitionLinkage == DefinitionLinkage.External;

        project.Structs.Add(
            new CheckedStruct(
                name: structure.Name,
                genericParameters: new List<Int32>(),
                fields: new List<CheckedVarDecl>(),
                scopeId: structScopeId,
                definitionLinkage: structure.DefinitionLinkage,
                definitionType: structure.DefinitionType,
                typeId: structTypeId));

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

        project.Structs[structId].GenericParameters = _genericParameters;

        foreach (var func in structure.Methods) {

            var methodScopeId = project.CreateScope(structScopeId);

            var isGeneric = 
                structure.GenericParameters.Any()
                || func.GenericParameters.Any();

            var checkedFunction = new CheckedFunction(
                name: func.Name,
                visibility: func.Visibility,
                throws: func.Throws,
                returnTypeId: Compiler.UnknownTypeId,
                parameters: new List<CheckedParameter>(),
                genericParameters: new List<FunctionGenericParameter>(),
                funcScopeId: methodScopeId,
                block: new CheckedBlock(),
                func: func,
                linkage: func.Linkage,
                isInstantiated: !isGeneric || isExtern);

            project.Functions.Add(checkedFunction);

            var funcId = project.Functions.Count - 1;

            var previousIndex = project.CurrentFunctionIndex;

            project.CurrentFunctionIndex = funcId;

            var genericParameters = new List<FunctionGenericParameter>();

            foreach (var (genParam, paramSpan) in func.GenericParameters) {

                project.Types.Add(new TypeVariable(genParam));

                var typeVarTypeId = project.Types.Count - 1;

                genericParameters.Add(new ParameterFunctionGenericParameter(typeVarTypeId));

                if (!func.MustInstantiate || isExtern) {

                    if (project.AddTypeToScope(
                        methodScopeId, 
                        genParam, 
                        typeVarTypeId, 
                        paramSpan).Error is Error e) {

                        error = error ?? e;
                    }
                }
            }

            checkedFunction.GenericParameters = genericParameters;

            Int32? checkScope = null;

            if (isGeneric && !isExtern) {

                checkScope = project.CreateScope(methodScopeId);
            }

            foreach (var param in func.Parameters) {

                if (param.Variable.Name == "this") {

                    var checkedVariable = new CheckedVariable(
                        name: param.Variable.Name,
                        typeId: structTypeId,
                        mutable: param.Variable.Mutable,
                        visibility: new PublicVisibility(),
                        definitionSpan: param.Variable.Span);

                    checkedFunction.Parameters.Add(
                        new CheckedParameter(
                            requiresLabel: param.RequiresLabel,
                            variable: checkedVariable));

                    if (checkScope is Int32 _checkScope) {

                        project.AddVarToScope(
                            _checkScope,
                            checkedVariable,
                            param.Variable.Span);
                    }
                }
                else {

                    var (paramType, err) = TypeCheckTypeName(param.Variable.Type, methodScopeId, project);

                    error = error ?? err;

                    var checkedVariable = new CheckedVariable(
                        name: param.Variable.Name,
                        typeId: paramType,
                        mutable: param.Variable.Mutable,
                        visibility: new PublicVisibility(),
                        definitionSpan: param.Variable.Span);

                    checkedFunction.Parameters.Add(
                        new CheckedParameter(
                            requiresLabel: param.RequiresLabel,
                            variable: checkedVariable));

                    if (checkScope is Int32 _checkScope) {

                        project.AddVarToScope(
                            _checkScope,
                            checkedVariable,
                            param.Variable.Span);
                    }
                }
            }

            if (project.AddFuncToScope(
                structScopeId, 
                func.Name, 
                project.Functions.Count - 1, 
                structure.Span).Error is Error e1) {

                error = error ?? e1;
            }

            if (!isExtern) {

                var (funcReturnTypeId, err) = TypeCheckTypeName(func.ReturnType, methodScopeId, project);

                error = error ?? err;

                checkedFunction.ReturnTypeId = funcReturnTypeId;

                if (isGeneric) {

                    var (block, _) = TypeCheckBlock(
                        func.Block,
                        checkScope
                            ?? throw new Exception("Generic method with generic parameters must have a check scope"),
                        project,
                        SafetyMode.Safe);

                    var returnTypeId = Compiler.VoidTypeId;

                    if (funcReturnTypeId == Compiler.UnknownTypeId) {

                        if (block.Stmts.LastOrDefault() is CheckedReturnStatement r) {

                            returnTypeId = r.Expr.GetTypeId(methodScopeId, project);
                        }
                    }
                    else {

                        returnTypeId = ResolveTypeVar(funcReturnTypeId, parentScopeId, project);
                    }

                    checkedFunction.Block = block;
                    checkedFunction.ReturnTypeId = returnTypeId;
                }
            }

            project.Functions[funcId] = checkedFunction;
            project.CurrentFunctionIndex = previousIndex;
        }

        project.Structs[structId].GenericParameters = _genericParameters;

        if (project.AddStructToScope(
            parentScopeId, 
            structure.Name, 
            structId, 
            structure.Span).Error is Error e2) {

            error = error ?? e2;
        }

        if (parentScopeId == 0) {

            // Cache various well-known struct IDs as they're used internally in
            // other Neu code.

            if (project.CachedArrayStructId == null && structure.Name == "Array") {
                
                project.CachedArrayStructId = structId;
            } 
            else if (project.CachedDictionaryStructId == null && structure.Name == "Dictionary") {
                
                project.CachedDictionaryStructId = structId;
            } 
            else if (project.CachedErrorStructId == null && structure.Name == "Error") {
                
                project.CachedErrorStructId = structId;
            } 
            else if (project.CachedOptionalStructId == null && structure.Name == "Optional") {
                
                project.CachedOptionalStructId = structId;
            } 
            else if (project.CachedRangeStructId == null && structure.Name == "Range") {
                
                project.CachedRangeStructId = structId;
            } 
            else if (project.CachedSetStructId == null && structure.Name == "Set") {
                
                project.CachedSetStructId = structId;
            }
            else if (project.CachedTupleStructId == null && structure.Name == "Tuple") {
                
                project.CachedTupleStructId = structId;
            } 
            else if (project.CachedWeakPointerStructId == null && structure.Name == "WeakPointer") {
                
                project.CachedWeakPointerStructId = structId;
            }
        }

        project.CurrentStructTypeId = null;

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

        project.CurrentStructTypeId = structTypeId;

        foreach (var uncheckedMember in structure.Fields) {

            var (checkedMemberType, checkedMemberTypeErr) = TypeCheckTypeName(uncheckedMember.Type, chkStructScopeId, project);

            error = error ?? checkedMemberTypeErr;

            fields.Add(
                new CheckedVarDecl(
                    name: uncheckedMember.Name,
                    typeId: checkedMemberType, 
                    mutable: uncheckedMember.Mutable, 
                    span: uncheckedMember.Span,
                    visibility: uncheckedMember.Visibility));
        }

        if (project.FindFuncInScope(chkStructScopeId, structure.Name) is Int32 constructorId) {

            if (structure.DefinitionType == DefinitionType.Class
                && structure.DefinitionLinkage == DefinitionLinkage.External) {

                // XXX: The parser always sets the linkage type of an extern class'
                //      constructor to External, but we actually want to call the
                //      class' ::create function, just like we do with a
                //      ImplicitConstructor class.

                project.Functions[constructorId].Linkage = FunctionLinkage.ExternalClassConstructor;
            }
        }
        else if (structure.DefinitionLinkage != DefinitionLinkage.External) {

            // No constructor found, so let's make one

            var constructorParams = new List<CheckedParameter>();

            foreach (var field in fields) {

                constructorParams.Add(
                    new CheckedParameter(
                        requiresLabel: true,
                        variable: new CheckedVariable(
                            name: field.Name,
                            typeId: field.TypeId,
                            mutable: field.Mutable,
                            // This is the constructor parameter, not the field. It can be public
                            visibility: new PublicVisibility(),
                            definitionSpan: field.Span)));
            }

            var funcScopeId = project.CreateScope(parentScopeId);

            var checkedConstructor = new CheckedFunction(
                name: structure.Name,
                // The default constructor is public
                visibility: new PublicVisibility(),
                throws: structure.DefinitionType == DefinitionType.Class,
                returnTypeId: structTypeId,
                parameters: constructorParams,
                genericParameters: new List<FunctionGenericParameter>(),
                funcScopeId,
                block: new CheckedBlock(),
                func: null,
                linkage: FunctionLinkage.ImplicitConstructor,
                isInstantiated: true);

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
        
        project.CurrentStructTypeId = null;

        return error;
    }

    public static Error? TypeCheckFuncPredecl(
        ParsedFunction func,
        Int32 parentScopeId,
        Int32? thisArgTypeId,
        Project project) {

        Error? error = null;

        var funcScopeId = project.CreateScope(parentScopeId);

        bool isGeneric;

        if (thisArgTypeId is Int32 someType) {

            if (project.Types[someType] is GenericInstance) {

                isGeneric = true;
            }
            else {

                isGeneric = func.GenericParameters.Any();
            }
        }
        else {

            isGeneric = func.GenericParameters.Any();
        }

        var checkedFunction = new CheckedFunction(
            name: func.Name,
            visibility: func.Visibility,
            throws: func.Throws,
            returnTypeId: Compiler.UnknownTypeId,
            parameters: new List<CheckedParameter>(),
            genericParameters: new List<FunctionGenericParameter>(),
            funcScopeId,
            block: new CheckedBlock(),
            func: func,
            linkage: func.Linkage,
            isInstantiated: !isGeneric);

        project.Functions.Add(checkedFunction);

        var funcId = project.Functions.Count - 1;

        var checkedFuncScopeId = checkedFunction.FuncScopeId;

        var previousIndex = project.CurrentFunctionIndex;

        project.CurrentFunctionIndex = funcId;

        var genericParams = new List<FunctionGenericParameter>();

        foreach (var (genParam, paramSpan) in func.GenericParameters) {

            project.Types.Add(new TypeVariable(genParam));

            var typeVarTypeId = project.Types.Count - 1;

            genericParams.Add(new ParameterFunctionGenericParameter(typeVarTypeId));

            if (!func.MustInstantiate || func.Linkage == FunctionLinkage.External) {

                if (project.AddTypeToScope(
                    checkedFuncScopeId, 
                    genParam, 
                    typeVarTypeId, 
                    paramSpan).Error is Error genErr) {

                    error = error ?? genErr;
                }
            }
        }

        checkedFunction.GenericParameters = genericParams;

        Int32? checkScope = null;

        if (isGeneric) {

            checkScope = project.CreateScope(funcScopeId);
        }

        var first = true;

        foreach (var param in func.Parameters) {

            var (paramType, typeCheckNameErr) = TypeCheckTypeName(param.Variable.Type, funcScopeId, project);

            error = error ?? typeCheckNameErr;

            if (first && param.Variable.Name == "this") {

                paramType = thisArgTypeId ?? paramType;
            }

            var checkedVariable = new CheckedVariable(
                name: param.Variable.Name,
                typeId: paramType,
                mutable: param.Variable.Mutable,
                visibility: new PublicVisibility(),
                definitionSpan: param.Variable.Span);

            checkedFunction.Parameters.Add(
                new CheckedParameter(
                    requiresLabel: param.RequiresLabel, 
                    variable: checkedVariable));

            if (checkScope is Int32 _checkScope) {

                project.AddVarToScope(
                    _checkScope, 
                    checkedVariable, 
                    param.Variable.Span);
            }
        }

        var (funcReturnTypeId, err) = TypeCheckTypeName(func.ReturnType, funcScopeId, project);

        error = error ?? err;

        checkedFunction.ReturnTypeId = funcReturnTypeId;

        if (func.GenericParameters.Any()) {

            var (block, _) = TypeCheckBlock(
                func.Block,
                checkScope ?? throw new Exception(""),
                project,
                SafetyMode.Safe);

            var returnTypeId = Compiler.VoidTypeId;

            if (funcReturnTypeId == Compiler.UnknownTypeId) {

                if (block.Stmts.LastOrDefault() is CheckedReturnStatement r) {

                    returnTypeId = r.Expr.GetTypeId(funcScopeId, project);
                }
            }
            else {

                returnTypeId = ResolveTypeVar(funcReturnTypeId, parentScopeId, project);
            }

            checkedFunction.Block = block;
            checkedFunction.ReturnTypeId = returnTypeId;
        }

        project.Functions[funcId] = checkedFunction;

        project.CurrentFunctionIndex = previousIndex;       
       
        if (project.AddFuncToScope(parentScopeId, func.Name, funcId, func.NameSpan).Error is Error e1) {

            error = error ?? e1;
        }

        return error;
    }

    public static Error? TypeCheckAndSpecializeGenericFunction(
        Int32 funcId,
        List<Int32> genericArguments,
        Int32 parentScopeId,
        Int32? thisTypeId,
        Dictionary<Int32, Int32> genericSubstitutions,
        Project project) {

        var _func = project.Functions[funcId];

        // Now we can actually resolve it, let's gooooo

        var _funcId = project.Functions.Count;

        var func = _func.GetParsedFunction();

        var scopeId = project.CreateScope(parentScopeId);

        Error? error = null;

        if (func.GenericParameters.Count != genericArguments.Count) {

            return new TypeCheckError(
                $"Generic function {func.Name} expects {func.GenericParameters.Count} generic arguments, but {genericArguments.Count} were given",
                func.NameSpan);
        }

        var span = func.NameSpan;

        foreach (var (k, v) in genericSubstitutions) {

            if (project.Types[k] is TypeVariable tv) {

                if (project.AddTypeToScope(scopeId, tv.Name, v, span).Error is Error err) {

                    error = error ?? err;
                }
            }
        }

        func.MustInstantiate = true;

        project.CurrentFunctionIndex = _funcId;

        var funcPredeclErr = TypeCheckFuncPredecl(func, scopeId, thisTypeId, project);

        error = error ?? funcPredeclErr;

        var funcErr = TypeCheckFunc(func, scopeId, project);

        error = error ?? funcErr;

        project.CurrentFunctionIndex = null;

        project.Functions[_funcId].IsInstantiated = true;
        project.Functions[_funcId].FuncScopeId = scopeId;

        return error;
    }

    public static Error? TypeCheckFunc(
        ParsedFunction func,
        Int32 parentScopeId,
        Project project) {
        
        if (func.GenericParameters.Any() && !func.MustInstantiate) {

            return null;
        }

        Error? error = null;

        var funcId = project
            .FindFuncInScope(parentScopeId, func.Name) 
            ?? throw new Exception("Internal error: missing previously defined function");

        if (func.Name == "main") {

            error = TypeCheckNeuMain(func);
        }

        var checkedFunction = project.Functions[funcId];

        var functionScopeId = checkedFunction.FuncScopeId;

        var functionLinkage = checkedFunction.Linkage;

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
    
        var (_funcReturnTypeId, returnTypeErr) = TypeCheckTypeName(func.ReturnType, functionScopeId, project);
        
        error = error ?? returnTypeErr;
        
        checkedFunction = project.Functions[funcId];

        checkedFunction.ReturnTypeId = _funcReturnTypeId;

        var (block, typeCheckBlockErr) = TypeCheckBlock(
            func.Block, 
            functionScopeId, 
            project, 
            SafetyMode.Safe);

        error = error ?? typeCheckBlockErr;

        // typecheck the return type again to resolve any generics

        var (funcReturnTypeId, typeCheckReturnTypeErr) = TypeCheckTypeName(func.ReturnType, functionScopeId, project);

        error = error ?? typeCheckReturnTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        Int32 returnTypeId;

        if (funcReturnTypeId == Compiler.UnknownTypeId) {

            if (block.Stmts.LastOrDefault() is CheckedReturnStatement ret) {

                returnTypeId = ret.Expr.GetTypeId(functionScopeId, project);
            }
            else {

                returnTypeId = Compiler.VoidTypeId;
            }
        }
        else {

            returnTypeId = ResolveTypeVar(funcReturnTypeId, parentScopeId, project);
        }

        if (functionLinkage != FunctionLinkage.External
            && returnTypeId != Compiler.VoidTypeId
            && !block.DefinitelyReturns) {

            // FIXME: Use better span

            error = error ??
                new TypeCheckError(
                    "Control reaches end of non-void function",
                    func.NameSpan);
        }

        checkedFunction = project.Functions[funcId];

        checkedFunction.Block = block;

        checkedFunction.ReturnTypeId = returnTypeId;

        return error;
    }

    public static Error? TypeCheckNeuMain(
        ParsedFunction func) {

        var paramTypeError = 
            new TypeCheckError(
                "Main function must take a single array of strings as its parameter",
                func.NameSpan);

        if (func.Parameters.Count > 1) {

            return paramTypeError;
        }

        if (func.Parameters.Any()) {

            switch (func.Parameters[0].Variable.Type) {

                case ParsedArrayType a: {

                    switch (a.Type) {

                        case ParsedEmptyType _: {

                            break;
                        }

                        case ParsedNameType n when n.Name == "String": {

                            break;
                        }

                        default: {

                            return paramTypeError;
                        }
                    }

                    break;
                }

                default: {

                    return paramTypeError;
                }
            }
        }

        var returnTypeError = 
            new TypeCheckError(
                "Main function must return CInt",
                func.NameSpan);

        switch (func.ReturnType) {

            case ParsedEmptyType _: {

                break;
            }

            case ParsedNameType n when n.Name == "CInt": {

                break;
            }

            default: {

                return returnTypeError;
            }
        }

        return null;
    }

    public static Error? TypeCheckMethod(
        ParsedFunction func,
        Project project,
        Int32 structId) { 

        Error? error = null;

        var structure = project.Structs[structId];

        if ((func.GenericParameters.Any() || structure.GenericParameters.Any())
            && !func.MustInstantiate) {

            return null;
        }

        var structureScopeId = structure.ScopeId;

        var structureLinkage = structure.DefinitionLinkage;

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
    
        // Set current function index before a block type check so that
        // method return type is checked against its implementation
        project.CurrentFunctionIndex = methodId;

        var (block, chkBlockErr) = TypeCheckBlock(func.Block, funcScopeId, project, SafetyMode.Safe);

        error = error ?? chkBlockErr;

        var (funcReturnTypeId, chkRetTypeErr) = TypeCheckTypeName(func.ReturnType, funcScopeId, project);

        error = error ?? chkRetTypeErr;

        // If the return type is unknown, and the function starts with a return statement,
        // we infer the return type from its expression.

        var returnTypeId = funcReturnTypeId;

        if (funcReturnTypeId == Compiler.UnknownTypeId) {

            if (block.Stmts.FirstOrDefault() is CheckedReturnStatement ret) {

                returnTypeId = ret.Expr.GetTypeId(funcScopeId, project);
            }
            else {

                returnTypeId = Compiler.VoidTypeId;
            }
        }

        if (structureLinkage != DefinitionLinkage.External
            && returnTypeId != Compiler.VoidTypeId
            && !block.DefinitelyReturns) {

            // FIXME: Use better span

            error = error ??
                new TypeCheckError(
                    "Control reaches end of non-void function",
                    func.NameSpan);
        }

        checkedFunction = project.Functions[methodId];

        checkedFunction.Block = block;

        checkedFunction.ReturnTypeId = returnTypeId;

        return error;       
    }

    public static bool StatementDefinitelyReturns(
        CheckedStatement statement) {

        switch (statement) {

            case CheckedReturnStatement _:
                return true;

            case CheckedIfStatement i when 
                i.Expr is CheckedBooleanExpression expr
                && expr.Value: {

                return i.Block.DefinitelyReturns;
            }

            case CheckedIfStatement i when
                i.Trailing is CheckedStatement elseStmt: {

                return i.Block.DefinitelyReturns && StatementDefinitelyReturns(elseStmt);
            }

            case CheckedBlockStatement b:
                return b.Block.DefinitelyReturns;

            case CheckedLoopStatement l:
                return l.Block.DefinitelyReturns;

            case CheckedWhileStatement w:
                return w.Block.DefinitelyReturns;

            default: 
                return false;
        }
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

            if (StatementDefinitelyReturns(checkedStmt)) {

                checkedBlock.DefinitelyReturns = true;
            }

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

            case ParsedTryStatement tryStmt: {

                var (checkedStmt, err) = TypeCheckStatement(tryStmt.Statement, scopeId, project, safetyMode);

                error = error ?? err;

                var errorStructId = project.GetErrorStructId(tryStmt.Span);

                var errorDecl = new CheckedVariable(
                    name: tryStmt.Name,
                    typeId: project.FindOrAddTypeId(new StructType(errorStructId)),
                    mutable: false,
                    visibility: new PublicVisibility(),
                    definitionSpan: tryStmt.Span);

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

            case ParsedThrowStatement ts: {

                var (checkedExpr, err) = TypeCheckExpression(ts.Expr, scopeId, project, safetyMode, null);

                error = error ?? err;

                var errorStructTypeId = project
                    .FindTypeInScope(0, "Error") 
                    ?? throw new Exception("internal error: Error builtin definition not found");

                if (checkedExpr.GetTypeId(scopeId, project) != errorStructTypeId) {

                    error = error ?? 
                        new TypeCheckError(
                            "throw expression does not produce an error",
                            ts.Expr.GetSpan());
                }

                return (
                    new CheckedThrowStatement(checkedExpr),
                    error);
            }

            case ParsedForStatement fs: {

                // Translate `for x in expr { body }` to
                // block {
                //     var _magic = expr
                //     loop {
                //         let x = _magic.next()
                //         if not x.hasValue() {
                //             break
                //         }
                //         let iteratorName = x!
                //         body
                //     }
                // }
                //
                // The only restrictions placed on the iterator are such:
                //     1- Must respond to .next(); the mutability of the iterator is inferred from .next()'s signature
                //     2- The result of .next() must be an Optional

                var (iterableExpr, iterErr) = TypeCheckExpression(fs.Range, scopeId, project, safetyMode, null);

                error = error ?? iterErr;

                var iterableType = project.Types[iterableExpr.GetTypeId(scopeId, project)];

                var iterableShouldBeMutable = false;

                // Requirement 1: Iterator must have a .next() method
                //                This currently limits it to structs and classes.
                // Right now we do a single pass check, and check generic functions at declaration time (as opposed to doing so at instantiation time)
                // so we have to pretend it's okay if we don't know the iterable type right now.

                Int32? _structId = iterableType switch {

                    GenericInstance gi => gi.StructId,
                    StructType st => st.StructId,
                    _ => null
                };

                switch (iterableType) {

                    case TypeVariable _: {

                        // Since we're not sure, just make it mutable.

                        iterableShouldBeMutable = true;

                        break;
                    }

                    case GenericInstance _:
                    case StructType _: {

                        var _struct = project.Structs[_structId ?? throw new Exception()];

                        var _nextMethodFunctionId = project
                            .FindFuncInScope(_struct.ScopeId, "next");

                        if (_nextMethodFunctionId is Int32 nextMethodFunctionId) {

                            var nextMethodFunc = project.Functions[nextMethodFunctionId];

                            // Check whether we need to make the iterator mutable

                            if (nextMethodFunc.IsMutating()) {

                                iterableShouldBeMutable = true;
                            }
                        }
                        else {

                            error = error ??
                                new TypeCheckError(
                                    "Iterator must have a .next() method",
                                    fs.Range.GetSpan());
                        }

                        break;
                    }

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "Iterator must have a .next() method",
                                fs.Iterator.Item2);

                        break;
                    }
                }

                var rewrittenStatement = new ParsedBlockStatement(
                    new ParsedBlock(
                        new List<ParsedStatement>(
                            new ParsedStatement[] {

                                // var _magic = expr

                                new ParsedVarDeclStatement(
                                    new ParsedVarDecl(
                                        name: "_magic",
                                        type: new ParsedEmptyType(),
                                        mutable: iterableShouldBeMutable,
                                        span: fs.Iterator.Item2,
                                        visibility: new PublicVisibility()),
                                    fs.Range),

                                // loop {

                                new ParsedLoopStatement(
                                    new ParsedBlock(
                                        new List<ParsedStatement>(
                                            new ParsedStatement[] {

                                                // let x = _magic.next()

                                                new ParsedVarDeclStatement(
                                                    new ParsedVarDecl(
                                                        name: "_magic_value",
                                                        type: new ParsedEmptyType(),
                                                        mutable: iterableShouldBeMutable,
                                                        span: fs.Iterator.Item2,
                                                        visibility: new PublicVisibility()
                                                    ),
                                                    new ParsedMethodCallExpression(
                                                        new ParsedVarExpression(
                                                            "_magic",
                                                            fs.Iterator.Item2),
                                                        new ParsedCall(
                                                            ns: new List<String>(),
                                                            name: "next",
                                                            args: new List<(String, ParsedExpression)>(),
                                                            typeArgs: new List<ParsedType>()),
                                                        fs.Iterator.Item2)),

                                                // if not x.hasValue() {

                                                new ParsedIfStatement(
                                                    new ParsedUnaryOpExpression(
                                                        new ParsedMethodCallExpression(
                                                            new ParsedVarExpression(
                                                                "_magic_value",
                                                                fs.Iterator.Item2),
                                                            new ParsedCall(
                                                                ns: new List<String>(),
                                                                name: "hasValue",
                                                                args: new List<(string, ParsedExpression)>(),
                                                                typeArgs: new List<ParsedType>()),
                                                            fs.Iterator.Item2),
                                                        new LogicalNotUnaryOperator(),
                                                        fs.Iterator.Item2),
                                                    new ParsedBlock(
                                                        new List<ParsedStatement>(
                                                            new ParsedStatement[] {

                                                                new ParsedBreakStatement()
                                                            })),
                                                    null),

                                                // let iterator_name = x!

                                                new ParsedVarDeclStatement(
                                                    new ParsedVarDecl(
                                                        name: fs.Iterator.Item1,
                                                        type: new ParsedEmptyType(),
                                                        mutable: iterableShouldBeMutable,
                                                        span: fs.Iterator.Item2,
                                                        visibility: new PublicVisibility()),
                                                    new ParsedForcedUnwrapExpression(
                                                        new ParsedVarExpression(
                                                            "_magic_value",
                                                            fs.Iterator.Item2),
                                                        fs.Iterator.Item2)),

                                                // body

                                                // new ParsedBlockStatement(new ParsedBlock(fs.Block.Statements.ToList()))
                                                new ParsedBlockStatement(fs.Block)
                                            })))

                            })));

                var (statement, stmtErr) = TypeCheckStatement(rewrittenStatement, scopeId, project, safetyMode);

                error = error ?? stmtErr;

                return (statement, error);
            }

            case ParsedContinueStatement cs: {

                return (
                    new CheckedContinueStatement(),
                    null);
            }

            case ParsedBreakStatement bs: {

                return (
                    new CheckedBreakStatement(),
                    null);
            }

            case ParsedExpressionStatement es: {

                var (checkedExpr, exprErr) = TypeCheckExpression(es.Expression, scopeId, project, safetyMode, null);

                return (
                    new CheckedExpressionStatement(checkedExpr),
                    exprErr);
            }

            case ParsedDeferStatement ds: {

                var wasInsideDefer = project.InsideDefer;

                project.InsideDefer = true;

                var (checkedStmt, err) = TypeCheckStatement(ds.Statement, scopeId, project, safetyMode);

                project.InsideDefer = wasInsideDefer;

                return (
                    new CheckedDeferStatement(checkedStmt),
                    err);
            }

            case ParsedUnsafeBlockStatement us: {

                var (checkedBlock, blockErr) = TypeCheckBlock(us.Block, scopeId, project, SafetyMode.Unsafe);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    blockErr);
            }

            case ParsedVarDeclStatement vds: {

                var (checkedTypeId, typenameErr) = TypeCheckTypeName(vds.Decl.Type, scopeId, project);

                var (checkedExpr, exprErr) = TypeCheckExpression(vds.Expr, scopeId, project, safetyMode, checkedTypeId);

                error = error ?? exprErr;

                if (checkedTypeId == Compiler.UnknownTypeId 
                    && checkedExpr.GetTypeId(scopeId, project) != Compiler.UnknownTypeId) {

                    checkedTypeId = checkedExpr.GetTypeId(scopeId, project);
                }
                else {

                    error = error ?? typenameErr;
                }

                var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                    checkedTypeId,
                    checkedExpr,
                    vds.Expr.GetSpan());

                error = error ?? tryPromoteErr;

                if (promotedExpr is not null) {

                    checkedExpr = promotedExpr;
                }

                var weakPointerStructId = project.GetWeakPointerStructId(vds.Decl.Span);

                switch (project.Types[checkedTypeId]) {

                    case GenericInstance gi when gi.StructId == weakPointerStructId: {

                        if (!vds.Decl.Mutable) {

                            error = error ??
                                new TypeCheckError(
                                    "Weak reference must be mutable",
                                    vds.Decl.Span);
                        }

                        break;
                    }

                    default: {

                        break;
                    }
                }

                var checkedVarDecl = new CheckedVarDecl(
                    name: vds.Decl.Name,
                    typeId: checkedTypeId,
                    span: vds.Decl.Span,
                    mutable: vds.Decl.Mutable,
                    visibility: vds.Decl.Visibility);

                if (project.AddVarToScope(
                    scopeId,
                    new CheckedVariable(
                        name: checkedVarDecl.Name, 
                        typeId: checkedVarDecl.TypeId, 
                        mutable: checkedVarDecl.Mutable,
                        visibility: checkedVarDecl.Visibility,
                        definitionSpan: checkedVarDecl.Span),
                    checkedVarDecl.Span).Error is Error e) {

                    error = error ?? e;
                }

                return (
                    new CheckedVarDeclStatement(checkedVarDecl, checkedExpr),
                    error);
            }

            case ParsedIfStatement ifStmt: {

                var (checkedCond, exprErr) = TypeCheckExpression(ifStmt.Expr, scopeId, project, safetyMode, null);
                
                error = error ?? exprErr;

                if (checkedCond.GetTypeId(scopeId, project) != Compiler.BoolTypeId) {
                    
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

            case ParsedLoopStatement ls: {

                var (checkedBlock, blockErr) = TypeCheckBlock(ls.Block, scopeId, project, safetyMode);

                error = error ?? blockErr;

                return (
                    new CheckedLoopStatement(checkedBlock),
                    error);
            }

            case ParsedWhileStatement ws: {

                var (checkedCond, exprErr) = TypeCheckExpression(ws.Expr, scopeId, project, safetyMode, null);
                
                error = error ?? exprErr;

                if (checkedCond.GetTypeId(scopeId, project) != Compiler.BoolTypeId) {

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

            case ParsedReturnStatement rs: {

                Int32? _retType = null;

                if (project.CurrentFunctionIndex is Int32 cfi) {

                    _retType = project.Functions[cfi].ReturnTypeId;
                }

                var (output, outputErr) = TypeCheckExpression(
                    rs.Expr, 
                    scopeId, 
                    project, 
                    safetyMode,
                    _retType);

                error = error ?? outputErr;

                if (project.InsideDefer) {

                    error = error ??
                        new TypeCheckError(
                            "'return' is not allowed inside 'defer'",
                            rs.Span);
                }

                return (
                    new CheckedReturnStatement(output), 
                    error);
            }

            case ParsedBlockStatement bs: {

                var (checkedBlock, checkedBlockErr) = TypeCheckBlock(bs.Block, scopeId, project, safetyMode);

                return (
                    new CheckedBlockStatement(checkedBlock),
                    checkedBlockErr);
            }

            case ParsedInlineCPPStatement i: {

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

                        case ParsedExpressionStatement es when es.Expression is ParsedQuotedStringExpression qs: {

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
            
            case ParsedGarbageStatement _: {

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

        if (checkedRhs.ToNumberConstant() is NumberConstant rhsConstant) {

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

    public static (CheckedWhenBody, Error?) TypeCheckWhenBody(
        WhenBody body,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode,
        Dictionary<Int32, Int32> genericParameters,
        ref Int32? finalResultType,
        Span span) {

        Error? error = null;

        CheckedWhenBody? _body = null;

        switch (body) {

            case BlockWhenBody block: {

                var (checkedBlock, err) = TypeCheckBlock(block.Block, scopeId, project, safetyMode);

                error = error ?? err;

                if (!checkedBlock.DefinitelyReturns) {

                    switch (finalResultType) {

                        case Int32 frt: {

                            if (CheckTypesForCompat(
                                Compiler.VoidTypeId,
                                frt,
                                genericParameters,
                                span, 
                                project) is Error e) {

                                error = error ?? e;
                            }

                            break;
                        }

                        default: {

                            finalResultType = Compiler.VoidTypeId;

                            break;
                        }
                    }
                }

                _body = new CheckedBlockWhenBody(checkedBlock);

                break;
            }

            case ExpressionWhenBody expr: {

                var (checkedBody, err) = TypeCheckExpression(expr.Expression, scopeId, project, safetyMode, finalResultType);

                error = error ?? err;

                var span2 = expr.Expression.GetSpan();

                switch (finalResultType) {

                    case Int32 frt: {

                        if (CheckTypesForCompat(
                            checkedBody.GetTypeId(scopeId, project),
                            frt,
                            genericParameters,
                            span2,
                            project) is Error e2) {

                            error = error ?? e2;
                        }

                        break;
                    }

                    default: {

                        finalResultType = checkedBody.GetTypeId(scopeId, project);

                        break;
                    }
                }

                _body = new CheckedExpressionWhenBody(checkedBody);

                break;
            }

            default: {

                throw new Exception();
            }
        }

        return (
            _body ?? throw new Exception(),
            error);
    }

    public static (CheckedExpression, Error?) TypeCheckExpression(
        ParsedExpression expr,
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode,
        Int32? typeHint) {

        Error? error = null;

        Func<Project, Int32, (Int32, Error?)> unifyWithTypeHint = (project, typeId) => {

            if (typeHint is Int32 hint) {

                if (hint == Compiler.UnknownTypeId) {

                    return (typeId, null);
                }

                var genericInterface = new Dictionary<Int32, Int32>();

                var err = CheckTypesForCompat(
                    hint,
                    typeId,
                    genericInterface,
                    expr.GetSpan(),
                    project);

                if (err != null) {

                    return (typeId, err);
                }

                return (
                    SubstituteTypeVarsInType(typeId, genericInterface, project),
                    null);
            }

            return (typeId, null);
        };

        switch (expr) {

            case ParsedRangeExpression re: {

                var (checkedStart, startErr) = TypeCheckExpression(re.Start, scopeId, project, safetyMode, null);

                error = error ?? startErr;

                var (checkedEnd, endErr) = TypeCheckExpression(re.End, scopeId, project, safetyMode, null);

                error = error ?? endErr;

                // If the range starts or ends at a constant number, we try promoting the constant to the
                // type of the other end. This makes ranges like `0..array.size()` (as the 0 becomes 0uz).

                var (promotedEnd, promoteEndErr) = TryPromoteConstantExprToType(
                    checkedStart.GetTypeId(scopeId, project), 
                    checkedEnd, 
                    re.Span);

                error = error ?? promoteEndErr;

                if (promotedEnd is not null) {

                    checkedEnd = promotedEnd;
                }

                var (promotedStart, promoteStartErr) = TryPromoteConstantExprToType(
                    checkedEnd.GetTypeId(scopeId, project), 
                    checkedStart, 
                    re.Span);

                error = error ?? promoteStartErr;

                if (promotedStart is not null) {

                    checkedStart = promotedStart;
                }

                if (checkedStart.GetTypeId(scopeId, project) != checkedEnd.GetTypeId(scopeId, project)) {

                    error = error ?? 
                        new TypeCheckError(
                            "Range start and end must be the same type",
                            re.Span);
                }

                var rangeStructId = project.GetRangeStructId(re.Span);

                var _typeId = new GenericInstance(
                    rangeStructId, 
                    new List<Int32>(new [] { checkedStart.GetTypeId(scopeId, project) }));

                var typeId = project.FindOrAddTypeId(_typeId);

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedRangeExpression(
                        checkedStart, 
                        checkedEnd,
                        re.Span,
                        unifiedTypeId),
                    error);
            }

            case ParsedBinaryOpExpression e: {

                var (checkedLhs, checkedLhsErr) = TypeCheckExpression(e.Lhs, scopeId, project, safetyMode, null);

                error = error ?? checkedLhsErr;

                var (checkedRhs, checkedRhsErr) = TypeCheckExpression(e.Rhs, scopeId, project, safetyMode, null);

                error = error ?? checkedRhsErr;

                var (promotedExpr, tryPromoteErr) = TryPromoteConstantExprToType(
                    checkedLhs.GetTypeId(scopeId, project), 
                    checkedRhs, 
                    e.Span);

                error = error ?? tryPromoteErr;

                if (promotedExpr is not null) {

                    checkedRhs = promotedExpr;
                }

                // TODO: actually do the binary operator typecheck against safe operations
                // For now, use a type we know
                
                var (typeId, chkBinOpErr) = TypeCheckBinaryOperation(
                    checkedLhs, 
                    e.Operator, 
                    checkedRhs,
                    scopeId,
                    e.Span,
                    project);

                error = error ?? chkBinOpErr;

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedBinaryOpExpression(
                        checkedLhs, 
                        e.Operator, 
                        checkedRhs,
                        e.Span,
                        unifiedTypeId),
                    error);
            }

            case ParsedUnaryOpExpression u: {

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
                    scopeId,
                    project, 
                    safetyMode);

                error = error ?? chkUnaryOpErr;

                return (_checkedExpr, error);
            }

            case ParsedOptionalNoneExpression e: {

                return (
                    new CheckedOptionalNoneExpression(
                        e.Span,
                        Compiler.UnknownTypeId),
                    error);
            }

            case ParsedOptionalSomeExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode, null);

                error = error ?? ckdExprError;

                var type = ckdExpr.GetTypeId(scopeId, project);

                return (
                    new CheckedOptionalSomeExpression(ckdExpr, e.Span, type),
                    error);
            }

            case ParsedForcedUnwrapExpression e: {

                var (ckdExpr, ckdExprError) = TypeCheckExpression(e.Expression, scopeId, project, safetyMode, null);

                error = error ?? ckdExprError;

                var type = project.Types[ckdExpr.GetTypeId(scopeId, project)];

                var optionalStructId = project.GetOptionalStructId(e.Span);
                
                var weakPointerStructId = project.GetWeakPointerStructId(e.Span);

                var typeId = Compiler.UnknownTypeId;

                switch (type) {

                    case GenericInstance gi when 
                        gi.StructId == optionalStructId
                        || gi.StructId == weakPointerStructId: {

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

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedForceUnwrapExpression(
                        ckdExpr, 
                        e.Span, 
                        unifiedTypeId),
                    error);
            }

            case ParsedBooleanExpression e: {

                return (
                    new CheckedBooleanExpression(e.Value, e.Span),
                    null);
            }

            case ParsedCallExpression e: {

                var (checkedCall, checkedCallErr) = TypeCheckCall(
                    e.Call, 
                    scopeId, 
                    e.Span, 
                    project, 
                    null, 
                    null, 
                    safetyMode,
                    typeHint);

                error = error ?? checkedCallErr;

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, checkedCall.TypeId);

                error = error ?? unifyErr;

                return (
                    new CheckedCallExpression(checkedCall, e.Span, unifiedTypeId),
                    error);
            }

            case ParsedNumericConstantExpression ne: {

                // FIXME: Don't ignore type hint unification errors

                var (unifiedTypeId, _) = unifyWithTypeHint(project, ne.Value.GetTypeId());

                return (
                    new CheckedNumericConstantExpression(
                        ne.Value, 
                        ne.Span, 
                        unifiedTypeId),
                    null);
            }

            case ParsedQuotedStringExpression e: {

                var (_, err) = unifyWithTypeHint(project, Compiler.StringTypeId);

                return (
                    new CheckedQuotedStringExpression(e.Value, e.Span),
                    err);
            }

            case ParsedByteLiteralExpression b: {

                var (_, err) = unifyWithTypeHint(project, Compiler.UInt8TypeId);

                return (
                    new CheckedByteConstantExpression(b.Value, b.Span),
                    err);
            }

            case ParsedCharacterLiteralExpression cle: {

                var (_, err) = unifyWithTypeHint(project, Compiler.CCharTypeId);

                return (
                    new CheckedCharacterConstantExpression(cle.Value, cle.Span),
                    err);
            }

            case ParsedVarExpression e: {

                if (project.FindVarInScope(scopeId, e.Value) is CheckedVariable v) {

                    var (_, err) = unifyWithTypeHint(project, v.TypeId);

                    return (
                        new CheckedVarExpression(v, e.Span),
                        err);
                }
                else {
                    
                    return (
                        new CheckedVarExpression(
                            new CheckedVariable(
                                e.Value, 
                                typeId: typeHint ?? Compiler.UnknownTypeId,
                                mutable: false,
                                visibility: new PublicVisibility(),
                                definitionSpan: e.Span),
                            e.Span),
                        new TypeCheckError(
                            $"variable '{e.Value}' not found",
                            e.Span));
                }
            }

            case ParsedNamespacedVarExpression e: {

                var scopes = new List<Int32?>(new Int32?[] { scopeId });

                foreach (var ns in e.Items) {

                    // Could either be a namespace or an enum with an underlying type, prefer the namespace

                    Int32? _scope = null;

                    var _lastScopeId = scopes.LastOrDefault();

                    if (_lastScopeId is Int32 lastScopeId) {

                        _scope = project.FindNamespaceInScope(lastScopeId, ns);

                        if (_scope == null) {

                            var _enumId = project.FindEnumInScope(lastScopeId, ns);

                            if (_enumId is Int32 enumId) {

                                _scope = project.Enums[enumId].ScopeId;
                            }
                        }
                    }

                    scopes.Add(_scope);
                }

                var scope = scopes.LastOrDefault();

                // if (e.Items.Count != scopes.Count) {

                //     throw new Exception();
                // }

                var checkedNamespace = new List<CheckedNamespace>();

                for (var i = 0; i < e.Items.Count; i++) {

                    checkedNamespace.Add(
                        new CheckedNamespace(
                            name: e.Items[i],
                            scopeId: i < scopes.Count ? scopes[i] ?? 0 : 0));
                }

                switch (scope) {

                    case Int32 variableScopeId: {

                        if (project.FindVarInScope(variableScopeId, e.Name) is CheckedVariable v) {

                            return (
                                new CheckedNamespacedVarExpression(checkedNamespace, v, e.Span),
                                CheckAccessibility(scopeId, variableScopeId, v, e.Span, project));
                        }
                        else {

                            return (
                                new CheckedNamespacedVarExpression(
                                    checkedNamespace,
                                    new CheckedVariable(
                                        name: e.Name,
                                        typeId: typeHint ?? Compiler.UnknownTypeId,
                                        mutable: false,
                                        visibility: new PublicVisibility(),
                                        definitionSpan: e.Span),
                                    e.Span),
                                new TypeCheckError(
                                    $"variable '{e.Name}' not found",
                                    e.Span));
                        }
                    }

                    default: {

                        return (
                            new CheckedNamespacedVarExpression(
                                checkedNamespace,
                                new CheckedVariable(
                                    name: e.Name,
                                    typeId: typeHint ?? Compiler.UnknownTypeId,
                                    mutable: false,
                                    visibility: new PublicVisibility(),
                                    definitionSpan: e.Span),
                                e.Span),
                            new TypeCheckError(
                                "namespace not found",
                                e.Span));
                    }
                }
            }

            case ParsedArrayExpression ve: {

                var innerTypeId = Compiler.UnknownTypeId;

                Span? innerTypeSpan = null;

                var output = new List<CheckedExpression>();

                var arrayStructId = project.GetArrayStructId(ve.Span);

                Int32? innerHint = null;

                if (typeHint is Int32 hint) {

                    if (project.Types[hint] is GenericInstance gi) {

                        if (gi.StructId == arrayStructId) {

                            innerHint = gi.TypeIds[0];
                        }
                    }
                }

                CheckedExpression? checkedFillSizeExpr = null;

                if (ve.FillSize is ParsedExpression fillSize) {

                    var (chkFillSizeExpr, chkFillSizeErr) = TypeCheckExpression(
                        fillSize, 
                        scopeId, 
                        project, 
                        safetyMode, 
                        innerHint);

                    checkedFillSizeExpr = chkFillSizeExpr;

                    error = error ?? chkFillSizeErr;
                }

                ///

                foreach (var v in ve.Expressions) {

                    var (checkedExpr, err) = TypeCheckExpression(v, scopeId, project, safetyMode, innerHint);

                    error = error ?? err;

                    var currentValueTypeId = checkedExpr.GetTypeId(scopeId, project);

                    if (innerTypeId is Compiler.UnknownTypeId) {

                        if (currentValueTypeId == Compiler.VoidTypeId) {

                            error = error ?? 
                                new TypeCheckError(
                                    "cannot create an array with values of type void",
                                    v.GetSpan());
                        }

                        innerTypeId = currentValueTypeId;
                        innerTypeSpan = v.GetSpan();
                    }
                    else if (innerTypeId != currentValueTypeId) {

                        var arrayTypeName = project.TypeNameForTypeId(innerTypeId);

                        error = error ??
                            new TypecheckErrorWithHint(
                                $"type '{project.TypeNameForTypeId(currentValueTypeId)}' does not match type '{arrayTypeName}' of previous values in array",
                                v.GetSpan(),
                                $"array was inferred to store type '{arrayTypeName}' here",
                                innerTypeSpan ?? throw new Exception());
                    }

                    output.Add(checkedExpr);
                }

                if (innerTypeId == Compiler.UnknownTypeId) {

                    if (innerHint is Int32 hintInnerType) {

                        innerTypeId = hintInnerType;
                    }
                    else if (typeHint == Compiler.UnknownTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "Cannot infer generic type for Array<T>",
                                ve.Span);
                    }
                }

                var typeId = project
                    .FindOrAddTypeId(new GenericInstance(arrayStructId, new List<Int32>(new [] { innerTypeId })));

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedArrayExpression(
                        expressions: output,
                        checkedFillSizeExpr,
                        ve.Span,
                        unifiedTypeId),
                    error);
            }

            case ParsedSetExpression se: {

                var innerTypeId = Compiler.UnknownTypeId;

                Span? innerTypeSpan = null;

                var output = new List<CheckedExpression>();

                var setStructId = project.GetSetStructId(se.Span);

                Int32? innerHint = null;

                if (typeHint is Int32 hint) {

                    if (project.Types[hint] is GenericInstance gi) {
                        
                        if (gi.StructId == setStructId) {

                            innerHint = gi.TypeIds[0];
                        }
                    }
                }

                foreach (var value in se.Items) {

                    var (checkedValue, err) = TypeCheckExpression(value, scopeId, project, safetyMode, innerHint);

                    error = error ?? err;

                    var currentValueTypeId = checkedValue.GetTypeId(scopeId, project);

                    if (innerTypeId == Compiler.UnknownTypeId) {
                         
                        if (currentValueTypeId == Compiler.VoidTypeId) {

                            error = error ?? 
                                new TypeCheckError(
                                    "cannot create a set with values of type void",
                                    value.GetSpan());
                        }

                        innerTypeId = currentValueTypeId;
                        innerTypeSpan = value.GetSpan();
                    }
                    else if (innerTypeId != currentValueTypeId) {

                        var setTypeName = project.TypeNameForTypeId(innerTypeId);

                        error = error ??
                            new TypecheckErrorWithHint(
                                $"type '{project.TypeNameForTypeId(currentValueTypeId)}' does not match type '{setTypeName}' of previous values in set",
                                value.GetSpan(),
                                $"set was inferred to store type '{setTypeName}' here",
                                innerTypeSpan ?? throw new Exception());
                    }

                    output.Add(checkedValue);
                }

                if (innerTypeId == Compiler.UnknownTypeId) {

                    if (innerHint is Int32 hintInnerType) {

                        innerTypeId = hintInnerType;
                    }
                    else if (typeHint == Compiler.UnknownTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "Cannot infer generic type for Set<T>",
                                se.Span);
                    }
                }

                var typeId = project
                    .FindOrAddTypeId(new GenericInstance(setStructId, new List<Int32>(new [] { innerTypeId })));

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedSetExpression(output, se.Span, unifiedTypeId),
                    error);
            }

            case ParsedDictionaryExpression de: {

                var keyTypeId = Compiler.UnknownTypeId;

                Span? keyTypeSpan = null;

                var valueTypeId = Compiler.UnknownTypeId;

                Span? valueTypeSpan = null;

                var output = new List<(CheckedExpression, CheckedExpression)>();

                var dictStructId = project.GetDictionaryStrutId(de.Span);

                Int32? keyHint = null;

                Int32? valueHint = null;

                if (typeHint is Int32 hint) {

                    if (project.Types[hint] is GenericInstance gi) {

                        if (gi.StructId == dictStructId) {

                            keyHint = gi.TypeIds[0];
                            valueHint = gi.TypeIds[1];
                        }
                    }
                }

                foreach (var (key, value) in de.Entries) {

                    var (checkedKey, keyErr) = TypeCheckExpression(key, scopeId, project, safetyMode, keyHint);

                    error = error ?? keyErr;

                    var currentKeyTypeId = checkedKey.GetTypeId(scopeId, project);

                    var (checkedValue, valueErr) = TypeCheckExpression(value, scopeId, project, safetyMode, valueHint);

                    error = error ?? valueErr;

                    var currentValueTypeId = checkedValue.GetTypeId(scopeId, project);

                    if (keyTypeId == Compiler.UnknownTypeId
                        && valueTypeId == Compiler.UnknownTypeId) {

                        if (currentKeyTypeId == Compiler.VoidTypeId) {

                            error = error ??
                                new TypeCheckError(
                                    "cannot create a dictionary with keys of type void",
                                    key.GetSpan());
                        }

                        if (currentValueTypeId == Compiler.VoidTypeId) {

                            error = error ??
                                new TypeCheckError(
                                    "cannot create a dictionary with values of type void",
                                    value.GetSpan());
                        }

                        keyTypeId = currentKeyTypeId;

                        keyTypeSpan = key.GetSpan();

                        valueTypeId = currentValueTypeId;

                        valueTypeSpan = value.GetSpan();
                    }
                    else {

                        if (keyTypeId != currentKeyTypeId) {

                            var keyTypeName = project.TypeNameForTypeId(keyTypeId);

                            error = error ?? 
                                new TypecheckErrorWithHint(
                                    $"type '{project.TypeNameForTypeId(currentKeyTypeId)}' does not match type '{keyTypeName}' of previous keys in dictionary",
                                    key.GetSpan(),
                                    $"dictionary was inferred to store keys of type '{keyTypeName}' here",
                                    keyTypeSpan ?? throw new Exception());
                        }

                        if (valueTypeId != currentValueTypeId) {

                            var valueTypeName = project.TypeNameForTypeId(valueTypeId);

                            error = error ??
                                new TypecheckErrorWithHint(
                                    $"type '{project.TypeNameForTypeId(currentValueTypeId)}' does not match type '{valueTypeName}' of previous values in dictionary",
                                    value.GetSpan(),
                                    $"dictionary was inferred to store values of type '{valueTypeName}' here",
                                    valueTypeSpan ?? throw new Exception());
                        }
                    }

                    output.Add((checkedKey, checkedValue));
                }

                if (keyTypeId == Compiler.UnknownTypeId) {

                    if (keyHint is Int32 kh) {

                        keyTypeId = kh;
                    }
                    else if (typeHint == Compiler.UnknownTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "Cannot infer key type for Dictionary<K, V>",
                                de.Span);
                    }
                }

                if (valueTypeId == Compiler.UnknownTypeId) {

                    if (valueHint is Int32 vh) {

                        valueTypeId = vh;
                    }
                    else if (typeHint == Compiler.UnknownTypeId) {

                        error = error ?? 
                            new TypeCheckError(
                                "Cannot infer value type for Dictionary",
                                de.Span);
                    }
                }

                var typeId = project
                    .FindOrAddTypeId(
                        new GenericInstance(
                            dictStructId, 
                            new List<Int32>(new [] { keyTypeId, valueTypeId })));

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedDictionaryExpression(
                        output, 
                        de.Span, 
                        unifiedTypeId),
                    error);
            }

            case ParsedTupleExpression te: {

                var checkedItems = new List<CheckedExpression>();

                var checkedTypes = new List<Int32>();

                foreach (var item in te.Expressions) {

                    var (checkedItemExpr, typeCheckItemExprErr) = TypeCheckExpression(item, scopeId, project, safetyMode, null);

                    error = error ?? typeCheckItemExprErr;

                    if (checkedItemExpr.GetTypeId(scopeId, project) == Compiler.VoidTypeId) {

                        error = error ?? 
                            new TypeCheckError(
                                "cannot create a tuple that contains a value of type void", 
                                te.GetSpan());
                    }

                    checkedTypes.Add(checkedItemExpr.GetTypeId(scopeId, project));

                    checkedItems.Add(checkedItemExpr);
                }

                var tupleStructId = project.GetTupleStructId(te.Span);

                var typeId = project
                    .FindOrAddTypeId(new GenericInstance(tupleStructId, checkedTypes));

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedTupleExpression(
                        checkedItems,
                        te.Span,
                        unifiedTypeId),
                    error);
            }

            case ParsedIndexedExpression ie: {

                var (checkedExpr, typeCheckExprErr) = TypeCheckExpression(ie.Expression, scopeId, project, safetyMode, null);
                
                error = error ?? typeCheckExprErr;

                var (checkedIdx, typeCheckIdxErr) = TypeCheckExpression(ie.Index, scopeId, project, safetyMode, null);
            
                error = error ?? typeCheckIdxErr;

                var exprType = Compiler.UnknownTypeId;

                var arrayStructId = project.GetArrayStructId(ie.Span);

                var dictStructId = project.GetDictionaryStrutId(ie.Span);

                var type = project.Types[checkedExpr.GetTypeId(scopeId, project)];

                switch (type) {

                    case GenericInstance ga when ga.StructId == arrayStructId: {

                        var _chkIdx = checkedIdx.GetTypeId(scopeId, project);

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

                        var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, exprType);

                        error = error ?? unifyErr;

                        return (
                            new CheckedIndexedExpression(
                                checkedExpr,
                                checkedIdx,
                                ie.Span,
                                unifiedTypeId),
                            error);
                    }

                    case GenericInstance gd when gd.StructId == dictStructId: {

                        exprType = gd.TypeIds[1];

                        var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, exprType);

                        error = error ?? unifyErr;

                        return (
                            new CheckedIndexedDictionaryExpression(
                                checkedExpr, 
                                checkedIdx, 
                                ie.Span,
                                unifiedTypeId),
                            error);
                    }

                    default: {

                        error = error ??
                            new TypeCheckError(
                                "index used on value that can't be indexed",
                                ie.Expression.GetSpan());

                        var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, exprType);

                        error = error ?? unifyErr;

                        return (
                            new CheckedIndexedExpression(
                                checkedExpr,
                                checkedIdx,
                                ie.Span,
                                unifiedTypeId),
                            error);
                    }
                }
            }

            case ParsedIndexedTupleExpression ite: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ite.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                var typeId = Compiler.UnknownTypeId;

                var tupleStructId = project.GetTupleStructId(ite.Span);

                var checkedExprType = project.Types[checkedExpr.GetTypeId(scopeId, project)];

                switch (checkedExprType) {

                    case GenericInstance gi when gi.StructId == tupleStructId: {

                        var idx = ToInt32(ite.Index);

                        switch (true) {

                            case var _ when gi.TypeIds.Count > idx: {

                                typeId = gi.TypeIds[idx];

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

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = error ?? unifyErr;

                return (
                    new CheckedIndexedTupleExpression(
                        checkedExpr, 
                        ite.Index, 
                        ite.Span, 
                        unifiedTypeId),
                    error);
            }

            case ParsedWhenExpression we: {

                var (checkedExpr, err) = TypeCheckExpression(we.Expression, scopeId, project, safetyMode, null);

                error = error ?? err;

                var checkedCases = new List<CheckedWhenCase>();

                var type = project.Types[checkedExpr.GetTypeId(scopeId, project)];

                var subjectTypeId = checkedExpr.GetTypeId(scopeId, project);

                var genericParams = new Dictionary<Int32, Int32>();

                if (type is GenericEnumInstance gei) {

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

                Int32? finalResultType = null;

                var allVariantsAreConstants = true;

                ///

                Int32? tyEnumId = null;

                switch (type) {

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

                        var seenCatchAll = false;

                        Span? catchAllSpan = null;

                        var coveredVariants = new HashSet<String>();

                        foreach (var c in we.Cases) {

                            switch (c) {

                                case CatchAllWhenCase cawc: {

                                    if (seenCatchAll) {

                                        error = error ??
                                            new TypeCheckError(
                                                "multiple catch-all cases in match are not allowed",
                                                cawc.MarkerSpan);
                                    }
                                    else {

                                        catchAllSpan = cawc.MarkerSpan;

                                        seenCatchAll = true;
                                    }

                                    var (body, err2) = TypeCheckWhenBody(
                                        cawc.Body,
                                        scopeId,
                                        project,
                                        safetyMode,
                                        genericParams,
                                        ref finalResultType,
                                        we.Span);

                                    error = error ?? err2;

                                    checkedCases.Add(new CheckedCatchAllWhenCase(body));

                                    break;
                                }

                                case ExpressionWhenCase ewc: {

                                    var (checkedMatchedExpr, err3) = TypeCheckExpression(
                                        ewc.MatchedExpression, 
                                        scopeId, 
                                        project, 
                                        safetyMode, 
                                        subjectTypeId);

                                    error = error ?? err3;

                                    if (checkedMatchedExpr.ToNumberConstant() == null) {

                                        allVariantsAreConstants = false;
                                    }

                                    // FIXME: In the future, we should really make this a "does it satisfy some trait" check.
                                    //        For now, we just check that the types are equal.

                                    if (CheckTypesForCompat(
                                        checkedMatchedExpr.GetTypeId(scopeId, project),
                                        subjectTypeId,
                                        genericParams,
                                        ewc.MarkerSpan,
                                        project) is Error err4) {

                                        error = error = err4;
                                    }

                                    var (body, err5) = TypeCheckWhenBody(
                                        ewc.Body,
                                        scopeId,
                                        project,
                                        safetyMode,
                                        genericParams,
                                        ref finalResultType,
                                        we.Span);

                                    error = error ?? err5;

                                    checkedCases.Add(new CheckedExpressionWhenCase(checkedMatchedExpr, body));

                                    break;
                                }

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

                                        continue;
                                    }

                                    Int32 variantIndex = 0;

                                    var vars = new List<(CheckedVariable, Span)>();

                                    // var _enum = project.Enums[enumId];

                                    var constructorName = name[1].Item1;

                                    var _variant = _enum.Variants.FirstOrDefault(v => {

                                        return v switch {

                                            CheckedWithValueEnumVariant w => w.Name == constructorName,
                                            CheckedUntypedEnumVariant u => u.Name == constructorName,
                                            CheckedTypedEnumVariant t => t.Name == constructorName,
                                            CheckedStructLikeEnumVariant s => s.Name == constructorName,
                                            _ => false
                                        };
                                    });

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
                                                    // FIXME: Figure this out.
                                                    Compiler.UnknownTypeId,
                                                    false),
                                                error);
                                        }

                                        case CheckedEnumVariant variant: {

                                            switch (variant) {

                                                case CheckedUntypedEnumVariant u: {

                                                    coveredVariants.Add(u.Name);

                                                    if (evwc.VariantArguments.Any()) {

                                                        error = error ?? 
                                                            new TypeCheckError(
                                                                $"when case '{u.Name}' cannot have arguments",
                                                                evwc.ArgumentsSpan);
                                                    }

                                                    break;
                                                }

                                                case CheckedTypedEnumVariant t: {

                                                    coveredVariants.Add(t.Name);

                                                    if (evwc.VariantArguments.Any()) {

                                                        if (evwc.VariantArguments.Count != 1) {

                                                            error = error ?? new TypeCheckError(
                                                                $"when case '{t.Name} must have exactly one argument",
                                                                evwc.ArgumentsSpan);
                                                        }

                                                        var _typeId = SubstituteTypeVarsInType(
                                                            t.TypeId,
                                                            genericParams,
                                                            project);

                                                        vars.Add((
                                                            new CheckedVariable(
                                                                name: evwc.VariantArguments[0].Item2,
                                                                _typeId,
                                                                mutable: false,
                                                                visibility: new PublicVisibility(),
                                                                definitionSpan: t.Span),
                                                            t.Span));
                                                    }

                                                    break;
                                                }

                                                case CheckedWithValueEnumVariant w: {

                                                    coveredVariants.Add(w.Name);

                                                    if (evwc.VariantArguments.Any()) {

                                                        error = error ?? 
                                                            new TypeCheckError(
                                                                $"when case '{w.Name}' cannot have arguments",
                                                                evwc.ArgumentsSpan);
                                                    }

                                                    break;
                                                }

                                                case CheckedStructLikeEnumVariant s: {

                                                    coveredVariants.Add(s.Name);

                                                    var variantName = s.Name;

                                                    var fields = s.Decls.ToList();

                                                    var namesSeen = new HashSet<String>();

                                                    foreach (var arg in evwc.VariantArguments) {

                                                        var _name = arg.Item1;

                                                        if (IsNullOrWhiteSpace(_name)) {

                                                            error = error ?? 
                                                                new TypeCheckError(
                                                                    $"when case argument '{arg.Item2}' for struct-like enum variant cannot be anonymous",
                                                                    evwc.ArgumentsSpan);
                                                                
                                                            continue;
                                                        }

                                                        if (namesSeen.Contains(_name)) {

                                                            error = error ?? 
                                                                new TypeCheckError(
                                                                    $"when case argument '{_name}' is already defined",
                                                                    evwc.ArgumentsSpan);
                                                                
                                                            continue;
                                                        }

                                                        namesSeen.Add(_name);

                                                        var fieldType = s
                                                            .Decls
                                                            .FirstOrDefault(f => f.Name == _name)?
                                                            .TypeId;

                                                        fieldType = fieldType switch {

                                                            Int32 ft => SubstituteTypeVarsInType(ft, genericParams, project),
                                                            _ => fieldType
                                                        };

                                                        switch (fieldType) {

                                                            case Int32 ft: {

                                                                vars.Add((
                                                                    new CheckedVariable(
                                                                        name: arg.Item2,
                                                                        typeId: ft,
                                                                        mutable: false,
                                                                        visibility: new PublicVisibility(),
                                                                        definitionSpan: we.Span),
                                                                    we.Span));
                                                                
                                                                break;
                                                            }

                                                            default: {

                                                                error = error ?? 
                                                                    new TypeCheckError(
                                                                        $"when case argument '{_name}' does not exist in struct-like enum variant '{variantName}'",
                                                                        evwc.ArgumentsSpan);

                                                                break;
                                                            }
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                        
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

                                    var newScopeId = project.CreateScope(scopeId);

                                    foreach (var (v, span) in vars) {

                                        if (project.AddVarToScope(newScopeId, v, span).Error is Error addVarErr) {

                                            error = error ?? addVarErr;
                                        }
                                    }

                                    switch (evwc.Body) {

                                        case ExpressionWhenBody e: {

                                            var (body, bodyErr) = TypeCheckExpression(
                                                e.Expression, 
                                                newScopeId, 
                                                project, 
                                                safetyMode, 
                                                finalResultType);

                                            error = error ?? bodyErr;

                                            switch (finalResultType) {

                                                case Int32 _frt: {

                                                    if (CheckTypesForCompat(
                                                        body.GetTypeId(scopeId, project),
                                                        _frt,
                                                        genericParams,
                                                        we.Span,
                                                        project) is Error compatErr) {

                                                        error = error ?? compatErr;
                                                    }

                                                    break;
                                                }

                                                default: {

                                                    finalResultType = body.GetTypeId(scopeId, project);

                                                    break;
                                                }
                                            }

                                            checkedCases.Add(
                                                new CheckedEnumVariantWhenCase(
                                                    variantName: name[1].Item1,
                                                    variantArguments: evwc.VariantArguments,
                                                    subjectTypeId: subjectTypeId,
                                                    variantIndex,
                                                    scopeId: newScopeId,
                                                    body: new CheckedExpressionWhenBody(body)));

                                            break;
                                        }

                                        case BlockWhenBody b: {

                                            var (body, bodyErr) = TypeCheckBlock(
                                                b.Block, 
                                                newScopeId, 
                                                project, 
                                                safetyMode);

                                            error = error ?? bodyErr;

                                            if (!body.DefinitelyReturns) {

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
                                            }


                                            checkedCases.Add(
                                                new CheckedEnumVariantWhenCase(
                                                    variantName: name[1].Item1,
                                                    variantArguments: evwc.VariantArguments,
                                                    subjectTypeId: subjectTypeId,
                                                    variantIndex,
                                                    scopeId: newScopeId,
                                                    body: new CheckedBlockWhenBody(body)));

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

                        // Check if all the variants are matched

                        var _enum2 = project.Enums[enumId];

                        var missingVariants = _enum2
                            .Variants
                            .Select(v => {
                                switch (v) {

                                    case CheckedWithValueEnumVariant w:
                                        return w.Name;
                                
                                    case CheckedUntypedEnumVariant u:
                                        return u.Name;

                                    case CheckedTypedEnumVariant t:
                                        return t.Name;

                                    case CheckedStructLikeEnumVariant s:
                                        return s.Name;

                                    default:
                                        throw new Exception();
                                }
                            })
                            .Where(name => !coveredVariants.Contains(name))
                            .ToList();

                        switch ((!missingVariants.Any(), seenCatchAll)) {

                            case (false, false): {

                                error = error ??
                                    new TypecheckErrorWithHint(
                                        $"match expression is not exhaustive, missing variants are: {Join(", ", missingVariants)}",
                                        we.Span,
                                        "add an irrefutable 'else' pattern or handle the missing variants",
                                        we.Expression.GetSpan());

                                break;
                            }

                            case (true, true): {

                                error = error ??
                                    new TypecheckErrorWithHint(
                                        "all variants are covered, but an irrefutable pattern is also present",
                                        we.Span,
                                        "remove this pattern",
                                        catchAllSpan ?? throw new Exception());

                                break;
                            }

                            default: {

                                break;
                            }
                        }

                        break;
                    }

                    default: {

                        var isEnumMatch = false;

                        var isValueMatch = false;

                        var seenCatchAll = false;

                        foreach (var _case in we.Cases) {

                            switch (_case) {

                                case EnumVariantWhenCase evwc: {

                                    if (isValueMatch) {

                                        error = error ??
                                            new TypeCheckError(
                                                "Cannot have an enum match case in a match expression containing value matches",
                                                evwc.MarkerSpan);
                                    }
                                    
                                    isEnumMatch = true;

                                    // We don't know what the enum type is, but we have the type var for it, so generate a generic enum match.
                                    // note that this will be fully checked when this match expression is actually instantiated.

                                    var (checkedBody, err2) = TypeCheckWhenBody(
                                        evwc.Body,
                                        scopeId,
                                        project,
                                        safetyMode,
                                        genericParams,
                                        ref finalResultType,
                                        evwc.MarkerSpan);
                                
                                    error = error ?? err2;

                                    if (finalResultType is Int32 typeId) {

                                        if (typeId == Compiler.UnknownTypeId) {

                                            finalResultType = null;
                                        }
                                    }

                                    checkedCases.Add(
                                        new CheckedEnumVariantWhenCase(
                                            variantName: evwc.VariantName.Last().Item1,
                                            variantArguments: evwc.VariantArguments,
                                            subjectTypeId,
                                            variantIndex: 0,
                                            scopeId,
                                            body: checkedBody));

                                    break;
                                }

                                case CatchAllWhenCase cawc: {

                                    if (seenCatchAll) {

                                        error = error ??
                                            new TypeCheckError(
                                                "Cannot have multiple catch-all match cases",
                                                cawc.MarkerSpan);
                                    }
                                    else {

                                        seenCatchAll = true;
                                    }

                                    CheckedWhenBody? body = null;

                                    switch (cawc.Body) {

                                        case BlockWhenBody block: {

                                            var (checkedBlock, err10) = TypeCheckBlock(block.Block, scopeId, project, safetyMode);

                                            error = error ?? err10;

                                            if (!checkedBlock.DefinitelyReturns) {

                                                switch (finalResultType) {

                                                    case Int32 _frt2: {

                                                        if (CheckTypesForCompat(
                                                            Compiler.VoidTypeId,
                                                            _frt2,
                                                            genericParams,
                                                            we.Span,
                                                            project) is Error err11) {

                                                            error = error ?? err11;
                                                        }

                                                        break;
                                                    }

                                                    default: {

                                                        finalResultType = Compiler.VoidTypeId;

                                                        break;
                                                    }
                                                }
                                            }

                                            body = new CheckedBlockWhenBody(checkedBlock);

                                            break;
                                        }

                                        case ExpressionWhenBody expr2: {

                                            var (_body, err8) = TypeCheckExpression(
                                                expr2.Expression,
                                                scopeId,
                                                project,
                                                safetyMode,
                                                finalResultType);

                                            error = error ?? err8;

                                            switch (finalResultType) {

                                                case Int32 _frt: {

                                                    if (CheckTypesForCompat(
                                                        _body.GetTypeId(scopeId, project),
                                                        _frt,
                                                        genericParams,
                                                        expr2.Expression.GetSpan(),
                                                        project) is Error err9) {

                                                        error = error ?? err9;
                                                    }

                                                    break;
                                                }

                                                default: {

                                                    finalResultType = _body.GetTypeId(scopeId, project);

                                                    break;
                                                }
                                            }

                                            body = new CheckedExpressionWhenBody(_body);

                                            break;
                                        }

                                        default: {

                                            throw new Exception();
                                        }
                                    }

                                    checkedCases.Add(new CheckedCatchAllWhenCase(body));

                                    break;
                                }

                                case ExpressionWhenCase ewc: {

                                    if (isEnumMatch) {

                                        error = error ??
                                            new TypeCheckError(
                                                "Cannot have a value match case in a match expression containing enum matches",
                                                ewc.MarkerSpan);

                                        continue;
                                    }

                                    isValueMatch = true;

                                    var (checkedExpr2, err5) = TypeCheckExpression(
                                        ewc.MatchedExpression,
                                        scopeId,
                                        project,
                                        safetyMode,
                                        subjectTypeId);

                                    error = error ?? err5;

                                    if (checkedExpr2.ToNumberConstant() == null) {

                                        allVariantsAreConstants = false;
                                    }

                                    // FIXME: In the future, we should really make this a "does it satisfy some trait" check.
                                    //        For now, we just check that the types are equal.

                                    if (CheckTypesForCompat(
                                        checkedExpr2.GetTypeId(scopeId, project),
                                        subjectTypeId,
                                        genericParams,
                                        ewc.MarkerSpan,
                                        project) is Error err6) {

                                        error = error ?? err6;
                                    }

                                    var (body, err7) = TypeCheckWhenBody(
                                        ewc.Body,
                                        scopeId,
                                        project,
                                        safetyMode,
                                        genericParams,
                                        ref finalResultType,
                                        ewc.MarkerSpan);

                                    error = error ?? err7;

                                    checkedCases.Add(
                                        new CheckedExpressionWhenCase(
                                            checkedExpr2,
                                            body));

                                    break;
                                }

                                default: {

                                    throw new Exception();
                                }
                            }
                        }

                        if (isValueMatch && !seenCatchAll) {

                            error = error ?? 
                                new TypeCheckError(
                                    "match expression is not exhaustive, a value match must contain an irrefutable 'else' pattern",
                                    we.Span);
                        }

                        break;
                    }
                }

                if (finalResultType is Int32 frt) {

                    var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, frt);

                    error = error ?? unifyErr;

                    finalResultType = unifiedTypeId;
                }

                return (
                    new CheckedWhenExpression(
                        checkedExpr,
                        checkedCases,
                        we.Span,
                        finalResultType ?? Compiler.VoidTypeId,
                        allVariantsAreConstants),
                    error);
            }

            case ParsedIndexedStructExpression ise: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(ise.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                var typeId = Compiler.UnknownTypeId;

                var checkedExprTypeId = checkedExpr.GetTypeId(scopeId, project);
                
                var checkedExprType = project.Types[checkedExprTypeId];

                Int32? _structId = checkedExprType switch {

                    GenericInstance gi => gi.StructId,
                    StructType st => st.StructId,
                    _ => null
                };

                switch (_structId) {

                    case Int32 structId: {

                        var structure = project.Structs[structId];

                        foreach (var member in structure.Fields) {

                            if (member.Name == ise.Name) {

                                var resolvedTypeId = ResolveTypeVar(member.TypeId, scopeId, project);

                                var (unifiedType, _) = unifyWithTypeHint(project, resolvedTypeId);

                                var _structure = project.Structs[structId];

                                return (
                                    new CheckedIndexedStructExpression(
                                        checkedExpr,
                                        ise.Name,
                                        ise.Span,
                                        unifiedType),
                                    CheckAccessibility(
                                        scopeId, 
                                        _structure.ScopeId, 
                                        member, 
                                        ise.Span, 
                                        project));
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
                                $"member access on value of non-struct type {project.TypeNameForTypeId(checkedExprTypeId)} is not allowed",
                                ise.Span);

                        break;
                    }
                }

                var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                error = unifyErr;

                return (
                    new CheckedIndexedStructExpression(
                        checkedExpr, 
                        ise.Name, 
                        ise.Span, 
                        unifiedTypeId),
                    error);
            }

            case ParsedMethodCallExpression mce: {

                var (checkedExpr, chkExprErr) = TypeCheckExpression(mce.Expression, scopeId, project, safetyMode, null);

                error = error ?? chkExprErr;

                if (checkedExpr.GetTypeId(scopeId, project) == Compiler.StringTypeId) {

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

                            var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, checkedCall.TypeId);

                            error = error ?? unifyErr;

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifiedTypeId),
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

                    var checkedExprType = project.Types[checkedExpr.GetTypeId(scopeId, project)];

                    switch (checkedExprType) {

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

                            var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, checkedCall.TypeId);

                            error = error ?? unifyErr;

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifiedTypeId),
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

                            var typeId = checkedCall.TypeId;

                            var (unifiedTypeId, unifyErr) = unifyWithTypeHint(project, typeId);

                            error = error ?? unifyErr;

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr, 
                                    checkedCall, 
                                    mce.Span, 
                                    unifiedTypeId),
                                error);
                        }

                        default: {

                            error = error ??
                                new TypeCheckError(
                                    $"no methods available on value (type: {checkedExpr.GetTypeId(scopeId, project)})",
                                    mce.Expression.GetSpan());

                            // Generate something usable, in case we're generating generic code.

                            var checkedArgs = new List<(String, CheckedExpression)>();

                            foreach (var arg in mce.Call.Args) {

                                var (checkedArg, argErr) = TypeCheckExpression(
                                    arg.Item2,
                                    scopeId,
                                    project,
                                    safetyMode,
                                    null);

                                error = error ?? argErr;

                                checkedArgs.Add((arg.Item1, checkedArg));
                            }

                            return (
                                new CheckedMethodCallExpression(
                                    checkedExpr,
                                    new CheckedCall(
                                        ns: new List<ResolvedNamespace>(),
                                        name: mce.Call.Name,
                                        calleeThrows: false,
                                        args: checkedArgs,
                                        typeArgs: new List<Int32>(),
                                        linkage: FunctionLinkage.Internal,
                                        typeId: Compiler.UnknownTypeId),
                                    mce.Span,
                                    Compiler.UnknownTypeId),
                                error);
                        }
                    }
                }
            }

            case ParsedOperatorExpression e: {

                return (
                    new CheckedGarbageExpression(e.Span),
                    new TypeCheckError(
                        "garbage in expression", 
                        e.Span));
            }

            case ParsedGarbageExpression e: {

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
        Int32 scopeId,
        Project project,
        SafetyMode safetyMode) {
    
        var exprTypeId = expr.GetTypeId(scopeId, project);

        var exprType = project.Types[exprTypeId];

        switch (op) {

            case CheckedIsUnaryOperator i: {

                return (
                    new CheckedUnaryOpExpression(
                        expr, 
                        new CheckedIsUnaryOperator(i.TypeId), 
                        span, 
                        Compiler.BoolTypeId),
                    null);
            }
            
            case CheckedTypeCastUnaryOperator tc: {

                return (
                    new CheckedUnaryOpExpression(
                        expr, 
                        op, 
                        span, 
                        tc.TypeCast.GetTypeId(scopeId, project)),
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

                        // FIXME: This at least allows us to check out-of-bounds constants at compile time.
                        //        We should expand it to check any compile-time known value.

                        if (expr is CheckedNumericConstantExpression nce) {

                            // Flipping the sign on a small enough unsigned constant is fine. We'll change the type to the signed variant.

                            if (NeuTypeFunctions.IsInteger(nce.Type) && !NeuTypeFunctions.IsSigned(nce.Type)) {

                                // FIXME: What about integer types whose signedness we can't yet flip?

                                var flippedSignType = NeuTypeFunctions.FlipSignedness(nce.Type) ?? throw new Exception();

                                var negativeValue = 0 - nce.Value.NumberConstant()?.ToBigInteger() ?? throw new Exception();

                                long? n = null;

                                try {

                                    n = (long) negativeValue;
                                }
                                catch { }

                                if ((n is not null && !NeuTypeFunctions.CanFitNumber(flippedSignType, new SignedNumberConstant(n.Value)))
                                    // This is the case if the above "as Int64" overflows.
                                    || negativeValue < Int64.MinValue) {

                                    return (
                                        new CheckedGarbageExpression(span),
                                        new TypeCheckError(
                                            $"Literal {nce.Value.NumberConstant()!.ToString()} too small for unsigned integer type {nce.Type}", 
                                            span));
                                }
                                else {

                                    NumericConstant c = flippedSignType switch {

                                        Compiler.Int8TypeId => new Int8Constant((sbyte) negativeValue),
                                        Compiler.Int16TypeId => new Int16Constant((short) negativeValue),
                                        Compiler.Int32TypeId => new Int32Constant((int) negativeValue),
                                        Compiler.Int64TypeId => new Int64Constant((long) negativeValue),
                                        
                                        Compiler.IntTypeId => new IntConstant((long) negativeValue),

                                        Compiler.UInt8TypeId => new UInt8Constant((byte) negativeValue),
                                        Compiler.UInt16TypeId => new UInt16Constant((ushort) negativeValue),
                                        Compiler.UInt32TypeId => new UInt32Constant((uint) negativeValue),
                                        Compiler.UInt64TypeId => new UInt64Constant((ulong) negativeValue),
                                        
                                        Compiler.UIntTypeId => new UIntConstant((ulong) negativeValue),

                                        _ => throw new Exception()
                                    };

                                    return (
                                        new CheckedNumericConstantExpression(c, span, flippedSignType),
                                        null);
                                }
                            }
                            else {

                                return (
                                    new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), span, exprTypeId),
                                    null);
                            }
                        }
                        else {

                            return (
                                new CheckedUnaryOpExpression(expr, new CheckedNegateUnaryOperator(), span, exprTypeId),
                                null);
                        }
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
                    case Compiler.DoubleTypeId:
                    case Compiler.UIntTypeId:
                    case Compiler.IntTypeId:
                    case Compiler.CCharTypeId:
                    case Compiler.CIntTypeId: {

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

    public static (Int32, Error?) UnifyWithType(
        Int32 expectedType,
        Int32 foundType,
        Span span,
        Project project) {

        var genericInferences = new Dictionary<Int32, Int32>();

        var err = CheckTypesForCompat(
            expectedType, 
            foundType, 
            genericInferences, 
            span, 
            project);

        return (expectedType, err);
    }

    public static (Int32, Error?) TypeCheckBinaryOperation(
        CheckedExpression lhs,
        BinaryOperator op,
        CheckedExpression rhs,
        Int32 scopeId,
        Span span,
        Project project) {

        var lhsTypeId = lhs.GetTypeId(scopeId, project);
        var rhsTypeId = rhs.GetTypeId(scopeId, project);

        var typeId = lhs.GetTypeId(scopeId, project);

        switch (op) {

            case BinaryOperator.LessThan:
            case BinaryOperator.LessThanOrEqual:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.GreaterThanOrEqual:
            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual: {

                if (lhsTypeId != rhsTypeId) {

                    return (
                        lhsTypeId,
                        new TypeCheckError(
                            $"Binary comparison operation between incompatible types ('{project.TypeNameForTypeId(lhsTypeId)}' and '{project.TypeNameForTypeId(rhsTypeId)}')",
                            span));
                }

                typeId = Compiler.BoolTypeId;

                break;
            }

            case BinaryOperator.LogicalAnd:
            case BinaryOperator.LogicalOr: {

                if (lhsTypeId != Compiler.BoolTypeId) {

                    return (
                        lhsTypeId,
                        new TypeCheckError(
                            "left side of logical binary operation is not a boolean",
                            span));
                }

                if (rhsTypeId != Compiler.BoolTypeId) {

                    return (
                        rhsTypeId,
                        new TypeCheckError(
                            "right side of logical binary operation is not a boolean",
                            span));
                }

                typeId = Compiler.BoolTypeId;

                break;
            }

            case BinaryOperator.Assign: {

                if (!lhs.IsMutable()) {

                    return (
                        lhsTypeId,
                        new TypeCheckError(
                            "assignment to immutable variable",
                            span));
                }

                if (rhs is CheckedOptionalNoneExpression) {

                    // if the right expression is None then the left expression must be an optional
                    var lhsType1 = project.Types[lhsTypeId];
                    
                    var optionalStructId1 = project
                        .FindStructInScope(0, "Optional")
                        ?? throw new Exception("internal error: can't find builtin Optional type");

                    if (lhsType1 is GenericInstance gi) {
                        
                        if (gi.StructId == optionalStructId1) {
                            
                            return (lhsTypeId, null);
                        }
                    }
                }

                var (typeId1, err) = UnifyWithType(lhsTypeId, rhsTypeId, rhs.GetSpan(), project);

                if (err is Error e) {

                    return (
                        typeId1,
                        new TypeCheckError(
                            $"Assignment between incompatible types ('{project.TypeNameForTypeId(lhsTypeId)}' and '{project.TypeNameForTypeId(rhsTypeId)}')",
                            span));
                }
                else {

                    return (typeId1, null);
                }
            }

            case BinaryOperator.AddAssign:
            case BinaryOperator.SubtractAssign:
            case BinaryOperator.MultiplyAssign:
            case BinaryOperator.DivideAssign:    
            case BinaryOperator.ModuloAssign:
            case BinaryOperator.BitwiseAndAssign:
            case BinaryOperator.BitwiseOrAssign:
            case BinaryOperator.BitwiseXorAssign:
            case BinaryOperator.BitwiseLeftShiftAssign:
            case BinaryOperator.BitwiseRightShiftAssign: {

                // This branch can be the same as the above BinaryOp.Assign branch.
                // unify_with_type uses check_types_for_compact which does the same
                // as below.

                var weakPointerStructId = project.GetWeakPointerStructId(span);

                if (project.Types[lhsTypeId] is GenericInstance gi) {

                    if (gi.StructId == weakPointerStructId) {

                        var innerTypeId = gi.TypeIds[0];

                        if (project.Types[innerTypeId] is StructType lhsStructTy) {

                            switch (project.Types[rhsTypeId]) {

                                case StructType rhsStructTy when lhsStructTy.StructId == rhsStructTy.StructId: {

                                    return (lhsTypeId, null);                                    
                                }

                                default: {

                                    break;
                                }
                            }
                        }
                    }
                }

                if (lhsTypeId != rhsTypeId) {

                    return (
                        lhsTypeId,
                        new TypeCheckError(
                            $"Assignment between incompatible types ('{project.TypeNameForTypeId(lhsTypeId)}' and '{project.TypeNameForTypeId(rhsTypeId)}')",
                            span));
                }

                if (!lhs.IsMutable()) {

                    return (
                        lhsTypeId, 
                        new TypeCheckError(
                            "assignment to immutable variable", 
                            span));
                }

                break;
            }

            case BinaryOperator.Add:
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
            case BinaryOperator.Modulo: {

                if (lhsTypeId != rhsTypeId) {

                    return (
                        lhsTypeId,
                        new TypeCheckError(
                            $"Binary arithmetic operation between incompatible types ('{project.TypeNameForTypeId(lhsTypeId)}' and '{project.TypeNameForTypeId(rhsTypeId)}')",
                            span));
                }

                typeId = lhsTypeId;

                break;
            }

            default: {

                break;
            }
        }

        return (typeId, null);
    }

    public static (Int32?, Error?) ResolveCall(
        ParsedCall call,
        List<ResolvedNamespace> namespaces,
        Span span,
        Int32 scopeId,
        Project project) {

        Int32? calleeId = null;
        
        Error? error = null;

        if (call.Namespace.FirstOrDefault() is String ns) {

            // For now, assume class is our namespace
            // In the future, we'll have real namespaces

            if (project.FindStructInScope(scopeId, ns) is Int32 structId) {

                var structure = project.Structs[structId];

                // Look for the constructor

                if (project.FindStructInScope(structure.ScopeId, call.Name) is Int32 _structId) {

                    var _structure = project.Structs[_structId];

                    if (project.FindFuncInScope(_structure.ScopeId, call.Name) is Int32 _funcId) {

                        calleeId = _funcId;
                    }
                }
                else if (project.FindFuncInScope(structure.ScopeId, call.Name) is Int32 funcId1) {

                    calleeId = funcId1;
                }

                if (structure.GenericParameters.Any()) {

                    namespaces[0].GenericParameters = structure.GenericParameters;
                }

                return (calleeId, error);
            }
            else if (project.FindEnumInScope(scopeId, ns) is Int32 enumId) {
                
                var _enum = project.Enums[enumId];

                if (project.FindFuncInScope(_enum.ScopeId, call.Name) is Int32 funcId) {

                    calleeId = funcId;
                }

                if (_enum.GenericParameters.Any()) {

                    namespaces[0].GenericParameters = _enum.GenericParameters;
                }

                return (calleeId, error);
            }
            else if (project.FindNamespaceInScope(scopeId, ns)is Int32 namespaceId) {

                if (project.FindStructInScope(namespaceId, call.Name) is Int32 nsStructId) {

                    var structure = project.Structs[nsStructId];

                    if (project.FindFuncInScope(structure.ScopeId, call.Name) is Int32 nsStructFuncId) {

                        calleeId = nsStructFuncId;
                    }
                }
                else if (project.FindFuncInScope(namespaceId, call.Name) is Int32 nsFuncId) {

                    calleeId = nsFuncId;
                }

                return (calleeId, error);
            }
            else if (project.FindFuncInScope(scopeId, call.Name) is Int32 funcId2) {

                calleeId = funcId2;

                return (calleeId, error);
            }
            else {

                error = error ?? 
                    new TypeCheckError(
                        $"unknown namespace or class: {ns}",
                        span);

                return (calleeId, error);
            }
        }
        else {

            // FIXME: Support function overloading.

            // Look for the constructor

            if (project.FindStructInScope(scopeId, call.Name) is Int32 _structId) {

                var _structure = project.Structs[_structId];

                if (project.FindFuncInScope(_structure.ScopeId, call.Name) is Int32 _funcId) {

                    calleeId = _funcId;
                }
            }
            else if (project.FindFuncInScope(scopeId, call.Name) is Int32 funcId3) {

                calleeId = funcId3;
            }
            
            if (calleeId == null) {

                error = error ?? 
                    new TypeCheckError(
                        $"call to unknown function: {call.Name}", 
                        span);
            }

            return (calleeId, error);
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

        var returnTypeId = Compiler.UnknownTypeId;

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

        Int32? genericCheckedFunctionToInstantiate = null;

        Int32? maybeThisTypeId = null;

        switch (call.Name) {

            case "print" when structId == null:
            case "printLine" when structId == null:
            case "warnLine" when structId == null: {

                // FIXME: This is a hack since printLine() and warnLine() are hard-coded into codegen at the moment

                foreach (var arg in call.Args) {

                    var (checkedArg, checkedArgErr) = TypeCheckExpression(arg.Item2, callerScopeId, project, safetyMode, null);

                    error = error ?? checkedArgErr;

                    var resultTypeId = SubstituteTypeVarsInType(
                        checkedArg.GetTypeId(calleeScopeId, project),
                        genericSubstitutions, 
                        project);

                    if (resultTypeId == Compiler.VoidTypeId) {

                        error = error ??
                            new TypeCheckError(
                                "printLine/warnLine can't take void values",
                                span);
                    }

                    returnTypeId = Compiler.VoidTypeId;

                    checkedArgs.Add((arg.Item1, checkedArg));
                }

                break;
            }

            ///

            default: {

                var (_calleeId, resolveErr) = ResolveCall(
                    call,
                    resolvedNamespaces,
                    span,
                    calleeScopeId, 
                    project);

                error = error ?? resolveErr;

                if (_calleeId is Int32 calleeId) {

                    var callee = project.Functions[calleeId];

                    if (!callee.IsInstantiated) {

                        genericCheckedFunctionToInstantiate = calleeId;
                    }

                    calleeThrows = callee.Throws;

                    returnTypeId = callee.ReturnTypeId;

                    linkage = callee.Linkage;

                    // Make sure we are allowed to access this method

                    var accessibilityError = CheckAccessibility(
                        callerScopeId, 
                        callee.FuncScopeId, 
                        callee, 
                        span, 
                        project);

                    error = error ?? accessibilityError;

                    // If the user gave us explicit type arguments, let's use them in our substitutions

                    for (var idx = 0; idx < call.TypeArgs.Count; idx++) {

                        var typeArg = call.TypeArgs[idx];

                        var (checkedTypeArg, argErr) = TypeCheckTypeName(typeArg, callerScopeId, project);

                        error = error ?? argErr;

                        if (callee.GenericParameters.Count <= idx) {

                            error = error ?? 
                                new TypeCheckError(
                                    "Trying to access generic parameter out of bounds",
                                    span);

                            continue;
                        }

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

                        var typeId = _thisExpr.GetTypeId(calleeScopeId, project);

                        maybeThisTypeId = typeId;

                        var paramType = project.Types[typeId];

                        if (paramType is GenericInstance gi) {

                            var structure = project.Structs[gi.StructId];

                            var idx = 0;

                            while (idx < structure.GenericParameters.Count) {

                                genericSubstitutions[structure.GenericParameters[idx]] = gi.TypeIds[idx];

                                idx += 1;
                            }
                        }
                    
                        // Make sure that our call doesn't have a 'this' pointer to a static callee

                        if (callee.IsStatic()) {

                            error = error ?? 
                                new TypeCheckError(
                                    "Cannot call static method on an instance of an object",
                                    span);
                        }

                        if (callee.IsMutating() && !thisExpr.IsMutable()) {

                            error = error ?? 
                                new TypeCheckError(
                                    "Cannot call mutating method on an immutable object instance",
                                    span);
                        }
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

                            var param = project.Functions[calleeId].Parameters[idx + argOffset];

                            var _typeHint = param.Variable.TypeId;

                            var (checkedArg, checkedArgErr) = TypeCheckExpression(
                                call.Args[idx].Item2, 
                                callerScopeId, 
                                project, 
                                safetyMode,
                                _typeHint);

                            error = error ?? checkedArgErr;

                            var (_calleeId2, _) = ResolveCall(
                                call, 
                                resolvedNamespaces,
                                span, 
                                calleeScopeId, 
                                project); // need to do something with defType here?

                            var calleeId2 = _calleeId2 ??
                                throw new Exception("internal error: previously resolved call is now unresolved");

                            var callee2 = project.Functions[calleeId2];

                            if (call.Args[idx].Item2 is ParsedVarExpression ve) {

                                if (ve.Value != callee2.Parameters[idx + argOffset].Variable.Name
                                    && callee2.Parameters[idx + argOffset].RequiresLabel
                                    && call.Args[idx].Item1 != callee2.Parameters[idx + argOffset].Variable.Name) {

                                    error = error ?? 
                                        new TypeCheckError(
                                            "Wrong parameter name in argument label",
                                            call.Args[idx].Item2.GetSpan());
                                }
                            }
                            else if (callee2.Parameters[idx + argOffset].RequiresLabel
                                && call.Args[idx].Item1 != callee2.Parameters[idx + argOffset].Variable.Name) {

                                error = error ?? 
                                    new TypeCheckError(
                                        "Wrong parameter name in argument label",
                                        call.Args[idx].Item2.GetSpan());
                            }

                            var lhsTypeId = callee2.Parameters[idx + argOffset].Variable.TypeId;

                            var (promoted, promoteErr) = TryPromoteConstantExprToType(lhsTypeId, checkedArg, span);

                            error = error ?? promoteErr;

                            lhsTypeId = callee2.Parameters[idx + argOffset].Variable.TypeId;

                            if (promoted is not null) {

                                checkedArg = promoted;
                            }

                            var rhsTypeId = checkedArg.GetTypeId(calleeScopeId, project);

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
                            returnTypeId,
                            th,
                            genericSubstitutions,
                            span,
                            project);
                    }

                    returnTypeId = SubstituteTypeVarsInType(returnTypeId, genericSubstitutions, project);

                    resolvedNamespaces = resolvedNamespaces
                        .Select(n =>
                            new ResolvedNamespace(
                                name: n.Name,
                                genericParameters: 
                                    n
                                    .GenericParameters?
                                    .Select(typeId => {
                                        return SubstituteTypeVarsInType(typeId, genericSubstitutions, project);
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

        if (genericCheckedFunctionToInstantiate is Int32 _funcId) {

            // Clear the generic parameters and typecheck in the fully specialised scope.

            if (maybeThisTypeId is Int32 id) {

                maybeThisTypeId = SubstituteTypeVarsInType(id, genericSubstitutions, project);
            }

            var err = TypeCheckAndSpecializeGenericFunction(
                _funcId,
                typeArgs,
                calleeScopeId,
                maybeThisTypeId,
                genericSubstitutions,
                project);

            error = error ?? err;
        }

        return (
            new CheckedCall(
                ns: resolvedNamespaces,
                call.Name, 
                calleeThrows,
                checkedArgs,
                typeArgs,
                linkage,
                returnTypeId
                // ,
                // calleDefType
                ),
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

        var type = project.Types[typeId];

        switch (type) {

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

        var optionalStructId = project.GetOptionalStructId(span);

        var weakPointerStructId = project.GetWeakPointerStructId(span);

        // This skips the type compatibility check if assigning a T to a T? or to a
        // weak T? without going through `Some`

        if (lhsType is GenericInstance _gi) {

            if ((_gi.StructId == optionalStructId || _gi.StructId == weakPointerStructId)
                && _gi.TypeIds.Any(argId => argId == rhsTypeId)) {

                return null;
            }
        }

        switch (lhsType) {

            case TypeVariable _: {

                // If the call expects a generic type variable, let's see if we've already seen it
                
                if (genericInferences.ContainsKey(lhsTypeId)) {

                    // We've seen this type variable assigned something before
                    // we should error if it's incompatible.

                    if (rhsTypeId != genericInferences[lhsTypeId]) {

                        error = error ?? 
                            new TypeCheckError(
                                $"Type mismatch: expected '{project.TypeNameForTypeId(genericInferences[lhsTypeId])}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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
                                    $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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
                        else {

                            error = error ??
                                new TypeCheckError(
                                    $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
                                    span);
                        }

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {

                            // They're the same type, might be okay to just leave now
                            
                            error = error ??
                                new TypeCheckError(
                                    $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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
                                    $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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

                    case GenericInstance gi when st.StructId == gi.StructId: {

                        // Same struct, perhaps this is an instantiation of it

                        var lhsStruct = project.Structs[st.StructId];

                        if (gi.TypeIds.Count != lhsStruct.GenericParameters.Count) {

                            return new TypeCheckError(
                                $"mismatched number of generic parameters for {lhsStruct.Name}",
                                span);
                        }

                        var idx = 0;

                        var lhsArgTypeId = lhsStruct.GenericParameters[idx];

                        var rhsArgTypeId = gi.TypeIds[idx];

                        while (idx < gi.TypeIds.Count) {

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

                        break;
                    }

                    default: {

                        if (rhsTypeId != lhsTypeId) {
                            
                            // They're the same type, might be okay to just leave now
                            
                            error = error ??
                                new TypeCheckError(
                                    $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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
                            $"Type mismatch: expected '{project.TypeNameForTypeId(lhsTypeId)}', but got '{project.TypeNameForTypeId(rhsTypeId)}'",
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

            case ParsedTupleType t: {

                var checkedTypes = new List<Int32>();

                foreach (var innerType in t.Types) {

                    var (_typeId, err) = TypeCheckTypeName(innerType, scopeId, project);

                    error = error ?? err;

                    checkedTypes.Add(_typeId);
                }

                var tupleStructId = project.GetTupleStructId(t.Span);

                // FIXME: Tuple is not a generic type since we don't have variadic generics yet, however
                // we don't actually check if the stuct_id is actually generic or not, so the type checking
                // works as expected for now

                var typeId =
                    project.FindOrAddTypeId(new GenericInstance(tupleStructId, checkedTypes));

                return (typeId, error);
            }

            case ParsedArrayType vt: {

                var (innerType, innerTypeErr) = TypeCheckTypeName(vt.Type, scopeId, project);

                error = error ?? innerTypeErr;

                var vectorStructId = project.GetArrayStructId(vt.Span);

                var typeId = project.FindOrAddTypeId(new GenericInstance(vectorStructId, new List<Int32>(new [] { innerType })));

                return (
                    typeId,
                    error);
            }

            case ParsedDictionaryType dt: {

                var (keyType, keyErr) = TypeCheckTypeName(dt.Key, scopeId, project);

                error = error ?? keyErr;

                var (valueType, valueErr) = TypeCheckTypeName(dt.Value, scopeId, project);

                error = error ?? valueErr;

                var dictStructId = project.GetDictionaryStrutId(dt.Span);

                var typeId = project.FindOrAddTypeId(
                    new GenericInstance(
                        dictStructId,
                        new List<Int32>(new [] { keyType, valueType })));

                return (
                    typeId,
                    error);
            }

            case ParsedSetType st: {

                var (innerTypeId, err) = TypeCheckTypeName(st.Type, scopeId, project);

                error = error ?? err;

                var setStructId = project.GetSetStructId(st.Span);

                var typeId = project.FindOrAddTypeId(new GenericInstance(setStructId, new List<Int32>(new [] { innerTypeId })));

                return (typeId, error);
            }

            case ParsedOptionalType opt: {

                var (innerType, err) = TypeCheckTypeName(opt.Type, scopeId, project);

                error = error ?? err;

                var optionalStructId = project.GetOptionalStructId(opt.Span);

                var typeId = project.FindOrAddTypeId(new GenericInstance(optionalStructId, new List<Int32>(new [] { innerType })));

                return (
                    typeId,
                    error);
            }

            case ParsedWeakPointerType wp: {

                var (innerTypeId, err) = TypeCheckTypeName(wp.Type, scopeId, project);

                error = error ?? err;

                var weakPointerStructId = project.GetWeakPointerStructId(wp.Span);

                var typeId = project
                    .FindOrAddTypeId(new GenericInstance(weakPointerStructId, new List<Int32>(new [] { innerTypeId })));

                return (
                    typeId, 
                    error);
            }

            case ParsedRawPointerType rp: {

                var (innerType, err) = TypeCheckTypeName(rp.Type, scopeId, project);

                error = error ?? err;

                var typeId = project
                    .FindOrAddTypeId(new RawPointerType(innerType));

                return (
                    typeId,
                    error);
            }

            case ParsedGenericType gt: {

                var checkedInnerTypes = new List<Int32>();

                foreach (var innerType in gt.Types) {

                    var (innerTypeId, innerTypeNameErr) = TypeCheckTypeName(innerType, scopeId, project);

                    error = error ?? innerTypeNameErr;

                    checkedInnerTypes.Add(innerTypeId);
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
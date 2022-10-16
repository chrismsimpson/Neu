
#pragma once

#include <stdint.h>

#include "AllOf.h"
#include "AnyOf.h"
#include "Array.h"
#include "Assertions.h"
#include "Atomic.h"
#include "Badge.h"
#include "BitCast.h"
#include "BuiltinWrappers.h"
#include "ByteBuffer.h"
#include "CharacterTypes.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Concepts.h"
#include "Debug.h"
#include "Dictionary.h"
#include "Error.h"
#include "Find.h"
#include "FixedPoint.h"
#include "Format.h"
#include "Function.h"
#include "GenericLexer.h"
#include "HashFunctions.h"
#include "HashMap.h"
#include "HashTable.h"
#include "IntegralMath.h"
#include "IterationDecision.h"
#include "Iterator.h"
#include "LinearArray.h"
#include "Math.h"
#include "Memory.h"
#include "NonCopyable.h"
#include "NonNullOwnPointer.h"
#include "NonNullRefPointer.h"
#include "NumericLimits.h"
#include "Optional.h"
#include "OwnPointer.h"
#include "Platform.h"
#include "RefCounted.h"
#include "RefPointer.h"
#include "Result.h"
#include "ReverseIterator.h"
#include "ScopeGuard.h"
#include "Set.h"
#include "Span.h"
#include "Detail.h"
#include "std.h"
#include "String.h"
#include "StringBuilder.h"
#include "StringHash.h"
#include "StringImpl.h"
#include "StringUtils.h"
#include "StringView.h"
#include "Traits.h"
#include "Try.h"
#include "Tuple.h"
#include "TypeCasts.h"
#include "TypeList.h"
#include "TypedTransfer.h"
#include "Types.h"
#include "UnicodeUtils.h"
#include "Utf8View.h"
#include "Variant.h"
#include "Vector.h"
#include "WeakPointer.h"
#include "Weakable.h"
#include "kmalloc.h"
#include "kstdio.h"

#include "Format.cpp"
#include "GenericLexer.cpp"
#include "String.cpp"
#include "StringBuilder.cpp"
#include "StringImpl.cpp"
#include "StringUtils.cpp"
#include "StringView.cpp"
#include "Utf8View.cpp"

#include "File.h"

#include "File.cpp"

#ifdef NEU_CONTINUE_ON_PANIC
constexpr auto _neu_continue_on_panic = true;
#else
constexpr auto _neu_continue_on_panic = false;
#endif


using Float = float; // 32-bit
using Double = double; // 64-bit

template<typename T>
struct Range {

    using IndexType = T;

    T start { };

    T end { };
};

ErrorOr<int> _neu_main(Array<String>);

template<typename T>
inline constexpr T __arithmeticShiftRight(T value, size_t steps) {

    if constexpr (IsSigned<T>) {

        if constexpr (sizeof(T) == 1) {

            auto sign = (value & 0x80);
            
            // 8-bit variant
            
            return ((value >> steps) | sign);
        }
        else if constexpr (sizeof(T) == 2) {
            
            auto sign = (value & 0x8000);
            
            // 16-bit variant
            
            return ((value >> steps) | sign);
        }
        else if constexpr (sizeof(T) == 4) {
            
            auto sign = (value & 0x80000000);
            
            // 32-bit variant
            
            return ((value >> steps) | sign);
        }
        else if constexpr (sizeof(T) == 8) {
            
            auto sign = (value & 0x8000000000000000);
            
            // 64-bit variant
            
            return ((value >> steps) | sign);
        }
    } 
    else {

        return (value >> steps);
    }
}






















































































template<typename Value>
struct _NeuExplicitValue {

    _NeuExplicitValue(Value&& v)
        : value(move(v)) { }

    _NeuExplicitValue(Value const& v)
        : value(v) { }

    Value value;
};

template<>
struct _NeuExplicitValue<void> {

    _NeuExplicitValue() { }
};

template<typename Value, typename Return>
struct _NeuExplicitValueOrReturn {

    template<typename U>
    _NeuExplicitValueOrReturn(_NeuExplicitValue<U>&& v)
        : value(_NeuExplicitValue<Value> { move(v.value) }) { }

    _NeuExplicitValueOrReturn(_NeuExplicitValue<void>&&)
        : value(_NeuExplicitValue<void> { }) { }


    template<typename U>
    _NeuExplicitValueOrReturn(U&& v) requires(!IsVoid<Return>)
        : value(Return { forward<U>(v) }) { }

    _NeuExplicitValueOrReturn(void) requires(IsVoid<Return>)
        : value(Empty { }) { }

    bool isReturn() const { 
        
        return value.template has<Conditional<IsVoid<Return>, Empty, Return>>();
    }

    Return releaseReturn() {

        if constexpr (IsVoid<Return>) {

            return;
        }
        else {

            return move(value).template get<Return>();
        }
    }

    Value releaseValue() {

        if constexpr (IsVoid<Value>) {

            return;
        }
        else {

            return move(value).template get<_NeuExplicitValue<Value>>().value;
        }
    }

    Variant<Conditional<IsVoid<Return>, Empty, Return>, _NeuExplicitValue<Value>> value;
};

#define NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(x) ({  \
    auto&& _neu_value = x;                          \
    if (_neu_value.isReturn()) {                    \
        return _neu_value.releaseReturn();          \
    }                                               \
    _neu_value.releaseValue();                      \
})





























































































int main(int argc, char** argv) {

    Array<String> args;

    for (int i = 0; i < argc; ++i) {

        MUST(args.push(argv[i]));
    }

    auto result = _neu_main(move(args));

    if (result.isError()) {

        warnLine("Runtime error: {}", result.error());

        return 1;
    }

    return result.value();

}

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);
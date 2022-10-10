
#pragma once

#include <stdint.h>

#include "AllOf.h"
#include "AnyOf.h"
#include "Assertions.h"
#include "Array.h"
#include "Atomic.h"
#include "Badge.h"
#include "BitCast.h"
#include "BuiltinWrappers.h"
#include "ByteBuffer.h"
#include "CharacterTypes.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Debug.h"
#include "Detail.h"
#include "Dictionary.h"
#include "Error.h"
#include "Find.h"
#include "FixedPoint.h"
#include "Format.h"
#include "Function.h"
#include "GenericLexer.h"
#include "HashFunctions.h"
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
#include "Span.h"
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

using Float = float; // 32-bit
using Double = double; // 64-bit

template<typename T>
struct Range {

    using IndexType = T;

    T start { };

    T end { };
};

// FIXME: Remove this once we can call qualified functions like "String.number" directly from neu

inline String runtimeHelperNumberToString(Int64 number) {

    return String::number(number);
}

int __neu_main(Array<String>);

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

int main(int argc, char** argv) {

    Array<String> args;

    for (int i = 0; i < argc; ++i) {

        args.append(argv[i]);
    }

    return __neu_main(move(args));
}

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);
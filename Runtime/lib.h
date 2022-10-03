
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
#include "Debug.h"
#include "Detail.h"
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

// FIXME: Remove this once we can call qualified functions like "String.number" directly from neu

inline String runtimeHelperNumberToString(Int64 number) {

    return String::number(number);
}

int __neu_main(Vector<String>);

int main(int argc, char** argv) {

    Vector<String> args;

    for (int i = 0; i < argc; ++i) {

        args.append(argv[i]);
    }

    return __neu_main(move(args));
}

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);